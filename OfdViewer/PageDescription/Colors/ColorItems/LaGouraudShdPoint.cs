using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OFDViewer.PageDescription.Colors.ColorItems
{
    public class LaGouraudShdPoint
    {
        /// <summary>
        /// 控制点对应的颜色, 应使用基本颜色 必选
        /// </summary>
        [XmlElement("Color")]
        public CT_Color Color { get; set; }

        /// <summary>
        /// 控制点水平位置 必选
        /// </summary>
        [XmlAttribute("X")]
        public double X { get; set; }

        /// <summary>
        /// 控制点垂直位置 必选
        /// </summary>
        [XmlAttribute("Y")]
        public double Y { get; set; }
    }
}
