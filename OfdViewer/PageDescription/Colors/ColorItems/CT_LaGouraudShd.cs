using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OFDViewer.PageDescription.Colors.ColorItems
{
    /// <summary>
    /// 网格高洛德渐变
    /// 网格高洛德渐变是高洛德渐变的一种特殊形式, 其允许定义4 个以上的控制点, 按照每行固定的网
    /// 格数(VerticesPerRow) 形成若干行列, 相邻的4 个控制点定义一个网格单元, 在一个网格单元内 
    /// EdgeFlag 固定为1, 网格单元及多个单元组成网格区域的规则如图42 所示。
    /// </summary>
    public class CT_LaGouraudShd
    {
        /// <summary>
        /// 渐变控制点, 至少出现4 个 必选
        /// </summary>
        [XmlElement("Point")]
        public List<LaGouraudShdPoint> Point { get; set; } = new List<LaGouraudShdPoint>();

        /// <summary>
        ///渐变范围外的填充颜色, 应使用基本颜色 可选
        /// </summary>
        [XmlElement("BackColor", IsNullable = false)]
        public CT_Color BackColor { get; set; }

        /// <summary>
        /// 渐变区域内每行的网格数 必选
        /// </summary>
        [XmlAttribute("VerticesPerRow")]
        public int VerticesPerRow { get; set; }

        /// <summary>
        /// 在渐变控制点所确定范围之外的部分是否填充
        /// 0 为不填充, 1 表示填充
        /// 默认值为0
        /// 可选
        /// </summary>
        [XmlAttribute("Extend")]
        public int Extend { get; set; }

        /// <summary>
        /// 用于标识 Extend 属性是否被序列化（处理可选int类型的默认值问题）
        /// </summary>
        [XmlIgnore]
        public bool ExtendSpecified { get; set; }

    }
}
