using System.Xml.Serialization;

namespace OFDViewer.Models.Annotation
{
    /// <summary>
    /// 注释参数(键值对)
    /// </summary>
    public class Parameter
    {
        /// <summary>
        /// 注释参数名称
        /// 必选
        /// </summary>
        [XmlAttribute("Name", DataType = "string", AttributeName = "Name")]
        public string Name { get; set; }

        /// <summary>
        /// 参数值
        /// </summary>
        [XmlText(DataType = "string")]
        public string Value { get; set; }
    }
}