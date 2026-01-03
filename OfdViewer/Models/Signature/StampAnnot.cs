using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using OFDViewer.Models.BaseType;
using OFDViewer.Utils;

namespace OFDViewer.Models.Signature
{
    /// <summary>
    /// 签名的外观
    /// 一个数字签名可以跟一个或多个外观描述关联,也可以不关联任何外观,其关联方式如图88所示。
    /// </summary>
    public class StampAnnot
    {
        /// <summary>
        /// 签章注释的标识 
        /// 必选
        /// </summary>
        [XmlAttribute("ID")]
        [XmlRequired(ErrorMsg = "ID为必选项，且不能为空")]
        public string ID { get; set; }

        /// <summary>
        /// 引用外观注释所在的页面的标识 
        /// 必选
        /// </summary>
        [XmlAttribute("PageRef")]
        [XmlRequired(ErrorMsg = "PageRef为必选项，且不能为空")]
        public ST_RefID PageRef { get; set; }

        /// <summary>
        /// 签章注释的外观外边框位置,可用于签章注释在页面内的定位 
        /// 必选
        /// </summary>
        [XmlAttribute("Boundary")]
        [XmlRequired(ErrorMsg = "Boundary为必选项，且不能为空")]
        public string BoundaryString
        {
            get => Boundary.ToString();
            set => Boundary = value;
        }
        [XmlIgnore]
        public ST_Loc Boundary { get; set; }

        /// <summary>
        /// 签章注释的外观裁剪设置 
        /// 可选
        /// </summary>
        [XmlAttribute("Clip")]
        public string ClipString
        {
            get => Clip.ToString();
            set => Clip = value;
        }
        [XmlIgnore]
        public ST_Loc Clip { get; set; }
    }
}
