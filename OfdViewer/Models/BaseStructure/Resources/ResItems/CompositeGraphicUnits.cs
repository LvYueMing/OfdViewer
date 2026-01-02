using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using OFDViewer.Utils;

namespace OFDViewer.Models.BaseStructure.Resources.ResItems
{
    public class CompositeGraphicUnits
    {
        // 对应XSD中的CompositeGraphicUnit元素（可重复无限次）
        [XmlElement("CompositeGraphicUnit")]
        [XmlRequired(MinItemCount = 1)]
        public List<CompositeGraphicUnit> compositeGraphicUnits { get; set; }

    }
}
