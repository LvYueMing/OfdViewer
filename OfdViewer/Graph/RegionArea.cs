using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using OFDViewer.BaseType;
using OFDViewer.Graph.PathItems;

namespace OFDViewer.Graph
{
    /// <summary>
    /// 区域的分路径结构
    /// RegionArea节点的Start属性所指定的位置为当前点,之后以上一个线条的结束位置为当前点。图形对象描述见表37。
    /// </summary>
    public class RegionArea
    {
        /// <summary>
        /// 混合路径节点集合：匹配 xs:choice maxOccurs="unbounded"
        /// 无外层包裹，直接生成Move/Line等节点
        /// </summary>
        [XmlElement("Move",typeof(Move))]
        [XmlElement("Line", typeof(Line))]
        [XmlElement("QuadraticBezier", typeof(QuadraticBezier))]
        [XmlElement("CubicBezier", typeof(CubicBezier))]
        [XmlElement("Arc", typeof(Arc))]
        [XmlElement("Close", typeof(Close))]
        public List<AreaPath> PathItems { get; set; } = new List<AreaPath>();


        /// <summary>
        /// 定义子图形的起始点坐标 必选
        /// </summary>
        [XmlAttribute("Start")]
        public string StartPos
        {
            get=> Start.ToString();
            set => Start = ST_Pos.Parse(value);
        }

        [XmlIgnore]
        public ST_Pos Start { get; set; }


        //无参构造函数
        public RegionArea()
        {
        }


        // 封装添加方法，自动同步枚举
        public void AddPathItem(AreaPath aPathItem)
        {
            // 1. 添加图形元素到ShapeItems
            this.PathItems.Add(aPathItem);
        }

        // 添加清空方法，保证两个集合同时清空
        public void ClearShapeItems()
        {
            PathItems.Clear();
        }
    }
}
