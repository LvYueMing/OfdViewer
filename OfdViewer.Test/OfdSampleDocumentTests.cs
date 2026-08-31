using System.Xml;
using OFDViewer.Models.BaseStructure.MainEntry;
using OFDViewer.Models.BaseStructure.Resources.ResItems;
using OFDViewer.Parse;
using OFDViewer.Render;
using SkiaSharp;
using Xunit;

namespace OFDViewer.Tests
{
    /// <summary>
    /// 真实 OFD 样本文档（Doc/会诊记录.ofd）解析测试。
    /// 该文档包含多处非标准写法（PDF 日期格式、大写资源目录、空日期属性等），
    /// 用于回归验证解析器的容错能力。
    /// </summary>
    public class OfdSampleDocumentTests
    {
        /// <summary>定位仓库内测试夹具，不依赖开发机绝对路径</summary>
        private static string FindSampleOfdPath()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null)
            {
                var candidate = Path.Combine(dir.FullName, "Doc", "会诊记录.ofd");
                if (File.Exists(candidate))
                    return candidate;
                dir = dir.Parent;
            }
            throw new FileNotFoundException("未找到测试夹具 Doc/会诊记录.ofd");
        }

        [Fact]
        public void Parse_SampleDocument_ShouldSucceed()
        {
            var path = FindSampleOfdPath();

            using var reader = new OFDReader(path);
            var root = reader.ParseOFDDocument();

            Assert.NotNull(root);
            Assert.NotNull(root.RootOfd);
            Assert.Equal(1, root.DocCount);
            Assert.Equal(2, root.DefaultOFDDocument.PageDocs.Count);
        }

        [Fact]
        public void Parse_SampleDocument_DocInfoDates_ShouldParse()
        {
            var path = FindSampleOfdPath();

            using var reader = new OFDReader(path);
            var root = reader.ParseOFDDocument();

            // CreationDate 为 PDF 日期格式 D:20180701152117+08'00'
            var docInfo = root.RootOfd.DocBodies[0].DocInfo;
            Assert.NotNull(docInfo);
            Assert.Equal(new DateTime(2018, 7, 1, 7, 21, 17, DateTimeKind.Utc), docInfo.CreationDate);
        }

        [Fact]
        public void Parse_SampleDocument_Annotations_ShouldParse()
        {
            var path = FindSampleOfdPath();

            using var reader = new OFDReader(path);
            var root = reader.ParseOFDDocument();

            var doc = root.DefaultOFDDocument;
            Assert.NotNull(doc.Annotations);
            Assert.Equal(2, doc.PageAnnotDocs.Count);

            var firstAnnotDoc = doc.PageAnnotDocs[0];
            Assert.NotNull(firstAnnotDoc.PageAnnot);
            Assert.NotEmpty(firstAnnotDoc.PageAnnot.Annotations);
            Assert.Equal(183, firstAnnotDoc.PageAnnot.Annotations[0].ID);
        }

        [Fact]
        public void Parse_SampleDocument_Resources_ShouldResolve()
        {
            var path = FindSampleOfdPath();

            using var reader = new OFDReader(path);
            var root = reader.ParseOFDDocument();
            var doc = root.DefaultOFDDocument;

            // 文档资源（MultiMedias 图片）
            Assert.NotNull(doc.DocumentResource);
            var multiMedias = doc.DocumentResource.ResItems.OfType<MultiMedias>().FirstOrDefault();
            Assert.NotNull(multiMedias);
            Assert.NotEmpty(multiMedias.multiMedias);

            // 公共资源（字体）
            Assert.NotNull(doc.PublicResource);
            var fonts = doc.PublicResource.ResItems.OfType<OFDFonts>().FirstOrDefault();
            Assert.NotNull(fonts);
            Assert.NotEmpty(fonts.ofdFonts);

            // 资源文件按 BaseLoc + MediaFile 定位（该文档资源目录为大写 DOC_0/Res）
            var imageData = doc.GetResourceFile(1, "Image_62.PNG");
            Assert.NotNull(imageData);
            Assert.True(imageData.Length > 0);
        }

        [Fact]
        public void Parse_SampleDocument_AllResourceFiles_ShouldLoad()
        {
            var path = FindSampleOfdPath();

            using var reader = new OFDReader(path);
            var root = reader.ParseOFDDocument();
            var doc = root.DefaultOFDDocument;

            // 该文档资源目录实际为大写 DOC_0/Res，图片扩展名亦为大写 .PNG
            string[] expectedImages = { "Image_62.PNG", "Image_126.PNG", "Image_185.PNG", "Image_249.PNG" };
            foreach (var image in expectedImages)
            {
                var data = doc.GetResourceFile(1, image);
                Assert.NotNull(data);
                Assert.True(data.Length > 0, $"资源 {image} 内容为空");
            }

            // 嵌入字体（PublicRes 中的 FontFile；ST_Loc 为值类型，需判断路径非空）
            var fonts = doc.PublicResource.ResItems.OfType<OFDFonts>().First();
            foreach (var font in fonts.ofdFonts.Where(f => !string.IsNullOrEmpty(f.FontFile.Path)))
            {
                var fontData = doc.GetResourceFile(1, font.FontFile.Path);
                Assert.NotNull(fontData);
                Assert.True(fontData.Length > 0, $"字体 {font.FontFile.Path} 内容为空");
            }
        }

        [Fact]
        public void Render_SampleDocument_AllPages_ShouldSucceed()
        {
            var path = FindSampleOfdPath();

            using var renderer = new OfdRenderer(path);

            Assert.Equal(2, renderer.PageCount);

            for (int pageIndex = 0; pageIndex < renderer.PageCount; pageIndex++)
            {
                var png = renderer.RenderPageToBitmap(pageIndex);

                Assert.NotNull(png);
                Assert.True(png.Length > 0, $"第 {pageIndex + 1} 页渲染结果为空");
            }
        }

        [Fact]
        public void Render_SampleDocument_Pages_ShouldNotBeBlank()
        {
            // 渲染不抛异常不等于内容正确：验证页面确有像素被绘制（文字/图像/签章）
            var path = FindSampleOfdPath();

            using var renderer = new OfdRenderer(path);

            for (int pageIndex = 0; pageIndex < renderer.PageCount; pageIndex++)
            {
                var png = renderer.RenderPageToBitmap(pageIndex);
                using var bitmap = SKBitmap.Decode(png);

                Assert.NotNull(bitmap);

                int nonWhitePixels = 0;
                for (int y = 0; y < bitmap.Height; y += 4)
                {
                    for (int x = 0; x < bitmap.Width; x += 4)
                    {
                        var color = bitmap.GetPixel(x, y);
                        if (color.Red < 240 || color.Green < 240 || color.Blue < 240)
                            nonWhitePixels++;
                    }
                }

                Assert.True(nonWhitePixels > 0, $"第 {pageIndex + 1} 页渲染结果为空白（无绘制内容）");
            }
        }
    }
}
