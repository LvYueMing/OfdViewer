using OFDViewer.BaseType;
using OFDViewer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OFDViewer.BasicStructure.MainEntry
{
    public class CT_DocInfo
    {
        /// <summary>
        /// 采用 UUID 算法生成的由 32 个字符组成的文件标识。 每个 DocID 在文档创建或生成的时候进行分配  
        /// 可选
        /// </summary>
        [XmlElement("DocID")]
        public string DocID { get; set; }

        /// <summary>
        /// 文档标题。 标题可以与文件名不同 
        /// 可选
        /// </summary>
        [XmlElement("Title")]
        public string Title { get; set; }

        /// <summary>
        /// 文档作者 
        /// 可选
        /// </summary>
        [XmlElement("Author")]
        public string Author { get; set; }

        /// <summary>
        /// 文档主题 
        /// 可选
        /// </summary>
        [XmlElement("Subject")]
        public string Subject { get; set; }


        /// <summary>
        /// 文档摘要与注释 
        /// 可选
        /// </summary>
        [XmlElement("Abstract")]
        public string Abstract { get; set; }

        /// <summary>
        /// 文档创建日期 
        /// 可选
        /// </summary>
        [XmlElement("CreationDate")]
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// 文档最近修改日期 
        /// 可选
        /// </summary>
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
        public string DocUsageString
        {
            get => EnumHelper.GetEnumName(DocUsage);
            set
            {
                DocUsage= EnumHelper.ParseEnum<DocumentUsage>(value);
            }
        }
        [XmlIgnore]
        public DocumentUsage DocUsage { get; set; }

        [XmlIgnore]
        public ST_Loc Cover { get; set; }

        /// <summary>
        /// 文档封面, 此路径指向一个图片文件 可选
        /// </summary>
        [XmlElement("Cover")]
        public string CoverPath
        {
            get => Cover.ToString();
            set => Cover = value;
        }

        /// <summary>
        /// 关键词集合, 每一个关键词用一个“Keyword”子节点来表达
        /// 可选
        /// </summary>
        [XmlArray("Keywords")]
        [XmlArrayItem("Keyword")]
        public List<Keyword> Keywords { get; set; } = new List<Keyword>();

        /// <summary>
        /// 创建文档的应用程序 
        /// 可选
        /// </summary>
        [XmlElement("Creator")]
        public string Creator { get; set; }

        /// <summary>
        /// 创建文档的应用程序的版本信息 
        /// 可选      
        /// </summary>
        [XmlElement("CreatorVersion")]
        public string CreatorVersion { get; set; }

        /// <summary>
        /// 用户自定义元数据集合。 其子节点为 CustomData 
        /// 可选
        /// </summary>
        [XmlArray("CustomDatas")]
        [XmlArrayItem("CustomData")]
        public List<CustomData> CustomDatas { get; set; } = new List<CustomData>();
    }

}
