using System.ComponentModel;

namespace OFDViewer.Models.Enums
{
    /// <summary>
    /// 图层类型描述
    /// </summary>
    public enum LayerType
    {
        /// <summary>
        /// 正文层
        /// </summary>
        [Description("正文层")]
        Body = 0,
        /// <summary>
        /// 前景层
        /// </summary>
        [Description("前景层")]
        Foreground = 1,
        /// <summary>
        /// 背景层
        /// </summary>
        [Description("背景层")]
        Background = 2,
        /// <summary>
        /// 自定义层
        /// </summary>
        [Description("自定义层")]
        Custom = 3
    }
}
