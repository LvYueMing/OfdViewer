namespace OFDViewer.Render.DataModels
{
    /// <summary>
    /// 渲染样式基类
    /// 包含颜色、透明度、Z轴层级等通用属性
    /// </summary>
    public class RenderStyle
    {
        /// <summary>
        /// 颜色（ARGB格式）
        /// </summary>
        public uint Color { get; set; } = 0xFF000000; // 默认黑色

        /// <summary>
        /// 透明度（0-255，0表示完全透明，255表示完全不透明）
        /// </summary>
        public byte Alpha { get; set; } = 255;

        /// <summary>
        /// Z轴层级，用于处理元素叠加顺序
        /// 数值越大，显示越靠上
        /// </summary>
        public int ZIndex { get; set; } = 0;
    }

    /// <summary>
    /// 图形样式类
    /// 继承自RenderStyle，添加图形特有的样式属性
    /// </summary>
    public class GraphStyle : RenderStyle
    {
        /// <summary>
        /// 描边宽度
        /// </summary>
        public float StrokeWidth { get; set; } = 1.0f;

        /// <summary>
        /// 描边颜色（ARGB格式）
        /// </summary>
        public uint StrokeColor { get; set; } = 0xFF000000; // 默认黑色

        /// <summary>
        /// 描边透明度
        /// </summary>
        public byte StrokeAlpha { get; set; } = 255;

        /// <summary>
        /// 是否填充
        /// </summary>
        public bool Fill { get; set; } = true;

        /// <summary>
        /// 是否描边
        /// </summary>
        public bool Stroke { get; set; } = false;

        /// <summary>
        /// 虚线样式（数组中的元素表示实线和虚线的长度）
        /// </summary>
        public float[] DashPattern { get; set; } = null;
    }

    /// <summary>
    /// 文本样式类
    /// 继承自RenderStyle，添加文本特有的样式属性
    /// </summary>
    public class TextStyle : RenderStyle
    {
        /// <summary>
        /// 字体资源对象
        /// </summary>
        public string FontFilePath { get; set; } = null;

        /// <summary>
        /// 字体名称
        /// </summary>
        public string FontFamily { get; set; } = "宋体";

        /// <summary>
        /// 字号（像素）
        /// </summary>
        public float FontSize { get; set; } = 12.0f;

        /// <summary>
        /// 字体粗细（100-900）
        /// </summary>
        public int FontWeight { get; set; } = 400;

        /// <summary>
        /// 是否斜体
        /// </summary>
        public bool Italic { get; set; } = false;

        /// <summary>
        /// 是否下划线
        /// </summary>
        public bool Underline { get; set; } = false;

        /// <summary>
        /// 是否删除线
        /// </summary>
        public bool StrikeThrough { get; set; } = false;

        /// <summary>
        /// 文本水平缩放比例
        /// </summary>
        public float HScale { get; set; } = 1.0f;

        /// <summary>
        /// 是否描边
        /// </summary>
        public bool Stroke { get; set; } = false;

        /// <summary>
        /// 描边宽度
        /// </summary>
        public float StrokeWidth { get; set; } = 1.0f;

        /// <summary>
        /// 描边颜色（ARGB格式）
        /// 默认透明
        /// </summary>
        public uint StrokeColor { get; set; } = 255;

    }

    /// <summary>
    /// 图像样式类
    /// 继承自RenderStyle，添加图像特有的样式属性
    /// </summary>
    public class ImageStyle : RenderStyle
    {
        /// <summary>
        /// 是否保持纵横比
        /// </summary>
        public bool PreserveAspectRatio { get; set; } = true;

        /// <summary>
        /// 图像插值模式
        /// </summary>
        public ImageInterpolationMode InterpolationMode { get; set; } = ImageInterpolationMode.HighQuality;
    }

    /// <summary>
    /// 图像插值模式枚举
    /// </summary>
    public enum ImageInterpolationMode
    {
        /// <summary>
        /// 低质量，高性能
        /// </summary>
        LowQuality,
        /// <summary>
        /// 中等质量
        /// </summary>
        MediumQuality,
        /// <summary>
        /// 高质量，低性能
        /// </summary>
        HighQuality
    }
}
