using System;
using System.IO;
using SkiaSharp;
using OFDViewer.Render.Abstractions;
using OFDViewer.Render.DataModels;

namespace OFDViewer.Render.Implementation
{
    /// <summary>
    /// 基于SkiaSharp的渲染上下文实现
    /// 实现IRenderContext及所有辅助渲染接口
    /// </summary>
    public class SkiaRenderContext : IRenderContext, IGraphicRenderer, ITextRenderer, IImageRenderer, IPathRenderer
    {
        #region 私有字段

        private SKBitmap _bitmap;
        private SKCanvas _canvas;
        private SKPaint _paint;
        private bool _disposed;
        private RenderConfig _config;

        #endregion

        #region 属性

        /// <summary>
        /// 渲染宽度
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// 渲染高度
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// 渲染配置
        /// </summary>
        public RenderConfig Config
        {
            get => _config;
            set => _config = value;
        }

        #endregion

        #region 构造函数与析构函数

        /// <summary>
        /// 构造函数
        /// </summary>
        public SkiaRenderContext()
        {
            _config = new RenderConfig();
            _paint = new SKPaint();
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~SkiaRenderContext()
        {
            Dispose(false);
        }

        #endregion

        #region IRenderContext实现

        /// <summary>
        /// 初始化渲染上下文，绑定渲染目标
        /// </summary>
        /// <param name="width">渲染宽度（像素）</param>
        /// <param name="height">渲染高度（像素）</param>
        public void Initialize(int width, int height)
        {
            Width = width;
            Height = height;

            // 创建或重新创建位图
            if (_bitmap == null || _bitmap.Width != width || _bitmap.Height != height)
            {
                _bitmap?.Dispose();
                _bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
            }

            // 创建或重新创建画布
            _canvas?.Dispose();
            _canvas = new SKCanvas(_bitmap);

            // 设置初始渲染质量
            SetRenderQuality(_config.Quality);

            // 清空画布
            Reset();
        }

        /// <summary>
        /// 重置渲染上下文，清空画布并恢复初始状态
        /// </summary>
        public void Reset()
        {
            if (_canvas == null) return;

            // 清空画布，使用白色背景
            _canvas.Clear(SKColors.White);
        }

        /// <summary>
        /// 保存当前渲染状态
        /// </summary>
        public void SaveState()
        {
            _canvas?.Save();
        }

        /// <summary>
        /// 恢复之前保存的渲染状态
        /// </summary>
        public void RestoreState()
        {
            _canvas?.Restore();
        }

        /// <summary>
        /// 设置背景色
        /// </summary>
        /// <param name="color">背景色（ARGB格式）</param>
        public void SetBackgroundColor(int color)
        {
            if (_canvas == null) return;

            var skColor = new SKColor(
                (byte)((color >> 16) & 0xFF),
                (byte)((color >> 8) & 0xFF),
                (byte)(color & 0xFF),
                (byte)((color >> 24) & 0xFF)
            );

            _canvas.Clear(skColor);
        }

        /// <summary>
        /// 平移画布
        /// </summary>
        /// <param name="dx">X轴平移量</param>
        /// <param name="dy">Y轴平移量</param>
        public void Translate(float dx, float dy)
        {
            _canvas?.Translate(dx, dy);
        }

        /// <summary>
        /// 旋转画布
        /// </summary>
        /// <param name="angle">旋转角度（弧度）</param>
        public void Rotate(float angle)
        {
            _canvas?.RotateRadians(angle);
        }

        /// <summary>
        /// 缩放画布
        /// </summary>
        /// <param name="sx">X轴缩放因子</param>
        /// <param name="sy">Y轴缩放因子</param>
        public void Scale(float sx, float sy)
        {
            _canvas?.Scale(sx, sy);
        }

        /// <summary>
        /// 设置渲染质量
        /// </summary>
        /// <param name="quality">渲染质量</param>
        public void SetRenderQuality(RenderQuality quality)
        {
            if (_paint == null) return;

            _config.Quality = quality;

            // 根据渲染质量设置抗锯齿
            switch (quality)
            {
                case RenderQuality.Performance:
                    _paint.IsAntialias = false;
                    break;
                case RenderQuality.HighQuality:
                    _paint.IsAntialias = true;
                    break;
            }
        }

        /// <summary>
        /// 获取渲染结果（位图数据）
        /// </summary>
        /// <returns>位图数据（PNG格式）</returns>
        public byte[] GetRenderResult()
        {
            if (_bitmap == null) return null;

            using (var ms = new MemoryStream())
            {
                _bitmap.Encode(ms, SKEncodedImageFormat.Png, 100);
                return ms.ToArray();
            }
        }

        #endregion

        #region IDisposable实现

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing">是否手动释放</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                // 释放托管资源
                _paint?.Dispose();
                _canvas?.Dispose();
                _bitmap?.Dispose();
            }

