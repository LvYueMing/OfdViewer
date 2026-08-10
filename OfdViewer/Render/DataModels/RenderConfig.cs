using OFDViewer.Render.Abstractions;

namespace OFDViewer.Render.DataModels
{
    /// <summary>
    /// 全局渲染配置
    /// </summary>
    public class RenderConfig
    {
        private int _maxEncodedImageBytes = 64 * 1024 * 1024;
        private long _maxDecodedImagePixels = 40_000_000;

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
        /// 单个编码图像允许读取的最大字节数，默认 64 MiB。
        /// </summary>
        public int MaxEncodedImageBytes
        {
            get => _maxEncodedImageBytes;
            set => _maxEncodedImageBytes = value > 0
                ? value
                : throw new ArgumentOutOfRangeException(nameof(MaxEncodedImageBytes));
        }

        /// <summary>
        /// 单个图像允许解码的最大像素数，默认 4000 万像素。
        /// </summary>
        public long MaxDecodedImagePixels
        {
            get => _maxDecodedImagePixels;
            set => _maxDecodedImagePixels = value > 0
                ? value
                : throw new ArgumentOutOfRangeException(nameof(MaxDecodedImagePixels));
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public RenderConfig()
        {
            // 核心渲染配置不读取窗口或屏幕状态，界面层可按实际 DeviceDpi 显式覆盖。
            Dpi = 96f;
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
