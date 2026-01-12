using System.Xml.Serialization;
using OFDViewer.Utils;

namespace OFDViewer.Models.BaseStructure.Resources.ResItems
{
    public class CompositeGraphicUnits : BaseRes
    {
        // 对应XSD中的CompositeGraphicUnit元素（可重复无限次）
        [XmlElement("CompositeGraphicUnit")]
        [XmlRequired(MinItemCount = 1)]
        public List<CompositeGraphicUnit> compositeGraphicUnits { get; set; }

    }
}
