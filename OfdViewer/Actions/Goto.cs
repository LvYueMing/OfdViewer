using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OFDViewer.Actions
{
    /// <summary>
    /// 跳转动作
    /// 跳转动作表明同一个文档内的跳转,包含一个目标区域或者书签位置,如图74所示。
    /// </summary>
    public class Goto
    {
        /// <summary>
        /// 跳转的目标区域 必选
        /// </summary>
        [XmlElement(ElementName = "Dest")]
        public CT_Dest Dest { get; set; }

        /// <summary>
        /// 跳转的目标书签 必选
        /// </summary>
        [XmlElement(ElementName = "Bookmark")]
        public GotoBookmark Bookmark { get; set; }
    }
}
