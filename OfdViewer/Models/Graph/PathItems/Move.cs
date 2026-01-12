using System.Xml.Serialization;
using OFDViewer.Models.BaseType;

namespace OFDViewer.Models.Graph.PathItems
{
    /// <summary>
    /// 移动
    /// 移动节点用于表示移动到新的绘制点指令,结构如图50所示。
    /// </summary>
    public class Move : BasePath
    {
        /// <summary>
        /// 移动后新的当前绘制点 必选
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
        public Move()
        {
            Point1 = ST_Pos.Zero;
        }
    }
}
