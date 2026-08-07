using OFDViewer.Render;
using OFDViewer.Render.DataModels;

namespace OFDViewer.Tests.Rendering
{
    /// <summary>
    /// OFD 渲染手工验证辅助方法。
    /// 输入与输出路径由调用方提供，避免绑定特定开发机目录。
    /// </summary>
    internal static class RenderTest
    {
        /// <summary>
        /// 将 OFD 首页渲染为图片。
        /// </summary>
        /// <param name="ofdFilePath">待渲染的 OFD 文件路径。</param>
        /// <param name="outputImagePath">输出图片路径。</param>
        internal static void RenderOfdToImage(string ofdFilePath, string outputImagePath)
        {
            using var renderer = new OfdRenderer(ofdFilePath);
            renderer.RenderPageToFile(outputImagePath, 0);
        }

        /// <summary>
        /// 将 OFD 的全部页面渲染到指定目录。
        /// </summary>
        /// <param name="ofdFilePath">待渲染的 OFD 文件路径。</param>
        /// <param name="outputDirectory">输出目录。</param>
        internal static void RenderOfdToMultipleImages(string ofdFilePath, string outputDirectory)
        {
            using var renderer = new OfdRenderer(ofdFilePath);
            renderer.RenderAllPagesToFile(outputDirectory);
        }

        /// <summary>
        /// 使用固定的高 DPI 配置将 OFD 首页渲染为图片。
        /// </summary>
        /// <param name="ofdFilePath">待渲染的 OFD 文件路径。</param>
        /// <param name="outputImagePath">输出图片路径。</param>
        internal static void RenderOfdWithCustomConfig(string ofdFilePath, string outputImagePath)
        {
            var renderConfig = new RenderConfig
            {
                Dpi = 150,
                AntiAlias = true
            };

            using var renderer = new OfdRenderer(ofdFilePath, renderConfig);
            renderer.RenderPageToFile(outputImagePath, 0);
        }
    }
}
