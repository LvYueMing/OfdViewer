using System.IO.Compression;
using System.Text;
using OFDViewer.Render;
using OFDViewer.Render.DataModels;
using OFDViewer.Render.Implementation;
using SkiaSharp;
using Xunit;

namespace OFDViewer.Tests
{
    /// <summary>
    /// 文字缺字形回退回归测试。
    ///
    /// 背景：部分 OFD 的字体资源只有 FontName 而无嵌入 FontFile（如湖北器械许可证样张中
    /// &lt;Font ID="98" FontName="AdobeSongStd-Light"/&gt;），系统未安装该字体时
    /// SKTypeface.FromFamilyName 落到无 CJK 字形的默认字体，中文渲染为方框。
    /// 修复：主字体缺字形时按字符用 SKFontManager 回退到系统字体。
    /// </summary>
    public class FontFallbackTests : IDisposable
    {
        private string _tempFilePath;

        public void Dispose()
        {
            if (!string.IsNullOrEmpty(_tempFilePath) && File.Exists(_tempFilePath))
                File.Delete(_tempFilePath);
        }

        /// <summary>
        /// 主字体（系统不存在的 AdobeSongStd-Light）无法覆盖中文字形时，
        /// 解析结果必须回退到含中文字形的系统字体。
        /// </summary>
        [Fact]
        public void ResolveTypeface_MissingCjkGlyphs_ShouldFallbackToSystemFont()
        {
            using var context = new SkiaRenderContext();
            context.Config = new RenderConfig(96);
            context.Initialize(200, 100);

            var style = new TextStyle { FontFamily = "AdobeSongStd-Light", FontSize = 12 };
            var typeface = context.ResolveTypefaceWithGlyphFallback("查询网址", style);

            Assert.NotNull(typeface);
            Assert.True(typeface.ContainsGlyph('查'),
                $"字体解析结果 {typeface.FamilyName} 缺少中文字形，缺字形回退未生效");
            Assert.True(typeface.ContainsGlyph('网'),
                $"字体解析结果 {typeface.FamilyName} 缺少中文字形，缺字形回退未生效");
        }

        /// <summary>主字体（系统默认拉丁字体）缺中文字形时同样应回退</summary>
        [Fact]
        public void ResolveTypeface_DefaultLatinFont_MissingCjkGlyphs_ShouldFallback()
        {
            using var context = new SkiaRenderContext();
            context.Config = new RenderConfig(96);
            context.Initialize(200, 100);

            // SKTypeface.Default 通常无中文字形；用一个确定存在的拉丁字体族名模拟
            var style = new TextStyle { FontFamily = "Times New Roman", FontSize = 12 };
            var typeface = context.ResolveTypefaceWithGlyphFallback("中文字形", style);

            Assert.NotNull(typeface);
            Assert.True(typeface.ContainsGlyph('中'),
                $"字体解析结果 {typeface.FamilyName} 缺少中文字形，缺字形回退未生效");
        }

        /// <summary>主字体完全覆盖文本字形时应返回主字体本身（不引入回退开销）</summary>
        [Fact]
        public void ResolveTypeface_AllGlyphsAvailable_ShouldReturnPrimaryTypeface()
        {
            using var context = new SkiaRenderContext();
            context.Config = new RenderConfig(96);
            context.Initialize(200, 100);

            var style = new TextStyle { FontFamily = "Times New Roman", FontSize = 12 };
            var typeface = context.ResolveTypefaceWithGlyphFallback("abc123", style);

            Assert.NotNull(typeface);
            Assert.True(typeface.ContainsGlyph('a'));
        }

        /// <summary>
        /// 端到端冒烟：字体资源只有 FontName 无 FontFile（复现湖北样张 Font 98 的结构），
        /// 渲染含中文+URL 的文本不应抛异常且必须产生墨迹。
        /// </summary>
        [Fact]
        public void Render_TextWithFontWithoutFontFile_ShouldProduceInk()
        {
            _tempFilePath = Path.Combine(Path.GetTempPath(), $"font_fallback_{Guid.NewGuid():N}.ofd");
            CreateMinimalOfd(_tempFilePath);

            using var renderer = new OfdRenderer(_tempFilePath, new RenderConfig(96));
            Assert.Equal(1, renderer.PageCount);

            var png = renderer.RenderPageToBitmap(0);
            Assert.True(HasInk(png), "页面未渲染出任何内容");
        }

        /// <summary>构造最小 OFD：字体仅有 FontName、无 FontFile，页面含引用该字体的文字</summary>
        private static void CreateMinimalOfd(string filePath)
        {
            const string ns = "xmlns:ofd=\"http://www.ofdspec.org/2016\"";

            var ofdXml = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<ofd:OFD {ns} DocType=""OFD"" Version=""1.1""><ofd:DocBody><ofd:DocInfo><ofd:DocID>font-fallback-test</ofd:DocID></ofd:DocInfo><ofd:DocRoot>Doc_0/Document.xml</ofd:DocRoot></ofd:DocBody></ofd:OFD>";

            var documentXml = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<ofd:Document {ns}><ofd:CommonData><ofd:MaxUnitID>2</ofd:MaxUnitID><ofd:PageArea><ofd:PhysicalBox>0 0 210 297</ofd:PhysicalBox></ofd:PageArea><ofd:PublicRes>PublicRes.xml</ofd:PublicRes></ofd:CommonData><ofd:Pages><ofd:Page ID=""1"" BaseLoc=""Pages/Page_0/Content.xml""/></ofd:Pages></ofd:Document>";

            var publicResXml = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<ofd:Res {ns}><ofd:Fonts><ofd:Font ID=""98"" FontName=""AdobeSongStd-Light"" CharSet=""prc""/></ofd:Fonts></ofd:Res>";

            var contentXml = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<ofd:Page {ns}><ofd:Area><ofd:PhysicalBox>0 0 210 297</ofd:PhysicalBox></ofd:Area><ofd:Content><ofd:Layer Type=""Body""><ofd:TextObject ID=""1"" Font=""98"" Size=""4"" Boundary=""10 10 60 8""><ofd:TextCode X=""0"" Y=""4"">查询网址：http://fda.hubei.gov.cn/</ofd:TextCode></ofd:TextObject></ofd:Layer></ofd:Content></ofd:Page>";

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            using (var zip = new ZipArchive(fileStream, ZipArchiveMode.Create))
            {
                WriteEntry(zip, "OFD.xml", ofdXml);
                WriteEntry(zip, "Doc_0/Document.xml", documentXml);
                WriteEntry(zip, "Doc_0/PublicRes.xml", publicResXml);
                WriteEntry(zip, "Doc_0/Pages/Page_0/Content.xml", contentXml);
            }
        }

        private static void WriteEntry(ZipArchive zip, string entryName, string content)
        {
            var entry = zip.CreateEntry(entryName);
            using var writer = new StreamWriter(entry.Open(), new UTF8Encoding(false));
            writer.Write(content);
        }

        private static bool HasInk(byte[] png)
        {
            using var bitmap = SKBitmap.Decode(png);
            Assert.NotNull(bitmap);

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    var c = bitmap.GetPixel(x, y);
                    if (c.Red < 250 || c.Green < 250 || c.Blue < 250)
                        return true;
                }
            }
            return false;
        }
    }
}
