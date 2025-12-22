using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using OFDViewer.BaseType;
using OFDViewer.Graph.ShapeItems;

namespace OFDViewer.Graph
{
    /// <summary>
    /// 区域的分路径结构
    /// RegionArea节点的Start属性所指定的位置为当前点,之后以上一个线条的结束位置为当前点。图形对象描述见表37。
    /// </summary>
    public class RegionArea
    {
        /// <summary>
        /// 图形对象描述
        /// </summary>
        /// <remarks>
        /// Choice节点（Move/Line/QuadraticBezier/CubicBezier/Arc/Close）可重复
        /// 注意：XmlChoiceIdentifier 用于标识具体选择的元素类型
        /// </remarks>
        [XmlElement("Move",typeof(Move))]
        [XmlElement("Line", typeof(Line))]
        [XmlElement("QuadraticBezier", typeof(QuadraticBezier))]
        [XmlElement("CubicBezier", typeof(CubicBezier))]
        [XmlElement("Arc", typeof(Arc))]
        [XmlElement("Close", typeof(Close))]
        [XmlChoiceIdentifier(MemberName = "ShapeItemNames")]
        public List<object> ShapeItems { get; set; } = new List<object>();


        // 用于标识Choice中具体元素类型的属性（序列化时不输出）
        [XmlIgnore]
        public List<ShapeItemEnum> ShapeItemNames { get; set; } = new List<ShapeItemEnum>();

        /// <summary>
        /// 定义子图形的起始点坐标 必选
        /// </summary>
        [XmlAttribute("Start")]
        public string StartPos
        {
            get=> Start.ToString();
            set => Start = ST_Pos.Parse(value);
        }

        [XmlIgnore]
        public ST_Pos Start { get; set; }
    }
}
