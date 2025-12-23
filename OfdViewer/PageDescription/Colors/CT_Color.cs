using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OFDViewer.BaseType;
using System.Xml.Serialization;
using OFDViewer.PageDescription.Colors.ColorItems;

namespace OFDViewer.PageDescription.Colors
{
    public class CT_Color
    {
        /// <summary>
        /// 选中的Choice元素实例
        /// </summary>
        [XmlElement("Pattern", Type = typeof(CT_Pattern))]
        [XmlElement("AxialShd", Type = typeof(CT_AxialShd))]
        [XmlElement("RadialShd", Type = typeof(CT_RadialShd))]
        [XmlElement("GouraudShd", Type = typeof(CT_GouraudShd))]
        [XmlElement("LaGourandShd", Type = typeof(CT_LaGouraudShd))]
        [XmlChoiceIdentifier("ColorItemName")] // 关联枚举标识
        public object ColorItem { get; set; }

        /// <summary>
        /// 标识当前选中的Choice元素类型（XmlChoiceIdentifier的核心）
        /// </summary>
        [XmlIgnore] // 不序列化到XML，仅用于标识
        public ColorItemEnum ColorItemName { get; set; } = ColorItemEnum.None;

        /// <summary>
        /// 颜色值,指定了当前颜色空间下各通道的取值。Value的取值应符
        /// 合"通道1 通道2 通道3 …"格式。此属性不出现时,应采用Index
        /// 属性从颜色空间的调色板中的取值。当二者都不出现时,该颜色各
        /// 通道的值全部为0
        /// 可选
        /// </summary>
        [XmlAttribute("Value")]
        public ST_Array Value { get; set; }

        /// <summary>
        /// 调色板中颜色的编号,非负整数,将从当前颜色空间的调色板中取
        /// 出相应索引的预定义颜色用来绘制。索引从0开始
        /// 可选
        /// </summary>
        [XmlAttribute("Index")]
        public int Index { get; set; }

        /// <summary>
        /// 控制Index属性是否序列化（未赋值时不输出）
        /// </summary>
        [XmlIgnore]
        public bool IndexSpecified { get; set; }

        /// <summary>
        /// 引用资源文件中颜色空间的标识
        /// 默认值为文档设定的颜色空间
        /// 可选
        /// </summary>
        [XmlAttribute("ColorSpace")]
        public ST_RefID ColorSpace { get; set; }

        /// <summary>
        /// 颜色透明度,在0~255之间取值。默认为255,表示完全不透明 可选
        /// </summary>
        [XmlAttribute("Alpha")]
        public int Alpha { get; set; } = 255;

        /// <summary>
        /// 控制Alpha属性是否序列化（默认值255时也输出）
        /// </summary>
        [XmlIgnore]
        public bool AlphaSpecified { get; set; } = true;


        // 新增：自动同步枚举值（可选但推荐，提升易用性）
        public void SetColorItem(object item)
        {
            ColorItem = item;
            if (item == null)
                ColorItemName = ColorItemEnum.None;
            else if (item is CT_Pattern)
                ColorItemName = ColorItemEnum.Pattern;
            else if (item is CT_AxialShd)
                ColorItemName = ColorItemEnum.AxialShd;
            else if (item is CT_RadialShd)
                ColorItemName = ColorItemEnum.RadialShd;
            else if (item is CT_GouraudShd)
                ColorItemName = ColorItemEnum.GouraudShd;
            else if (item is CT_LaGouraudShd)
                ColorItemName = ColorItemEnum.LaGourandShd;
        }
    }
}
