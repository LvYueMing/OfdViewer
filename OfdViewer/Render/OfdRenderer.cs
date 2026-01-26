using System;
using System.IO;
using System.Collections.Generic;
using OFDViewer.Parse;
using OFDViewer.Render.Abstractions;
using OFDViewer.Render.DataModels;
using OFDViewer.Render.Implementation;
using OFDViewer.Models.BaseStructure.Pages;
using OFDViewer.Models.BaseStructure.Pages.PageBlockItems;
using OFDViewer.Models.BaseStructure.Resources.ResItems;
using OFDViewer.Models.Font;

namespace OFDViewer.Render
{
    /// <summary>
    /// OFD文档渲染服务
    /// 负责协调OFD文档的解析、渲染和结果管理
    /// </summary>
    public class OfdRenderer : IDisposable
    {
        #region 私有字段
        
        /// <summary>
        /// OFD读取器，负责解析OFD文件
        /// </summary>
        private readonly OFDReader _ofdReader;
        
        /// <summary>
        /// 渲染配置
        /// </summary>
        private readonly RenderConfig _renderConfig;
        
        /// <summary>
        /// 文本样式缓存，避免重复创建 TextStyle 对象
        /// 缓存键：字体ID_字号_字体粗细_斜体_水平缩放
        /// </summary>
        private readonly Dictionary<string, TextStyle> _textStyleCache = new Dictionary<string, TextStyle>();
        
        /// <summary>
        /// 样式缓存锁，确保线程安全的缓存操作
        /// </summary>
        private readonly object _styleCacheLock = new object();
        
        /// <summary>
        /// 释放状态标志，防止重复释放资源
        /// </summary>
        private bool _disposed = false;
        

        #endregion

        #region 属性
        
        /// <summary>
        /// 当前OFD文档
        /// </summary>
        public OFDRootDocument RootDocument { get; private set; }
        
        /// <summary>
        /// 文档总页数
        /// </summary>
        public int PageCount { get; private set; }
        
        /// <summary>
        /// 获取指定文档的页数
        /// </summary>
        /// <param name="docIndex">文档索引</param>
        /// <returns>指定文档的页数</returns>
        public int GetDocumentPageCount(int docIndex)
        {
            CheckDisposed();
            
            if (RootDocument == null || RootDocument.Docs == null || RootDocument.Docs.Count == 0)
                throw new InvalidOperationException("OFD文档未加载或为空");
            
            // 验证文档索引
            if (docIndex < 0 || docIndex >= RootDocument.Docs.Count)
                throw new ArgumentOutOfRangeException(nameof(docIndex), "文档索引超出范围");
            
            return RootDocument.Docs[docIndex].PageDocs?.Count ?? 0;
        }
        
        /// <summary>
        /// 获取文档数量
        /// </summary>
        public int DocumentCount
        {
            get
            {
                if (RootDocument == null || RootDocument.Docs == null)
                    return 0;
                return RootDocument.Docs.Count;
            }
        }
        
        #endregion

        #region 构造函数
        
        /// <summary>
        /// 从文件路径初始化OFD渲染器
        /// </summary>
        /// <param name="filePath">OFD文件路径</param>
        public OfdRenderer(string filePath)
            : this(filePath, new RenderConfig())
        {
        }
        
