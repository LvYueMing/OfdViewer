using OFDViewer.Models.BaseStructure.Pages;
using OFDViewer.Models.BaseStructure.Pages.PageBlockItems;
using OFDViewer.Models.PageDesc;
using OFDViewer.Parse;
using OFDViewer.Render.Abstractions;

namespace OFDViewer.Render.Implementation
{
    /// <summary>
    /// 渲染上下文对象
    /// 封装渲染过程中传递的共性参数，简化方法签名
    /// </summary>
    public class RenderContextObject
    {
        /// <summary>
        /// 渲染上下文接口，提供渲染能力和单位转换
        /// </summary>
        public IRenderContext RenderContext { get; set; }

        /// <summary>
        /// 当前OFD文档对象
        /// </summary>
        public OFDDocument OfdDocument { get; set; }

        /// <summary>
        /// 当前页面对象
        /// </summary>
        public Page CurrentPage { get; set; }

        /// <summary>
        /// 当前图层对象
        /// </summary>
        public CT_Layer CurrentLayer { get; set; }

        /// <summary>
        /// 是否为模板页渲染
        /// </summary>
        public bool IsTemplate { get; set; }

        /// <summary>
        /// 当前页面索引
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// 当前文档索引
        /// </summary>
        public int DocumentIndex { get; set; }

        /// <summary>
        /// 将毫米转换为像素
        /// </summary>
        /// <param name="millimeters">毫米值</param>
        /// <returns>像素值</returns>
        public float MillimetersToPixels(float millimeters)
        {
            return RenderContext?.MillimetersToPixels(millimeters) ?? millimeters;
        }

        /// <summary>
        /// 将毫米转换为像素（double重载）
        /// </summary>
        /// <param name="millimeters">毫米值</param>
        /// <returns>像素值</returns>
        public float MillimetersToPixels(double millimeters)
        {
            return RenderContext?.MillimetersToPixels((float)millimeters) ?? (float)millimeters;
        }
    }
}
