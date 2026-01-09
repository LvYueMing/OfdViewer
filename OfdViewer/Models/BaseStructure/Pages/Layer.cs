using System.Xml.Serialization;
using OFDViewer.Models.BaseType;

namespace OFDViewer.Models.BaseStructure.Pages
{
    /// <summary>
    /// 图层
    /// </summary>
    public class Layer : CT_Layer
    {
        /// <summary>
        /// ID 属性（必填，类型为 ST_ID）
        /// </summary>
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
