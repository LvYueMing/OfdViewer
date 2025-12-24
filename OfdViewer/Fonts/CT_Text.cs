using OFDViewer.BaseType;
using OFDViewer.PageDescription;
using OFDViewer.PageDescription.Colors;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OFDViewer.Fonts
{
    /// <summary>
    /// 文字对象
    /// </summary>
    public class CT_Text : CT_GraphicUnit
    {
        /// <summary>
        /// 填充颜色 默认为黑色
        /// 可选
        /// </summary>
        [XmlElement(ElementName = "FillColor", IsNullable = false)]
        public CT_Color FillColor { get; set; }

        /// <summary>
        /// 勾边颜色 默认为透明色
        /// 可选
        /// </summary>
        [XmlElement(ElementName = "StrokeColor", IsNullable = false)]
        public CT_Color StrokeColor { get; set; }

        /// <summary>
        /// 指定字符编码到字符索引之间的变换关系, 描述见11.4 字符变换
        /// 可选
        /// </summary>
        [XmlElement(ElementName = "CGTransform", IsNullable = false)]
        public List<CT_CGTransform> CGTransforms { get; set; }

        /// <summary>
        /// 文字内容, 也就是一段字符编码串
        /// 如果字符编码不在 XML 编码方式的字符范围之内, 应采用“\”加四位
        /// 十六进制数的格式转义; 文字内容中出现的空格也需要转义
        /// 若 TextCode 作为占位符使用时, 一律采用“¤”(u00A4) 占位
        /// 必选
        /// </summary>
        [XmlElement(ElementName = "TextCode", IsNullable = false)]
        public List<TextCode> TextCodes { get; set; } = new List<TextCode>();

        /// <summary>
        /// 引用资源文件中定义的字型的标识 必选
        /// </summary>
        [XmlAttribute(AttributeName = "Font")]
        public ST_RefID Font { get; set; }

        /// <summary>
        /// 字号, 单位为毫米 必选
        /// </summary>
        [XmlAttribute(AttributeName = "Size")]
        public double Size { get; set; }

        /// <summary>
        /// 是否勾边
        /// 默认值为false
        /// 可选
        /// </summary>
        [XmlAttribute(AttributeName = "Stroke")]
        public bool Stroke { get; set; } = false;

        /// <summary>
        /// 是否填充 默认值为true 可选
        /// </summary>
        [XmlAttribute(AttributeName = "Fill")]
        public bool Fill { get; set; } = true;

        /// <summary>
        /// 字型在水平方向的放缩比 默认值为1.0
        ///例如: 当 HScale 值为0.5 时表示实际显示的字宽为原来字宽的一半
        ///可选
        /// </summary>
        [XmlAttribute(AttributeName = "HScale")]
        public double HScale { get; set; } = 1.0;

        /// <summary>
        /// 阅读方向, 指定了文字排列的方向, 描述见11.3 文字定位
        /// 默认值为0
        /// 可选
        /// </summary>
        [XmlAttribute(AttributeName = "ReadDirection")]
        public int ReadDirection { get; set; } = 0;

        /// <summary>
        /// 字符方向, 指定了文字放置的方式, 具体内容见11.3 文字定位
        /// 默认值为0
        /// 可选
        /// </summary>
        [XmlAttribute(AttributeName = "CharDirection")]
        public int CharDirection { get; set; } = 0;

        /// <summary>
        /// 文字对象的粗细值;可选取值为 100,200,300,400,500,600,700,800,900
        /// 默认值为400
        /// 可选
        /// </summary>
        [XmlAttribute(AttributeName = "Weight")]
        public int Weight { get; set; } = 400;

        /// <summary>
        /// 是否是斜体样式
        /// 默认值为false
        /// 可选
        /// </summary>
        [XmlAttribute(AttributeName = "Italic")]
        public bool Italic { get; set; } = false;

    }
}
