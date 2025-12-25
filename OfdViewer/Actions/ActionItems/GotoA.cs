using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OFDViewer.Actions.ActionItems
{
    /// <summary>
    /// 附件动作
    /// 附件动作表明打开当前文档内的一个附件,附件动作的结构如图76所示
    /// </summary>
    public class GotoA : BaseAction
    {
        /// <summary>
        /// 附件的标识 必选
        /// IDREF 对应 C# string
        /// </summary>
        [XmlAttribute(AttributeName = "AttachID")]
        public string AttachID { get; set; } 

        // <summary>
        /// 是否在新窗口中打开 可选
        /// </summary>
        [XmlAttribute(AttributeName = "NewWindow")]
        public bool NewWindow { get; set; } = true; // 默认值 true
    }
}
