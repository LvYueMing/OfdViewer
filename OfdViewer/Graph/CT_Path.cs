using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using OFDViewer.Enums;
using OFDViewer.PageDescription;
using OFDViewer.PageDescription.Colors;
using OFDViewer.Utils;

namespace OFDViewer.Graph
{
    /// <summary>
    /// 图形对象具有一般图元对象的一切属性和行为特征。
    /// 图形对象结构如图46所示。
    /// </summary>
    public class CT_Path : CT_GraphicUnit
    {
        /// <summary>
        /// 勾边颜色 默认为黑色
        /// 可选
        /// </summary>
        [XmlElement(ElementName = "StrokeColor", IsNullable = true)]
        public CT_Color StrokeColor { get; set; }

        /// <summary>
        /// 填充颜色 默认为透明色
        /// 可选
        /// </summary>
        [XmlElement(ElementName = "FillColor", IsNullable = true)]
        public CT_Color FillColor { get; set; }

        /// <summary>
        /// 图形轮廓数据,由一系列紧缩的操作符和操作数构成 
        /// 必选
        /// </summary>
        [XmlElement(ElementName = "AbbreviatedData")]
        public string AbbreviatedData { get; set; }

        /// <summary>
        /// 图形是否被勾边 默认值为true
        /// 可选
        /// </summary>
        [XmlAttribute(AttributeName = "Stroke")]
        public bool Stroke { get; set; } = true;

        /// <summary>
        /// 图形是否被填充 默认值为false
        /// 可选
        /// </summary>
        [XmlAttribute(AttributeName = "Fill")]
        public bool Fill { get; set; } = false;

        /// <summary>
        /// 图形的填充规则,当 Fill属性存在时出现
        /// 可选值为 NonZero和 Even-Odd
        /// 默认值为 NonZero
        /// 可选
        /// </summary>
        [XmlAttribute(AttributeName = "Rule")]
        public string RuleString
        {
            get => Rule.ToString();
            set => Rule = EnumHelper.ParseEnum<PathFillRule>(value);
        }

        [XmlIgnore]
        public PathFillRule Rule { get; set; } = PathFillRule.NonZero;

    }
}
