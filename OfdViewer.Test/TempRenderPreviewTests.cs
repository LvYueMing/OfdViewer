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
                foreach (var candidate in new[]
                {
                    Path.Combine(dir.FullName, "Doc", "ofdtest", "会诊记录.ofd"),
                    Path.Combine(dir.FullName, "Doc", "会诊记录.ofd"),
                })
                {
                    if (File.Exists(candidate))
                        return candidate;
                }
                dir = dir.Parent;
            }
            throw new FileNotFoundException("未找到 Doc/ofdtest/会诊记录.ofd");
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

        /// <summary>
        /// 批量验证：对 Doc/ofdtest 下所有样本执行 解析 + 全页面渲染，
        /// 每个文件/页面的异常单独捕获，汇总为报告文件供人工核对。
        /// </summary>
        [Fact]
        public void VerifyAllOfdSamples()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "Doc", "ofdtest")))
                dir = dir.Parent;
            Assert.NotNull(dir);

            var sampleDir = Path.Combine(dir!.FullName, "Doc", "ofdtest");
            var outputDir = Path.Combine(sampleDir, "_verify");
            Directory.CreateDirectory(outputDir);

            var samples = Directory.GetFiles(sampleDir, "*.ofd");
            // 防呆：样本缺失时直接失败，避免空报告造成"全部通过"的假象
            Assert.True(samples.Length > 0, $"未在 {sampleDir} 找到任何 .ofd 样本");

            var report = new List<string>();
            foreach (var ofdPath in samples)
            {
                var name = Path.GetFileName(ofdPath);
                report.Add($"===== {name} =====");

                try
                {
                    using var renderer = new OfdRenderer(ofdPath, new RenderConfig(150));
                    report.Add($"  解析成功: {renderer.PageCount} 页");
                    var fileDir = Path.Combine(outputDir, Path.GetFileNameWithoutExtension(ofdPath));
                    Directory.CreateDirectory(fileDir);

                    for (int pageIndex = 0; pageIndex < renderer.PageCount; pageIndex++)
                    {
                        try
                        {
                            var png = renderer.RenderPageToBitmap(pageIndex);
                            File.WriteAllBytes(Path.Combine(fileDir, $"page-{pageIndex + 1}.png"), png);
                            report.Add($"  页 {pageIndex + 1}: 渲染成功 ({png.Length} 字节)");
                        }
                        catch (Exception ex)
                        {
                            report.Add($"  页 {pageIndex + 1}: 渲染失败");
                            AppendExceptionChain(report, ex, "    ");
                        }
                    }
                }
                catch (Exception ex)
                {
                    report.Add("  解析失败");
                    AppendExceptionChain(report, ex, "    ");
                }
            }

            var reportPath = Path.Combine(outputDir, "verification-report.txt");
            File.WriteAllLines(reportPath, report);
            Console.WriteLine(string.Join(Environment.NewLine, report));
        }

        private static void AppendExceptionChain(List<string> report, Exception ex, string indent = "    ")
        {
            var level = ex;
            while (level != null)
            {
                report.Add($"{indent}{level.GetType().Name}: {level.Message}");
                if (level.StackTrace != null)
                    report.Add(indent + level.StackTrace.Replace(Environment.NewLine, Environment.NewLine + indent));
                level = level.InnerException;
            }
        }
    }
}
