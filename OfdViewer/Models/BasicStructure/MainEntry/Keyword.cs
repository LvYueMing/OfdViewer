using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OFDViewer.Models.BasicStructure.MainEntry
{
    /// <summary>
    /// 关键词 
    /// 必选
    /// </summary>
    public class Keyword
    {
        [XmlText]
        public string Value { get; set; }
    }
}
