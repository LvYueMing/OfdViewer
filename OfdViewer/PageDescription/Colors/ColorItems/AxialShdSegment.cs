using OFDViewer.PageDescription.Colors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OFDViewer.PageDescription.Colors.ColorItems
{
    /// <summary>
    /// 渐变段类型
    /// </summary>
    public class AxialShdSegment
    {
        /// <summary>
        /// 段颜色
        /// </summary>
        [XmlElement("Color")]
        public CT_Color Color { get; set; }

        /// <summary>
        /// 段位置
        /// </summary>
        [XmlAttribute("Position")]
        public double Position { get; set; }
    }
}
