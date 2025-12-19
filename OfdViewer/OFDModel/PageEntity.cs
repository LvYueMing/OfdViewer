using OFDViewer.BaseType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OFDViewer.OFDModel
{
    /// <summary>
    /// 页对象
    /// </summary>
    [XmlRoot("Page", Namespace = Constants.OFD_NAMESPACE_URI)]
    public class PageEntity
    {
        /// <summary>
        /// 定义该页页面区域的大小和位置, 仅对该页有效。 该节点不出现时则使用模板页中的定义, 
        /// 如果模板页不存在或模板页中没有定义页面区域, 则使用文件 CommonData 中的定义
        /// 可选
        /// </summary>
        [XmlElement("Area")]
        public CT_PageArea Area { get; set; }


        /// <summary>
        /// 该页所使用的模板页。 模板页的内容结构和普通页相同, 定义 在CommonData 指定的 XML 文件中。 一个页可以使用多个模板页。
        /// 该节点使用时通过 TemplateID 来引用具体的模板, 并通过 ZOrder属性来控制模板在页面中的呈现顺序
        /// 注: 在模板页的内容描述中该属性无效
        /// 可选
        /// </summary>
        [XmlElement("Template")]
        public List<Template> Template { get; set; } = new List<CT_TemplatePage>();


        // PageRes（0..∞ 重复，类型ST_Loc）
        [XmlArray("PageRes")]
        [XmlArrayItem("PageRes")]
        public List<ST_Loc> PageRes { get; set; } = new List<ST_Loc>();


        // Content（包含Layer，Layer是1..∞ 重复）
        [XmlElement("Content")]
        public ContentItem Content { get; set; }


        // Actions（包含Action，Action是1..∞ 重复）
        [XmlElement("Actions")]
        public ActionsItem Actions { get; set; }
    }
}
