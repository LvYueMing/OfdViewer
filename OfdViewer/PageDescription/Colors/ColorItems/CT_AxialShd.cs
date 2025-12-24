using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OFDViewer.BaseType;
using System.Xml.Serialization;
using OFDViewer.Enums;
using OFDViewer.Utils;

namespace OFDViewer.PageDescription.Colors.ColorItems
{
    /// <summary>
    /// 轴向渐变
    /// 在轴向渐变中,颜色渐变沿着一条指定的轴线方向进行,轴线由起始点和结束点决定,与这条轴线
    /// 垂直的直线上的点颜色相同。
    /// </summary>
    public class CT_AxialShd
    {
        /// <summary>
        /// 颜色段,至少出现两个
        /// </summary>
        [XmlElement("Segment")]
        public List<AxialShdSegment> Segment { get; set; } = new List<AxialShdSegment>();

        /// <summary>
        /// 渐变绘制的方式,可选值为 Direct、Repeat、Reflect
        /// 默认值为 Direct
        /// 可选
        /// </summary>
        [XmlAttribute("MapType")]
        public string MapTypeString
        {
            get => MapType.ToString();
            set => MapType = EnumHelper.ParseEnum<AxialShdMapType>(value);
        }


        [XmlIgnore]
        public AxialShdMapType MapType { get; set; } = AxialShdMapType.Direct;

        /// <summary>
        /// 轴线一个渐变区间的长度,当 MapType的值不等于 Direct时出现
        /// 默认值为轴线长度
        /// 可选
        /// </summary>
        [XmlAttribute("MapUnit")]
        public double MapUnit { get; set; }

        /// <summary>
        /// 轴线延长线方向是否继续绘制渐变。可选值为0、1、2、3
        /// 0:不向两侧继续绘制渐变
        /// 1:在结束点至起始点延长线方向绘制渐变
        /// 2:在起始点至结束点延长线方向绘制渐变
        /// 3:向两侧延长线方向绘制渐变
        /// 默认值为0
        /// 可选
        /// </summary>
        [XmlAttribute("Extend")]
        public string ExtendString
        {
            get => EnumHelper.GetEnumDesc(Extend);
            set => Extend = EnumHelper.ParseEnum<AxialShdExtend>(value);
        }


        [XmlIgnore]
        public AxialShdExtend Extend { get; set; } = AxialShdExtend.Value0;

        /// <summary>
        /// 轴线的起始点 必选
        /// </summary>
        [XmlAttribute("StartPoint")]
        public string StartPointString
        {
            get => StartPoint.ToString();
            set => StartPoint = ST_Pos.Parse(value);
        }

        [XmlIgnore]
        public ST_Pos StartPoint { get; set; }

        /// <summary>
        /// 轴线的结束点 必选
        /// </summary>
        [XmlAttribute("EndPoint")]
        public string EndPointString
        {
            get => EndPoint.ToString();
            set => EndPoint = ST_Pos.Parse(value);
        }

        [XmlIgnore]
        public ST_Pos EndPoint { get; set; }

    }
}
