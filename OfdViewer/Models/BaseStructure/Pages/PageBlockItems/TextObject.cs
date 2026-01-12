using System.Xml.Serialization;
using OFDViewer.Models.BaseType;
using OFDViewer.Models.Font;

namespace OFDViewer.Models.BaseStructure.Pages.PageBlockItems
{
    /// <summary>
    /// 文字对象
    /// </summary>
    public class TextObject : CT_Text
    {
        [XmlAttribute("ID")]
        public string IDString
        {
            get => ID.ToString();
            set => ID = ST_ID.Parse(value);
        }

        [XmlIgnore]
        public ST_ID ID { get; set; }
    }
}
