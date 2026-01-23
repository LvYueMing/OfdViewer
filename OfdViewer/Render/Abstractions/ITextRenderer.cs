using OFDViewer.Render.DataModels;

namespace OFDViewer.Render.Abstractions
{
    /// <summary>
    /// 文本渲染器接口
    /// 封装普通文本、富文本、条码、字体样式、文本对齐等渲染方法
    /// </summary>
    public interface ITextRenderer
    {
        /// <summary>
        /// 绘制文本
        /// </summary>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        /// <param name="text">文本内容</param>
        /// <param name="style">文本样式</param>
        void DrawText(float x, float y, string text, TextStyle style);

        /// <summary>
        /// 测量文本宽度
        /// </summary>
        /// <param name="text">文本内容</param>
        /// <param name="style">文本样式</param>
        /// <returns>文本宽度（像素）</returns>
        float MeasureTextWidth(string text, TextStyle style);

        /// <summary>
        /// 测量文本高度
        /// </summary>
        /// <param name="text">文本内容</param>
        /// <param name="style">文本样式</param>
        /// <returns>文本高度（像素）</returns>
        float MeasureTextHeight(string text, TextStyle style);

        /// <summary>
        /// 批量绘制字形
        /// 优化：使用批量绘制提高性能
        /// </summary>
        /// <param name="glyphs">字形信息数组</param>
        /// <param name="style">文本样式</param>
        void DrawGlyphs(GlyphInfo[] glyphs, TextStyle style);
    }


}
