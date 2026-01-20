using OFDViewer.Render.Abstractions;
using System.Drawing;

namespace OFDViewer.Render.DataModels
{
    /// <summary>
    /// 全局渲染配置
    /// </summary>
    public class RenderConfig
    {
        /// <summary>
        /// 渲染质量
        /// </summary>
        public RenderQuality Quality { get; set; } = RenderQuality.HighQuality;

        /// <summary>
        /// 是否启用抗锯齿
        /// </summary>
        public bool AntiAlias { get; set; } = true;

        /// <summary>
        /// 是否渲染批注
        /// </summary>
        public bool RenderAnnotations { get; set; } = true;

        /// <summary>
        /// 是否渲染印章
        /// </summary>
        public bool RenderSeals { get; set; } = true;

        /// <summary>
        /// 是否启用硬件加速
        /// </summary>
        public bool HardwareAcceleration { get; set; } = true;

        /// <summary>
        /// 分辨率（DPI）
        /// </summary>
        public float Dpi { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public RenderConfig()
        {
            // 默认获取当前屏幕的DPI
            using (var graphics = Graphics.FromHwnd(IntPtr.Zero))
            {
                Dpi = graphics.DpiX;
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="dpi">指定的DPI</param>
        public RenderConfig(float dpi)
        {
            Dpi = dpi;
        }
    }
}
