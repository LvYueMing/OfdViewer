using System.Xml.Serialization;

namespace OFDViewer.Models.CustomTag
{
    /// <summary>
    /// 外部系统或用户可以添加自定义的标记和信息,从而达到与其他系统、数据进行交互的目的并扩展
    /// 应用。一个文档可以带有多个自定义标引。
    /// </summary>
    [XmlRoot("CustomTags", Namespace = "http://www.ofdspec.org/2016")]
    public class CustomTags
    {
        /// <summary>
        /// 自定义标引元素集合
        /// 0..n
        /// </summary>
        [XmlElement("CustomTag", IsNullable = false)]
        public List<CustomTag> Tags { get; set; } = new List<CustomTag>();
    }
}