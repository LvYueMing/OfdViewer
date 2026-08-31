using System.Xml.Serialization;
using OFDViewer.Models.BaseType;
using OFDViewer.Models.PageDesc.Colors.ColorItems;
using OFDViewer.Utils;

namespace OFDViewer.Models.PageDesc.Colors
{
    public class CT_Color
    {
        /// <summary>
        /// 颜色定义, 渐变和填充被看作颜色的一种
        /// xs:choice minOccurs="0" maxOccurs="1"
        /// Pattern：底纹填充, 复杂颜色的一种。 描述见8.3.3 可选
        /// AxialShd：轴向渐变, 复杂颜色的一种。 描述见8.3.4.2 可选
        /// RadialShd：径向渐变, 复杂颜色的一种。 描述见8.3.4.3 可选
        /// GouraudShd：高洛德渐变, 复杂颜色的一种。 描述见8.3.4.4 可选
        /// LaGouraudShd：格构高洛德渐变, 复杂颜色的一种。 描述见8.3.4.5 可选
        /// </summary>
        [XmlElement("Pattern", Type = typeof(CT_Pattern))]
        [XmlElement("AxialShd", Type = typeof(CT_AxialShd))]
        [XmlElement("RadialShd", Type = typeof(CT_RadialShd))]
        [XmlElement("GouraudShd", Type = typeof(CT_GouraudShd))]
        [XmlElement("LaGourandShd", Type = typeof(CT_LaGouraudShd))]
        public BaseComplexColor ColorItem { get; set; }

        /// <summary>
        /// 颜色值,指定了当前颜色空间下各通道的取值。Value的取值应符
        /// 合"通道1 通道2 通道3 …"格式。此属性不出现时,应采用Index
        /// 属性从颜色空间的调色板中的取值。当二者都不出现时,该颜色各
        /// 通道的值全部为0
        /// 可选
        /// </summary>
        [XmlAttribute("Value")]
        public string ValueString
        {
            get { return Value.ToString(); }
            set { Value = ST_Array.Parse(value); }
        }
        [XmlIgnore]
        public ST_Array Value { get; set; }

        /// <summary>
        /// 调色板中颜色的编号,非负整数,将从当前颜色空间的调色板中取
        /// 出相应索引的预定义颜色用来绘制。索引从0开始
        /// 可选
        /// </summary>
        [XmlAttribute("Index")]
        public int Index { get; set; }


        /// <summary>
        /// 引用资源文件中颜色空间的标识
        /// 默认值为文档设定的颜色空间
        /// 可选
        /// 兼容：部分生成工具使用预定义色彩空间名称（GRAY/RGB/CMYK）而非引用 ID
        /// </summary>
        [XmlAttribute("ColorSpace")]
        public string ColorSpaceString
        {
            get { return ColorSpace.ToString(); }
            set
            {
                // 1. 标准写法：数字形式的资源引用 ID
                if (ST_RefID.TryParse(value, out var refId))
                {
                    ColorSpace = refId;
                }
                // 2. 兼容写法：预定义色彩空间名称
                else if (PredefinedColorSpace.TryParse(value, out refId))
                {
                    ColorSpace = refId;
                }
                // 3. 无法识别：置为无效引用，由渲染层按默认色彩空间（RGB）处理
                else
                {
                    ColorSpace = ST_RefID.Invalid;
                }
            }
        }
        [XmlIgnore]
        public ST_RefID ColorSpace { get; set; }

        /// <summary>
        /// 颜色透明度,在0~255之间取值。默认为255,表示完全不透明 可选
        /// </summary>
        [XmlAttribute("Alpha")]
        public int Alpha { get; set; } = 255;

    }
}
