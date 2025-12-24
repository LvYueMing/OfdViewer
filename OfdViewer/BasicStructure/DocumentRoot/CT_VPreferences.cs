using OFDViewer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OFDViewer.BasicStructure.DocumentRoot
{
    /// <summary>
    /// 文档视图首选项
    /// 达到限定文档初始化视图便于阅读的目标。 文档视图首选项结构如图10 所示。
    /// </summary>
    public class CT_VPreferences
    {
        /// <summary>
        /// 窗口模式
        /// 可选
        /// 默认值为 None
        /// </summary>
        [XmlIgnore]
        public VPreferencesPageMode PageMode { get; set; } = VPreferencesPageMode.None;

        /// <summary>
        /// 窗口模式
        /// 可选
        /// 默认值为 None
        /// </summary>
        [XmlElement("PageMode", IsNullable = false)]
        public string PageModeString
        {
            get => PageMode.ToString();
            set => PageMode = EnumHelper.ParseEnum<VPreferencesPageMode>(value);
        }

        /// <summary>
        /// 页面布局模式
        /// 默认值为 OneColumn
        /// 可选
        /// </summary>
        [XmlIgnore]
        public VPreferencesPageLayout PageLayout { get; set; } = VPreferencesPageLayout.OneColumn;

        /// <summary>
        ///  页面布局模式
        /// </summary>
        [XmlElement("PageLayout", IsNullable = false)]
        public string PageLayoutString
        {
            get => PageLayout.ToString();
            set => PageLayout = EnumHelper.ParseEnum<VPreferencesPageLayout>(value);
        }


        /// <summary>
        /// 标题栏显示模式
        /// 默认值为 FileName, 当设置为 DocTitle 但不存在 Title 属性时, 按照FileName 处理
        /// </summary>
        [XmlIgnore]
        public VPreferencesTabDisplay TabDisplay { get; set; } = VPreferencesTabDisplay.DocTitle;

        /// <summary>
        /// TabDisplay 字符串属性（用于 XML 序列化）
        /// </summary>
        [XmlElement("TabDisplay", IsNullable = false)]
        public string TabDisplayString
        {
            get => TabDisplay.ToString();
            set => TabDisplay = EnumHelper.ParseEnum<VPreferencesTabDisplay>(value);
        }


        /// <summary>
        /// 是否隐藏工具栏
        /// 默认值:false
        /// 可选
        /// </summary>
        [XmlElement("HideToolbar", IsNullable = false)]
        public bool HideToolbar { get; set; } = false;

        /// <summary>
        /// 是否隐藏菜单栏
        /// 默认值:false
        /// 可选
        /// </summary>
        [XmlElement("HideMenubar", IsNullable = false)]
        public bool HideMenubar { get; set; } = false;

        /// <summary>
        /// 是否隐藏主窗口之外的其他窗体组件
        /// 默认值:false
        /// 可选
        /// </summary>
        [XmlElement("HideWindowUI", IsNullable = false)]
        public bool HideWindowUI { get; set; } = false;


        /// <summary>
        /// 自动缩放模式
        /// 默认值为 Default
        /// 可选
        /// </summary>
        [XmlIgnore]
        public VPreferencesZoomMode? ZoomMode { get; set; }

        /// <summary>
        /// 自动缩放模式
        /// 默认值为 Default
        /// 可选
        /// </summary>
        [XmlElement("ZoomMode")]
        public string ZoomModeString
        {
            get => ZoomMode?.ToString();
            set => ZoomMode = string.IsNullOrEmpty(value) ? null : EnumHelper.ParseEnum<VPreferencesZoomMode>(value);
        }

        /// <summary>
        /// 文档的缩放率
        /// 可选
        /// </summary>
        [XmlElement("Zoom")]
        public double? Zoom { get; set; }



        #region 辅助方法：处理 XSD minOccurs="0" 的默认值序列化
        // 重写 ShouldSerialize 方法，确保默认值不序列化（符合 XSD minOccurs="0" 特性）
        public bool ShouldSerializePageModeString() => PageMode != VPreferencesPageMode.None;
        public bool ShouldSerializePageLayoutString() => PageLayout != VPreferencesPageLayout.OneColumn;
        public bool ShouldSerializeTabDisplayString() => TabDisplay != VPreferencesTabDisplay.DocTitle;
        public bool ShouldSerializeHideToolbar() => HideToolbar != false;
        public bool ShouldSerializeHideMenubar() => HideMenubar != false;
        public bool ShouldSerializeHideWindowUI() => HideWindowUI != false;
        public bool ShouldSerializeZoomModeString() => ZoomMode.HasValue;
        public bool ShouldSerializeZoom() => Zoom.HasValue;
        #endregion

    }
}
