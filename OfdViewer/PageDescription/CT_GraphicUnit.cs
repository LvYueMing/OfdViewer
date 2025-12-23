using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OFDViewer.BaseType;
using System.Xml.Serialization;
using OFDViewer.Actions;
using OFDViewer.Utils;

namespace OFDViewer.PageDescription
{
    /// <summary>
    /// 图元对象
    /// 图元对象是版式文档中页面上呈现内容的最基本单元,所有页面显示内容,
    /// 包括文字、图形、图像等,都属于图元对象,或是图元对象的组合。
    /// 图元对象结构如图45所示。
    /// </summary>
    public abstract class CT_GraphicUnit
    {
        /// <summary>
        /// 图元对象的动作序列
        ///当存在多个 Action对象时, 所有动作依次执行
        ///可选
        /// </summary>
        [XmlArray(ElementName = "Actions")]
        [XmlArrayItem(ElementName = "Action")]
        public List<CT_Action> Actions { get; set; }

        /// <summary>
        /// Clips 集合（CT_Clip 类型），使用 XmlArray/XArrayItem 匹配 XSD 结构
        /// </summary>
        [XmlArray(ElementName = "Clips")]
        [XmlArrayItem(ElementName = "Clip")]
        public List<CT_Clip> Clips { get; set; } = new List<CT_Clip>();

        /// <summary>
        /// 外接矩形,采用当前空间坐标系(页面坐标或其他容器坐标),当图
        /// 元绘制超出此矩形区域时进行裁剪
        /// 必选
        /// </summary>
        [XmlAttribute(AttributeName = "Boundary")]
        public string BoundaryString
        {
            get=> Boundary.ToString();
            set=> Boundary = ST_Box.Parse(value);
        }

        [XmlIgnore]
        public ST_Box Boundary { get; set; }

        /// <summary>
        /// 图元对象的名字 默认值为空
        /// 可选
        /// </summary>
        [XmlAttribute(AttributeName = "Name")]
        public string Name { get; set; }

        /// <summary>
        /// 图元是否可见 默认值为true 可选
        /// </summary>
        [XmlAttribute(AttributeName = "Visible")]
        public bool Visible { get; set; } = true;

        /// <summary>
        /// 对象空间内的图元变换矩阵 可选
        /// </summary>
        [XmlAttribute(AttributeName = "CTM")]
        public ST_Array CTM { get; set; }

        /// <summary>
        /// 引用资源文件中的绘制参数标识 可选
        /// </summary>
        [XmlAttribute(AttributeName = "DrawParam")]
        public ST_RefID DrawParam { get; set; }

        /// <summary>
        /// 绘制路径时使用的线宽
        /// 如果图元对象有 DrawParam 属性,则用此值覆盖 DrawParam 中对应的值
        ///可选
        /// </summary>
        [XmlAttribute(AttributeName = "LineWidth")]
        public double LineWidth { get; set; }

        /// <summary>
        /// 线端点样式,枚举值,指定了一条线的端点样式
        /// 默认值为 Butt
        /// 线条端点样式取值与效果之间关系见表24
        /// 见8.2绘制参数
        /// </summary>
        [XmlAttribute(AttributeName = "Cap")]
        public string CapString
        {
            get => Cap.ToString();
            set => Cap = EnumHelper.ParseEnum<Cap>(value);
        }
        [XmlIgnore]
        public Cap Cap { get; set; } = Cap.Butt;

        /// <summary>
        /// 线条连接样式,指定了两个线的端点结合时采用的样式
        /// 默认值为 Miter
        /// 线条连接样式的取值和显示效果之间的关系见表2
        /// 见8.2绘制参数
        /// </summary>
        [XmlAttribute(AttributeName = "Join")]
        public string JoinString
        { 
            get=> Join.ToString();
            set=> Join = EnumHelper.ParseEnum<Join>(value);
        }

        [XmlIgnore]
        public Join Join { get; set; } = Join.Miter;

        /// <summary>
        /// Join为 Miter时,MiterSize的截断值
        /// 如果图元对象有 DrawParam 属性,则用此值覆盖 DrawParam 中对应的值
        ///可选
        /// </summary>
        [XmlAttribute(AttributeName = "MiterLimit")]
        public double MiterLimit { get; set; }

        /// <summary>
        /// 见8.2绘制参数
        /// 如果图元对象有 DrawParam 属性,则用此值覆盖 DrawParam 中对应的值
        ///可选
        /// </summary>
        [XmlAttribute(AttributeName = "DashOffset")]
        public double DashOffset { get; set; }

        /// <summary>
        /// 见8.2绘制参数
        /// 如果图元对象有 DrawParam 属性,则用此值覆盖 DrawParam 中对应的值
        /// <remars>
        /// 线条虚线的重复样式,数组中共含两个值,第一个值代表虚线线段
        /// 的长度,第二个值代表虚线间隔的长度。默认值为空。线条虚线样式的控制效果见表23
        /// </remars> 
        /// </summary>
        [XmlAttribute(AttributeName = "DashPattern")]
        public string DashPatternString
        {
            get=> DashPattern.ToString();
            set=> DashPattern = ST_Array.Parse(value);       
        }
        [XmlIgnore]
        public ST_Array DashPattern { get; set; }

        /// <summary>
        /// 图元对象的透明度,取值区间为[0,255]
        /// 0表示全透明,255表示完全不透明
        /// 默认为0
        /// 可选
        /// </summary>
        [XmlAttribute(AttributeName = "Alpha")]
        public int Alpha { get; set; } = 255;

    }
}
