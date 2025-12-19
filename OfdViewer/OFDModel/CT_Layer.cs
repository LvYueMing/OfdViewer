using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using OFDViewer.OFDEnum;

namespace OFDViewer.OFDModel
{
    /// <summary>
    /// 图层结构
    /// </summary>
    [XmlType(TypeName = "CT_Layer")]
    public class CT_Layer : CT_PageBlock
    {
        /// <summary>
        /// Type属性（枚举类型，默认值Body）
        /// 对应XSD中<xs:attribute name="Type" default="Body">的定义
        /// </summary>
        [XmlAttribute("Type")]
        public LayerType Type { get; set; }

        /// <summary>
        /// 控制Type属性是否序列化（处理可选属性+默认值）
        /// 仅当Type不是默认值时才序列化，符合XSD规范
        /// </summary>
        [XmlIgnore]
        public bool TypeSpecified { get; set; }

        /// <summary>
        /// DrawParam属性（可选引用ID，类型为ST_RefID）
        /// 对应XSD中<xs:attribute name="DrawParam" type="ST_RefID"/>
        /// </summary>
        [XmlAttribute("DrawParam")]
        public string DrawParam { get; set; }

        /// <summary>
        /// 构造函数：设置Type默认值为Body
        /// 对应XSD中default="Body"的约束
        /// </summary>
        public CT_Layer() : base()
        {
            Type = LayerType.Body;
        }
    }
}
