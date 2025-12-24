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
    /// 线段
    /// </summary>
    public class Line
    {
        /// <summary>
        /// 线段的结束点 必选
        /// </summary>
        [XmlAttribute("Point1")]
        public string Point1Pos
        {
            get => Point1.ToString();
            set => Point1 = ST_Pos.Parse(value);
        }

        [XmlIgnore]
        public ST_Pos Point1 { get; set; }

        //无参构造函数
        public Line()
        {
            Point1 = ST_Pos.Zero;
        }
    }
}
