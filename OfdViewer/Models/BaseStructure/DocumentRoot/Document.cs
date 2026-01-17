using System.Xml.Serialization;
using OFDViewer.Models.Action;
using OFDViewer.Models.BaseStructure.Outlines;
using OFDViewer.Models.BaseType;
using OFDViewer.Utils;

namespace OFDViewer.Models.BaseStructure.DocumentRoot
{
    /// <summary>
    /// 文档根节点结构 图5
    /// </summary>
    [XmlRoot("Document", Namespace = Constants.OFD_NAMESPACE_URI)]
    public class Document
    {
        /// <summary>
        /// 文档公共数据,定义了页面区域、公共资源等数据 
        /// 必选
        /// </summary>
        [XmlElement("CommonData")]
        [XmlRequired(ErrorMsg = "文档公共数据为必选属性，不能为空")]
        public CT_CommonData CommonData { get; set; }

        /// <summary>
        /// 页树,有关页树的描述见7.6 
        /// 必选 1..n
        /// </summary>
        [XmlArray("Pages")]
        [XmlArrayItem("Page")]
        [XmlRequired(ErrorMsg = "页树为必选属性，不能为空")]
        public List<DocumentPage> Pages { get; set; }

        /// <summary>
        /// 大纲,有关大纲的描述见7.8 
        /// 可选
        /// </summary>
        [XmlArray("Outlines")]
        [XmlArrayItem("OutlineElem")]
        public List<CT_OutlineElem> Outlines { get; set; }

        /// <summary>
        /// 文档的权限声明 
        /// 可选
        /// </summary>
        [XmlElement("Permissions")]
        public CT_Permission Permissions { get; set; }

        /// <summary>
        /// 文档关联的动作序列,当存在多个Action对象时,所有动作依次执行 
        /// 可选 1..n
        /// </summary>
        [XmlArray("Actions")]
        [XmlArrayItem("Action")]
        public List<CT_Action> Actions { get; set; }

        /// <summary>
        /// 文档的视图首选项 
        /// 可选
        /// </summary>
        [XmlElement("VPreferences")]
        public CT_VPreferences VPreferences { get; set; }

        /// <summary>
        /// 文档的书签集,包含一组书签 
        /// 可选  1..n
        /// </summary>
        [XmlArray("Bookmarks")]
        [XmlArrayItem("Bookmark")]
        public List<CT_Bookmark> Bookmarks { get; set; }

        /// <summary>
        /// 指向注释列表文件,有关注释描述见第15章
        /// 可选
        /// </summary>
        [XmlElement("Annotations")]
        public ST_Loc Annotations { get; set; }

        /// <summary>
        /// 指向自定义标引列表文件,有关自定义标引描述见第16章 
        /// 可选
        /// </summary>
        [XmlElement("CustomTags")]
        public ST_Loc CustomTags { get; set; }

        /// <summary>
        /// 指向附件列表文件。有关附件描述见第20章 
        /// 可选
        /// </summary>
        [XmlElement("Attachments")]
        public ST_Loc Attachments { get; set; }

        /// <summary>
        /// 指向扩展列表文件,有关扩展描述见第17章 
        /// 可选
        /// </summary>
        [XmlElement("Extensions")]
        public ST_Loc Extensions { get; set; }


        /// <summary>
        /// 无参构造函数，必选属性初始化
        /// </summary>
        public Document()
        {
            CommonData = new CT_CommonData();
            Pages = new List<DocumentPage>();
        }

    }
}
