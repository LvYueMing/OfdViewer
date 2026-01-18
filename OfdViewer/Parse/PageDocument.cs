using OFDViewer.Models.BaseStructure.Pages;
using OFDViewer.Models.BaseStructure.Resources;

namespace OFDViewer.Parse
{
    /// <summary>
    /// OFD页面对象，对应 Page_N 目录（N可从0开始）
    /// </summary>
    public class PageDocument
    {
        /// <summary>
        /// 页面序号（从0开始，自动计算）
        /// </summary>
        public int PageIndex { get; internal set; }

        /// <summary>
        /// 所属文档序号（从0开始）
        /// </summary>
        public int BelongDocIndex { get; internal set; }

        /// <summary>
        /// 页面内容描述文件（Doc_{0}/Pages/Page_{1}/Content.xml）
        /// 记录文字、图形、图片的坐标、样式、层级等
        /// </summary>
        public Page Page { get; set; } 

        /// <summary>
        /// 页面资源映射文件（Doc_{0}/Pages/Page_{1}/PageRes.xml）
        /// 定义当前页面专属资源的引用关系
        /// </summary>
        public Res PageRes { get; set; }

        /// <summary>
        /// 页面私有资源路径集合（Doc_{0}/Pages/Page_{1}/Res）
        /// 如局部插图、水印等
        /// </summary>
        public Dictionary<string, byte[]> PageResFiles { get; set; }

        /// <summary>
        /// 页面目录路径（相对根目录，格式：Doc_{BelongDocIndex}/Pages/Page_{PageIndex}）
        /// </summary>
        public string PageDirectoryPath => $"Doc_{BelongDocIndex}/Pages/Page_{PageIndex}";


        /// <summary>
        /// 构造函数
        /// </summary>
        public PageDocument()
        {
            PageIndex = 0;
            BelongDocIndex = 0;
            Page = new Page();
        }
    }
}
