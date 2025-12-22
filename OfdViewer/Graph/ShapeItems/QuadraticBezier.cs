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
    /// 二阶贝塞尔曲线
    /// </summary>
    public class QuadraticBezier
    {
        /// <summary>
        /// 二次贝塞尔曲线的控制点 必选
        /// </summary>
        [XmlAttribute("Point1")]
        public string Point1Pos
        {
            get => Point1.ToString();
            set => Point1 = ST_Pos.Parse(value);
        }

        [XmlIgnore]
        public ST_Pos Point1 { get; set; }

        /// <summary>
        /// 二次贝塞尔曲线的结束点,下一路径的起始点 必选
        /// </summary>
        [XmlAttribute("Point2")]
        public string Point2Pos
        {
            get => Point2.ToString();
            set => Point2 = ST_Pos.Parse(value);
        }

        [XmlIgnore]
        public ST_Pos Point2 { get; set; }
    }
}
