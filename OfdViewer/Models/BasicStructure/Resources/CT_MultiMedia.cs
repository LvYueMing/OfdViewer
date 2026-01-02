using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OFDViewer.Utils;
using System.Xml.Serialization;
using OFDViewer.Models.BaseType;

namespace OFDViewer.Models.BasicStructure.Resources
{
    /// <summary>
    /// 多媒体
    /// </summary>
    public class CT_MultiMedia
    {
        /// <summary>
        /// 指向 OFD包内的多媒体文件的位置 
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
        /// 资源的格式。支 持 BMP、JPEG、PNG、TIFF 及 AVS等 格 式,其 中TIFF格式不支持多页
        /// 可选
        /// </summary>
        [XmlAttribute("Format")]
        public string FormatString
        {
            get => Format.ToString();
            set => Format = EnumHelper.ParseEnum<MultiMediaFormatType>(value);
        }
        [XmlIgnore]
        public MultiMediaFormatType Format { get; set; }

    }
}
