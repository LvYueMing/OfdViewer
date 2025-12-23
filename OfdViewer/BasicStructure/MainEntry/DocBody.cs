using OFDViewer.BaseType;
using OFDViewer.Versions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OFDViewer.BasicStructure.MainEntry
{
    /// <summary>
    /// 文件对象入口模型
    /// </summary>
    [XmlRoot("DocBody", Namespace = Constants.OFD_NAMESPACE_URI)]
    public class DocBody
    {
        [XmlElement("DocInfo")]
        public CT_DocInfo DocInfo { get; set; }

        [XmlIgnore]
        public ST_Loc DocRoot { get; set; }

        [XmlElement("DocRoot")]
        public string DocRootPath
        {
            get => DocRoot.ToString();
            set => DocRoot = value;
        }

        [XmlArray("Versions")]
        [XmlArrayItem("Version")]
        public List<Versions.Version> Versions { get; set; }

        [XmlIgnore]
        public ST_Loc Signatures { get; set; }

        [XmlElement("Signatures")]
        public string SignaturesPath
        {
            get => Signatures.ToString();
            set => Signatures = value;
        }
    }
}
