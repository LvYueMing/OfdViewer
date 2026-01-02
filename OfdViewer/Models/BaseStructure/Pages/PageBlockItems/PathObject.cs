using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using OFDViewer.Models.Graph;
using OFDViewer.Models.BaseType;

namespace OFDViewer.Models.BaseStructure.Pages.PageBlockItems
{
    /// <summary>
    /// 图形对象
    /// </summary>
    public class PathObject : CT_Path
    {
        [XmlAttribute("ID")]
        public ST_ID ID { get; set; }
    }
}
