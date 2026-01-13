using System.Xml.Serialization;

namespace OFDViewer.Models.Extension
{
    /// <summary>
    /// 扩展信息的根节点
    /// </summary>
    [XmlRoot("Extensions", Namespace = "http://www.ofdspec.org/2016")]
    public class Extensions
    {
        /// <summary>
        /// 扩展元素集合
        /// 1..*
        /// </summary>
        [XmlElement("Extension", IsNullable = false)]
        public List<CT_Extension> ExtensionList { get; set; } = new List<CT_Extension>();
    }
}