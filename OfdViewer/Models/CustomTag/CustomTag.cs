using System.Xml.Serialization;
using OFDViewer.Models.BaseType;

namespace OFDViewer.Models.CustomTag
{
    /// <summary>
    /// 自定义标引
    /// </summary>
    public class CustomTag
    {
        /// <summary>
        /// 指向自定义标引内容节点适用的Schema文件
        /// 可选
        /// </summary>
        [XmlElement("SchemaLoc", IsNullable = true)]
        public string SchemaLocString
        {
            get => SchemaLoc.ToString();
            set => SchemaLoc = string.IsNullOrEmpty(value) ? null : value;
        }

        [XmlIgnore]
        public ST_Loc SchemaLoc { get; set; }

        /// <summary>
        /// 指向自定义标引文件
        /// 该类文件中通过“非接触方式”引用版式内容流中的图元和相关信息
        /// 必选
        /// </summary>
        [XmlElement("FileLoc", IsNullable = false)]
        public string FileLocString
        {
            get => FileLoc.ToString();
            set => FileLoc = value;
        }

        [XmlIgnore]
        public ST_Loc FileLoc { get; set; }

        /// <summary>
        /// 自定义标引的命名空间
        /// 必选
        /// </summary>
        [XmlAttribute("NameSpace", DataType = "string")]
        public string NameSpace { get; set; }
    }
}