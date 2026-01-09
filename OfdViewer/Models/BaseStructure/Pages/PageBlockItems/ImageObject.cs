using System.Xml.Serialization;
using OFDViewer.Models.BaseType;
using OFDViewer.Models.Image;

namespace OFDViewer.Models.BaseStructure.Pages.PageBlockItems
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
