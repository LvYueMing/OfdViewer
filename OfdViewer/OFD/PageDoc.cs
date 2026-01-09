using OFDViewer.Models.BaseStructure.Pages;
using OFDViewer.Models.BaseStructure.Resources;

namespace OFDViewer.OFD
{
    /// <summary>
    /// OFD页面对象，对应 Page_N 目录（N可从0开始）
    /// </summary>
    public class PageDoc
    {
        /// <summary>
        /// 页面序号（从0开始）
        /// </summary>
        public int PageIndex { get; }

        /// <summary>
        /// 所属文档序号（从0开始）
        /// </summary>
        public int BelongDocIndex { get; }

        /// <summary>
        /// 页面内容描述文件（Doc_{0}/Pages/Page_{1}/Content.xml）
        /// 记录文字、图形、图片的坐标、样式、层级等
        /// </summary>
        public Page Content { get; set; } = new Page();

        /// <summary>
        /// 页面资源映射文件（Doc_{0}/Pages/Page_{1}/PageRes.xml）
        /// 定义当前页面专属资源的引用关系
        /// </summary>
        public Res PageRes { get; set; } = new Res();

        /// <summary>
        /// 页面私有资源路径集合（Doc_{0}/Pages/Page_{1}/Res）
        /// 如局部插图、水印等
        /// </summary>
        public Dictionary<string, byte[]> PageResFiles { get; set; }

        /// <summary>
        /// 页面目录路径（相对根目录，格式：Doc_{BelongDocIndex}/Pages/Page_{PageIndex}）
        /// </summary>
        public string PageDirectoryPath =>
            $"Doc_{BelongDocIndex}/Pages/Page_{PageIndex}";

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="pageIndex">页面序号（从0开始）</param>
        /// <param name="belongDocIndex">所属文档序号（从0开始）</param>
        public PageDoc(int pageIndex, int belongDocIndex)
        {
            if (pageIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(pageIndex), "页面序号必须从0开始，不允许为负数");
            if (belongDocIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(belongDocIndex), "所属文档序号必须从0开始，不允许为负数");

            PageIndex = pageIndex;
            BelongDocIndex = belongDocIndex;
        }
    }
}
