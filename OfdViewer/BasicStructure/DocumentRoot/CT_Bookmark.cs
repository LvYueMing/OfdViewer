using OFDViewer.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OFDViewer.BasicStructure.DocumentRoot
{
    /// <summary>
    /// 书签
    /// </summary>
    public class CT_Bookmark
    {
        /// <summary>
        /// 书签对应的文档位置, 见表54 必选
        /// </summary>
        [XmlElement("Dest", IsNullable = false)]
        public CT_Dest Dest { get; set; }

        /// <summary>
        /// 书签名称 必选
        /// </summary>
        [XmlAttribute("Name")]
        public string Name { get; set; }
    }
}
