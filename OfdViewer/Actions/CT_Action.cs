using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using OFDViewer.Graph;
using OFDViewer.Graph.Shape;

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
        /// 标识当前选中的 Choice 项（与 XmlChoiceIdentifier 配合）
        /// </summary>
        [XmlIgnore]
        public ActionTypeEnum SelectedAction { get; set; }

        /// <summary>
        /// 统一的 Choice 内容属性，通过 XmlChoiceIdentifier 关联枚举
        /// </summary>
        [XmlElement("Goto", typeof(Goto))]
        [XmlElement("URI", typeof(URI))]
        [XmlElement("GotoA", typeof(GotoA))]
        [XmlElement("Sound", typeof(Sound))]
        [XmlElement("Movie", typeof(Movie))]
        [XmlChoiceIdentifier(MemberName = "SelectedAction")]
        public object ChoiceAction { get; set; }


        #endregion

        /// <summary>
        /// Event 属性（必选），对应枚举值：DO/PO/CLICK
        /// </summary>
        [XmlAttribute(AttributeName = "Event")]
        public EventType Event { get; set; }
    }
}
