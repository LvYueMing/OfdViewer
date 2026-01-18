using System.IO;
using OFDViewer.Render.DataModels;

namespace OFDViewer.Render.Abstractions
{
    /// <summary>
    /// 图像渲染器接口
    /// 封装OFD内嵌图片、表单资源、印章等渲染方法
    /// </summary>
    public interface IImageRenderer
    {
        /// <summary>
        /// 绘制图像
        /// </summary>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="imageData">图像数据</param>
        /// <param name="style">图像样式</param>
        void DrawImage(float x, float y, float width, float height, byte[] imageData, ImageStyle style);

        /// <summary>
        /// 绘制图像
        /// </summary>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="stream">图像数据流</param>
        /// <param name="style">图像样式</param>
        void DrawImage(float x, float y, float width, float height, Stream stream, ImageStyle style);
    }
}
