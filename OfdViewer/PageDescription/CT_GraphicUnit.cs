using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OFDViewer.BaseType;
using System.Xml.Serialization;

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
        public List<string> ActionString
        {
            get => Actions.Select(item => item.ToString()).ToList();
            set => Actions = value.Select(item => CT_Action.Parse(item)).ToList();
        }

        public List<CT_Action> Actions { get; set; }

        /// <summary>
        /// Clips 集合（CT_Clip 类型），使用 XmlArray/XArrayItem 匹配 XSD 结构
        /// </summary>
        [XmlArray(ElementName = "Clips")]
        [XmlArrayItem(ElementName = "Clip")]
        public List<CT_Clip> Clips { get; set; } = new List<CT_Clip>();

        /// <summary>
        /// Boundary 属性（必填），严格匹配 XSD 名称
        /// </summary>
        [XmlAttribute(AttributeName = "Boundary")]
        public ST_Box Boundary { get; set; }

        /// <summary>
        /// Name 属性，严格匹配 XSD 名称
        /// </summary>
        [XmlAttribute(AttributeName = "Name")]
        public string Name { get; set; }

        /// <summary>
        /// Visible 属性，默认值 true
        /// </summary>
        [XmlAttribute(AttributeName = "Visible")]
        public bool Visible { get; set; } = true;

        /// <summary>
        /// CTM 属性，严格匹配 XSD 名称
        /// </summary>
        [XmlAttribute(AttributeName = "CTM")]
        public ST_Array CTM { get; set; }

        /// <summary>
        /// DrawParam 属性，严格匹配 XSD 名称
        /// </summary>
        [XmlAttribute(AttributeName = "DrawParam")]
        public ST_RefID DrawParam { get; set; }

        /// <summary>
        /// LineWidth 属性，默认值 0.353
        /// </summary>
        [XmlAttribute(AttributeName = "LineWidth")]
        public double LineWidth { get; set; } = 0.353;

        /// <summary>
        /// Cap 属性，默认值 Butt
        /// </summary>
        [XmlAttribute(AttributeName = "Cap")]
        public Cap Cap { get; set; } = Cap.Butt;

        /// <summary>
        /// Join 属性，默认值 Miter
        /// </summary>
        [XmlAttribute(AttributeName = "Join")]
        public Join Join { get; set; } = Join.Miter;

        /// <summary>
        /// MiterLimit 属性，默认值 4.234
        /// </summary>
        [XmlAttribute(AttributeName = "MiterLimit")]
        public double MiterLimit { get; set; } = 4.234;

        /// <summary>
        /// DashOffset 属性，默认值 0
        /// </summary>
        [XmlAttribute(AttributeName = "DashOffset")]
        public double DashOffset { get; set; } = 0;

        /// <summary>
        /// DashPattern 属性，严格匹配 XSD 名称
        /// </summary>
        [XmlAttribute(AttributeName = "DashPattern")]
        public ST_Array DashPattern { get; set; }

        /// <summary>
        /// Alpha 属性，默认值 255
        /// </summary>
        [XmlAttribute(AttributeName = "Alpha")]
        public int Alpha { get; set; } = 255;

    }
}
