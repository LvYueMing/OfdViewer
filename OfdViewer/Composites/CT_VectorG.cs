using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OFDViewer.BaseType;
using OFDViewer.BasicStructure.Pages;
using System.Xml.Serialization;

namespace OFDViewer.Composites
{
    /// <summary>
    /// 矢量图像结构
    /// </summary>
    public class CT_VectorG
    {
        /// <summary>
        /// 矢量图像的宽度
        /// 超出部分做裁剪处理
        /// 必选
        /// </summary>
        [XmlAttribute("Width")]
        public double Width { get; set; }

        /// <summary>
        /// 矢量图像的高度
        /// 超出部分做裁剪处理
        /// 必选
        /// </summary>
        [XmlAttribute("Height")]
        public double Height { get; set; }


        /// <summary>
        /// 缩略图,指向包内的图像文件 可选
        /// </summary>
        [XmlElement("Thumbnail")]
        public string ThumbnailString
        {
            get => Thumbnail.ToString();
            set => Thumbnail = ST_RefID.Parse(value);
        }
        [XmlIgnore]
        public ST_RefID Thumbnail { get; set; }

        /// <summary>
        /// 替换图像,用于高分辨率输出时将缩略图替换为此高分辨率的图像
        /// 指向包内的图像文件
        /// 可选
        /// </summary>
        [XmlElement("Substitution")]
        public string SubstitutionString
        {
            get => Substitution.ToString();
            set => Substitution = ST_RefID.Parse(value);
        }

        [XmlIgnore]
        public ST_RefID Substitution { get; set; }

        /// <summary>
        /// 内容的矢量描述 必选
        /// </summary>
        [XmlElement("Content")]
        public CT_PageBlock Content { get; set; }
    }
}
