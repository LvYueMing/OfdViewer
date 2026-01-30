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
        /// 绘制圆弧
        /// </summary>
        /// <param name="rx">椭圆的长轴长度</param>
        /// <param name="ry">椭圆的短轴长度</param>
        /// <param name="angle">椭圆旋转角度（度）</param>
        /// <param name="largeArc">是否为大弧（>180度）</param>
        /// <param name="sweep">是否为顺时针方向</param>
        /// <param name="x">终点X坐标</param>
        /// <param name="y">终点Y坐标</param>
        void ArcTo(float rx, float ry, float angle, bool largeArc, bool sweep, float x, float y);

        /// <summary>
        /// 闭合路径
        /// </summary>
        void ClosePath();

        /// <summary>
        /// 填充路径
        /// </summary>
        /// <param name="style">图形样式</param>
        void FillPath(GraphStyle style);

        /// <summary>
        /// 描边路径
        /// </summary>
        /// <param name="style">图形样式</param>
        void StrokePath(GraphStyle style);

        /// <summary>
        /// 填充并描边路径
        /// </summary>
        /// <param name="style">图形样式</param>
        void FillAndStrokePath(GraphStyle style);

        /// <summary>
        /// 对当前路径进行归一化处理
        /// </summary>
        void NormalizePath();
    }
}
