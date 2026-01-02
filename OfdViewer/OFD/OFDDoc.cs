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

        /// <summary>
        /// 文档签章集合
        /// </summary>
        public List<OfdSign> Signs { get; set; } = new List<OfdSign>();

        /// <summary>
        /// 文档页面集合
        /// </summary>
        public List<OfdPage> Pages { get; set; } = new List<OfdPage>();

        /// <summary>
        /// 文档共享资源集合
        /// </summary>
        public List<OfdRes> SharedRes { get; set; } = new List<OfdRes>();

        internal OfdDoc(OfdDocument parentDocument, int docIndex)
        {
            _parentDocument = parentDocument;
            DocIndex = docIndex;
        }

        /// <summary>
        /// 添加新签章到当前文档
        /// </summary>
        public OfdSign AddNewSign()
        {
            var newSign = new OfdSign(this, Signs.Count + 1);
            Signs.Add(newSign);
            return newSign;
        }

        /// <summary>
        /// 添加新页面到当前文档
        /// </summary>
        public OfdPage AddNewPage()
        {
            var newPage = new OfdPage(this, Pages.Count + 1);
            Pages.Add(newPage);
            return newPage;
        }

        /// <summary>
        /// 添加文档共享图片资源
        /// </summary>
        public OfdRes AddSharedImageRes(string imageName = "Image")
        {
            var res = new OfdRes(this, SharedRes.Count + 1, OfdResType.Image, imageName);
            SharedRes.Add(res);
            return res;
        }

        /// <summary>
        /// 添加文档共享字体资源
        /// </summary>
        public OfdRes AddSharedFontRes(string fontName = "Font")
        {
            var res = new OfdRes(this, SharedRes.Count + 1, OfdResType.Font, fontName);
            SharedRes.Add(res);
            return res;
        }
    }
}
