using System.Xml.Schema;
using System.Xml.Serialization;
using OFDViewer.Models.BaseType;

namespace OFDViewer.Models.Annotation
{
    /// <summary>
    /// 注释页面关联信息
    /// </summary>
    public class Page
    {
        /// <summary>
        /// 指向包内的分页注释文件
        /// 必选
        /// </summary>
        [XmlElement("FileLoc", IsNullable = false)]
        public ST_Loc FileLoc { get; set; }

        /// <summary>
        /// 引用注释所在页面的标识 
        /// 必选
        /// </summary>
        [XmlAttribute("PageID", AttributeName = "PageID", DataType = "string", Form = XmlSchemaForm.Unqualified)]
        public string PageIDString
        { 
            get => PageID.Value; 
            set => PageID = ST_RefID.Parse(value);
        }

        [XmlIgnore]
        public ST_RefID PageID { get; set; }
    }
}