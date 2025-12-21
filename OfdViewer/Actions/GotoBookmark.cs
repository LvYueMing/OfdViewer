using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OFDViewer.Actions
{
    /// <summary>
    /// 跳转的目标书签 必选
    /// </summary>
    public class GotoBookmark
    {
        [XmlAttribute(AttributeName = "Name")]
        public string Name { get; set; }
    }
}
