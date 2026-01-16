using System.Xml.Serialization;
using OFDViewer.Models.BaseType;

namespace OFDViewer.Models.Versions
{
    // Version元素对应的复杂类型
    public class Version
    {
        /// <summary>
        /// 版本标识
        /// 对应ID属性（type="xs:ID"、use="required" → 必填）
        /// </summary>
        [XmlAttribute("ID")]
        public string ID { get; set; }

        /// <summary>
        /// 版本号
        /// 对应Index属性（type="xs:int"、use="required" → 必填）
        /// </summary>
        [XmlAttribute("Index")]
        public int Index { get; set; }

        /// <summary>
        /// 是否是默认版本
        /// 默认为false
        /// 对应Current属性（type="xs:boolean"、default="false" → 可选，默认false）
        /// </summary>
        [XmlAttribute("Current")]
        public bool Current { get; set; } = false; // 初始化默认值匹配XSD

        /// <summary>
        /// 控制Current属性是否序列化
        /// </summary>
        public bool ShouldSerializeCurrent()
        {
            // 当Current为默认值false时不序列化
            return Current != false;
        }

        [XmlIgnore]
        public ST_Loc BaseLoc { get; set; }

        /// <summary>
        /// 指向包内的版本描述文件
        /// 对应BaseLoc属性（type="ST_Loc"、use="required" → 必填）
        /// </summary>
        [XmlAttribute("BaseLoc")]
        public string BaseLocPath
        {
            get => BaseLoc.ToString();
            set => BaseLoc = value;
        }
    }
}
