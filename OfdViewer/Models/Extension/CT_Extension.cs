using System;
using System.Xml.Serialization;
using OFDViewer.Models.BaseType;
using OFDViewer.Models.Extension.ExtensionItems;

namespace OFDViewer.Models.Extension
{

    /// <summary>
    /// 扩展元素
    /// </summary>
    public class CT_Extension
    {
        /// <summary>
        /// 扩展项集合（Property、Data、ExtendData的选择）
        /// 1..*
        /// </summary>
        [XmlElement("Property", typeof(Property))]
        [XmlElement("Data", typeof(Data))]
        [XmlElement("ExtendData", typeof(ExtendData))]
        public List<ExtensionItem> ExtensionItems { get; set; } = new List<ExtensionItem>();

        /// <summary>
        /// 控制ExtensionItems属性是否序列化
        /// </summary>
        public bool ShouldSerializeExtensionItems()
        {
            // 当ExtensionItems为空时不序列化
            return ExtensionItems != null && ExtensionItems.Count > 0;
        }

        /// <summary>
        /// 用于生成或解释该自定义对象数据的扩展应用程序名称
        /// 必选
        /// </summary>
        [XmlAttribute("AppName", DataType = "string", AttributeName = "AppName")]
        public string AppName { get; set; }

        /// <summary>
        /// 形成此扩展信息的软件厂商标识
        /// 可选
        /// </summary>
        [XmlAttribute("Company", DataType = "string", AttributeName = "Company")]
        public string Company { get; set; }

        /// <summary>
        /// 形成此扩展信息的软件版本
        /// 可选
        /// </summary>
        [XmlAttribute("AppVersion", DataType = "string", AttributeName = "AppVersion")]
        public string AppVersion { get; set; }

        /// <summary>
        /// 形成此扩展信息的日期时间
        /// 可选
        /// </summary>
        [XmlAttribute("Date", DataType = "dateTime", AttributeName = "Date")]
        public DateTime Date { get; set; }

        /// <summary>
        /// 控制Date属性是否序列化
        /// </summary>
        public bool ShouldSerializeDate()
        {
            // 当Date为默认值DateTime.MinValue时不序列化
            return Date != DateTime.MinValue;
        }

        /// <summary>
        /// 引用扩展项针对的文档项目的标识
        /// 必选
        /// </summary>
        [XmlAttribute("RefId", DataType = "string", AttributeName = "RefId")]
        public string RefIdString
        {
            get => RefId.ToString();
            set => RefId = ST_RefID.Parse(value);
        }

        [XmlIgnore]
        public ST_RefID RefId { get; set; }
    }
}