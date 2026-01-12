using System.Xml.Serialization;
using OFDViewer.Models.BaseType;
using OFDViewer.Models.Composite;

namespace OFDViewer.Models.BaseStructure.Pages.PageBlockItems
{
    /// <summary>
    /// 复合对象,见第13章
    /// </summary>
    public class CompositeObject : CT_Composite
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
