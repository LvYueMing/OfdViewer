using System.Xml.Serialization;

namespace OFDViewer.Models.BaseStructure.MainEntry
{
    /// <summary>
    /// 关键词 
    /// 必选
    /// </summary>
    public class Keyword
    {
        [XmlText]
        public string Value { get; set; }
    }
}
