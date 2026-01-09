using System.Xml.Serialization;
using OFDViewer.Models.Action;

namespace OFDViewer.Models.BaseStructure.DocumentRoot
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
