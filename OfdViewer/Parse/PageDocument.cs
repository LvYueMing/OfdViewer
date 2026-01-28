using System.Text.RegularExpressions;
using OFDViewer.Models.BaseStructure.Pages;
using OFDViewer.Models.BaseStructure.Resources;

namespace OFDViewer.Parse
{
    /// <summary>
    /// OFD页面对象，对应 Page_N 目录（N可从0开始）
    /// </summary>
    public class PageDocument
    {
        public uint PageId { get; internal set; }
        /// <summary>
        /// 页面序号（从0开始，自动计算）
        /// </summary>
        public int PageIndex { get; internal set; }

        /// <summary>
        /// 所属文档序号（从0开始）
        /// </summary>
        public int BelongDocIndex { get; internal set; }

        /// <summary>
        /// 所属文档路径（当使用路径构造函数时赋值）
        /// </summary>
        public string BelongDocPath { get; internal set; }

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

        private string _pageFilePath;
        /// <summary>
        /// 页面文件绝对路径
        /// </summary>
        public string PageFilePath
        {
            get
            {
                if (string.IsNullOrEmpty(_pageFilePath))
                {
                    _pageFilePath = Constants.GetFilePath(Constants.Page_ContentFile, BelongDocIndex, PageIndex);
                }
                return _pageFilePath;
            }
            set
            {
                _pageFilePath = value;
                PageIndex = int.Parse(Regex.Match(value, @"Page_(\d+)").Groups[1].Value);
            }
        }


        /// <summary>
        /// 页面目录路径（相对根目录，格式：Doc_{BelongDocIndex}/Pages/Page_{PageIndex}）
        /// </summary>
        public string PageDirectoryPath => string.IsNullOrEmpty(PageFilePath)
            ? $"Doc_{BelongDocIndex}/Pages/Page_{PageIndex}"
            : Path.GetDirectoryName(PageFilePath);


        
        private string _pageResFilePath;
        /// <summary>
        /// //页面资源映射文件绝对路径
        /// </summary>
        public string PageResFilePath
        {
            get
            {
                if (string.IsNullOrEmpty(_pageResFilePath))
                {
                    _pageResFilePath = Constants.GetFilePath(Constants.Page_PageResFile, BelongDocIndex, PageIndex);
                }
                return _pageResFilePath;
            }
            set => _pageResFilePath = value;
        }


        /// <summary>
        /// 构造函数
        /// </summary>
        public PageDocument()
        {
            PageIndex = 0;
            BelongDocIndex = 0;
            Page = new Page();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="belongDocPath">所属文档路径</param>
        public PageDocument(string belongDocPath)
        {
            if (string.IsNullOrEmpty(belongDocPath))
            {
                throw new ArgumentNullException(nameof(belongDocPath), "所属文档路径不能为空");
            }
            BelongDocPath = belongDocPath;
            BelongDocIndex = int.Parse(Regex.Match(belongDocPath, @"Doc_(\d+)").Groups[1].Value);
            Page = new Page();
        }
    }
}
