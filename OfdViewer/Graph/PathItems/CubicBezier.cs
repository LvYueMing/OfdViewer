using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using OFDViewer.BaseType;

namespace OFDViewer.Graph.PathItems
{
    /// <summary>
    /// 三阶贝塞尔曲线
    /// </summary>
    public class CubicBezier : AreaPath
    {
        /// <summary>
        /// 三次贝塞尔曲线的第一个控制点 可选
        /// </summary>
        [XmlAttribute("Point1")]
        public string? Point1Pos
        {
            get => Point1.ToString();
            set => Point1 = ST_Pos.Parse(value);
        }
        [XmlIgnore]
        public ST_Pos Point1 { get; set; }

        /// <summary>
        /// 三次贝塞尔曲线的第二个控制点 可选
        /// </summary>
        [XmlAttribute("Point2")]
        public string? Point2Pos
        {
            get => Point2.ToString();
            set => Point2 = ST_Pos.Parse(value);
        }
        [XmlIgnore]
        public ST_Pos Point2 { get; set; }

        /// <summary>
        /// 三次贝塞尔曲线的结束点,下一路径的起始点 必选
        /// </summary>
        [XmlAttribute("Point3")]
        public string Point3Pos
        {
            get => Point3.ToString();
            set => Point3 = ST_Pos.Parse(value);
        }
        [XmlIgnore]
        public ST_Pos Point3 { get; set; }

        //无参构造函数
        public CubicBezier()
        {
            Point1 = ST_Pos.Zero;
            Point2 = ST_Pos.Zero;
            Point3 = ST_Pos.Zero;
        }
    }
}
