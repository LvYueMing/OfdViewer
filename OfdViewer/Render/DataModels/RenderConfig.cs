using OFDViewer.Render.Abstractions;

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
        public float Dpi { get; set; } = 96.0f;
    }
}
