using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OFDViewer.Models.BaseStructure.DocumentRoot;
using OFDViewer.Models.BaseStructure.Resources;
using OFDViewer.Models.Signature;

namespace OFDViewer.OFD
{
    /// <summary>
    /// OFD子文档对象，对应 Doc_N 目录
    /// </summary>
    public class OFDDoc
    {
        /// <summary>
        /// 文档主描述文件（Document.xml），定义页面尺寸、页面总数、文档结构等属性
        /// </summary>
        public Document Document { get; set; }

        /// <summary>
        /// 文档序号（从0开始，只读，构造时赋值）
        /// </summary>
        public int DocIndex { get; }

        /// <summary>
        /// 全文档公共资源描述文件（PublicRes.xml）
        /// </summary>
        public Res PublicResource { get; set; }

        /// <summary>
        /// 当前文档的资源描述文件（DocumentRes.xml）
        /// </summary>
        public Res DocumentResource { get; set; }

        /// <summary>
        /// 签章列表索引对象（对应Signatures.xml，记录所有签章信息）
        /// </summary>
        public Signatures Signatures { get; set; }

        /// <summary>
        /// 签章对象集合（对应Sign_N目录，一个文档可包含多个签章）
        /// </summary>
        public List<SignDoc> SignDocs { get; set; } 

        /// <summary>
        /// 页面对象集合（对应Page_N目录，存储文档所有页面）
        /// </summary>
        public List<PageDoc> PageDocs { get; set; }

        /// <summary>
        /// 文档级资源文件集合（存储Res目录下的字体、图片等资源）
        /// </summary>
        public Dictionary<string, byte[]> ResFiles { get; set; }

        /// <summary>
        /// 文档主描述文件路径（相对根目录，基于文档序号动态生成）
        /// </summary>
        public string DocumentFilePath => Constants.GetFilePath(Constants.Doc_DocumentFile, DocIndex);

        /// <summary>
        /// 文档公共资源描述文件路径（相对根目录）
        /// </summary>
        public string PublicResourceFilePath => Constants.GetFilePath(Constants.Doc_PublicResFile, DocIndex);

        /// <summary>
        /// 文档私有资源描述文件路径（相对根目录）
        /// </summary>
        public string DocumentResourceFilePath => Constants.GetFilePath(Constants.Doc_DocumentResFile, DocIndex);

        /// <summary>
        /// 文档级资源目录路径（对应Doc_N/Res目录，存储共享资源）
        /// </summary>
        public string ResDirectoryPath => Constants.GetFilePath(Constants.Doc_ResDirectory, DocIndex);


        /// <summary>
        /// 构造函数，初始化文档序号及默认对象（DocIndex从0开始）
        /// </summary>
        /// <param name="docIndex">文档序号（从0开始）</param>
        public OFDDoc(int docIndex)
        {
            // 校验文档序号合法性：从0开始，不允许负数
            if (docIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(docIndex), "文档序号必须从0开始，不允许为负数");
            }
            DocIndex = docIndex;
            Document = new Document();
        }


    }
}
