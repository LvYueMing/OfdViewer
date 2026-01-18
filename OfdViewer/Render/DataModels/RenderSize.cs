using System;

namespace OFDViewer.Render.DataModels
{
    /// <summary>
    /// 渲染尺寸类
    /// 包含宽、高、分辨率（DPI）和单位转换功能
    /// </summary>
    public class RenderSize
    {
        /// <summary>
        /// 宽度（像素）
        /// </summary>
        public float Width { get; set; }

        /// <summary>
        /// 高度（像素）
        /// </summary>
        public float Height { get; set; }

        /// <summary>
        /// 分辨率（DPI）
        /// </summary>
        public float Dpi { get; set; } = 96.0f;

        /// <summary>
        /// 将毫米转换为像素
        /// </summary>
        /// <param name="millimeters">毫米值</param>
        /// <returns>像素值</returns>
        public float MillimetersToPixels(float millimeters)
        {
            return millimeters * Dpi / 25.4f;
        }

        /// <summary>
        /// 将像素转换为毫米
        /// </summary>
        /// <param name="pixels">像素值</param>
        /// <returns>毫米值</returns>
        public float PixelsToMillimeters(float pixels)
        {
            return pixels * 25.4f / Dpi;
        }

        /// <summary>
        /// 将OFD点转换为像素
        /// OFD中1点 = 1/72英寸
        /// </summary>
        /// <param name="points">点数</param>
        /// <returns>像素值</returns>
        public float PointsToPixels(float points)
        {
            return points * Dpi / 72.0f;
        }

        /// <summary>
        /// 将像素转换为OFD点
        /// </summary>
        /// <param name="pixels">像素值</param>
        /// <returns>点数</returns>
        public float PixelsToPoints(float pixels)
        {
            return pixels * 72.0f / Dpi;
        }
    }
}
