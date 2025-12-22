using OFDViewer.BaseType;
using OFDViewer.BasicStructure.DocumentRoot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OFDViewer.BasicStructure.Pages
{
    /// <summary>
    /// 页对象
    /// </summary>
    [XmlRoot("Page", Namespace = Constants.OFD_NAMESPACE_URI)]
    public class Page
    {
        /// <summary>
        /// 该页所使用的模板页。 模板页的内容结构和普通页相同, 定义 在CommonData 指定的 XML 文件中。 一个页可以使用多个模板页。
        /// 该节点使用时通过 TemplateID 来引用具体的模板, 并通过 ZOrder属性来控制模板在页面中的呈现顺序
        /// 注: 在模板页的内容描述中该属性无效
        /// 可选
        /// </summary>
        [XmlElement("Template")]
        public List<PageTemplate> Template { get; set; }

        /// <summary>
        /// 页资源,指向该页使用的资源文件 
        /// 可选 0..∞ 
        /// </summary>
        [XmlElement("PageRes")]
        public List<string> PageResPath
        {
            get => PageRes?.Select(item => item.ToString()).ToList();
            set => PageRes = value?.Select(item => (ST_Loc)item).ToList() ?? new List<ST_Loc>();
        }

        public List<ST_Loc> PageRes { get; set; }

        /// <summary>
        /// 定义该页页面区域的大小和位置, 仅对该页有效。 该节点不出现时则使用模板页中的定义, 
        /// 如果模板页不存在或模板页中没有定义页面区域, 则使用文件 CommonData 中的定义
        /// 可选
        /// </summary>
        [XmlElement("Area")]
        public CT_PageArea Area { get; set; }

        /// <summary>
        /// 页面内容描述,该节点不存在时,表示空白页
        /// 可选 1..∞
        /// </summary>
        [XmlArray("Content")]
        [XmlArrayItem("Layer")]
        public List<Layer> Content { get; set; }



        // Actions（包含Action，Action是1..∞ 重复）
        [XmlElement("Actions")]
        public ActionsItem Actions { get; set; }
    }
}
