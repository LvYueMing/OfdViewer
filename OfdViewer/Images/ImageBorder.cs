using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OFDViewer.PageDescription.Colors;
using System.Xml.Serialization;
using OFDViewer.BaseType;

namespace OFDViewer.Images
{
    /// <summary>
    /// 图像边框设置
    /// </summary>
    public class ImageBorder
    {
        /// <summary>
        /// 边框颜色,有关边框颜色描述见8.3.2基本颜色
        /// 默认为黑色
        ///  可选
        /// </summary>
        [XmlElement("BorderColor", IsNullable = false)]
        public CT_Color BorderColor { get; set; }

        /// <summary>
        /// 边框线宽,如果为0则表示边框不进行绘制
        /// 默认值为0.353mm
        /// 可选
        /// </summary>
        [XmlAttribute("LineWidth")]
        public double LineWidth { get; set; } = 0.353;

        /// <summary>
        /// 边框水平角半径
        /// 默认值为0
        /// 可选
        /// </summary>
        [XmlAttribute("HorizonalCornerRadius")]
        public double HorizonalCornerRadius { get; set; } = 0;

        /// <summary>
        /// 边框垂直角半径
        /// 默认值为0
        /// 可选
        /// </summary>
        [XmlAttribute("VerticalCornerRadius")]
        public double VerticalCornerRadius { get; set; } = 0;

        /// <summary>
        /// 边框虚线重复样式开始的位置,边框的起始点位置为左上角,绕行
        /// 方向为顺时针
        /// 默认值为0
        /// 可选
        /// </summary>
        [XmlAttribute("DashOffset")]
        public double DashOffset { get; set; } = 0;

        /// <summary>
        /// 边框虚线重复样式,边框的起始点位置为左上角,绕行方向为顺时针
        /// 可选
        /// </summary>
        [XmlAttribute("DashPattern")]
        public string DashPatternString
        {
            get => DashPattern.ToString();
            set => DashPattern = ST_Array.Parse(DashPatternString);
        }
        [XmlIgnore]
        public ST_Array DashPattern { get; set; }

    }
}
