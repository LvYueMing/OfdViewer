using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OFDViewer.Actions.ActionItems
{
    /// <summary>
    /// URI动作
    /// URI动作表明的是指向一个 URI位置。URI动作结构如图77所示。
    /// </summary>
    public class URI
    {
        /// <summary>
        /// 目标 URI的位置 必选
        /// </summary>
        [XmlAttribute(AttributeName = "URI")]
        public string URIValue { get; set; } // 避免属性名与类名冲突

        /// <summary>
        /// BaseURI,用于相对地址 可选
        /// </summary>
        [XmlAttribute(AttributeName = "Base")]
        public string Base { get; set; }


        /// <summary>
        /// 图 77 URI动作结构 没有Target 属性，在xsd中有Target属性
        /// </summary>
        [XmlAttribute(AttributeName = "Target")]
        public string Target { get; set; }
    }
}
