using OfdViewer.BaseType.DocumentStructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OfdViewer.OFDModel
{
    /// <summary>
    /// 文件对象入口模型
    /// </summary>
    [XmlRoot("DocBody", Namespace = Constants.OFD_NAMESPACE_URI)]
    public class DocBody
    {
        [XmlElement("DocInfo")]
        public CT_DocInfo DocInfo { get; set; }

        [XmlElement("DocRoot")]
        public ST_Loc DocRoot { get; set; }

        [XmlArray("Versions")]
        [XmlArrayItem("Version")]
        public List<Version> Versions { get; set; }

        [XmlElement("Signatures")]
        public ST_Loc Signatures { get; set; }
    }
}
