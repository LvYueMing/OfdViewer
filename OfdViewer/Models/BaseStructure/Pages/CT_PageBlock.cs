using System.Xml.Serialization;
using OFDViewer.Models.BaseStructure.Pages.PageBlockItems;

namespace OFDViewer.Models.BaseStructure.Pages
{
    /// <summary>
    /// 页面块结构
    /// </summary>
    public class CT_PageBlock : BasePageBlock
    {
        /// <summary>
        /// 存储CT_PageBlock的子元素（xs:choice minOccurs="0" maxOccurs="unbounded"）
        /// </summary>
        [XmlElement("TextObject", typeof(TextObject))]
        [XmlElement("PathObject", typeof(PathObject))]
        [XmlElement("ImageObject", typeof(ImageObject))]
        [XmlElement("CompositeObject", typeof(CompositeObject))]
        [XmlElement("PageBlock", typeof(PageBlock))]
        public List<BasePageBlock> PageBlockItems { get; set; } = new List<BasePageBlock>();

    }
}
