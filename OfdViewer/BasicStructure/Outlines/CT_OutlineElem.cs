using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OFDViewer.Actions;
using System.Xml.Serialization;

namespace OFDViewer.BasicStructure.Outlines
{
    /// <summary>
    /// 大纲节点
    /// </summary>
    public class CT_OutlineElem
    {
        /// <summary>
        /// 当此大纲节点被激活时将执行的动作序列 
        /// 可选
        /// </summary>
        [XmlArray("Actions")]
        [XmlArrayItem("Action")]
        public List<CT_Action> Actions { get; set; }

        /// <summary>
        /// 该节点的子大纲节点。层层嵌套,形成树状结构 
        /// 可选
        /// </summary>
        [XmlElement("OutlineElem")]
        public List<CT_OutlineElem> OutlineElem { get; set; }

        /// <summary>
        /// 大纲节点标题 
        /// 必选
        /// </summary>
        [XmlAttribute("Title")]
        public string Title { get; set; }

        /// <summary>
        /// 该节点下所有叶节点的数目参考值,应根据该节点下实际出现的子节点数为准
        /// 默认值为0
        /// 可选
        /// </summary>
        [XmlAttribute("Count")]
        public int Count { get; set; }

        // 标记 Count 属性是否序列化（处理可选性）
        [XmlIgnore]
        public bool CountSpecified { get; set; }

        /// <summary>
        /// 在有子节点存在时有效,如果为true,表示该大纲在初始状态下展
        /// 开子节点;如果为false,则表示不展开
        /// 默认值为true
        /// 可选
        /// </summary>
        [XmlAttribute("Expanded")]
        public bool Expanded { get; set; } = true;

        // 标记 Expanded 属性是否序列化（处理默认值和可选性）
        [XmlIgnore]
        public bool ExpandedSpecified { get; set; } = true;

    }
}
