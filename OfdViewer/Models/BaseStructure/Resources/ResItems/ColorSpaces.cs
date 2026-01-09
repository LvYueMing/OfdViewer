using System.Xml.Serialization;
using OFDViewer.Utils;

namespace OFDViewer.Models.BaseStructure.Resources.ResItems
{
    public class ColorSpaces : BaseRes
    {
        // 对应XSD中的ColorSpace元素（可重复无限次）
        [XmlElement("ColorSpace")]
        [XmlRequired(MinItemCount = 1)]
        public List<ColorSpace> colorSpaces { get; set; }
    }
}