            _disposed = true;
        }

        #endregion

        #region IGraphicRenderer实现

        /// <summary>
        /// 绘制直线
        /// </summary>
        /// <param name="x1">起点X坐标</param>
        /// <param name="y1">起点Y坐标</param>
        /// <param name="x2">终点X坐标</param>
        /// <param name="y2">终点Y坐标</param>
        /// <param name="style">图形样式</param>
        public void DrawLine(float x1, float y1, float x2, float y2, GraphicStyle style)
        {
            if (_canvas == null || _paint == null) return;

            using (var paint = CreatePaintFromGraphicStyle(style))
            {
                _canvas.DrawLine(x1, y1, x2, y2, paint);
            }
        }

        /// <summary>
        /// 绘制矩形
        /// </summary>
        /// <param name="x">左上角X坐标</param>
        /// <param name="y">左上角Y坐标</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="style">图形样式</param>
        public void DrawRectangle(float x, float y, float width, float height, GraphicStyle style)
        {
            if (_canvas == null || _paint == null) return;

            var rect = new SKRect(x, y, x + width, y + height);

            using (var paint = CreatePaintFromGraphicStyle(style))
            {
                if (style.Fill)
                {
                    _canvas.DrawRect(rect, paint);
                }
                if (style.Stroke)
                {
                    using (var strokePaint = CreateStrokePaintFromGraphicStyle(style))
                    {
                        _canvas.DrawRect(rect, strokePaint);
                    }
                }
            }
        }

        /// <summary>
        /// 绘制圆形
        /// </summary>
        /// <param name="x">圆心X坐标</param>
        /// <param name="y">圆心Y坐标</param>
        /// <param name="radius">半径</param>
        /// <param name="style">图形样式</param>
        public void DrawCircle(float x, float y, float radius, GraphicStyle style)
        {
            if (_canvas == null || _paint == null) return;

            using (var paint = CreatePaintFromGraphicStyle(style))
            {
                if (style.Fill)
                {
                    _canvas.DrawCircle(x, y, radius, paint);
                }
                if (style.Stroke)
                {
                    using (var strokePaint = CreateStrokePaintFromGraphicStyle(style))
                    {
                        _canvas.DrawCircle(x, y, radius, strokePaint);
                    }
                }
            }
        }

        /// <summary>
        /// 绘制椭圆
        /// </summary>
        /// <param name="x">左上角X坐标</param>
        /// <param name="y">左上角Y坐标</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="style">图形样式</param>
        public void DrawEllipse(float x, float y, float width, float height, GraphicStyle style)
        {
            if (_canvas == null || _paint == null) return;

            var rect = new SKRect(x, y, x + width, y + height);

            using (var paint = CreatePaintFromGraphicStyle(style))
            {
                if (style.Fill)
                {
                    _canvas.DrawOval(rect, paint);
                }
                if (style.Stroke)
                {
                    using (var strokePaint = CreateStrokePaintFromGraphicStyle(style))
                    {
                        _canvas.DrawOval(rect, strokePaint);
                    }
                }
            }
        }

        /// <summary>
        /// 绘制多边形
        /// </summary>
        /// <param name="points">顶点坐标数组（x1,y1,x2,y2,...）</param>
        /// <param name="style">图形样式</param>
        public void DrawPolygon(float[] points, GraphicStyle style)
        {
            if (_canvas == null || _paint == null || points == null || points.Length < 6) return;

            using (var path = new SKPath())
            {
                // 移动到第一个点
                path.MoveTo(points[0], points[1]);

                // 连接其他点
                for (int i = 2; i < points.Length; i += 2)
                {
                    path.LineTo(points[i], points[i + 1]);
                }

                // 闭合路径
                path.Close();

                using (var paint = CreatePaintFromGraphicStyle(style))
                {
                    if (style.Fill)
                    {
                        _canvas.DrawPath(path, paint);
                    }
                    if (style.Stroke)
                    {
                        using (var strokePaint = CreateStrokePaintFromGraphicStyle(style))
                        {
                            _canvas.DrawPath(path, strokePaint);
                        }
                    }
                }
            }
        }

        #endregion

        #region ITextRenderer实现

        /// <summary>
        /// 绘制文本
        /// </summary>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        /// <param name="text">文本内容</param>
        /// <param name="style">文本样式</param>
        public void DrawText(float x, float y, string text, TextStyle style)
        {
            if (_canvas == null || string.IsNullOrEmpty(text)) return;

            using (var paint = CreatePaintFromTextStyle(style))
            {
                // 创建字体
                var typeface = SKTypeface.FromFamilyName(style.FontFamily, 
                    (SKFontStyleWeight)style.FontWeight, 
                    SKFontStyleWidth.Normal, 
                    style.Italic ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright);
                
                var font = new SKFont(typeface, style.FontSize);
                font.ScaleX = style.HScale;
                
                // 绘制文本
                _canvas.DrawText(text, x, y, font, paint);
            }
        }