        /// <summary>
        /// 从文件路径初始化OFD渲染器，并指定渲染配置
        /// </summary>
        /// <param name="filePath">OFD文件路径</param>
        /// <param name="renderConfig">渲染配置</param>
        public OfdRenderer(string filePath, RenderConfig renderConfig)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath), "OFD文件路径不能为空");
            
            if (!File.Exists(filePath))
                throw new FileNotFoundException("指定的OFD文件不存在", filePath);
            
            _renderConfig = renderConfig ?? new RenderConfig();
            _ofdReader = new OFDReader(filePath);
            
            // 读取文档并计算页数
            LoadDocument();
        }
        
        /// <summary>
        /// 从流初始化OFD渲染器
        /// </summary>
        /// <param name="stream">OFD文件流</param>
        public OfdRenderer(Stream stream)
            : this(stream, new RenderConfig())
        {
        }
        
        /// <summary>
        /// 从流初始化OFD渲染器，并指定渲染配置
        /// </summary>
        /// <param name="stream">OFD文件流</param>
        /// <param name="renderConfig">渲染配置</param>
        public OfdRenderer(Stream stream, RenderConfig renderConfig)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream), "OFD文件流不能为空");
            
            if (!stream.CanRead)
                throw new ArgumentException("OFD文件流不支持读取操作", nameof(stream));
            
            _renderConfig = renderConfig ?? new RenderConfig();
            _ofdReader = new OFDReader(stream, true);
            
            // 读取文档并计算页数
            LoadDocument();
        }
        
        #endregion

        #region 核心方法
        
        /// <summary>
        /// 加载OFD文档
        /// </summary>
        private void LoadDocument()
        {
            RootDocument = _ofdReader.ParseOFDDocument();
            PageCount = CalculatePageCount();
        }
        
        /// <summary>
        /// 计算所有文档的总页数
        /// </summary>
        /// <returns>总页数</returns>
        private int CalculatePageCount()
        {
            if (RootDocument == null || RootDocument.Docs == null || RootDocument.Docs.Count == 0)
                return 0;
            
            int totalPages = 0;
            foreach (var doc in RootDocument.Docs)
            {
                totalPages += doc.PageDocs?.Count ?? 0;
            }
            return totalPages;
        }
        
        /// <summary>
        /// 渲染指定文档中的指定页面到内存位图
        /// </summary>
        /// <param name="docIndex">文档索引</param>
        /// <param name="pageIndex">页面索引</param>
        /// <returns>渲染结果（PNG格式字节数组）</returns>
        public byte[] RenderPageToBitmap(int docIndex, int pageIndex)
        {
            CheckDisposed();
            
            if (RootDocument == null || RootDocument.Docs == null || RootDocument.Docs.Count == 0)
                throw new InvalidOperationException("OFD文档未加载或为空");
            
            // 验证文档索引
            if (docIndex < 0 || docIndex >= RootDocument.Docs.Count)
                throw new ArgumentOutOfRangeException(nameof(docIndex), "文档索引超出范围");
            
            var ofdDoc = RootDocument.Docs[docIndex];
            if (ofdDoc == null || ofdDoc.Document == null)
                throw new InvalidOperationException("OFD文档结构无效");
            
            // 验证页面索引
            int docPageCount = ofdDoc.PageDocs?.Count ?? 0;
            if (pageIndex < 0 || pageIndex >= docPageCount)
                throw new ArgumentOutOfRangeException(nameof(pageIndex), "页面索引超出当前文档的范围");
            
            // 获取页面尺寸（OFD标准单位：毫米）
            var pageWidth = (float)ofdDoc.Document.CommonData.PageArea.PhysicalBox.Width;
            var pageHeight = (float)ofdDoc.Document.CommonData.PageArea.PhysicalBox.Height;
            
            // 计算渲染尺寸（像素）
            // 创建渲染上下文
            using var renderContext = new SkiaRenderContext();
            renderContext.Config = _renderConfig;
            
            // 使用渲染上下文的单位转换方法（利用预计算的转换因子）
            int renderWidth = (int)renderContext.MillimetersToPixels(pageWidth);
            int renderHeight = (int)renderContext.MillimetersToPixels(pageHeight);
            
            // 初始化渲染上下文
            renderContext.Initialize(renderWidth, renderHeight);

            // 设置资源管理器
            renderContext.ResourceManager = new ResourceManager(ofdDoc, pageIndex);

            // 设置背景色为白色
            renderContext.SetBackgroundColor(0xFFFFFFFF);
            
            // 渲染页面内容
            if (ofdDoc.PageDocs != null && ofdDoc.PageDocs.Count > pageIndex)
            {
                var pageDoc = ofdDoc.PageDocs[pageIndex];
                if (pageDoc != null && pageDoc.Page != null)
                {
                    // 渲染页面内容
                    RenderPageContent(renderContext, pageDoc.Page);
                }
            }
            
            // 返回渲染结果
            return renderContext.GetRenderResult();
        }


        
        /// <summary>
        /// 渲染指定页面到内存位图（按全局页面索引）
        /// </summary>
        /// <param name="pageIndex">全局页面索引，默认为第1页</param>
        /// <returns>渲染结果（PNG格式字节数组）</returns>
        public byte[] RenderPageToBitmap(int pageIndex = 0)
        {
            CheckDisposed();
            
            if (RootDocument == null || RootDocument.Docs == null || RootDocument.Docs.Count == 0)
                throw new InvalidOperationException("OFD文档未加载或为空");
            
            // 验证全局页面索引
            if (pageIndex < 0 || pageIndex >= PageCount)
                throw new ArgumentOutOfRangeException(nameof(pageIndex), "页面索引超出范围");
            
            // 查找页面所在的文档和文档内的页面索引
            int cumulativePages = 0;
            int targetDocIndex = 0;
            int targetPageIndex = pageIndex;
            
            foreach (var doc in RootDocument.Docs)
            {
                int docPageCount = doc.PageDocs?.Count ?? 0;
                if (pageIndex < cumulativePages + docPageCount)
                {
                    targetPageIndex = pageIndex - cumulativePages;
                    break;
                }
                cumulativePages += docPageCount;
                targetDocIndex++;
            }
            
            return RenderPageToBitmap(targetDocIndex, targetPageIndex);
        }
        
        /// <summary>
        /// 渲染页面内容
        /// 遍历页面元素并调用渲染上下文的绘制方法
        /// </summary>
        /// <param name="renderContext">渲染上下文</param>
        /// <param name="page">页面对象</param>
        /// <param name="pageIndex">页面索引</param>
        private void RenderPageContent(IRenderContext renderContext, Page page)
        {
            if (page == null || page.Content == null)
                return;

            // 遍历所有图层
            // 渲染图层顺序：背景层、正文层、前景层，按照出现的先后顺序依次进行渲染
            foreach (var layer in page.Content)
            {
                if (layer == null || layer.PageBlockItems == null)
                    continue;

                // 遍历图层中的所有页面块 
                foreach (var blockItem in layer.PageBlockItems)
                {
                    // 根据页面块类型调用相应的渲染方法
                    // 注意：这里需要根据实际的页面块类型进行扩展
                    // 当前仅作为框架示例
                    RenderPageBlock(renderContext, blockItem);
                }
            }
        }
        
        /// <summary>
        /// 渲染页面块
        /// 根据页面块类型调用相应的渲染方法
        /// </summary>
        /// <param name="renderContext">渲染上下文</param>
        /// <param name="blockItem">页面块对象</param>
        /// <param name="pageIndex">页面索引</param>
        private void RenderPageBlock(IRenderContext renderContext, object blockItem)
        {
            if (renderContext == null || blockItem == null)
                return;

            // 检查渲染上下文是否实现了相应的渲染接口
            var graphicRenderer = renderContext as IGraphicRenderer;           

            // 根据页面块类型调用相应的渲染方法
            switch (blockItem)
            {
                case Models.BaseStructure.Pages.PageBlockItems.TextObject textObj:
                    {
                        var textRenderer = renderContext as ITextRenderer;
                        RenderTextObject(textRenderer, textObj);
                        break;
                    }
                case Models.BaseStructure.Pages.PageBlockItems.PathObject pathObj:
                    {
                        var pathRenderer = renderContext as IPathRenderer;
                        RenderPathObject(pathRenderer, pathObj);
                        break;
                    }

                case Models.BaseStructure.Pages.PageBlockItems.ImageObject imageObj:
                    {
                        var imageRenderer = renderContext as IImageRenderer;
                        RenderImageObject(imageRenderer, imageObj);
                        break;
                    }

                case Models.BaseStructure.Pages.PageBlockItems.CompositeObject compositeObj:
                    {
                        RenderCompositeObject(renderContext, compositeObj);
                        break;
                    }

                case Models.BaseStructure.Pages.PageBlockItems.PageBlock pageBlock:
                    {
                        RenderPageBlockObject(renderContext, pageBlock);
                        break;
                    }
                default:
                    // 未知类型，跳过
                    break;
            }
        }
        
        /// <summary>
        /// 渲染文本对象
        /// </summary>
        /// <param name="textRenderer">文本渲染器</param>
        /// <param name="textObject">文本对象</param>
        private void RenderTextObject(ITextRenderer textRenderer, TextObject textObject)
        {
            if (textRenderer == null || textObject == null)
                return;
            
            // 获取渲染上下文
            var renderContext = textRenderer as IRenderContext;
            if (renderContext == null)
                return;
            
            // 获取图元外接矩形位置（页面坐标系，毫米转换为像素）
            float boundaryX = renderContext.MillimetersToPixels((float)textObject.Boundary.X);
            float boundaryY = renderContext.MillimetersToPixels((float)textObject.Boundary.Y);
            float boundaryWidth = renderContext.MillimetersToPixels((float)textObject.Boundary.Width);
            float boundaryHeight = renderContext.MillimetersToPixels((float)textObject.Boundary.Height);
            
            // 保存当前渲染状态
            renderContext?.SaveState();
            
            // 设置裁剪区（使用图元的外接矩形作为默认裁剪区）
            renderContext?.SetClipRect(boundaryX, boundaryY, boundaryWidth, boundaryHeight);

            // 转换文本样式
            var textStyle = ConvertToTextStyle(textObject, renderContext);
            
            // 判断处理情况：
            // 1. 一个TextObject有一个或多个TextCode，但没有CGTransform - 直接按照多个TextCode处理
            // 2. 一个TextObject有一个TextCode，有一个或多个CGTransform - 使用CGTransform处理
            // 3. 一个TextObject有多个TextCode，并且有一个或多个CGTransform - 不处理CGTransform，直接按照多个TextCode处理
            bool hasCGTransforms = textObject.CGTransforms != null && textObject.CGTransforms.Count > 0;
            bool hasMultipleTextCodes = textObject.TextCodes != null && textObject.TextCodes.Count > 1;
            
            // 情况2：只有一个TextCode且有CGTransform，使用CGTransform处理
            if (!hasMultipleTextCodes && textObject.TextCodes.Count == 1 && hasCGTransforms )
            {
                var textCode = textObject.TextCodes[0];
                if (!string.IsNullOrEmpty(textCode.Text))
                {
                    RenderSingleTextCodeWithCGTransforms(textRenderer, textObject, textCode, boundaryX, boundaryY, textStyle, renderContext);
                }
            }
            else
            {
                // 情况1和3：按照多个TextCode处理，不使用CGTransform
                foreach (var textCode in textObject.TextCodes)
                {
                    if (string.IsNullOrEmpty(textCode.Text))
                        continue;
                    
                    RenderTextCodeWithoutCGTransforms(textRenderer, textCode, boundaryX, boundaryY, textStyle, renderContext);
                }
            }
            
            // 恢复渲染状态
            renderContext?.RestoreState();
        }

        /// <summary>
        /// 渲染单个TextCode带字符变换的文本（情况2）
        /// 优化：使用批量绘制提高性能，确保编码和字型一一对应，不要遗漏编码
        /// </summary>
        /// <param name="textRenderer">文本渲染器</param>
        /// <param name="textObject">文本对象</param>
        /// <param name="textCode">文本代码</param>
        /// <param name="boundaryX">边界X坐标</param>
        /// <param name="boundaryY">边界Y坐标</param>
        /// <param name="textStyle">文本样式</param>
        /// <param name="renderContext">渲染上下文</param>
        private void RenderSingleTextCodeWithCGTransforms(ITextRenderer textRenderer, TextObject textObject, TextCode textCode, float boundaryX, float boundaryY, TextStyle textStyle, IRenderContext renderContext)
        {
            // 计算第一个文字的位置（边界矩形位置 + TextCode内部坐标，都转换为像素）
            float startX = boundaryX + renderContext.MillimetersToPixels((float)textCode.X);
            float startY = boundaryY + renderContext.MillimetersToPixels((float)textCode.Y);
            
            // 将TextCode.DeltaX和DeltaY转换为double数组（如果存在）
            double[] deltaXArray = new double[0];
            if (textCode.DeltaX != null)
            {
                deltaXArray = textCode.DeltaX.ToDoubleArray();
                if (deltaXArray == null)
                {
                    deltaXArray = new double[0];
                }
            }
            
            double[] deltaYArray = new double[0];
            if (textCode.DeltaY != null)
            {
                deltaYArray = textCode.DeltaY.ToDoubleArray();
                if (deltaYArray == null)
                {
                    deltaYArray = new double[0];
                }
            }
            
            int charIndex = 0;
            float currentX = startX;
            float currentY = startY;
            List<GlyphInfo> glyphInfos = new List<GlyphInfo>();
            
            // 遍历文本中的每个字符，收集所有需要绘制的字形信息
            if (textObject.CGTransforms.Count == 1 && textObject.CGTransforms[0].CodeCount == textCode.Text.Length)
            {
                // 优化：只有一个CGTransform且CodeCount等于文本长度，直接处理
                var cgTransform = textObject.CGTransforms[0];
                if (cgTransform.Glyphs != null)
                {
                    var glyphs = cgTransform.Glyphs.ToIntArray();
                    if (glyphs != null && glyphs.Length > 0)
                    {
                        for (int i = 0; i < textCode.Text.Length; i++)
                        {
                            int glyphIndex = Math.Min(i, glyphs.Length - 1);
                            string glyph = glyphs[glyphIndex].ToString();
                            
                            if (!string.IsNullOrEmpty(glyph))
                            {
                                glyphInfos.Add(new GlyphInfo(currentX, currentY, glyph));
                            }
                            
                            if (i < deltaXArray.Length)
                            {
                                currentX += renderContext.MillimetersToPixels((float)deltaXArray[i]);
                            }
                            if (i < deltaYArray.Length)
                            {
                                currentY += renderContext.MillimetersToPixels((float)deltaYArray[i]);
                            }
                        }
                    }
                }

                // 批量绘制所有字形
                if (glyphInfos.Count > 0)
                {
                    textRenderer.DrawGlyphs(glyphInfos.ToArray(), textStyle);
                }
            }
            else
            {
                // 多个CGTransform或长度不匹配，逐个字符查找对应的CGTransform
                while (charIndex < textCode.Text.Length)
                {
                    var cgTransform = textObject.CGTransforms.FirstOrDefault(t => 
                        t.CodePosition <= charIndex && t.CodePosition + t.CodeCount > charIndex);
                    
                    if (cgTransform != null && cgTransform.Glyphs != null)
                    {
                        var glyphs = cgTransform.Glyphs.ToIntArray();
                        if (glyphs != null && glyphs.Length > 0)
                        {
                            // 处理当前CGTransform对应的所有字符
                            int endIndex = Math.Min(cgTransform.CodePosition + cgTransform.CodeCount, textCode.Text.Length);
                            for (int i = charIndex; i < endIndex; i++)
                            {
                                int relativePosition = i - cgTransform.CodePosition;
                                int glyphIndex = Math.Min(relativePosition, glyphs.Length - 1);
                                string glyph = glyphs[glyphIndex].ToString();
                                
                                if (!string.IsNullOrEmpty(glyph))
                                {
                                    glyphInfos.Add(new GlyphInfo(currentX, currentY, glyph));
                                }
                                
                                // 计算下一个字符的位置
                                if (i < deltaXArray.Length)
                                {
                                    currentX += renderContext.MillimetersToPixels((float)deltaXArray[i]);
                                }
                                if (i < deltaYArray.Length)
                                {
                                    currentY += renderContext.MillimetersToPixels((float)deltaYArray[i]);
                                }
                            }
                        }
                        
                        // 跳过当前CGTransform处理的字符数
                        charIndex += cgTransform.CodeCount;
                    }
                    else
                    {
                        string currentChar = textCode.Text.Substring(charIndex, 1);
                        glyphInfos.Add(new GlyphInfo(currentX, currentY, currentChar));
                        charIndex++;
                        
                        // 计算下一个字符的位置
                        if (charIndex - 1 < deltaXArray.Length)
                        {
                            currentX += renderContext.MillimetersToPixels((float)deltaXArray[charIndex - 1]);
                        }
                        if (charIndex - 1 < deltaYArray.Length)
                        {
                            currentY += renderContext.MillimetersToPixels((float)deltaYArray[charIndex - 1]);
                        }
                    }
                }

                foreach (var glyphInfo in glyphInfos)
                {
                    // 绘制字形
                    textRenderer.DrawText(glyphInfo.X, glyphInfo.Y, glyphInfo.Glyph, textStyle);
                }
            }         
        }

        /// <summary>
        /// 渲染TextCode不带字符变换的文本（情况1和3）
        /// 优化：使用批量绘制提高性能
        /// </summary>
        /// <param name="textRenderer">文本渲染器</param>
        /// <param name="textCode">文本代码</param>
        /// <param name="boundaryX">边界X坐标</param>
        /// <param name="boundaryY">边界Y坐标</param>
        /// <param name="textStyle">文本样式</param>
        /// <param name="renderContext">渲染上下文</param>
        private void RenderTextCodeWithoutCGTransforms(ITextRenderer textRenderer, TextCode textCode, float boundaryX, float boundaryY, TextStyle textStyle, IRenderContext renderContext)
        {
            // 计算第一个文字的位置（边界矩形位置 + TextCode内部坐标，都转换为像素）
            float startX = boundaryX + renderContext.MillimetersToPixels((float)textCode.X);
            float startY = boundaryY + renderContext.MillimetersToPixels((float)textCode.Y);
            
            // 将TextCode.DeltaX和DeltaY转换为double数组（如果存在）
            double[] deltaXArray = new double[0];
            if (textCode.DeltaX != null)
            {
                deltaXArray = textCode.DeltaX.ToDoubleArray();
                if (deltaXArray == null)
                {
                    deltaXArray = new double[0];
                }
            }
            
            double[] deltaYArray = new double[0];
            if (textCode.DeltaY != null)
            {
                deltaYArray = textCode.DeltaY.ToDoubleArray();
                if (deltaYArray == null)
                {
                    deltaYArray = new double[0];
                }
            }
            
            float currentX = startX;
            float currentY = startY;
            List<GlyphInfo> glyphInfos = new List<GlyphInfo>();
            
            // 遍历每个字符，考虑DeltaX和DeltaY偏移，收集所有需要绘制的字形信息
            for (int i = 0; i < textCode.Text.Length; i++)
            {
                // 获取当前字符
                string currentChar = textCode.Text.Substring(i, 1);
                
                // 添加到字形信息列表
                glyphInfos.Add(new GlyphInfo(currentX, currentY, currentChar));
                
                // 计算下一个字符的位置（根据DeltaX和DeltaY，转换为像素）
                if (i < deltaXArray.Length)
                {
                    currentX += renderContext.MillimetersToPixels((float)deltaXArray[i]);
                }
                if (i < deltaYArray.Length)
                {
                    currentY += renderContext.MillimetersToPixels((float)deltaYArray[i]);
                }
            }

            foreach (var glyphInfo in glyphInfos)
            {
                // 绘制字形
                textRenderer.DrawText(glyphInfo.X, glyphInfo.Y, glyphInfo.Glyph, textStyle);
            }
        
        }

        /// <summary>
        /// 将OFD文本对象转换为TextStyle
        /// 优化：使用缓存避免重复创建对象，预计算DPI转换系数
        /// </summary>
        /// <param name="textObject">OFD文本对象</param>
        /// <param name="renderContext">渲染上下文</param>
        /// <returns>文本样式</returns>
        private TextStyle ConvertToTextStyle(Models.Font.CT_Text textObject, IRenderContext renderContext)
        {
            // 创建缓存键（基于字体ID和文本属性）
            string cacheKey = $"{textObject.FontRefID}_{textObject.Size}_{textObject.Weight}_{textObject.Italic}_{textObject.HScale}";
            
            // 检查缓存
            lock (_styleCacheLock)
            {
                if (_textStyleCache.TryGetValue(cacheKey, out var cachedStyle))
                {
                    return cachedStyle;  // 缓存命中，直接返回
                }
            }


            // 缓存未命中，创建新的样式
            var style = new TextStyle();

            OFDFont oFDFont = renderContext.ResourceManager.GetResource<OFDFont>(textObject.FontRefID);
            if (oFDFont != null)
            {
                // 字体资源（使用延迟加载的MemoryStream）
                style.FontFilePath = oFDFont.FontFile != null ? oFDFont.FontFile.Path : null;
                // 字体名称（使用更通用的中文字体，确保能正确显示中文和英文）
                style.FontFamily = oFDFont.FontName ?? oFDFont.FamilyName ?? "宋体";
            }
            
            // 字号转换：使用SkiaRenderContext的只读属性（避免重复计算除法）
            var skiaRenderContext = renderContext as SkiaRenderContext;
            float dpiScaleFactor = skiaRenderContext?.MmToPixel ?? (_renderConfig.Dpi / 25.4f);
            style.FontSize = (float)(textObject.Size * dpiScaleFactor);
            
            // 字体粗细
            style.FontWeight = textObject.Weight;
            
            // 是否斜体
            style.Italic = textObject.Italic;
            
            // 水平缩放比例
            style.HScale = (float)textObject.HScale;
            
            // 填充颜色（默认黑色）
            style.Color = textObject.FillColor != null ? ConvertToARGB(textObject.FillColor) : 0xFF000000;

            // 是否描边
            style.Stroke = textObject.Stroke;

            // 描边颜色（默认透明）
            style.StrokeColor =  textObject.StrokeColor != null ? ConvertToARGB(textObject.StrokeColor) : 255;
            
            // 透明度（默认完全不透明）
            style.Alpha = 255;

            // 缓存结果（线程安全）
            lock (_styleCacheLock)
            {
                if (!_textStyleCache.ContainsKey(cacheKey))
                {
                    _textStyleCache[cacheKey] = style;
                }
            }
            
            return style;
        }
        
        /// <summary>
        /// 将OFD颜色转换为ARGB格式
        /// </summary>
        /// <param name="ofdColor">OFD颜色对象</param>
        /// <returns>ARGB格式颜色值</returns>
        private uint ConvertToARGB(Models.PageDesc.Colors.CT_Color ofdColor)
        {
            // 默认颜色为黑色
            if (ofdColor == null)
                return 0xFF000000;
            
            // 获取透明度（0-255）
            byte alpha = (byte)(ofdColor.Alpha >= 0 && ofdColor.Alpha <= 255 ? ofdColor.Alpha : 255);
            
            // 简单处理RGB颜色（后续需要支持更多颜色空间）
            if (ofdColor.Value != null && ofdColor.Value.Count >= 3)
            {
                // 将object类型转换为double类型
                double rValue = Convert.ToDouble(ofdColor.Value[0]);
                double gValue = Convert.ToDouble(ofdColor.Value[1]);
                double bValue = Convert.ToDouble(ofdColor.Value[2]);
                
                // 计算RGB值（0-255）
                byte r = (byte)(rValue * 255);
                byte g = (byte)(gValue * 255);
                byte b = (byte)(bValue * 255);
                return (uint)((uint)alpha << 24 | ((uint)r << 16) | ((uint)g << 8) | b);
            }
            
            return (uint)((uint)alpha << 24 | 0x000000);
        }
        
        /// <summary>
        /// 渲染路径对象
        /// </summary>
        /// <param name="pathRenderer">路径渲染器</param>
        /// <param name="pathObj">路径对象</param>
        private void RenderPathObject(IPathRenderer pathRenderer, object pathObj)
        {
            if (pathRenderer == null || pathObj == null)
                return;
            
            var pathObject = pathObj as Models.BaseStructure.Pages.PageBlockItems.PathObject;
            if (pathObject == null)
                return;
            
            // 如果路径数据为空，跳过渲染
            if (string.IsNullOrEmpty(pathObject.AbbreviatedData))
                return;
            
            // 获取渲染上下文
            var renderContext = pathRenderer as IRenderContext;
            if (renderContext == null)
                return;
            
            // 获取图元外接矩形位置（页面坐标系，毫米转换为像素）
            float boundaryX = renderContext.MillimetersToPixels((float)pathObject.Boundary.X);
            float boundaryY = renderContext.MillimetersToPixels((float)pathObject.Boundary.Y);
            float boundaryWidth = renderContext.MillimetersToPixels((float)pathObject.Boundary.Width);
            float boundaryHeight = renderContext.MillimetersToPixels((float)pathObject.Boundary.Height);
            
            // 保存当前渲染状态
            renderContext?.SaveState();
            
            // 设置裁剪区（使用图元的外接矩形作为默认裁剪区）
            renderContext?.SetClipRect(boundaryX, boundaryY, boundaryWidth, boundaryHeight);
            
            // 转换图形样式
            var graphStyle = ConvertToGraphStyle(pathObject);
            
            // 开始绘制路径
            pathRenderer.BeginPath();
            
            // 解析并绘制路径（将图形平移到页面空间）
            ParseAndRenderPath(pathRenderer, renderContext, pathObject.AbbreviatedData, boundaryX, boundaryY);
            
            // 根据样式绘制路径
            if (graphStyle.Fill && graphStyle.Stroke)
            {
                pathRenderer.FillAndStrokePath(graphStyle);
            }
            else if (graphStyle.Fill)
            {
                pathRenderer.FillPath(graphStyle);
            }
            else if (graphStyle.Stroke)
            {
                pathRenderer.StrokePath(graphStyle);
            }
            
            // 恢复渲染状态
            renderContext?.RestoreState();
        }
        
        /// <summary>
        /// 解析OFD路径数据并调用路径渲染器绘制
        /// 支持的命令：
        /// S x y - 定义子绘制图形边线的起始点坐标
        /// M x y - 将当前点移动到指定点
        /// L x y - 绘制线段到指定点
        /// Q x1 y1 x2 y2 - 二次贝塞尔曲线
        /// B x1 y1 x2 y2 x3 y3 - 三次贝塞尔曲线
        /// A rx ry angle large sweep x y - 圆弧
        /// C - 自动闭合子路径
        /// </summary>
        /// <param name="pathRenderer">路径渲染器</param>
        /// <param name="renderContext">渲染上下文</param>
        /// <param name="abbreviatedData">OFD路径数据</param>
        /// <param name="boundaryX">图元外接矩形X坐标（页面坐标系，像素）</param>
        /// <param name="boundaryY">图元外接矩形Y坐标（页面坐标系，像素）</param>
        private void ParseAndRenderPath(IPathRenderer pathRenderer, IRenderContext renderContext, string abbreviatedData, float boundaryX = 0, float boundaryY = 0)
        {
            if (string.IsNullOrEmpty(abbreviatedData))
                return;
            
            // OFD路径数据格式：操作符+空格+参数+空格+...
            // 例如："M 100 100 L 200 200 C"
            var tokens = abbreviatedData.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 0)
                return;
            
            int index = 0;
            while (index < tokens.Length)
            {
                string command = tokens[index++];
                
                switch (command.ToUpper())
                {
                    case "S":// 定义子绘制图形边线的起始点坐标
                        if (index + 1 < tokens.Length)
                        {
                            float x = renderContext.MillimetersToPixels(float.Parse(tokens[index])) + boundaryX;
                            float y = renderContext.MillimetersToPixels(float.Parse(tokens[index + 1])) + boundaryY;
                            pathRenderer.MoveTo(x, y);
                            index += 2;
                        }
                        break;
                    
                    case "M":// 将当前点移动到指定点
                        if (index + 1 < tokens.Length)
                        {
                            float x = renderContext.MillimetersToPixels(float.Parse(tokens[index])) + boundaryX;
                            float y = renderContext.MillimetersToPixels(float.Parse(tokens[index + 1])) + boundaryY;
                            pathRenderer.MoveTo(x, y);
                            index += 2;
                        }
                        break;
                    
                    case "L":// 绘制线段到指定点
                        if (index + 1 < tokens.Length)
                        {
                            float x = renderContext.MillimetersToPixels(float.Parse(tokens[index])) + boundaryX;
                            float y = renderContext.MillimetersToPixels(float.Parse(tokens[index + 1])) + boundaryY;
                            pathRenderer.LineTo(x, y);
                            index += 2;
                        }
                        break;
                    
                    case "Q":// 二次贝塞尔曲线
                        if (index + 3 < tokens.Length)
                        {
                            float x1 = renderContext.MillimetersToPixels(float.Parse(tokens[index])) + boundaryX;
                            float y1 = renderContext.MillimetersToPixels(float.Parse(tokens[index + 1])) + boundaryY;
                            float x2 = renderContext.MillimetersToPixels(float.Parse(tokens[index + 2])) + boundaryX;
                            float y2 = renderContext.MillimetersToPixels(float.Parse(tokens[index + 3])) + boundaryY;
                            pathRenderer.QuadTo(x1, y1, x2, y2);
                            index += 4;
                        }
                        break;
                    
                    case "B":// 三次贝塞尔曲线
                        if (index + 5 < tokens.Length)
                        {
                            float x1 = renderContext.MillimetersToPixels(float.Parse(tokens[index])) + boundaryX;
                            float y1 = renderContext.MillimetersToPixels(float.Parse(tokens[index + 1])) + boundaryY;
                            float x2 = renderContext.MillimetersToPixels(float.Parse(tokens[index + 2])) + boundaryX;
                            float y2 = renderContext.MillimetersToPixels(float.Parse(tokens[index + 3])) + boundaryY;
                            float x3 = renderContext.MillimetersToPixels(float.Parse(tokens[index + 4])) + boundaryX;
                            float y3 = renderContext.MillimetersToPixels(float.Parse(tokens[index + 5])) + boundaryY;
                            pathRenderer.CubicTo(x1, y1, x2, y2, x3, y3);
                            index += 6;
                        }
                        break;
                    
                    case "A":// 圆弧
                        if (index + 6 < tokens.Length)
                        {
                            float rx = renderContext.MillimetersToPixels(float.Parse(tokens[index]));
                            float ry = renderContext.MillimetersToPixels(float.Parse(tokens[index + 1]));
                            float angle = renderContext.MillimetersToPixels(float.Parse(tokens[index + 2]));
                            int large = (int)renderContext.MillimetersToPixels(int.Parse(tokens[index + 3]));
                            int sweep = (int)renderContext.MillimetersToPixels(int.Parse(tokens[index + 4]));
                            float x = renderContext.MillimetersToPixels(float.Parse(tokens[index + 5])) + boundaryX;
                            float y = renderContext.MillimetersToPixels(float.Parse(tokens[index + 6])) + boundaryY;
                            
                            pathRenderer.ArcTo(rx, ry, angle, large == 1, sweep == 1, x, y);
                            index += 7;
                        }
                        break;
                    
                    case "C":// 自动闭合子路径
                        pathRenderer.ClosePath();
                        break;
                    
                    default:
                        // 未知命令，跳过
                        System.Diagnostics.Debug.WriteLine($"未知的路径命令: {command}");
                        break;
                }
            }
        }

        /// <summary>
        /// 将OFD路径对象转换为图形样式
        /// </summary>
        /// <param name="pathObject">OFD路径对象</param>
        /// <returns>图形样式</returns>
        private GraphStyle ConvertToGraphStyle(Models.Graph.CT_Path pathObject)
        {
            var style = new GraphStyle
            {
                // 填充颜色 默认透明色
                Color = ConvertToARGB(pathObject.FillColor),
                Alpha = (byte)pathObject.Alpha,

                // 描边颜色 默认黑色
                StrokeColor = ConvertToARGB(pathObject.StrokeColor),
                StrokeAlpha = 255,

                // 描边宽度
                StrokeWidth = (float)pathObject.LineWidth,

                // 是否填充和描边
                Fill = pathObject.Fill,
                Stroke = pathObject.Stroke,

                // 虚线样式（暂时不支持）
                DashPattern = null
            };

            return style;
        }
        
        /// <summary>
        /// 渲染图像对象
        /// </summary>
        /// <param name="imageRenderer">图像渲染器</param>
        /// <param name="imageObj">图像对象</param>
        /// <param name="pageIndex">页面索引</param>
        private void RenderImageObject(IImageRenderer imageRenderer, object imageObj)
        {
            if (imageRenderer == null || imageObj == null)
                return;

            var imageObject = imageObj as Models.BaseStructure.Pages.PageBlockItems.ImageObject;
            if (imageObject == null)
                return;

            // 转换图像样式
            var imageStyle = ConvertToImageStyle(imageObject);

            // 获取图像位置和大小（OFD坐标，单位：毫米）
            float x = (float)imageObject.Boundary.X;
            float y = (float)imageObject.Boundary.Y;
            float width = (float)imageObject.Boundary.Width;
            float height = (float)imageObject.Boundary.Height;

            // 获取渲染上下文
            var renderContext = imageRenderer as IRenderContext;
            if (renderContext == null)
                return;

            // 从资源管理器获取图像数据
            byte[] imageData = null;
            if (renderContext.ResourceManager != null && imageObject.ResourceID != null)
            {
                // 首先尝试获取多媒体资源对象
                var multiMedia = renderContext.ResourceManager.GetResource<Models.BaseStructure.Resources.ResItems.MultiMedia>(imageObject.ResourceID.ToString());
                if (multiMedia != null && multiMedia.MediaFile != null)
                {
                    // 从资源文件获取图像数据
                    imageData = renderContext.ResourceManager.GetResourceFile(multiMedia.MediaFile.Path);
                }
            }

            // 如果图像数据为空，跳过渲染
            if (imageData == null || imageData.Length == 0)
                return;

            // 绘制图像
            imageRenderer.DrawImage(x, y, width, height, imageData, imageStyle);
        }
        
        /// <summary>
        /// 将OFD图像对象转换为图像样式
        /// </summary>
        /// <param name="imageObject">OFD图像对象</param>
        /// <returns>图像样式</returns>
        private ImageStyle ConvertToImageStyle(Models.Image.CT_Image imageObject)
        {
            var style = new ImageStyle
            {
                // 图像插值模式
                InterpolationMode = ImageInterpolationMode.HighQuality,
                // 保持纵横比
                PreserveAspectRatio = true,
                // 透明度（默认完全不透明）
                Alpha = 255
            };
            
            return style;
        }
        
        /// <summary>
        /// 渲染复合对象
        /// </summary>
        /// <param name="renderContext">渲染上下文</param>
        /// <param name="compositeObj">复合对象</param>
        /// <param name="pageIndex">页面索引</param>
        private void RenderCompositeObject(IRenderContext renderContext, object compositeObj)
        {
            if (renderContext == null || compositeObj == null)
                return;

            var compositeObject = compositeObj as Models.BaseStructure.Pages.PageBlockItems.CompositeObject;
            if (compositeObject == null)
                return;

            // 从资源管理器获取复合对象内容
            if (renderContext.ResourceManager != null && compositeObject.ResourceID != null)
            {
                // 获取矢量图形资源
                var vectorGraphic = renderContext.ResourceManager.GetResource<Models.BaseStructure.Resources.ResItems.CompositeGraphicUnit>(compositeObject.ResourceID.ToString());
                if (vectorGraphic != null && vectorGraphic.Content != null)
                {
                    // 保存当前渲染状态
                    renderContext.SaveState();

                    // 获取复合对象位置和大小（OFD坐标，单位：毫米）
                    float x = (float)compositeObject.Boundary.X;
                    float y = (float)compositeObject.Boundary.Y;
                    float width = (float)compositeObject.Boundary.Width;
                    float height = (float)compositeObject.Boundary.Height;

                    // 应用变换
                    renderContext.Translate(x, y);

                    // 递归渲染复合对象中的子对象
                    if (vectorGraphic.Content.PageBlockItems != null && vectorGraphic.Content.PageBlockItems.Count > 0)
                    {
                        foreach (var childBlock in vectorGraphic.Content.PageBlockItems)
                        {
                            RenderPageBlock(renderContext, childBlock);
                        }
                    }

                    // 恢复渲染状态
                    renderContext.RestoreState();
                }
            }
        }
        
        /// <summary>
        /// 渲染页面块对象
        /// </summary>
        /// <param name="renderContext">渲染上下文</param>
        /// <param name="pageBlock">页面块对象</param>
        /// <param name="pageIndex">页面索引</param>
        private void RenderPageBlockObject(IRenderContext renderContext, object pageBlock)
        {
            if (renderContext == null || pageBlock == null)
                return;

            var pageBlockObj = pageBlock as Models.BaseStructure.Pages.PageBlockItems.PageBlock;
            if (pageBlockObj == null)
                return;

            // 保存当前渲染状态
            renderContext.SaveState();

            // 遍历页面块中的所有子元素并递归渲染
            if (pageBlockObj.PageBlockItems != null && pageBlockObj.PageBlockItems.Count > 0)
            {
                foreach (var childBlock in pageBlockObj.PageBlockItems)
                {
                    RenderPageBlock(renderContext, childBlock);
                }
            }

            // 恢复渲染状态
            renderContext.RestoreState();
        }
        
        /// <summary>
        /// 渲染所有页面到内存位图
        /// </summary>
        /// <returns>所有页面的渲染结果（PNG格式字节数组列表）</returns>
        public byte[][] RenderAllPagesToBitmap()
        {
            CheckDisposed();
            
            var results = new byte[PageCount][];
            
            for (int i = 0; i < PageCount; i++)
            {
                results[i] = RenderPageToBitmap(i);
            }
            
            return results;
        }
        
        /// <summary>
        /// 渲染指定页面到文件
        /// </summary>
        /// <param name="outputPath">输出文件路径</param>
        /// <param name="pageIndex">页面索引，默认为第1页</param>
        public void RenderPageToFile(string outputPath, int pageIndex = 0)
        {
            CheckDisposed();
            
            var renderResult = RenderPageToBitmap(pageIndex);
            File.WriteAllBytes(outputPath, renderResult);
        }
        
        /// <summary>
        /// 渲染所有页面到文件
        /// </summary>
        /// <param name="outputDirectory">输出目录路径</param>
        /// <param name="fileNamePattern">文件名模板，默认为"page_{0}.png"</param>
        public void RenderAllPagesToFile(string outputDirectory, string fileNamePattern = "page_{0}.png")
        {
            CheckDisposed();
            
            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);
            
            for (int i = 0; i < PageCount; i++)
            {
                var outputPath = Path.Combine(outputDirectory, string.Format(fileNamePattern, i + 1));
                RenderPageToFile(outputPath, i);
            }
        }
        
        #endregion

        #region IDisposable实现
        
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            // 调用带参数的Dispose方法
            Dispose(true);
            // 阻止垃圾回收器调用终结器
            GC.SuppressFinalize(this);
        }
        
        /// <summary>
        /// 释放资源（带参数）
        /// </summary>
        /// <param name="disposing">是否释放托管资源</param>
        protected virtual void Dispose(bool disposing)
        {
            // 检查是否已释放
            if (_disposed)
                return;
            
            // 释放托管资源
            if (disposing)
            {
                // 释放OFD读取器
                _ofdReader?.Dispose();
                
                // 清空文本样式缓存
                lock (_styleCacheLock)
                {
                    if (_textStyleCache != null)
                    {
                        _textStyleCache.Clear();
                    }
                }
            }
            
            // 释放非托管资源（如果有）
            // 目前没有非托管资源需要释放
            
            // 标记为已释放
            _disposed = true;
        }
        
        /// <summary>
        /// 终结器
        /// </summary>
        ~OfdRenderer()
        {
            Dispose(false);
        }
        
        /// <summary>
        /// 检查对象是否已释放
        /// </summary>
        /// <exception cref="ObjectDisposedException">对象已释放时抛出</exception>
        private void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name, "OFD渲染器已释放，无法执行操作");
            }
        }
        
        #endregion
    }
}