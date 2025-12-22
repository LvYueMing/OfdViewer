using OFDViewer.BaseType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OFDViewer.Version
{
    // Version元素对应的复杂类型
    public class Version
    {
        // 对应ID属性（type="xs:ID"、use="required" → 必填）
        [XmlAttribute("ID")]
        public string ID { get; set; }

        // 对应Index属性（type="xs:int"、use="required" → 必填）
        [XmlAttribute("Index")]
        public int Index { get; set; }

        // 对应Current属性（type="xs:boolean"、default="false" → 可选，默认false）
        [XmlAttribute("Current")]
        public bool Current { get; set; } = false; // 初始化默认值匹配XSD
        
        [XmlIgnore]
        public ST_Loc BaseLoc { get; set; }

        // 对应BaseLoc属性（type="ST_Loc"、use="required" → 必填）
        [XmlAttribute("BaseLoc")]
        public string BaseLocPath
        {
            get => BaseLoc.ToString();
            set => BaseLoc = value;
        }
    }
}
