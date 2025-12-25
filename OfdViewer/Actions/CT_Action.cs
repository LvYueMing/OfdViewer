using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using OFDViewer.Actions.ActionItems;
using OFDViewer.Graph;
using OFDViewer.Graph.PathItems;
using OFDViewer.Utils;

namespace OFDViewer.Actions
{
    /// <summary>
    /// 动作描述
    /// </summary>
    public class CT_Action
    {
        /// <summary>
        /// 指定多个复杂区域为该链接对象的启动区域,不出现时以所在图元或
        /// 页面的外接矩形作为启动区域,见9.3
        /// 可选
        /// </summary>
        [XmlElement("Region",IsNullable = false)]
        public CT_Region Region { get; set; }


        #region xs:choice 核心实现

        /// <summary>
        /// 通过多个XmlElement标记，指定不同子类型对应的XML节点名
        /// 让 ActionItem 属性在序列化 / 反序列化时自动匹配对应的 XML 节点类型，实现 “多选一”
        /// </summary>
        [XmlElement("Goto", typeof(Goto))]
        [XmlElement("URI", typeof(URI))]
        [XmlElement("GotoA", typeof(GotoA))]
        [XmlElement("Sound", typeof(Sound))]
        [XmlElement("Movie", typeof(Movie))]
        public BaseAction ActionItem { get; set; }

        #endregion

        /// <summary>
        /// Event 属性（必选），对应枚举值：DO/PO/CLICK
        /// </summary>
        [XmlAttribute(AttributeName = "Event")]
        public string EventString
        {
            get=> Event.ToString();
            set=> Event = EnumHelper.ParseEnum<ActionEventEnum>(value);
        }

        [XmlIgnore]
        public ActionEventEnum Event { get; set; }


    }
}
