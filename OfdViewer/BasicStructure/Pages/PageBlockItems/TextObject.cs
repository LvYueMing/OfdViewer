using OFDViewer.BaseType;
using OFDViewer.Fonts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OFDViewer.BasicStructure.Pages.PageBlockItems
{
    /// <summary>
    /// 文字对象
    /// </summary>
    public class TextObject : CT_Text
    {
        [XmlAttribute("ID")]
        public ST_ID ID { get; set; }
    }
}
