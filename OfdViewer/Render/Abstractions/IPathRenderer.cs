using OFDViewer.Render.DataModels;

namespace OFDViewer.Render.Abstractions
{
    /// <summary>
    /// 路径渲染器接口
    /// 封装OFD路径对象、描边样式（宽度/颜色/虚线）、填充样式渲染
    /// </summary>
    public interface IPathRenderer
    {
        /// <summary>
        /// 开始绘制路径
        /// </summary>
        void BeginPath();

        /// <summary>
        /// 移动到指定点
        /// </summary>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        void MoveTo(float x, float y);

        /// <summary>
        /// 绘制直线到指定点
        /// </summary>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        void LineTo(float x, float y);

        /// <summary>
        /// 绘制贝塞尔曲线
        /// </summary>
        /// <param name="cp1x">控制点1 X坐标</param>
        /// <param name="cp1y">控制点1 Y坐标</param>
        /// <param name="cp2x">控制点2 X坐标</param>
        /// <param name="cp2y">控制点2 Y坐标</param>
        /// <param name="x">终点X坐标</param>
        /// <param name="y">终点Y坐标</param>
        void CubicTo(float cp1x, float cp1y, float cp2x, float cp2y, float x, float y);

        /// <summary>
        /// 绘制二次贝塞尔曲线
        /// </summary>
        /// <param name="cpx">控制点X坐标</param>
        /// <param name="cpy">控制点Y坐标</param>
        /// <param name="x">终点X坐标</param>
        /// <param name="y">终点Y坐标</param>
        void QuadTo(float cpx, float cpy, float x, float y);

        /// <summary>
        /// 闭合路径
        /// </summary>
        void ClosePath();

        /// <summary>
        /// 填充路径
        /// </summary>
        /// <param name="style">图形样式</param>
        void FillPath(GraphicStyle style);

        /// <summary>
        /// 描边路径
        /// </summary>
        /// <param name="style">图形样式</param>
        void StrokePath(GraphicStyle style);

        /// <summary>
        /// 填充并描边路径
        /// </summary>
        /// <param name="style">图形样式</param>
        void FillAndStrokePath(GraphicStyle style);
    }
}
