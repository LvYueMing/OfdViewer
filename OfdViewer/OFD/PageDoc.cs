using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using OFDViewer.Models.BaseStructure.Pages;
using OFDViewer.Models.BaseStructure.Resources;
using OFDViewer.Models.BaseStructure.Resources.ResItems;

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
        /// 页面内容描述文件（Content.xml）
        /// 记录文字、图形、图片的坐标、样式、层级等
        /// </summary>
        public Page Content { get; set; } = new Page();

        /// <summary>
        /// 页面资源映射文件（PageRes.xml）
        /// 定义当前页面专属资源的引用关系
        /// </summary>
        public Res PageRes { get; set; } = new Res();

        /// <summary>
        /// 页面私有资源路径集合（Res目录下的图片等资源）
        /// 如局部插图、水印等
        /// </summary>
        public List<string> PrivateResourcePaths { get; set; } = new List<string>();

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
