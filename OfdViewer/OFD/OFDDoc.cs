using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OFDViewer.Models.BaseStructure.DocumentRoot;
using OFDViewer.Models.BaseStructure.Resources;

namespace OFDViewer.OFD
{
    /// <summary>
    /// OFD子文档对象，对应 Doc_N 目录
    /// </summary>
    public class OFDDoc
    {
        /// <summary>
        /// 文档主描述文件，定义页面尺寸、页面总数、文档结构等属性
        /// </summary>
        public Document Document { get; set; }

        /// <summary>
        /// 文档序号（从1开始）
        /// </summary>
        public int DocIndex { get; }

        //全文档公共资源描述文件
        public Res PublicRes { get; set; }

        //当前文档的资源描述文件
        public Res DocumentRes { get; set; }



        /// <summary>
        /// 文档主描述文件路径（相对根目录）
        /// </summary>
        public string DocumentFilePath => Constants.GetFilePath(Constants.Doc_DocumentFile, DocIndex);

        /// <summary>
        /// 文档公共资源描述文件路径
        /// </summary>
        public string PublicResFilePath => Constants.GetFilePath(Constants.Doc_PublicResFile, DocIndex);

        /// <summary>
        /// 文档私有资源描述文件路径
        /// </summary>
        public string DocumentResFilePath => Constants.GetFilePath(Constants.Doc_DocumentResFile, DocIndex);

        /// <summary>
        /// 文档级资源目录路径
        /// </summary>
        public string ResDirectoryPath => Constants.GetFilePath(Constants.Doc_ResDirectory, DocIndex);

       
    }
}
