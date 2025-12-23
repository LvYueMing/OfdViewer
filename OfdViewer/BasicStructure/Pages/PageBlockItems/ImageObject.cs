using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OFDViewer.BaseType;
using System.Xml.Serialization;
using OFDViewer.Images;

namespace OFDViewer.BasicStructure.Pages.PageBlockItems
{
    /// <summary>
    /// 图像对象,见第10章
    /// 带有播放视频动作时,见第12章
    /// </summary>
    public class ImageObject : CT_Image
    {
        [XmlAttribute("ID")]
        public ST_ID ID { get; set; }
    }
}
