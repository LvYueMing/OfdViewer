using System.Xml.Serialization;
using OFDViewer.Models.BaseType;
using OFDViewer.Utils;

namespace OFDViewer.Models.BaseStructure.Resources.ResItems
{
    public class MultiMedia : CT_MultiMedia
    {
        [XmlAttribute("ID")]
        [XmlRequired(ErrorMsg = "ID 必选属性为必选项，且不能为空")]
        public string IDString
        {
            get { return ID.ToString(); }
            set { ID = ST_ID.Parse(value); }
        }
        [XmlIgnore]
        public ST_ID ID { get; set; }
    }
}
