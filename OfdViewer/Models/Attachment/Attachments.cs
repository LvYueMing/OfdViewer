using System.Xml.Serialization;

namespace OFDViewer.Models.Attachment
{
    /// <summary>
    /// 附件集合的根节点
    /// 附件列表文件的入口点在7.5 文档根节点中定义。 一个 OFD 文档可以定义多个附件, 附件列表结构如图91 所示。
    /// </summary>
    [XmlRoot("Attachments", Namespace = Constants.OFD_NAMESPACE_URI)]
    public class Attachments
    {
        /// <summary>
        /// 附件元素集合
        /// 0..*
        /// </summary>
        [XmlElement("Attachment", IsNullable = false)]
        public List<CT_Attachment> AttachmentList { get; set; } = new List<CT_Attachment>();
    }
}