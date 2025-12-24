using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using OFDViewer.BaseType;
using OFDViewer.Graph.ShapeItems;

namespace OFDViewer.Graph
{
    /// <summary>
    /// 区域的分路径结构
    /// RegionArea节点的Start属性所指定的位置为当前点,之后以上一个线条的结束位置为当前点。图形对象描述见表37。
    /// </summary>
    public class RegionArea
    {
        /// <summary>
        /// 图形对象描述
        /// </summary>
        /// <remarks>
        /// Choice节点（Move/Line/QuadraticBezier/CubicBezier/Arc/Close）可重复
        /// 注意：XmlChoiceIdentifier 用于标识具体选择的元素类型
        /// </remarks>
        [XmlElement("Move",typeof(Move))]
        [XmlElement("Line", typeof(Line))]
        [XmlElement("QuadraticBezier", typeof(QuadraticBezier))]
        [XmlElement("CubicBezier", typeof(CubicBezier))]
        [XmlElement("Arc", typeof(Arc))]
        [XmlElement("Close", typeof(Close))]
        [XmlChoiceIdentifier(MemberName = "ShapeItemNames")]
        public List<object> ShapeItems { get; set; } = new List<object>();


        // 用于标识Choice中具体元素类型的属性（序列化时不输出）
        // 数组类型，这是XmlChoiceIdentifier的强制要求
        [XmlIgnore]
        public ShapeItemEnum[] ShapeItemNames { get; set; } = Array.Empty<ShapeItemEnum>();

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
        public void AddShapeItem(object aShapeItem)
        {
            // 1. 添加图形元素到ShapeItems
            this.ShapeItems.Add(aShapeItem);

            // 2. 匹配对应的枚举
            var itemEnum = aShapeItem switch
            {
                Move => ShapeItemEnum.Move,
                Line => ShapeItemEnum.Line,
                QuadraticBezier => ShapeItemEnum.QuadraticBezier,
                CubicBezier => ShapeItemEnum.CubicBezier,
                Arc => ShapeItemEnum.Arc,
                Close => ShapeItemEnum.Close,
                _ => throw new ArgumentException($"不支持的图形类型：{aShapeItem.GetType().Name}")
            };

            //// 3. 同步更新ShapeItemNames数组（转为列表操作后再转回数组）
            var _enumList = new List<ShapeItemEnum>(ShapeItemNames);
            _enumList.Add(itemEnum);
            this.ShapeItemNames = _enumList.ToArray();

        }

        // 添加清空方法，保证两个集合同时清空
        public void ClearShapeItems()
        {
            ShapeItems.Clear();
            ShapeItemNames = Array.Empty<ShapeItemEnum>();
        }
    }
}
