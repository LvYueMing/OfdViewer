using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using OFDViewer.BaseType;
using OFDViewer.OFDEnum;
using OFDViewer.Utils;

namespace OFDViewer.OFDModel
{
    /// <summary>
    /// 图层结构
    /// </summary>
    public class CT_Layer : CT_PageBlock
    {
        /// <summary>
        /// 层类型描述,预定义的值见表15
        /// 默认为 Body
        /// 可选
        /// </summary>
        [XmlAttribute("Type")]
        public string TypeString
        {
            get => Type.ToString();
            set => Type = EnumHelper.ParseEnum<LayerType>(value);
        }

        /// <summary>
        /// 控制Type属性是否序列化（处理可选属性+默认值）
        /// 仅当Type不是默认值时才序列化，符合XSD规范
        /// </summary>
        [XmlIgnore]
        public LayerType Type { get; set; } = LayerType.Body;

        /// <summary>
        /// 图层的绘制参数,引用资源文件中定义的绘制参数标识 
        /// 可选
        /// </summary>
        [XmlAttribute("DrawParam")]
        public ST_RefID DrawParam { get; set; }

    }
}
