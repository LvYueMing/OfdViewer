using OFDViewer.BaseType;
using OFDViewer.Enums;
using OFDViewer.PageDescription.Colors;
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
    /// 径向渐变颜色
    /// 图 35 径向渐变结构
    /// </summary>
    public class CT_RadialShd
    {
        /// <summary>
        /// 颜色段, 至少出现两个
        /// </summary>
        [XmlElement("Segment", IsNullable = true)]
        public List<RadialShdSegment> Segment { get; set; } = new List<RadialShdSegment>();

        /// <summary>
        /// 渐变绘制的方式, 可选值为 Direct、Repeat、Reflect
        /// 默认值为 Direct
        /// 可选
        /// </summary>
        [XmlAttribute("MapType")]
        public string MapTypestring
        {
            get => MapType.ToString();
            set => MapType = EnumHelper.ParseEnum<RadialShdMapType>(value);
        }

        // 被 XML 序列化忽略的枚举属性（带默认值 Direct）
        [XmlIgnore]
        public RadialShdMapType MapType { get; set; } = RadialShdMapType.Direct;

        /// <summary>
        /// 中心点连线上一个渐变区间所绘制的长度, 当 MapType 的值不为Direct 时出现
        /// 默认值为中心点连线长度
        /// 可选
        /// </summary>
        [XmlAttribute("MapUnit")]
        public double MapUnit { get; set; }

        /// <summary>
        /// 两个椭圆的离心率, 即椭圆焦距与长轴的比值, 取值范围是[0, 1.0)
        /// 默认值为0, 在这种情况下椭圆退化为圆
        /// 可选
        /// </summary>
        [XmlAttribute("Eccentricity")]
        public double Eccentricity { get; set; } = 0;

        /// <summary>
        /// 两个椭圆的倾斜角度, 椭圆长轴与x 轴正向的夹角, 单位为度
        /// 默认值为0
        /// 可选
        /// </summary>
        [XmlAttribute("Angle")]
        public double Angle { get; set; } = 0;

        /// <summary>
        /// 起始椭圆的的中心点 必选
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
        /// 起始椭圆的长半轴
        /// 默认值为0
        /// 可选
        /// </summary>
        [XmlAttribute("StartRadius")]
        public double StartRadius { get; set; } = 0;

        /// <summary>
        /// 结束椭圆的的中心点 必选
        /// </summary>
        [XmlAttribute("EndPoint")]
        public string EndPointString
        {
            get => EndPoint.ToString();
            set => EndPoint = ST_Pos.Parse(value);
        }


        [XmlIgnore]
        public ST_Pos EndPoint { get; set; }

        /// <summary>
        /// 结束椭圆的长半轴 必选
        /// </summary>
        [XmlAttribute("EndRadius")]
        public double EndRadius { get; set; }

        /// <summary>
        /// 径向延长线方向是否继续绘制渐变。 可选值为0、1、2、3
        /// 0: 不向圆心联线两侧继续绘制渐变
        /// 1: 在结束点椭圆至起始点椭圆延长线方向绘制渐变
        /// 2: 在起始点椭圆至结束点椭圆延长线方向绘制渐变
        /// 3: 向两侧延长线方向绘制渐变
        /// 默认值为0
        /// </summary>
        [XmlAttribute("Extend")]
        public string ExtendString
        {
            get => Extend.ToString();
            set => Extend = EnumHelper.ParseEnum<RadialShdExtend>(value);
        }

        [XmlIgnore]
        public RadialShdExtend Extend { get; set; } = 0;

    }
}
