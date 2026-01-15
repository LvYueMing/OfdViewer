using System; 
using System.Collections.Generic;
using System.Xml.Serialization;
using OFDViewer.Models.BaseType;
using OFDViewer.Models.Version;
using OFDViewer.Utils;

namespace OFDViewer.Models.Versions
{
    /// <summary>
    /// DocVersion元素
    /// </summary>
    [XmlRoot("DocVersion", Namespace = "http://www.ofdspec.org/2016")]
    public class DocVersion
    {
        /// <summary>
        /// 版本包含的文件列表
        /// 必选
        /// </summary>
        [XmlElement("FileList", IsNullable = false)]
        [XmlRequired(ErrorMsg = "FileList 必选元素为必选项，且不能为空")]
        public FileList FileList { get; set; } = new FileList();

        /// <summary>
        /// 该版本的入口文件
        /// 必选
        /// </summary>
        [XmlIgnore]
        public ST_Loc DocRoot { get; set; }

        [XmlElement("DocRoot", IsNullable = false)]
        [XmlRequired(ErrorMsg = "DocRoot 必选元素为必选项，且不能为空")]
        public string DocRootString
        {
            get => DocRoot.ToString();
            set => DocRoot = value;
        }

        /// <summary>
        /// 版本标识
        /// 必选
        /// </summary>
        [XmlIgnore]
        public ST_ID ID { get; set; }

        [XmlAttribute("ID")]
        [XmlRequired(ErrorMsg = "ID 必选属性为必选项，且不能为空")]
        public string IDString
        {
            get => ID.ToString();
            set => ID = ST_ID.Parse(value);
        }

        /// <summary>
        /// 该文件适用的格式版本
        /// 可选
        /// </summary>
        [XmlAttribute("Version")]
        public string Version { get; set; }

        /// <summary>
        /// 版本名称
        /// 可选
        /// </summary>
        [XmlAttribute("Name")]
        public string Name { get; set; }

        /// <summary>
        /// 创建日期
        /// 可选
        /// </summary>
        [XmlAttribute("CreationDate")]
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// 无参构造函数
        /// </summary>
        public DocVersion()
        {
            ID = ST_ID.CreateNew();
            FileList = new FileList();
            DocRoot = new ST_Loc();
        }
    }
    
}