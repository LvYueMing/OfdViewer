using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using OFDViewer.BaseType;

namespace OFDViewer.Graph.ShapeItems
{
    /// <summary>
    /// 圆弧
    /// </summary>
    public class Arc
    {
        /// <summary>
        /// 弧线方向是否为顺时针
        /// true表示由圆弧起始点到结束点是顺时针旋转,false表示由圆弧起
        /// 始点到结束点是逆时针旋转
        /// 对于经过坐标系上指定两点,给定旋转角度和长短轴长度的椭圆,
        /// 满足条件的可能有2个,对应圆弧有4条,通过 LargeArc属性可以
        /// 排除2条,由此属性从余下的2条圆弧中确定一条
        /// 必选
        /// </summary>
        [XmlAttribute("SweepDirection")]
        public bool SweepDirection { get; set; }

        /// <summary>
        /// 是否是大圆弧
        /// true表示此线型对应的为度数大于180°的弧,false表示对应度数小于180°的弧
        /// 对于一个给定长、短轴的椭圆以及起始点和结束点,有一大一小两
        /// 条圆弧, 如果所描述线型恰好为180°的弧,则此属性的值不被参考,
        /// 可由SweepDirection属性确定圆弧的形状
        /// 必选
        /// </summary>
        [XmlAttribute("LargeArc")]
        public bool LargeArc { get; set; }

        /// <summary>
        /// 表示按 EllipseSize 绘制的椭圆在当前坐标系下旋转的角度,正值为顺时针,负值为逆时针
        /// [异常处理] 如果角度大于360°, 则以360取模
        /// 必选
        /// </summary>
        [XmlAttribute("RotationAngle")]
        public double RotationAngle { get; set; }

        /// <summary>
        /// 形如[200100]的数组,2个正浮点数值依次对应椭圆的长、短轴长度,较大的一个为长轴
        ///[异常处理] 如果数组长度超过2, 则只取前两个数值
        ///[异常处理]如果数组长度为1,则认为这是一个圆,该数值为圆半径
        ///[异常处理] 如果数组前两个数值中有一个为0, 或者数组为空, 则圆弧退化为一条从当前点到 EndPoint的线段
        ///[异常处理]如果数组数值为负值,则取其绝对值
        ///必选
        /// </summary>
        [XmlAttribute("EllipseSize")]
        public string EllipseSizeString
        {
            get => EllipseSize.ToString();
            set => EllipseSize = ST_Array.Parse(value);
        }

        [XmlIgnore]
        public ST_Array EllipseSize { get; set; }

        /// <summary>
        /// 圆弧的结束点,下个路径的起始点不能与当前的绘制起始点为同一位置
        ///必选
        /// </summary>
        [XmlAttribute("EndPoint")]
        public string EndPointPos
        {
            get => EndPoint.ToString();
            set => EndPoint = ST_Pos.Parse(value);
        }

        [XmlIgnore]
        public ST_Pos EndPoint { get; set; }
    }
}
