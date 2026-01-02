using OFDViewer.Models.BaseType;
using OFDViewer.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OFDViewer.Models.BasicStructure.MainEntry
{
    /// <summary>
    /// 文件对象入口,可以存在多个,以便在一个文档中包含多个版式文档 
    /// 必选
    /// </summary>
    [XmlRoot("DocBody", Namespace = Constants.OFD_NAMESPACE_URI)]
    public class DocBody
    {
        /// <summary>
        /// 文档元数据信息描述,文档元数据信息具体结构见图4 
        /// 必选
        /// </summary>
        [XmlElement("DocInfo")]
        [XmlRequired(ErrorMsg = "DocInfo 必选属性为必选项，且不能为空")]
        public CT_DocInfo DocInfo { get; set; }

        [XmlIgnore]
        public ST_Loc DocRoot { get; set; }

        /// <summary>
        /// 指向文档根节点文档,有关文档根节点描述见7.5 文档根节点 
        /// 可选
        /// </summary>
        [XmlElement("DocRoot")]
        public string DocRootPath
        {
            get => DocRoot.ToString();
            set => DocRoot = value;
        }

        /// <summary>
        /// 包含多个版本描述节点,用于定义文件因注释和其他改动产生的版本信息,见第19章
        /// 可选
        /// </summary>
        [XmlArray("Versions")]
        [XmlArrayItem("Version")]
        public List<Versions.Version> Versions { get; set; }

        [XmlIgnore]
        public ST_Loc Signatures { get; set; }

        /// <summary>
        /// 指向该文档中签名和签章结构,见第18章 
        /// 可选
        /// </summary>
        [XmlElement("Signatures")]
        public string SignaturesPath
        {
            get => Signatures.ToString();
            set => Signatures = value;
        }

        /// <summary>
        /// 无参构造函数，初始化必选属性
        /// </summary>
        public DocBody()
        {
            DocInfo = new CT_DocInfo();
        }
    }
}
