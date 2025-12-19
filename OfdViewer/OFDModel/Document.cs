using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using OFDViewer.BaseType;

namespace OFDViewer.OFDModel
{
    /// <summary>
    /// 文档根节点结构 图5
    /// </summary>
    [XmlRoot("Document", Namespace = Constants.OFD_NAMESPACE_URI)] // 可选：指定根节点的XML名称
    public class Document
    {
        /// <summary>
        /// 文档公共数据,定义了页面区域、公共资源等数据 
        /// 必选
        /// </summary>
        [XmlElement("CommonData")]
        public CT_CommonData CommonData { get; set; }

        /// <summary>
        /// 页树,有关页树的描述见7.6 
        /// 必选
        /// </summary>
        [XmlArray("Pages")]
        [XmlArrayItem("Page")]
        public List<PageNode> Pages { get; set; }

        /// <summary>
        /// 大纲,有关大纲的描述见7.8 
        /// 可选
        /// </summary>
        [XmlElement("Outlines")]
        public Outlines Outlines { get; set; }

        /// <summary>
        /// 文档的权限声明 
        /// 可选
        /// </summary>
        [XmlElement("Permissions")]
        public CT_Permission Permissions { get; set; }

        /// <summary>
        /// 文档关联的动作序列,当存在多个Action对象时,所有动作依次执行 
        /// 可选
        /// </summary>
        [XmlArray("Actions")]
        [XmlArrayItem("Action")]
        public List<Action> Actions { get; set; }

        /// <summary>
        /// 文档的视图首选项 
        /// 可选
        /// </summary>
        [XmlElement("VPreferences")]
        public CT_VPreferences VPreferences { get; set; }

        /// <summary>
        /// 文档的书签集,包含一组书签 
        /// 可选
        /// </summary>
        [XmlArray("Bookmarks")]
        [XmlArrayItem("Bookmark")]
        public List<Bookmark> Bookmarks { get; set; }

        /// <summary>
        /// 指向注释列表文件,有关注释描述见第15章
        /// 可选
        /// </summary>
        [XmlElement("Annotations")]
        public string AnnotationsPath
        {
            get => Annotations.ToString();
            set => Annotations = value;
        }

        [XmlIgnore]
        public ST_Loc Annotations { get; set; }

        /// <summary>
        /// 指向自定义标引列表文件,有关自定义标引描述见第16章 
        /// 可选
        /// </summary>
        [XmlElement("CustomTags")]
        public string CustomTagsPath
        {
            get => CustomTags.ToString();
            set => CustomTags = value;
        }

        [XmlIgnore]
        public ST_Loc CustomTags { get; set; }

        /// <summary>
        /// 指向附件列表文件。有关附件描述见第20章 
        /// 可选
        /// </summary>
        [XmlElement("Attachments")]
        public string AttachmentsPath
        {
            get => Attachments.ToString();
            set => Attachments = value;
        }

        [XmlIgnore]
        public ST_Loc Attachments { get; set; }

        /// <summary>
        /// 指向扩展列表文件,有关扩展描述见第17章 
        /// 可选
        /// </summary>
        [XmlElement("Extensions")]
        public string ExtensionsPath
        {
            get => Extensions.ToString();
            set => Extensions = value;
        }

        [XmlIgnore]
        public ST_Loc Extensions { get; set; }
    }
}