        /// <summary>
        /// 测量文本宽度
        /// </summary>
        /// <param name="text">文本内容</param>
        /// <param name="style">文本样式</param>
        /// <returns>文本宽度（像素）</returns>
        public float MeasureTextWidth(string text, TextStyle style)
        {
            if (string.IsNullOrEmpty(text)) return 0;

            // 创建字体
            var typeface = SKTypeface.FromFamilyName(style.FontFamily, 
                (SKFontStyleWeight)style.FontWeight, 
                SKFontStyleWidth.Normal, 
                style.Italic ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright);
            
            var font = new SKFont(typeface, style.FontSize);
            font.ScaleX = style.HScale;
            
            // 测量文本宽度
            return font.MeasureText(text);
        }

        /// <summary>
        /// 测量文本高度
        /// </summary>
        /// <param name="text">文本内容</param>
        /// <param name="style">文本样式</param>
        /// <returns>文本高度（像素）</returns>
        public float MeasureTextHeight(string text, TextStyle style)
        {
            if (string.IsNullOrEmpty(text)) return 0;

            // 创建字体
            var typeface = SKTypeface.FromFamilyName(style.FontFamily, 
                (SKFontStyleWeight)style.FontWeight, 
                SKFontStyleWidth.Normal, 
                style.Italic ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright);
            
            var font = new SKFont(typeface, style.FontSize);
            font.ScaleX = style.HScale;
            
            // 测量文本高度
            var fontMetrics = font.Metrics;
            return fontMetrics.Descent - fontMetrics.Ascent;
        }

        #endregion

        #region IImageRenderer实现

        /// <summary>
        /// 绘制图像
        /// </summary>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="imageData">图像数据</param>
        /// <param name="style">图像样式</param>
        public void DrawImage(float x, float y, float width, float height, byte[] imageData, ImageStyle style)
        {
            if (_canvas == null || _paint == null || imageData == null || imageData.Length == 0) return;

            using (var stream = new MemoryStream(imageData))
            {
                DrawImage(x, y, width, height, stream, style);
            }
        }

        /// <summary>
        /// 绘制图像
        /// </summary>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="stream">图像数据流</param>
        /// <param name="style">图像样式</param>
        public void DrawImage(float x, float y, float width, float height, Stream stream, ImageStyle style)
        {
            if (_canvas == null || _paint == null || stream == null) return;

            try
            {
                using (var skImage = SKImage.FromEncodedData(stream))
                {
                    if (skImage == null) return;

                    // 设置图像插值模式
                    _paint.IsAntialias = style.InterpolationMode != ImageInterpolationMode.LowQuality;

                    // 绘制图像（使用正确的API调用）
                    _canvas.DrawImage(skImage, new SKRect(x, y, x + width, y + height), _paint);
                }
            }
            catch (Exception ex)
            {
                // 忽略图像加载错误，避免影响整体渲染
            }
        }

        #endregion

        #region IPathRenderer实现

        private SKPath _currentPath;

        /// <summary>
        /// 开始绘制路径
        /// </summary>
        public void BeginPath()
        {
            _currentPath?.Dispose();
            _currentPath = new SKPath();
        }

        /// <summary>
        /// 移动到指定点
        /// </summary>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        public void MoveTo(float x, float y)
        {
            _currentPath?.MoveTo(x, y);
        }

        /// <summary>
        /// 绘制直线到指定点
        /// </summary>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        public void LineTo(float x, float y)
        {
            _currentPath?.LineTo(x, y);
        }

        /// <summary>
        /// 绘制贝塞尔曲线
        /// </summary>
        /// <param name="cp1x">控制点1 X坐标</param>
        /// <param name="cp1y">控制点1 Y坐标</param>
        /// <param name="cp2x">控制点2 X坐标</param>
        /// <param name="cp2y">控制点2 Y坐标</param>
        /// <param name="x">终点X坐标</param>
        /// <param name="y">终点Y坐标</param>
        public void CubicTo(float cp1x, float cp1y, float cp2x, float cp2y, float x, float y)
        {
            _currentPath?.CubicTo(cp1x, cp1y, cp2x, cp2y, x, y);
        }

        /// <summary>
        /// 绘制二次贝塞尔曲线
        /// </summary>
        /// <param name="cpx">控制点X坐标</param>
        /// <param name="cpy">控制点Y坐标</param>
        /// <param name="x">终点X坐标</param>
        /// <param name="y">终点Y坐标</param>
        public void QuadTo(float cpx, float cpy, float x, float y)
        {
            _currentPath?.QuadTo(cpx, cpy, x, y);
        }

