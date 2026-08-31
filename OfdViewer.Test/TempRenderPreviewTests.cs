using OFDViewer.Render;
using OFDViewer.Render.DataModels;
using Xunit;

namespace OFDViewer.Tests
{
    /// <summary>
    /// 临时工具：渲染样本文档页面为 PNG，供人工核对排版。输出完成后删除本文件。
    /// </summary>
    public class TempRenderPreviewTests
    {
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
            throw new FileNotFoundException("未找到 Doc/会诊记录.ofd");
        }

        [Fact]
        public void SaveRenderPreview()
        {
            var path = FindSampleOfdPath();
            var outputDir = Path.Combine(Path.GetDirectoryName(path)!, "_preview");
            Directory.CreateDirectory(outputDir);

            // 150 DPI 便于核对排版细节（默认 96 偏模糊）
            using var renderer = new OfdRenderer(path, new RenderConfig(150));

            for (int pageIndex = 0; pageIndex < renderer.PageCount; pageIndex++)
            {
                var png = renderer.RenderPageToBitmap(pageIndex);
                File.WriteAllBytes(Path.Combine(outputDir, $"page-{pageIndex + 1}.png"), png);
            }

            Assert.True(Directory.GetFiles(outputDir, "*.png").Length == renderer.PageCount);
        }
    }
}
