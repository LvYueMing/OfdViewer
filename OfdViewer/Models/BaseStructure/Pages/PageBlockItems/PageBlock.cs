using System.Xml.Serialization;
using OFDViewer.Models.BaseType;

namespace OFDViewer.Models.BaseStructure.Pages.PageBlockItems
{
    /// <summary>
    /// 页面块,可以嵌套
    /// </summary>
    public class PageBlock : CT_PageBlock
    {
        [XmlAttribute("ID")]
        public ST_ID ID { get; set; }
    }
}
