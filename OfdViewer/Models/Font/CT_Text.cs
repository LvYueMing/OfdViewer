using System.Xml.Serialization;
using OFDViewer.Models.BaseType;
using OFDViewer.Models.PageDesc;
using OFDViewer.Models.PageDesc.Colors;

namespace OFDViewer.Models.Font
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
        [XmlElement(ElementName = "FillColor")]
        public CT_Color FillColor { get; set; }

        /// <summary>
        /// 勾边颜色 默认为透明色
        /// 可选
        /// </summary>
        [XmlElement(ElementName = "StrokeColor")]
        public CT_Color StrokeColor { get; set; }

        /// <summary>
        /// 指定字符编码到字符索引之间的变换关系, 描述见 11.4 字符变换
        /// 可选
        /// </summary>
        [XmlElement(ElementName = "CGTransform")]
        public List<CT_CGTransform> CGTransforms { get; set; }

        /// <summary>
        /// 文字内容, 也就是一段字符编码串
        /// 如果字符编码不在 XML 编码方式的字符范围之内, 应采用“\”加四位
        /// 十六进制数的格式转义; 文字内容中出现的空格也需要转义
        /// 若 TextCode 作为占位符使用时, 一律采用“¤”(u00A4) 占位
        /// 必选
        /// </summary>
        [XmlElement(ElementName = "TextCode")]
        public List<TextCode> TextCodes { get; set; } = new List<TextCode>();

        /// <summary>
        /// 引用资源文件中定义的字型的标识 必选
        /// </summary>
        [XmlAttribute(AttributeName = "Font")]
        public string FontString
        {
            get => Font.ToString();
            set => Font = ST_RefID.Parse(value);
        }

        [XmlIgnore]
        public ST_RefID Font { get; set; }

        /// <summary>
        /// 字号, 单位为毫米 
        /// 必选
        /// </summary>
        [XmlAttribute(AttributeName = "Size")]
        public double Size { get; set; }

        //控制 Size 属性是否序列化
        public bool ShouldSerializeSize()
        {
            // 当Size为默认值10.0时不序列化
            return Size != 0;
        }

        /// <summary>
        /// 是否勾边
        /// 默认值为false
        /// 可选
        /// </summary>
        [XmlAttribute(AttributeName = "Stroke")]
        public bool Stroke { get; set; } = false;

        /// <summary>
        /// 控制Stroke属性是否序列化
        /// </summary>
        public bool ShouldSerializeStroke()
        {
            // 当Stroke为默认值false时不序列化
            return Stroke != false;
        }

        /// <summary>
        /// 是否填充 默认值为true 可选
        /// </summary>
        [XmlAttribute(AttributeName = "Fill")]
        public bool Fill { get; set; } = true;

        /// <summary>
        /// 控制Fill属性是否序列化
        /// </summary>
        public bool ShouldSerializeFill()
        {
            // 当Fill为默认值true时不序列化
            return Fill != true;
        }

        /// <summary>
        /// 字型在水平方向的放缩比 默认值为1.0
        ///例如: 当 HScale 值为0.5 时表示实际显示的字宽为原来字宽的一半
        ///可选
        /// </summary>
        [XmlAttribute(AttributeName = "HScale")]
        public double HScale { get; set; } = 1.0;

        /// <summary>
        /// 控制HScale属性是否序列化
        /// </summary>
        public bool ShouldSerializeHScale()
        {
            // 当HScale为默认值1.0时不序列化
            return HScale != 1.0;
        }

        /// <summary>
        /// 阅读方向, 指定了文字排列的方向, 描述见11.3 文字定位
        /// 默认值为0
        /// 可选
        /// </summary>
        [XmlAttribute(AttributeName = "ReadDirection")]
        public int ReadDirection { get; set; } = 0;

        /// <summary>
        /// 控制ReadDirection属性是否序列化
        /// </summary>
        public bool ShouldSerializeReadDirection()
        {
            // 当ReadDirection为默认值0时不序列化
            return ReadDirection != 0;
        }

        /// <summary>
        /// 字符方向, 指定了文字放置的方式, 具体内容见11.3 文字定位
        /// 默认值为0
        /// 可选
        /// </summary>
        [XmlAttribute(AttributeName = "CharDirection")]
        public int CharDirection { get; set; } = 0;

        /// <summary>
        /// 控制CharDirection属性是否序列化
        /// </summary>
        public bool ShouldSerializeCharDirection()
        {
            // 当CharDirection为默认值0时不序列化
            return CharDirection != 0;
        }

        /// <summary>
        /// 文字对象的粗细值;可选取值为 100,200,300,400,500,600,700,800,900
        /// 默认值为400
        /// 可选
        /// </summary>
        [XmlAttribute(AttributeName = "Weight")]
        public int Weight { get; set; } = 400;

        /// <summary>
        /// 控制Weight属性是否序列化
        /// </summary>
        public bool ShouldSerializeWeight()
        {
            // 当Weight为默认值400时不序列化
            return Weight != 400;
        }

        /// <summary>
        /// 是否是斜体样式
        /// 默认值为false
        /// 可选
        /// </summary>
        [XmlAttribute(AttributeName = "Italic")]
        public bool Italic { get; set; } = false;

        /// <summary>
        /// 控制Italic属性是否序列化
        /// </summary>
        public bool ShouldSerializeItalic()
        {
            // 当Italic为默认值false时不序列化
            return Italic != false;
        }

    }
}
