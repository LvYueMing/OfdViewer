using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using OFDViewer.Models.BasicStructure.Pages;
using OFDViewer.Models.BaseType;

namespace OFDViewer.Models.BasicStructure.Pages.PageBlockItems
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
