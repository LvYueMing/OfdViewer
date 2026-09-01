using OFDViewer.Render.DataModels;
using Xunit;

namespace OFDViewer.Tests
{
    /// <summary>
    /// 目标渲染 DPI 计算回归测试。
    ///
    /// 背景：WinForm 展示层此前固定以 96 DPI 渲染，在高分屏（125%/150% 系统缩放）
    /// 或放大阅读（Zoom&gt;100%）时位图被上采样拉伸导致页面模糊。
    /// 修复策略：渲染 DPI = 设备 DPI × max(1, Zoom)，并钳制上限防止内存爆炸；
    /// 缩小显示时不降低渲染分辨率（缩小高位图仍清晰）。
    /// </summary>
    public class RenderConfigDpiTests
    {
        private const float MaxDpi = 300f;

        [Theory]
        [InlineData(96f, 1.0, 96f)]    // 标准屏 100%：维持 96
        [InlineData(144f, 1.0, 144f)]  // 150% 系统缩放：跟随设备 DPI
        [InlineData(120f, 1.25, 150f)] // 125% 缩放屏：设备 DPI × Zoom
        [InlineData(96f, 2.0, 192f)]   // 放大阅读 200%：按 Zoom 提升渲染分辨率
        [InlineData(144f, 3.0, 300f)]  // 超过上限：钳制到 300 DPI
        [InlineData(96f, 0.5, 96f)]    // 缩小显示：不降低渲染分辨率
        [InlineData(192f, 0.5, 192f)]  // 200% 屏缩小显示：仍按设备 DPI
        public void CalcTargetRenderDpi_ShouldFollowDeviceDpiAndZoom(float deviceDpi, double zoom, float expected)
        {
            var actual = RenderConfig.CalcTargetRenderDpi(deviceDpi, zoom, MaxDpi);

            Assert.Equal(expected, actual);
        }
    }
}
