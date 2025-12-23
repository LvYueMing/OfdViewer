using OFDViewer.Enums;
using OFDViewer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OFDViewer.PageDescription.Colors.ColorItems
{
    /// <summary>
    /// 渐变控制点, 至少出现3 个
    /// </summary>
    public class GouraudShdPoint
    {
        /// <summary>
        /// 控制点对应的颜色, 应使用基本颜色
        /// 必选
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

        /// <summary>
        /// 三角单元切换的方向标志 可选
        /// </summary>
        [XmlIgnore]
        public GouraudShdEdgeFlag EdgeFlag { get; set; }

        /// <summary>
        /// 三角单元切换的方向标志 可选
        /// </summary>
        [XmlAttribute("EdgeFlag")]
        public string EdgeFlagString
        {
            get => ((int)EdgeFlag).ToString(); // 转换为数字字符串（匹配 XSD 的 int 类型）
            set => EdgeFlag = EnumHelper.ParseEnum<GouraudShdEdgeFlag>(value);
        }
    }
}
