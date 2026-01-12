using System.Xml.Serialization;
using OFDViewer.Models.BaseType;
using OFDViewer.Utils;

namespace OFDViewer.Models.BaseStructure.DocumentRoot
{
    /// <summary>
    /// 页节点
    /// 一个页树中可以包含一个或多个页节点, 页顺序是根据页树进行前序遍历时叶节点的访问顺序
    /// 必选
    /// </summary>
    [XmlRoot("Page", Namespace = Constants.OFD_NAMESPACE_URI)]
    public class DocumentPage
    {
        /// <summary>
        /// 声明该页的标识, 不能与已有标识重复 
        /// 必选
        /// </summary>
        [XmlAttribute("ID")]
        [XmlRequired(ErrorMsg = "页标识为必选属性，不能为空")]
        public string IDString
        {
            get => ID.ToString();
            set => ID = ST_ID.Parse(value);
        }
        [XmlIgnore]
        public ST_ID ID { get; private set; }


        /// <summary>
        /// 指向页对象描述文件 
        /// 必选
        /// </summary>
        [XmlAttribute("BaseLoc")]
        [XmlRequired(ErrorMsg = "页对象描述文件为必选属性，不能为空")]
        public string BaseLocString
        {
            get => BaseLoc.ToString();
            set => BaseLoc = value;
        }
        [XmlIgnore]
        public ST_Loc BaseLoc { get; set; }

        //无参构造函数 必选属性初始化
        public DocumentPage()
        {
            ID = ST_ID.CreateNew();
            BaseLoc = new ST_Loc();
        }

    }
}