        /// <summary>
        /// 闭合路径
        /// </summary>
        public void ClosePath()
        {
            _currentPath?.Close();
        }

        /// <summary>
        /// 填充路径
        /// </summary>
        /// <param name="style">图形样式</param>
        public void FillPath(GraphicStyle style)
        {
            if (_canvas == null || _paint == null || _currentPath == null) return;

            using (var paint = CreatePaintFromGraphicStyle(style))
            {
                _canvas.DrawPath(_currentPath, paint);
            }
        }

        /// <summary>
        /// 描边路径
        /// </summary>
        /// <param name="style">图形样式</param>
        public void StrokePath(GraphicStyle style)
        {
            if (_canvas == null || _paint == null || _currentPath == null) return;

            using (var paint = CreateStrokePaintFromGraphicStyle(style))
            {
                _canvas.DrawPath(_currentPath, paint);
            }
        }

        /// <summary>
        /// 填充并描边路径
        /// </summary>
        /// <param name="style">图形样式</param>
        public void FillAndStrokePath(GraphicStyle style)
        {
            if (_canvas == null || _paint == null || _currentPath == null) return;

            // 先填充
            using (var fillPaint = CreatePaintFromGraphicStyle(style))
            {
                _canvas.DrawPath(_currentPath, fillPaint);
            }

            // 再描边
            using (var strokePaint = CreateStrokePaintFromGraphicStyle(style))
            {
                _canvas.DrawPath(_currentPath, strokePaint);
            }
        }

        #endregion

        #region 私有辅助方法

        /// <summary>
        /// 从GraphicStyle创建填充样式的SKPaint
        /// </summary>
        /// <param name="style">图形样式</param>
        /// <returns>SKPaint对象</returns>
        private SKPaint CreatePaintFromGraphicStyle(GraphicStyle style)
        {
            var paint = new SKPaint();
            paint.IsAntialias = _config.AntiAlias;
            paint.Style = SKPaintStyle.Fill;
            paint.Color = ConvertToSKColor(style.Color, style.Alpha);
            return paint;
        }

        /// <summary>
        /// 从GraphicStyle创建描边样式的SKPaint
        /// </summary>
        /// <param name="style">图形样式</param>
        /// <returns>SKPaint对象</returns>
        private SKPaint CreateStrokePaintFromGraphicStyle(GraphicStyle style)
        {
            var paint = new SKPaint();
            paint.IsAntialias = _config.AntiAlias;
            paint.Style = SKPaintStyle.Stroke;
            paint.StrokeWidth = style.StrokeWidth;
            paint.Color = ConvertToSKColor(style.StrokeColor, style.StrokeAlpha);
            
            // 设置虚线样式
            if (style.DashPattern != null && style.DashPattern.Length > 0)
            {
                paint.PathEffect = SKPathEffect.CreateDash(style.DashPattern, 0);
            }
            
            return paint;
        }

        /// <summary>
        /// 从TextStyle创建SKPaint
        /// </summary>
        /// <param name="style">文本样式</param>
        /// <returns>SKPaint对象</returns>
        private SKPaint CreatePaintFromTextStyle(TextStyle style)
        {
            var paint = new SKPaint();
            paint.IsAntialias = _config.AntiAlias;
            paint.Style = SKPaintStyle.Fill;
            paint.Color = ConvertToSKColor(style.Color, style.Alpha);
            
            // 创建字体
            var typeface = SKTypeface.FromFamilyName(style.FontFamily, 
                (SKFontStyleWeight)style.FontWeight, 
                SKFontStyleWidth.Normal, 
                style.Italic ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright);
            
            var font = new SKFont(typeface, style.FontSize);
            font.ScaleX = style.HScale;
            
            // 设置下划线和删除线
            paint.PathEffect = SKPathEffect.CreateDash(null, 0);
            
            return paint;
        }

        /// <summary>
        /// 将ARGB颜色转换为SKColor
        /// </summary>
        /// <param name="color">ARGB颜色值</param>
        /// <param name="alpha">透明度</param>
        /// <returns>SKColor对象</returns>
        private SKColor ConvertToSKColor(uint color, byte alpha)
        {
            byte a = (byte)((color >> 24) & 0xFF);
            byte r = (byte)((color >> 16) & 0xFF);
            byte g = (byte)((color >> 8) & 0xFF);
            byte b = (byte)(color & 0xFF);
            
            // 使用style.Alpha覆盖颜色中的alpha通道
            return new SKColor(r, g, b, alpha);
        }

        #endregion
    }
}
