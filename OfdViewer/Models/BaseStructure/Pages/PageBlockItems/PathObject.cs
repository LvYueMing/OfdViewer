using System.Xml.Serialization;
using OFDViewer.Models.BaseType;
using OFDViewer.Models.Graph;

namespace OFDViewer.Models.BaseStructure.Pages.PageBlockItems
{
    /// <summary>
    /// 图形对象
    /// </summary>
    public class PathObject : CT_Path
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
