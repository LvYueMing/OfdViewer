using System.Xml.Serialization;
using OFDViewer.Utils;

namespace OFDViewer.Models.BaseStructure.Resources.ResItems
{
    public class OFDFonts : BaseRes
    {
        // 对应XSD中的Font元素（可重复无限次）
        [XmlElement("Font")]
        [XmlRequired(MinItemCount = 1)]
        public List<OFDFont> ofdFonts { get; set; }
    }
}
