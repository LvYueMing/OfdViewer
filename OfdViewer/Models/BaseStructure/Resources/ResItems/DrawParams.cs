using System.Xml.Serialization;
using OFDViewer.Utils;

namespace OFDViewer.Models.BaseStructure.Resources.ResItems
{
    public class DrawParams
    {
        // 对应XSD中的DrawParam元素（可重复无限次）
        [XmlElement("DrawParam")]
        [XmlRequired(MinItemCount = 1)]
        public List<DrawParam> drawParams { get; set; }
    }
}
