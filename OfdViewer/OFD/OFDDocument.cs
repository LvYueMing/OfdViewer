using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OFDViewer.Models.BasicStructure.MainEntry;

namespace OFDViewer.OFD
{
    /// <summary>
    /// OFD根文档对象，对应整个OFD压缩包
    /// </summary>
    public class OFDDocument
    {
        /// <summary>
        /// OFD.xml 对应的全局元数据对象
        /// </summary>
        public Models.BasicStructure.MainEntry.OFD Ofd { get; set; }

        /// <summary>
        /// 全局元数据文件路径（相对根目录）
        /// </summary>
        public string OfdFilePath => Constants.Root_OfdMetadataFile;

        /// <summary>
        /// 文档集合（对应 Doc_1、Doc_2...）
        /// </summary>
        public List<OFDDoc> Docs { get; set; } = new List<OFDDoc>();


        //无参构造函数
        public OFDDocument()
        {
            Ofd = new Models.BasicStructure.MainEntry.OFD();
        }

        /// <summary>
        /// 添加新文档到OFD根文档，并自动同步到 OfdMetadata 的 Docs 列表
        /// </summary>
        /// <returns>新增的文档对象</returns>
        public OFDDoc AddNewDoc()
        {

        }
    }
}
