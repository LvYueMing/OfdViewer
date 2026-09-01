using System.Xml.Serialization;
using OFDViewer.Models.BaseType;
using OFDViewer.Utils;

namespace OFDViewer.Models.BaseStructure.Resources
{
    /// <summary>
    /// 多媒体
    /// </summary>
    public class CT_MultiMedia
    {
        /// <summary>
        /// 指向 OFD 包内的多媒体文件的位置 
        /// 必选
        /// </summary>
        [XmlElement("MediaFile")]
        [XmlRequired(ErrorMsg = "MediaFile 必选属性为必选项，且不能为空")]
        public ST_Loc MediaFile { get; set; }

        // 枚举属性 Type（忽略XML序列化，无默认值，因XSD中use="required"）
        [XmlIgnore]
        public MultiMediaType Type { get; set; }

        /// <summary>
        /// 多媒体类型。支持位图图像、视频、音频三种多媒体类型 
        /// 必选
        /// </summary>
        [XmlAttribute("Type")]
        [XmlRequired(ErrorMsg = "Type 必选属性为必选项，且不能为空")]
        public string TypeString
        {
            get => Type.ToString();
            set => Type = EnumHelper.ParseEnum<MultiMediaType>(value);
        }

        /// <summary>
        /// 资源的格式。支持 BMP、JPEG、PNG、TIFF 及 AVS 等格式,其中 TIFF 格式不支持多页
        /// 可选
        /// </summary>
        [XmlAttribute("Format")]
        public string FormatString
        {
            get => Format.ToString();
            set
            {
                // 容错解析：Format 为可选属性，实际生成器常写 "jpg"/"tif" 等非标准值；
                // 无法识别时保持默认值，避免整个文档解析失败
                if (EnumHelper.TryParseEnum<MultiMediaFormatType>(value, out var format))
                {
                    Format = format;
                }
                else if (string.Equals(value, "jpg", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(value, "jpe", StringComparison.OrdinalIgnoreCase))
                {
                    Format = MultiMediaFormatType.JPEG;
                }
                else if (string.Equals(value, "tif", StringComparison.OrdinalIgnoreCase))
                {
                    Format = MultiMediaFormatType.TIFF;
                }
            }
        }
        [XmlIgnore]
        public MultiMediaFormatType Format { get; set; }

    }
}
