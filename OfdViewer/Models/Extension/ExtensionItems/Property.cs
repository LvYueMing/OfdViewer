using System.Xml.Serialization;

namespace OFDViewer.Models.Extension
{
    /// <summary>
    /// 扩展属性元素
    /// </summary>
    public class Property : ExtensionItem
    {
        /// <summary>
        /// 属性值
        /// </summary>
        [XmlText(DataType = "string")]
        public string Value { get; set; }

        /// <summary>
        /// 属性名称
        /// 必选
        /// </summary>
        [XmlAttribute("Name", DataType = "string", AttributeName = "Name")]
        public string Name { get; set; }

        /// <summary>
        /// 属性类型
        /// 可选
        /// </summary>
        [XmlAttribute("Type", DataType = "string", AttributeName = "Type")]
        public string Type { get; set; }
    }
}