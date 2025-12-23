using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OFDViewer.BaseType;
using OFDViewer.Graph;
using System.Xml.Serialization;

namespace OFDViewer.BasicStructure.Pages.PageBlockItems
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
