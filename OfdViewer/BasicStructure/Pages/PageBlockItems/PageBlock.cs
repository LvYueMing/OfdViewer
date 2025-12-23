using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OFDViewer.BaseType;
using System.Xml.Serialization;

namespace OFDViewer.BasicStructure.Pages.PageBlockItems
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
