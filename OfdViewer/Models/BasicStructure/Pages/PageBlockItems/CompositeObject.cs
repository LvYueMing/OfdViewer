using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using OFDViewer.Models.Composites;
using OFDViewer.Models.BaseType;

namespace OFDViewer.Models.BasicStructure.Pages.PageBlockItems
{
    /// <summary>
    /// 复合对象,见第13章
    /// </summary>
    public class CompositeObject : CT_Composite
    {
        [XmlAttribute("ID")]
        public ST_ID ID { get; set; }
    }
}
