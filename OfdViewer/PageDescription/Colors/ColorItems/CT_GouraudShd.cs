using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OFDViewer.PageDescription.Colors.ColorItems
{
    /// <summary>
    /// 高洛德渐变
    /// 高洛德渐变的基本原理是指定三个带有可选颜色的顶点, 在其构成的三角形区域内采用高洛德算法绘制渐变图形。 如图38。
    /// </summary>
    public class CT_GouraudShd
    {
        /// <summary>
        /// 渐变控制点, 至少出现3 个 必选
        /// </summary>
        [XmlElement("Point")] 
        public List<GouraudShdPoint> Points { get; set; } = new List<GouraudShdPoint>();

        /// <summary>
        /// 渐变范围外的填充颜色, 应使用基本颜色 可选
        /// </summary>
        [XmlElement("BackColor", IsNullable = false)]
        public CT_Color BackColor { get; set; }

        /// <summary>
        /// 在渐变控制点所确定范围之外的部分是否填充
        /// 0 为不填充, 1 表示填充
        /// 默认值为0
        /// 可选
        /// </summary>
        [XmlAttribute("Extend")]
        public int Extend { get; set; } = 0;
    }
}
