using System.Xml.Serialization;
using OFDViewer.Models.BaseType;
using OFDViewer.Utils;

namespace OFDViewer.Models.Font
{
    /// <summary>
    /// 字型
    /// 字型结构描述如图58所示。
    /// </summary>
    public class CT_Font
    {
        /// <summary>
        /// 指向内嵌字型文件,嵌入字型文件应使用 OpenType格式 
        /// 可选
        /// </summary>
        [XmlElement("FontFile")]
        public string FontFileString
        {
            get => FontFile.ToString();
            set => FontFile = value;
        }
        [XmlIgnore]
        public ST_Loc FontFile { get; set; }

        /// <summary>
        /// 字型名 
        /// 必选
        /// </summary>
        [XmlAttribute("FontName")]
        [XmlRequired(ErrorMsg = "FontName 必选属性为必选项，且不能为空")]
        public string FontName { get; set; }

        /// <summary>
        /// 字型族名,用于匹配替代字型 
        /// 可选
        /// </summary>
        [XmlAttribute("FamilyName")]
        public string FamilyName { get; set; }


        [XmlIgnore]
        public FontCharset Charset { get; set; } = FontCharset.unicode;

        /// <summary>
        /// 字型适用的字符分类,用于匹配替代字型
        /// 可取值为symbol、prc、big5、unicode等
        /// 默认值为unicode
        ///可选
        /// </summary>
        [XmlAttribute("Charset")]
        public string CharsetString
        {
            // 枚举中name有下划线，获取枚举描述
            get => EnumHelper.GetEnumDesc<FontCharset>(Charset);
            set => Charset = EnumHelper.ParseEnum<FontCharset>(value);
        }

        /// <summary>
        /// 是否是斜体字型,用于匹配替代字型
        /// 默认值是false
        /// 可选
        /// </summary>
        [XmlAttribute("Italic")]
        public bool Italic { get; set; } = false;

        /// <summary>
        /// 是否是粗体字型,用于匹配替代字型
        /// 默认值是false
        // 可选
        /// </summary>
        [XmlAttribute("Bold")]
        public bool Bold { get; set; } = false;

        /// <summary>
        /// 是否是带衬线字型,用于匹配替代字型
        /// 默认值是false
        /// 可选
        /// </summary>
        [XmlAttribute("Serif")]
        public bool Serif { get; set; } = false;

        /// <summary>
        /// 是否是等宽字型,用于匹配替代字型
        /// 默认值是false
        /// 可选
        /// </summary>
        [XmlAttribute("FixedWidth")]
        public bool FixedWidth { get; set; } = false;
    }
}
