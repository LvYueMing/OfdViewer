using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OFDViewer.BaseType;
using OFDViewer.BasicStructure.Pages;
using System.Xml.Serialization;

namespace OFDViewer.Colors.ColorItems
{
    /// <summary>
    /// 底纹单元,用底纹填充目标区域时,所使用的单元对象
    /// </summary>
    public class PatternCellContent : CT_PageBlock
    {
        /// <summary>
        /// 引用资源文件中缩略图图像的标识 可选
        /// </summary>
        [XmlAttribute("Thumbnail")]
        public ST_RefID Thumbnail { get; set; }
    }
}
