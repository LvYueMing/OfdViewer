using System.Xml.Serialization;

namespace OFDViewer.Models.Action
{
    /// <summary>
    /// 跳转的目标书签 必选
    /// </summary>
    public class GotoBookmark
    {
        [XmlAttribute(AttributeName = "Name")]
        public string Name { get; set; }
    }
}
