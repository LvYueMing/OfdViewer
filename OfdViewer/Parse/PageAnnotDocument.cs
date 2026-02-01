using OFDViewer.Models.Annotation;
using OFDViewer.Models.BaseType;

namespace OFDViewer.Parse
{
    /// <summary>
    /// 页面注释文档类，对应 OFD 标准中的 Annot_N 目录
    /// <remarks>
    /// 负责管理单个页面的注释内容，包括：
    /// 1. 页面注释根元素（PageAnnot.xml）
    /// 2. 注释内容文件路径
    /// </remarks>
    /// </summary>
    public class PageAnnotDocument
    {
        /// <summary>
        /// 页面注释根元素（PageAnnot.xml）
        /// </summary>
        public PageAnnot PageAnnot { get; set; }

        /// <summary>
        /// 注释所在页面的标识
        /// </summary>
        public ST_RefID PageId { get; set; }

        /// <summary>
        /// 页面注释文件路径
        /// </summary>
        public string PageAnnotFilePath { get; set; }

        /// <summary>
        /// 所属文档序号
        /// </summary>
        public int BelongDocIndex { get; set; }

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public PageAnnotDocument()
        {
        }

        /// <summary>
        /// 从文件路径初始化页面注释文档
        /// </summary>
        /// <param name="filePath">页面注释文件路径</param>
        public PageAnnotDocument(string filePath)
        {
            PageAnnotFilePath = filePath;
        }
    }
}
