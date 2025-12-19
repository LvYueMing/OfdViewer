using OFDViewer.BaseType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OFDViewer.OFDModel
{
    /// <summary>
    /// 页节点。 一个页树中可以包含一个或多个页节点, 页顺序是根据页树进行前序遍历时叶节点的访问顺序
    /// 必选
    /// </summary>
    [XmlRoot("Page", Namespace = Constants.OFD_NAMESPACE_URI)]
    public class PageNode
    {
        //声明该页的标识, 不能与已有标识重复 必选
        [XmlAttribute("ID")]
        public ST_ID ID { get; set; }

        //指向页对象描述文件 必选
        [XmlAttribute("BaseLoc")]
        public string BaseLocString
        {
            get=> BaseLoc.ToString();
            set => BaseLoc = value;          
        }

        public ST_Loc BaseLoc { get; set; }

    }
}
