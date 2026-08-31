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
        /// <summary>
        /// 渲染目标位图
        /// </summary>
        private SKBitmap _bitmap;
        /// <summary>
        /// 渲染画布
        /// </summary>
        private SKCanvas _canvas;
        /// <summary>
        /// 渲染画笔
        /// </summary>

        /// <summary>
        /// 渲染配置
        /// </summary>
        private RenderConfig _config;
        /// <summary>
        /// 当前绘制路径
        /// </summary>
        private SKPath _currentPath;
        /// <summary>
        /// SKTypeface缓存，避免重复创建字体对象
        /// 缓存键：字体名称_字体粗细_斜体
        /// </summary>
        private readonly Dictionary<string, SKTypeface> _typefaceCache = new Dictionary<string, SKTypeface>();
        /// <summary>
        /// SKFont缓存，避免重复创建字体对象
        /// 缓存键：字体名称_字体粗细_斜体_字号_水平缩放
        /// </summary>
        private readonly Dictionary<string, SKFont> _fontCache = new Dictionary<string, SKFont>();
        /// <summary>
        /// Skia对象缓存锁，确保线程安全
        /// </summary>
        private readonly object _skiaCacheLock = new object();
        /// <summary>
        /// 缺字形回退字体缓存，避免重复调用 SKFontManager.MatchCharacter
        /// 缓存键：字符码位
        /// </summary>
        private readonly Dictionary<int, SKTypeface> _fallbackTypefaceCache = new Dictionary<int, SKTypeface>();
        /// <summary>
        /// 可重用的SKPaint对象，避免频繁创建和销毁
        /// </summary>
        private readonly SKPaint _reusablePaint = new SKPaint();
        /// <summary>
        /// 可重用SKPaint的锁
        /// </summary>
        private readonly object _reusablePaintLock = new object();
        /// <summary>
        /// 毫米到像素的转换因子（只读）
        /// 预计算的转换因子，避免重复计算除法运算
        /// </summary>
        public float MmToPixel { get; private set; }

        /// <summary>
        /// 像素到毫米的转换因子（只读）
        /// 预计算的转换因子，避免重复计算除法运算
        /// </summary>
        public float PixelToMm { get; private set; }
        /// <summary>
        /// 资源管理器
        /// </summary>
        private IResourceManager _resourceManager;


        /// <summary>
        /// 是否已释放
        /// </summary>
        private bool _disposed;

        #endregion

        #region 单位转换与坐标适配

        /// <summary>
        /// 更新转换因子
        /// </summary>
        private void UpdateConversionFactors()
        {
            if (_config != null)
            {
                MmToPixel = _config.Dpi / 25.4f;
                PixelToMm = 25.4f / _config.Dpi;
            }
            else
            {
                MmToPixel = 96.0f / 25.4f; // 默认96 DPI
                PixelToMm = 25.4f / 96.0f;
            }
        }

        /// <summary>
        /// 将OFD毫米单位转换为SkiaSharp像素单位
        /// </summary>
        /// <param name="millimeters">OFD毫米值</param>
        /// <returns>像素值</returns>
        public float MillimetersToPixels(float millimeters)
        {
            return millimeters * MmToPixel;
        }

        /// <summary>
        /// 将SkiaSharp像素单位转换为OFD毫米单位
        /// </summary>
        /// <param name="pixels">像素值</param>
        /// <returns>OFD毫米值</returns>
        public float PixelsToMillimeters(float pixels)
        {
            return pixels * PixelToMm;
        }

        /// <summary>
        /// 转换OFD坐标到SkiaSharp坐标
        /// OFD和SkiaSharp都以左上角为原点，因此只需要进行单位转换
        /// </summary>
        /// <param name="x">OFD X坐标（毫米）</param>
        /// <param name="y">OFD Y坐标（毫米）</param>
        /// <returns>转换后的SkiaSharp坐标（像素）</returns>
        public SKPoint ConvertOfdToSkiaCoordinates(float x, float y)
        {
            return new SKPoint(MillimetersToPixels(x), MillimetersToPixels(y));
        }

        /// <summary>
        /// 转换OFD矩形到SkiaSharp矩形
        /// </summary>
        /// <param name="x">OFD X坐标（毫米）</param>
        /// <param name="y">OFD Y坐标（毫米）</param>
        /// <param name="width">OFD宽度（毫米）</param>
        /// <param name="height">OFD高度（毫米）</param>
        /// <returns>转换后的SkiaSharp矩形（像素）</returns>
        public SKRect ConvertOfdToSkiaRect(float x, float y, float width, float height)
        {
            float skX = MillimetersToPixels(x);
            float skY = MillimetersToPixels(y);
            float skWidth = MillimetersToPixels(width);
            float skHeight = MillimetersToPixels(height);
            return new SKRect(skX, skY, skX + skWidth, skY + skHeight);
        }

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
            set
            {
                _config = value;
                UpdateConversionFactors();
            }
        }

        /// <summary>
        /// 资源管理器
        /// </summary>
        public IResourceManager ResourceManager
        {
            get => _resourceManager;
            set => _resourceManager = value;
        }

        #endregion

        #region 构造函数与析构函数

        /// <summary>
        /// 构造函数
        /// </summary>
        public SkiaRenderContext()
        {
            _config = new RenderConfig();
            // 初始化转换因子
            UpdateConversionFactors();
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
                // 创建新的位图
                // 确保位图的颜色类型为Rgba8888，透明度为Premul
                // Premul:定义了透明度（Alpha）与 RGB 颜色值的计算关系，核心是 “预乘” 的概念
                // Skia 引擎（SKBitmap 是 Skia 库的封装）内部渲染时，默认使用预乘透明度计算，能避免颜色混合时的精度丢失，让渲染结果更准确、效率更高
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
        public void SetBackgroundColor(uint color)
        {
            if (_canvas == null) return;

            // 转换ARGB颜色到SkiaSharp颜色
            // 31-24位：Alpha (A)  |  23-16位：Red (R)  |  15-8位：Green (G)  |  7-0位：Blue (B)
            var skColor = new SKColor(
                (byte)((color >> 16) & 0xFF),   //red
                (byte)((color >> 8) & 0xFF),    //green
                (byte)(color & 0xFF),           //blue
                (byte)((color >> 24) & 0xFF)    //alpha
            );
            // 清空画布，使用指定背景色
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
        /// 应用矩阵变换（使用SKMatrix）
        /// </summary>
        /// <param name="matrix">变换矩阵</param>
        public void ConcatMatrix(SKMatrix matrix)
        {
            _canvas?.Concat(ref matrix);
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
            _config.Quality = quality;

            // 根据渲染质量设置抗锯齿
            lock (_reusablePaintLock)
            {
                switch (quality)
                {
                    case RenderQuality.Performance:
                        _reusablePaint.IsAntialias = false;
                        break;
                    case RenderQuality.HighQuality:
                        _reusablePaint.IsAntialias = true;
                        break;
                }
            }
        }

        /// <summary>
        /// 设置矩形裁剪区
        /// </summary>
        /// <param name="x">左上角X坐标</param>
        /// <param name="y">左上角Y坐标</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        public void SetClipRect(float x, float y, float width, float height)
        {
            if (_canvas == null) return;

            // 创建裁剪矩形
            var clipRect = new SKRect(x, y, x + width, y + height);

            // 设置裁剪区
            _canvas.ClipRect(clipRect, SKClipOperation.Intersect, true);
        }

        /// <summary>
        /// 重置裁剪区
        /// </summary>
        public void ResetClip()
        {
            if (_canvas == null) return;

            // 重置裁剪区为整个画布
            _canvas.ClipRect(new SKRect(0, 0, Width, Height), SKClipOperation.Difference, true);
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
                var result = ms.ToArray();
                // 调试：仅在调试环境下保存渲染结果到本地文件，查看图片质量
#if DEBUG_SAVE
                try
                {
                    string debugDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "OFD_Debug");
                    if (!Directory.Exists(debugDir))
                    { 
                        Directory.CreateDirectory(debugDir);
                    }

                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
                    string debugFilePath = Path.Combine(debugDir, $"ofd_render_{timestamp}.png");
                    File.WriteAllBytes(debugFilePath, result);

                    // 输出调试信息
                    System.Diagnostics.Debug.WriteLine($"渲染结果已保存到: {debugFilePath}");
                }
                catch (Exception ex)
                {
                    // 忽略保存错误，避免影响正常渲染
                    System.Diagnostics.Debug.WriteLine($"保存调试图片失败: {ex.Message}");
                }
#endif

                return result;
            }
        }

        /// <summary>
        /// 获取当前渲染位图的副本（用于离屏瓦片、图案填充等场景），调用方负责释放
        /// </summary>
        public SKBitmap CopyRenderBitmap()
        {
            return _bitmap?.Copy();
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
        public void DrawLine(float x1, float y1, float x2, float y2, GraphStyle style)
        {
            if (_canvas == null) return;

            using (var paint = CreateFillPaintFromGraphStyle(style))
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
        public void DrawRectangle(float x, float y, float width, float height, GraphStyle style)
        {
            if (_canvas == null) return;

            var rect = new SKRect(x, y, x + width, y + height);

            using (var paint = CreateFillPaintFromGraphStyle(style))
            {
                if (style.Fill)
                {
                    _canvas.DrawRect(rect, paint);
                }
                if (style.Stroke)
                {
                    using (var strokePaint = CreateStrokePaintFromGraphStyle(style))
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
        public void DrawCircle(float x, float y, float radius, GraphStyle style)
        {
            if (_canvas == null) return;

            using (var paint = CreateFillPaintFromGraphStyle(style))
            {
                if (style.Fill)
                {
                    _canvas.DrawCircle(x, y, radius, paint);
                }
                if (style.Stroke)
                {
                    using (var strokePaint = CreateStrokePaintFromGraphStyle(style))
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
        public void DrawEllipse(float x, float y, float width, float height, GraphStyle style)
        {
            if (_canvas == null) return;

            var rect = new SKRect(x, y, x + width, y + height);

            using (var paint = CreateFillPaintFromGraphStyle(style))
            {
                if (style.Fill)
                {
                    _canvas.DrawOval(rect, paint);
                }
                if (style.Stroke)
                {
                    using (var strokePaint = CreateStrokePaintFromGraphStyle(style))
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
        public void DrawPolygon(float[] points, GraphStyle style)
        {
            if (_canvas == null || points == null || points.Length < 6) return;

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

                using (var paint = CreateFillPaintFromGraphStyle(style))
                {
                    if (style.Fill)
                    {
                        _canvas.DrawPath(path, paint);
                    }
                    if (style.Stroke)
                    {
                        using (var strokePaint = CreateStrokePaintFromGraphStyle(style))
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
        /// 优化：使用缓存避免重复创建 SKTypeface 和 SKFont，重用 SKPaint 对象
        /// </summary>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        /// <param name="text">文本内容</param>
        /// <param name="style">文本样式</param>
        public void DrawText(float x, float y, string text, TextStyle style)
        {
            if (_canvas == null || string.IsNullOrEmpty(text)) return;

            // 创建缓存键
            string typefaceKey = $"{style.FontFamily}_{style.FontWeight}_{style.Italic}";
            string fontKey = $"{typefaceKey}_{style.FontSize}_{style.HScale}";

            SKTypeface skTypeface = null;
            SKFont skFont = null;

            // 检查缓存
            lock (_skiaCacheLock)
            {
                if (_typefaceCache.TryGetValue(typefaceKey, out var cachedTypeface))
                {
                    skTypeface = cachedTypeface;
                }

                if (_fontCache.TryGetValue(fontKey, out var cachedFont))
                {
                    skFont = cachedFont;
                }
            }

            // 缓存未命中，创建新的 SKTypeface
            if (skTypeface == null)
            {
                skTypeface = ResolvePrimaryTypeface(style, typefaceKey);
            }

            // 缓存未命中，创建新的 SKFont
            if (skFont == null)
            {
                skFont = new SKFont(skTypeface, style.FontSize);
                skFont.ScaleX = style.HScale;

                // 缓存 SKFont
                lock (_skiaCacheLock)
                {
                    if (!_fontCache.ContainsKey(fontKey))
                    {
                        _fontCache[fontKey] = skFont;
                    }
                }
            }

            // 重用 SKPaint 对象（避免频繁创建和销毁）
            lock (_reusablePaintLock)
            {
                _reusablePaint.IsAntialias = true; // 强制启用抗锯齿，提高文字清晰度
                _reusablePaint.Style = style.Stroke ? SKPaintStyle.StrokeAndFill : SKPaintStyle.Fill;
                _reusablePaint.StrokeWidth = style.StrokeWidth;
                _reusablePaint.Color = ConvertToSKColor(style.Color, style.Alpha);
                _reusablePaint.BlendMode = SKBlendMode.SrcOver; // 设置混合模式，确保文字颜色正确

                // 优化：设置字体渲染质量
                //_reusablePaint.TextEncoding = SKTextEncoding.Utf8;

                // 绘制文本：主字体完全覆盖字形时走原路径；缺字形时按字符回退分段绘制
                if (AllGlyphsAvailable(skTypeface, text))
                {
                    _canvas.DrawText(text, x, y, skFont, _reusablePaint);
                }
                else
                {
                    DrawTextWithGlyphFallback(x, y, text, style, skTypeface, skFont, _reusablePaint);
                }
            }
        }

        /// <summary>
        /// 解析文本绘制字体：主字体（嵌入文件或按字体族名匹配）无法覆盖文本全部字形时，
        /// 对缺字字符使用系统字体回退。用于字体资源只有 FontName 而无嵌入 FontFile、
        /// 或嵌入字体为子集导致缺字的场景（否则缺字字符渲染为方框）。
        /// </summary>
        /// <param name="text">待绘制的文本</param>
        /// <param name="style">文本样式</param>
        /// <returns>可用于绘制该文本的字体</returns>
        public SKTypeface ResolveTypefaceWithGlyphFallback(string text, TextStyle style)
        {
            var typefaceKey = $"{style.FontFamily}_{style.FontWeight}_{style.Italic}";
            var primary = ResolvePrimaryTypeface(style, typefaceKey);

            if (string.IsNullOrEmpty(text) || AllGlyphsAvailable(primary, text))
                return primary;

            foreach (var ch in text)
            {
                if (!primary.ContainsGlyph(ch))
                    return GetFallbackTypeface(ch);
            }

            return primary;
        }

        /// <summary>解析主字体：优先嵌入字体文件，其次按字体族名匹配系统字体，最后回退系统默认字体</summary>
        private SKTypeface ResolvePrimaryTypeface(TextStyle style, string typefaceKey)
        {
            SKTypeface skTypeface;

            //从style.FontResource 加载SKTypeface
            if (!string.IsNullOrEmpty(style.FontFilePath))
            {
                var fontByte = ResourceManager.GetResourceFile(style.FontFilePath);
                using (var stream = new MemoryStream(fontByte))
                {
                    skTypeface = SKTypeface.FromStream(stream);
                }
            }
            else
            {
                skTypeface = SKTypeface.FromFamilyName(
                    style.FontFamily,
                    (SKFontStyleWeight)style.FontWeight,
                    SKFontStyleWidth.Normal,
                    style.Italic ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright);
            }

            // 如果指定字体不存在，使用系统默认字体
            if (skTypeface == null)
            {
                skTypeface = SKTypeface.Default;
            }

            // 缓存 SKTypeface
            lock (_skiaCacheLock)
            {
                if (!_typefaceCache.ContainsKey(typefaceKey))
                {
                    _typefaceCache[typefaceKey] = skTypeface;
                }
            }

            return skTypeface;
        }

        /// <summary>检查主字体是否覆盖文本全部字形</summary>
        private static bool AllGlyphsAvailable(SKTypeface typeface, string text)
        {
            foreach (var ch in text)
            {
                if (!typeface.ContainsGlyph(ch))
                    return false;
            }
            return true;
        }

        /// <summary>获取指定字符的系统回退字体（按码位缓存）</summary>
        private SKTypeface GetFallbackTypeface(int codepoint)
        {
            lock (_skiaCacheLock)
            {
                if (_fallbackTypefaceCache.TryGetValue(codepoint, out var cached))
                    return cached;
            }

            var fallback = SKFontManager.Default.MatchCharacter(codepoint) ?? SKTypeface.Default;

            lock (_skiaCacheLock)
            {
                _fallbackTypefaceCache[codepoint] = fallback;
            }

            return fallback;
        }

        /// <summary>获取或创建回退字体的 SKFont（复用全局字体缓存）</summary>
        private SKFont GetOrCreateFallbackFont(SKTypeface typeface, TextStyle style)
        {
            string fontKey = $"fb_{typeface.FamilyName}_{style.FontSize}_{style.HScale}";

            lock (_skiaCacheLock)
            {
                if (_fontCache.TryGetValue(fontKey, out var cached))
                    return cached;
            }

            var font = new SKFont(typeface, style.FontSize) { ScaleX = style.HScale };

            lock (_skiaCacheLock)
            {
                if (!_fontCache.ContainsKey(fontKey))
                {
                    _fontCache[fontKey] = font;
                }
            }

            return font;
        }

        /// <summary>
        /// 缺字形分段绘制：相邻的同字体字符合并为一段依次绘制，
        /// 段间位置按各段字体的实际排印宽度推进，保证不同字体混排时不断行错位
        /// </summary>
        private void DrawTextWithGlyphFallback(float x, float y, string text, TextStyle style,
            SKTypeface primaryTypeface, SKFont primaryFont, SKPaint paint)
        {
            float cursor = x;
            int index = 0;

            while (index < text.Length)
            {
                bool usePrimary = primaryTypeface.ContainsGlyph(text[index]);
                var segmentTypeface = usePrimary
                    ? primaryTypeface
                    : GetFallbackTypeface(text[index]);

                // 找到可由同一字体绘制的连续字符段
                int end = index + 1;
                while (end < text.Length)
                {
                    bool segmentable = usePrimary
                        ? primaryTypeface.ContainsGlyph(text[end])
                        : GetFallbackTypeface(text[end]) == segmentTypeface;
                    if (!segmentable)
                        break;
                    end++;
                }

                var segment = text.Substring(index, end - index);
                var font = usePrimary ? primaryFont : GetOrCreateFallbackFont(segmentTypeface, style);
                _canvas.DrawText(segment, cursor, y, font, paint);
                cursor += font.MeasureText(segment);
                index = end;
            }
        }

        /// <summary>
        /// 批量绘制字形
        /// 优化：使用批量绘制提高性能
        /// </summary>
        /// <param name="glyphs">字形信息数组</param>
        /// <param name="style">文本样式</param>
        public void DrawGlyphs(GlyphInfo[] glyphs, TextStyle style)
        {
            if (_canvas == null || glyphs == null || glyphs.Length == 0) return;

            // 创建缓存键
            string typefaceKey = $"{style.FontFamily}_{style.FontWeight}_{style.Italic}";
            string fontKey = $"{typefaceKey}_{style.FontSize}_{style.HScale}";

            SKTypeface skTypeface = null;
            SKFont skFont = null;

            // 检查缓存
            lock (_skiaCacheLock)
            {
                if (_typefaceCache.TryGetValue(typefaceKey, out var cachedTypeface))
                {
                    skTypeface = cachedTypeface;
                }

                if (_fontCache.TryGetValue(fontKey, out var cachedFont))
                {
                    skFont = cachedFont;
                }
            }

            // 缓存未命中，创建新的 SKTypeface
            if (skTypeface == null)
            {
                skTypeface = ResolvePrimaryTypeface(style, typefaceKey);
            }

            // 缓存未命中，创建新的 SKFont
            if (skFont == null)
            {
                skFont = new SKFont(skTypeface, style.FontSize);
                skFont.ScaleX = style.HScale;

                // 缓存 SKFont
                lock (_skiaCacheLock)
                {
                    if (!_fontCache.ContainsKey(fontKey))
                    {
                        _fontCache[fontKey] = skFont;
                    }
                }
            }

            // 重用 SKPaint 对象（避免频繁创建和销毁）
            lock (_reusablePaintLock)
            {
                _reusablePaint.IsAntialias = _config.AntiAlias;
                _reusablePaint.Style = style.Stroke ? SKPaintStyle.StrokeAndFill : SKPaintStyle.Fill;
                _reusablePaint.StrokeWidth = style.StrokeWidth;
                _reusablePaint.Color = ConvertToSKColor(style.Color, style.Alpha);

                List<ushort> allGlyphs = new List<ushort>();
                List<SKPoint> allPositions = new List<SKPoint>();

                // 设置每个字形的位置
                foreach (var glyph in glyphs)
                {
                    allGlyphs.Add(ushort.Parse(glyph.Glyph));
                    allPositions.Add(new SKPoint(glyph.X, glyph.Y));
                }

                using var builder = new SKTextBlobBuilder();

                var run = builder.AllocatePositionedRun(skFont, glyphs.Length);
                run.SetGlyphs(allGlyphs.ToArray());
                run.SetPositions(allPositions.ToArray());

                using var positionedTextBlob = builder.Build();

                _canvas.DrawText(positionedTextBlob, 0, 0, _reusablePaint);
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
            if (_canvas == null || imageData == null || imageData.Length == 0) return;
            if (imageData.Length > _config.MaxEncodedImageBytes) return;

            DrawEncodedImage(x, y, width, height, imageData, style);
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
            if (_canvas == null || stream == null) return;

            try
            {
                byte[]? encodedImage = ReadEncodedImageWithinLimit(stream);
                if (encodedImage == null) return;

                DrawEncodedImage(x, y, width, height, encodedImage, style);
            }
            catch (IOException)
            {
                // 损坏或不可读的图像资源按“跳过当前图像”处理，不影响其余页面对象。
            }
        }

        private void DrawEncodedImage(
            float x,
            float y,
            float width,
            float height,
            byte[] encodedImage,
            ImageStyle style)
        {
            using var imageData = SKData.CreateCopy(encodedImage);
            using var codec = SKCodec.Create(imageData);
            if (codec == null) return;

            long pixelCount = (long)codec.Info.Width * codec.Info.Height;
            if (codec.Info.Width <= 0 || codec.Info.Height <= 0 ||
                pixelCount > _config.MaxDecodedImagePixels)
            {
                return;
            }

            using var skImage = SKImage.FromEncodedData(imageData);
            if (skImage == null) return;

            lock (_reusablePaintLock)
            {
                _reusablePaint.IsAntialias = style.InterpolationMode != ImageInterpolationMode.LowQuality;
                _canvas?.DrawImage(skImage, new SKRect(x, y, x + width, y + height), _reusablePaint);
            }
        }

        private byte[]? ReadEncodedImageWithinLimit(Stream stream)
        {
            if (_config.MaxEncodedImageBytes <= 0 || _config.MaxDecodedImagePixels <= 0)
                return null;

            if (stream.CanSeek && stream.Length - stream.Position > _config.MaxEncodedImageBytes)
                return null;

            using var encodedImage = new MemoryStream();
            byte[] buffer = new byte[81920];
            int read;
            while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                if (read > _config.MaxEncodedImageBytes - encodedImage.Length)
                    return null;

                encodedImage.Write(buffer, 0, read);
            }

            return encodedImage.ToArray();
        }

        #endregion

        #region IPathRenderer实现

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
        /// 绘制圆弧
        /// </summary>
        /// <param name="rx">椭圆的长轴长度</param>
        /// <param name="ry">椭圆的短轴长度</param>
        /// <param name="angle">椭圆旋转角度（度）</param>
        /// <param name="largeArc">是否为大弧（>180度）</param>
        /// <param name="sweep">是否为顺时针方向</param>
        /// <param name="x">终点X坐标</param>
        /// <param name="y">终点Y坐标</param>
        public void ArcTo(float rx, float ry, float angle, bool largeArc, bool sweep, float x, float y)
        {
            if (_currentPath == null) return;

            // SkiaSharp的ArcTo参数与OFD的A命令参数有所不同
            // 需要进行参数转换
            _currentPath.ArcTo(rx, ry, angle,
                largeArc ? SKPathArcSize.Large : SKPathArcSize.Small,
                sweep ? SKPathDirection.Clockwise : SKPathDirection.CounterClockwise,
                x, y);
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
        public void FillPath(GraphStyle style)
        {
            if (_canvas == null || _currentPath == null) return;

            using (var paint = CreateFillPaintFromGraphStyle(style))
            {
                _canvas.DrawPath(_currentPath, paint);
            }
        }

        /// <summary>
        /// 描边路径
        /// </summary>
        /// <param name="style">图形样式</param>
        public void StrokePath(GraphStyle style)
        {
            if (_canvas == null || _currentPath == null) return;

            using (var paint = CreateStrokePaintFromGraphStyle(style))
            {
                _canvas.DrawPath(_currentPath, paint);
            }
        }

        /// <summary>
        /// 填充并描边路径
        /// </summary>
        /// <param name="style">图形样式</param>
        public void FillAndStrokePath(GraphStyle style)
        {
            if (_canvas == null || _currentPath == null) return;

            // 填充和描边合并为一个绘制调用，提升性能
            using (var fillPaint = CreatePaintFromGraphStyle(style))
            {
                _canvas.DrawPath(_currentPath, fillPaint);
            }

        }

        /// <summary>
        /// 对当前路径进行归一化处理
        /// </summary>
        public void NormalizePath()
        {
            if (_currentPath == null)
                return;

            // 获取路径边界
            SKRect pathRect = new SKRect();
            _currentPath.GetBounds(out pathRect);

            // 检查边界是否有效
            if (pathRect.Width <= 0 || pathRect.Height <= 0)
                return;

            // 计算归一化矩阵
            float translateX = -pathRect.Left;
            float translateY = -pathRect.Top;
            float scaleX = 1.0f / pathRect.Width;
            float scaleY = 1.0f / pathRect.Height;
            SKMatrix normalizeMatrix = new SKMatrix
            {
                ScaleX = scaleX,
                ScaleY = scaleY,
                TransX = translateX * scaleX,
                TransY = translateY * scaleY,
                Persp0 = 0,
                Persp1 = 0,
                Persp2 = 1
            };

            // 应用归一化矩阵到路径
            _currentPath.Transform(normalizeMatrix);
        }

        #endregion

        #region 私有辅助方法

        /// <summary>
        /// 从GraphicStyle创建填充样式的SKPaint
        /// </summary>
        /// <param name="style">图形样式</param>
        /// <returns>SKPaint对象</returns>
        private SKPaint CreateFillPaintFromGraphStyle(GraphStyle style)
        {
            var paint = new SKPaint();
            paint.IsAntialias = _config.AntiAlias;
            paint.Style = SKPaintStyle.Fill;

            // 优先使用填充着色器（如底纹 Pattern 平铺填充），否则使用纯色
            if (style.FillShader != null)
            {
                paint.Shader = style.FillShader;
            }
            else
            {
                paint.Color = ConvertToSKColor(style.Color, style.Alpha);
            }

            // 设置渐变样式（如果有）
            // paint.Shader = CreateGradientShader(style);

            return paint;
        }

        /// <summary>
        /// 从GraphicStyle创建描边样式的SKPaint
        /// </summary>
        /// <param name="style">图形样式</param>
        /// <returns>SKPaint对象</returns>
        private SKPaint CreateStrokePaintFromGraphStyle(GraphStyle style)
        {
            var paint = new SKPaint();
            paint.IsAntialias = _config.AntiAlias;
            paint.Style = SKPaintStyle.Stroke;
            paint.StrokeWidth = style.StrokeWidth * this.MmToPixel;
            paint.Color = ConvertToSKColor(style.StrokeColor, style.StrokeAlpha);

            // 设置虚线样式
            if (style.DashPattern != null && style.DashPattern.Length > 0)
            {
                paint.PathEffect = SKPathEffect.CreateDash(style.DashPattern, 0);
            }

            return paint;
        }


        /// <summary>
        /// 从GraphicStyle创建描边和填充样式的SKPaint
        /// </summary>
        /// <param name="style">图形样式</param>
        /// <returns>SKPaint对象</returns>
        private SKPaint CreatePaintFromGraphStyle(GraphStyle style)
        {
            var paint = new SKPaint();
            paint.IsAntialias = _config.AntiAlias;
            paint.Style = SKPaintStyle.StrokeAndFill;
            paint.StrokeWidth = style.StrokeWidth * this.MmToPixel;
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

            return paint;
        }

        /// <summary>
        /// 将ARGB颜色转换为SKColor
        /// </summary>
        /// <param name="color">ARGB颜色值</param>
        /// <param name="alpha">透明度</param>
        /// <returns>SKColor对象</returns>
        private SKColor ConvertToSKColor(ColorARGB color, byte alpha)
        {
            // 使用传入的alpha值（如果alpha为255则使用颜色中的alpha）
            if (alpha == 255)
            {
                return new SKColor(color.R, color.G, color.B, color.A);
            }
            else
            {
                return new SKColor(color.R, color.G, color.B, alpha);
            }
        }

        #endregion

        #region 样式映射

        /// <summary>
        /// 创建渐变着色器
        /// </summary>
        /// <param name="style">图形样式</param>
        /// <returns>渐变着色器</returns>
        // private SKShader CreateGradientShader(GraphicStyle style)
        // {
        //     // 实现渐变样式转换逻辑
        //     // 这里可以根据GraphicStyle中的渐变配置创建SKGradientShader
        //     return null;
        // }

        /// <summary>
        /// 创建虚线效果
        /// </summary>
        /// <param name="dashPattern">虚线模式</param>
        /// <returns>虚线效果</returns>
        private SKPathEffect CreateDashEffect(float[] dashPattern)
        {
            if (dashPattern == null || dashPattern.Length == 0)
            {
                return null;
            }
            return SKPathEffect.CreateDash(dashPattern, 0);
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
                _canvas?.Dispose();
                _bitmap?.Dispose();
                _currentPath?.Dispose();
                _reusablePaint?.Dispose();

                // 清空字体缓存
                lock (_skiaCacheLock)
                {
                    if (_typefaceCache != null)
                    {
                        foreach (var typeface in _typefaceCache.Values)
                        {
                            typeface?.Dispose();
                        }
                        _typefaceCache.Clear();
                    }

                    if (_fontCache != null)
                    {
                        foreach (var font in _fontCache.Values)
                        {
                            font?.Dispose();
                        }
                        _fontCache.Clear();
                    }
                }
            }

            _disposed = true;
        }

        #endregion
    }
}
