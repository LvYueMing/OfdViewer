using OFDViewer.Parse;
using OFDViewer.Render;
using SkiaSharp;
using Xunit;

namespace OFDViewer.Tests
{
    /// <summary>
    /// Doc/ofdtest/OFD文件 目录全量样本批量回归测试。
    /// 自动发现目录下（含子目录）所有 .ofd 文件，每份文件依次验证：
    /// 解析成功 → 全部页面渲染非空且非空白。
    /// </summary>
    public class OfdBatchFileTests
    {
        /// <summary>从测试输出目录向上定位仓库内样本目录，不依赖开发机绝对路径</summary>
        private static string? FindSampleDirectory()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null)
            {
                var candidate = Path.Combine(dir.FullName, "Doc", "ofdtest", "OFD文件");
                if (Directory.Exists(candidate))
                    return candidate;

                dir = dir.Parent;
            }

            return null;
        }

        /// <summary>递归枚举全部 .ofd 样本（按路径排序保证结果稳定），目录缺失时返回空数组</summary>
        private static string[] EnumerateOfdFiles()
        {
            var sampleDirectory = FindSampleDirectory();
            if (sampleDirectory == null)
                return Array.Empty<string>();

            return Directory
                .EnumerateFiles(sampleDirectory, "*.ofd", SearchOption.AllDirectories)
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();
        }

        public static IEnumerable<object[]> OfdFiles =>
            EnumerateOfdFiles().Select(path => new object[] { path });

        [Fact]
        public void SampleDirectory_ShouldContainOfdFiles()
        {
            // 守卫：目录缺失或无样本时让测试显式失败，避免 Theory 静默退化为 0 个用例
            var files = EnumerateOfdFiles();
            Assert.True(
                files.Length > 0,
                "未在 Doc/ofdtest/OFD文件 目录下发现任何 .ofd 样本文件");
        }

        [Theory]
        [MemberData(nameof(OfdFiles))]
        public void ParseAndRender_ShouldSucceed(string ofdPath)
        {
            try
            {
                // 阶段一：解析验证
                using (var reader = new OFDReader(ofdPath))
                {
                    var root = reader.ParseOFDDocument();

                    Assert.NotNull(root);
                    Assert.NotNull(root.RootOfd);
                }

                // 阶段二：全部页面渲染验证（解析失败不会走到这里）
                using (var renderer = new OfdRenderer(ofdPath))
                {
                    Assert.True(renderer.PageCount > 0, "文档页面数为 0");

                    for (int pageIndex = 0; pageIndex < renderer.PageCount; pageIndex++)
                    {
                        var png = renderer.RenderPageToBitmap(pageIndex);

                        Assert.NotNull(png);
                        Assert.True(png.Length > 0, $"第 {pageIndex + 1} 页渲染结果为空");

                        using var bitmap = SKBitmap.Decode(png);
                        Assert.NotNull(bitmap);

                        Assert.True(
                            HasDrawnContent(bitmap),
                            $"第 {pageIndex + 1} 页渲染结果为空白（无绘制内容）");
                    }
                }
            }
            catch (Exception ex)
            {
                // Theory 的参数名在 DisplayName 中会被截断，把文件名放进异常消息保证可定位
                throw new InvalidOperationException(
                    $"样本处理失败 [{Path.GetFileName(ofdPath)}]", ex);
            }
        }

        /// <summary>抽样像素检查页面是否绘制过内容（文字/图像/签章），与现有样本测试口径一致</summary>
        private static bool HasDrawnContent(SKBitmap bitmap)
        {
            for (int y = 0; y < bitmap.Height; y += 4)
            {
                for (int x = 0; x < bitmap.Width; x += 4)
                {
                    var color = bitmap.GetPixel(x, y);
                    if (color.Red < 240 || color.Green < 240 || color.Blue < 240)
                        return true;
                }
            }

            return false;
        }
    }
}
