using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OFDViewer.Utils;
using System.Xml.Serialization;

namespace OFDViewer.Models.BasicStructure.Resources.ResItems
{
    public class DrawParams
    {
        // 对应XSD中的DrawParam元素（可重复无限次）
        [XmlElement("DrawParam")]
        [XmlRequired(MinItemCount = 1)]
        public List<DrawParam>  drawParams { get; set; }
    }
}
