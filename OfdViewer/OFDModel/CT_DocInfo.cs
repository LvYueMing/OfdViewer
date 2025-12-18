using OFDViewer.BaseType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OFDViewer.OFDModel
{
    [XmlRoot("CT_DocInfo")]
    public class CT_DocInfo
    {
        // 采用 UUID 算法生成的由32 个字符组成的文件标识。 每个 DocID在文档创建或生成的时候进行分配  可选
        [XmlElement("DocID")]
        public string DocID { get; set; }

        //文档标题。 标题可以与文件名不同 可选
        [XmlElement("Title")]
        public string Title { get; set; }

        // 文档作者 可选
        [XmlElement("Author")]
        public string Author { get; set; }

        // 文档主题 可选
        [XmlElement("Subject")]
        public string Subject { get; set; }

        // 文档摘要与注释 可选
        [XmlElement("Abstract")]
        public string Abstract { get; set; }

        // 文档创建日期 可选
        [XmlElement("CreationDate")]
        public DateTime CreationDate { get; set; }

        // 文档最近修改日期 可选
        [XmlElement("ModDate")]        
        public DateTime ModDate { get; set; }

        /// <summary>
        /// 文档分类, 可取值如下:
        ///   Normal———普通文档
        ///   EBook———电子书
        ///   ENewsPaper———电子报纸
        ///   EMagzine———电子期刊杂志
        ///   默认值为 Normal
        /// 可选
        /// </summary>        
        [XmlElement("DocUsage")]
        public string DocUsage { get; set; }

        // 文档封面, 此路径指向一个图片文件 可选
        [XmlElement("Cover")]
        public ST_Loc Cover { get; set; }

        // 关键词集合, 每一个关键词用一个“Keyword”子节点来表达 可选
        [XmlArray("Keywords")]
        [XmlArrayItem("Keyword")]
        public List<Keyword> Keywords { get; set; } = new List<Keyword>();

        // 创建文档的应用程序 可选
        [XmlElement("Creator")]
        public string Creator { get; set; }

        // 创建文档的应用程序的版本信息 可选      
        [XmlElement("CreatorVersion")]
        public string CreatorVersion { get; set; }

        // 用户自定义元数据集合。 其子节点为 CustomData 可选
        [XmlArray("CustomDatas")]
        [XmlArrayItem("CustomData")]
        public List<CustomData> CustomDatas { get; set; } = new List<CustomData>();
    }

    // 关键词 必选
    public class Keyword
    {
        [XmlText]
        public string Value { get; set; }
    }

    //用户自定义元数据, 可以指定一个名称及其对应的值 必选
    public class CustomData
    {
        // 用户自定义元数据名称 必选
        [XmlAttribute("Name")]
        public string Name { get; set; }

        // type="xs:string"（元素值）
        [XmlText]
        public string Value { get; set; }
    }
}
