using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OFDViewer.PageDescription.Colors.ColorItems
{
    /// <summary>
    /// 颜色段
    /// </summary>
    public class RadialShdSegment
    {
        /// <summary>
        /// 此段的颜色, 应使用基本颜色
        /// 必选
        /// </summary>
        [XmlElement("Color")]
        public CT_Color Color { get; set; }

        /// <summary>
        /// 用于确定StartPoint 和 EndPoint 中的各颜色的位置值, 取值范围是
        /// [0, 1.0], 各颜色的 Position 应根据颜色出现的顺序递增。 第一个
        /// Segment 的 Position 属性默认值为0, 最后一个 Segment 的 Position
        /// 属性默认值为1.0, 当不存在时, 在空缺区间内平均分配。 例如 Segment 个数等于2 且不出现 Position 属性时, 
        /// 按照“0 1.0” 处理;Segment 个数等于3 且不出现 Position 属性时, 按照“0 0.5 1.0” 处理;
        /// Segment 个数等于5 且不出现Position 属性时, 按照“00.25 0.5 0.751.0”处理
        /// 可选
        /// </summary>
        [XmlAttribute("Position")]
        public double Position { get; set; }
    }
}
