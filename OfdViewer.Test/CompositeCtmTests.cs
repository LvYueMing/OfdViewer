using System.IO.Compression;
using System.Text;
using OFDViewer.Render;
using OFDViewer.Render.DataModels;
using SkiaSharp;
using Xunit;

namespace OFDViewer.Tests
{
    /// <summary>
    /// 复合对象（CompositeObject → CompositeGraphicUnit）CTM 渲染回归测试。
    ///
    /// 背景：部分生成器（如 suwell-pdf2ofd）以"磅(pt)"为图元局部坐标单位，
    /// 通过 CompositeObject 的 CTM="0.3528 0 0 0.3528 0 0"（1pt=0.3528mm）
    /// 把 CGU 内部坐标缩放到页面毫米坐标系。若渲染时丢弃 CTM，
    /// CGU 内文字将按毫米直接渲染，字号与位置均放大约 2.83 倍（表现为"超大字体"）。
    /// </summary>
    public class CompositeCtmTests : IDisposable
    {
        private string _tempFilePath;

        public void Dispose()
        {
            if (!string.IsNullOrEmpty(_tempFilePath) && File.Exists(_tempFilePath))
                File.Delete(_tempFilePath);
        }

        /// <summary>
        /// CGU 内 Size=10 的单个汉字，经 CompositeObject CTM="0.5 0 0 0.5 0 0" 缩放后，
        /// 实际字高应为 5mm（96DPI 下约 18.9px，墨迹约 16px）。
        /// 若 CTM 被忽略，字高为 10mm（约 37.8px，墨迹约 32px），测试将失败。
        /// </summary>
        [Fact]
        public void Render_CompositeObject_WithScaleCtm_TextSizeShouldRespectCtm()
        {
            _tempFilePath = Path.Combine(Path.GetTempPath(), $"composite_ctm_{Guid.NewGuid():N}.ofd");
            CreateMinimalOfd(_tempFilePath);

            using var renderer = new OfdRenderer(_tempFilePath, new RenderConfig(96));
            Assert.Equal(1, renderer.PageCount);

            var png = renderer.RenderPageToBitmap(0);
            var (minX, minY, maxX, maxY, inkCount) = MeasureInkBBox(png);

            Assert.True(inkCount > 0, "页面上未渲染出任何内容");

            float inkWidth = maxX - minX + 1;
            float inkHeight = maxY - minY + 1;

            // 正确：单个汉字墨迹约 16px（5mm 字高）；放大约 2.83 倍后约 32px
            Assert.True(inkWidth < 28,
                $"复合对象内文字宽度 {inkWidth:F1}px 超出预期（<28px），疑似 CompositeObject CTM 未生效");
            Assert.True(inkHeight < 28,
                $"复合对象内文字高度 {inkHeight:F1}px 超出预期（<28px），疑似 CompositeObject CTM 未生效");
        }

        /// <summary>构造最小可渲染 OFD：页面内容含一个带缩放 CTM 的复合对象，CGU 内含单个文字</summary>
        private static void CreateMinimalOfd(string filePath)
        {
            const string ns = "xmlns:ofd=\"http://www.ofdspec.org/2016\"";

            var ofdXml = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<ofd:OFD {ns} DocType=""OFD"" Version=""1.1""><ofd:DocBody><ofd:DocInfo><ofd:DocID>composite-ctm-test</ofd:DocID></ofd:DocInfo><ofd:DocRoot>Doc_0/Document.xml</ofd:DocRoot></ofd:DocBody></ofd:OFD>";

            var documentXml = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<ofd:Document {ns}><ofd:CommonData><ofd:MaxUnitID>2</ofd:MaxUnitID><ofd:PageArea><ofd:PhysicalBox>0 0 210 297</ofd:PhysicalBox></ofd:PageArea><ofd:DocumentRes>DocumentRes.xml</ofd:DocumentRes></ofd:CommonData><ofd:Pages><ofd:Page ID=""1"" BaseLoc=""Pages/Page_0/Content.xml""/></ofd:Pages></ofd:Document>";

            var documentResXml = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<ofd:Res {ns}><ofd:CompositeGraphicUnits><ofd:CompositeGraphicUnit ID=""1"" Width=""30"" Height=""15""><ofd:Content ID=""1""><ofd:TextObject ID=""1"" Boundary=""0 0 20 12"" Size=""10""><ofd:TextCode X=""0"" Y=""8"">国</ofd:TextCode></ofd:TextObject></ofd:Content></ofd:CompositeGraphicUnit></ofd:CompositeGraphicUnits></ofd:Res>";

            var contentXml = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<ofd:Page {ns}><ofd:Area><ofd:PhysicalBox>0 0 210 297</ofd:PhysicalBox></ofd:Area><ofd:Content><ofd:Layer Type=""Body""><ofd:CompositeObject ID=""2"" ResourceID=""1"" CTM=""0.5 0 0 0.5 0 0"" Boundary=""10 10 60 30""/></ofd:Layer></ofd:Content></ofd:Page>";

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            using (var zip = new ZipArchive(fileStream, ZipArchiveMode.Create))
            {
                WriteEntry(zip, "OFD.xml", ofdXml);
                WriteEntry(zip, "Doc_0/Document.xml", documentXml);
                WriteEntry(zip, "Doc_0/DocumentRes.xml", documentResXml);
                WriteEntry(zip, "Doc_0/Pages/Page_0/Content.xml", contentXml);
            }
        }

        private static void WriteEntry(ZipArchive zip, string entryName, string content)
        {
            var entry = zip.CreateEntry(entryName);
            using var writer = new StreamWriter(entry.Open(), new UTF8Encoding(false));
            writer.Write(content);
        }

        /// <summary>扫描位图，计算非白色像素（墨迹）的外接矩形</summary>
        private static (int minX, int minY, int maxX, int maxY, int count) MeasureInkBBox(byte[] png)
        {
            using var bitmap = SKBitmap.Decode(png);
            Assert.NotNull(bitmap);

            int minX = bitmap.Width, minY = bitmap.Height, maxX = -1, maxY = -1, count = 0;

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    var c = bitmap.GetPixel(x, y);
                    if (c.Red < 250 || c.Green < 250 || c.Blue < 250)
                    {
                        count++;
                        if (x < minX) minX = x;
                        if (y < minY) minY = y;
                        if (x > maxX) maxX = x;
                        if (y > maxY) maxY = y;
                    }
                }
            }

            return (minX, minY, maxX, maxY, count);
        }
    }
}
