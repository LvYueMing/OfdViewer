using OFDViewer.Render.DataModels;

namespace OFDViewer.Render.Abstractions
{
    /// <summary>
    /// 图形渲染器接口
    /// 封装直线、矩形、圆形、多边形、贝塞尔曲线等OFD图形渲染方法
    /// </summary>
    public interface IGraphicRenderer
    {
        /// <summary>
        /// 绘制直线
        /// </summary>
        /// <param name="x1">起点X坐标</param>
        /// <param name="y1">起点Y坐标</param>
        /// <param name="x2">终点X坐标</param>
        /// <param name="y2">终点Y坐标</param>
        /// <param name="style">图形样式</param>
        void DrawLine(float x1, float y1, float x2, float y2, GraphicStyle style);

        /// <summary>
        /// 绘制矩形
        /// </summary>
        /// <param name="x">左上角X坐标</param>
        /// <param name="y">左上角Y坐标</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="style">图形样式</param>
        void DrawRectangle(float x, float y, float width, float height, GraphicStyle style);

        /// <summary>
        /// 绘制圆形
        /// </summary>
        /// <param name="x">圆心X坐标</param>
        /// <param name="y">圆心Y坐标</param>
        /// <param name="radius">半径</param>
        /// <param name="style">图形样式</param>
        void DrawCircle(float x, float y, float radius, GraphicStyle style);

        /// <summary>
        /// 绘制椭圆
        /// </summary>
        /// <param name="x">左上角X坐标</param>
        /// <param name="y">左上角Y坐标</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="style">图形样式</param>
        void DrawEllipse(float x, float y, float width, float height, GraphicStyle style);

        /// <summary>
        /// 绘制多边形
        /// </summary>
        /// <param name="points">顶点坐标数组</param>
        /// <param name="style">图形样式</param>
        void DrawPolygon(float[] points, GraphicStyle style);
    }
}
