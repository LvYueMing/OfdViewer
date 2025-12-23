using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OFDViewer.PageDescription.Colors;
using OFDViewer.PageDescription;
using System.Xml.Serialization;
using OFDViewer.BaseType;

namespace OFDViewer.Images
{
    /// <summary>
    /// 图像
    /// 图像对象的基本结构如图57所示
    /// </summary>
    public class CT_Image : CT_GraphicUnit
    {
        /// <summary>
        /// 图像边框设置 可选
        /// </summary>
        [XmlElement("Border", IsNullable = false)]
        public ImageBorder Border { get; set; }

        /// <summary>
        /// 引用资源文件中定义的多媒体的标识 必选
        /// </summary>
        [XmlAttribute("ResourceID")]
        public string ResourceIDString
        {
            get => ResourceID.ToString();
            set => ResourceID = ST_RefID.Parse(value);
        }

        [XmlIgnore]
        public ST_RefID ResourceID { get; set; }

        /// <summary>
        /// 可替换图像,引用资源文件中定义的多媒体的标识,用于某些情况如高分辨率输出时进行图像替换
        /// 可选
        /// </summary>
        [XmlAttribute("Substitution")]
        public string SubstitutionString
        {
            get => Substitution.ToString();
            set => Substitution = ST_RefID.Parse(value);
        }
        [XmlIgnore]
        public ST_RefID Substitution { get; set; }

        /// <summary>
        /// 图像蒙版,引用资源文件中定义的多媒体的标识,用作蒙版的图像
        /// 应是与 ResouceID指向的图像相同大小的二值图
        /// </summary>
        [XmlAttribute("ImageMask")]
        public string ImageMaskString
        {
            get => ImageMask.ToString();
            set => ImageMask = ST_RefID.Parse(value);
        }


        [XmlIgnore]
        public ST_RefID ImageMask { get; set; }

    }
}
