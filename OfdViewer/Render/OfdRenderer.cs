using JBig2Decoder.NETStandard;
using OFDViewer.Models.BaseStructure.Pages;
using OFDViewer.Models.BaseStructure.Pages.PageBlockItems;
using OFDViewer.Models.BaseStructure.Resources.ResItems;
using OFDViewer.Models.BaseType;
using OFDViewer.Models.Enums;
using OFDViewer.Models.Font;
using OFDViewer.Models.Graph;
using OFDViewer.Models.Image;
using OFDViewer.Models.PageDesc;
using OFDViewer.Models.PageDesc.Colors;
using OFDViewer.Models.PageDesc.DrawParams;
using OFDViewer.Parse;
using OFDViewer.Render.Abstractions;
using OFDViewer.Render.DataModels;
using OFDViewer.Render.Implementation;
using OfdViewer.ESeal.Abstractions.Factory;
using OfdViewer.ESeal.Abstractions.Interfaces;
using OfdViewer.ESeal.Implementations.Common;
using OfdViewer.ESeal.Implementations.Gomain;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

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


        /// <summary>
        /// 静态构造函数，注册电子印章解析器
        /// </summary>
        static OfdRenderer()
        {
            // 注册国脉电子印章解析器
            EsealParserFactory.Register("Gomain", () => new GomainEsealParser());

            // 注册默认解析器（作为后备）
            EsealParserFactory.Register("Default", () => new DefaultEsealParser());
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

            // 设置背景色为白色
            renderContext.SetBackgroundColor(0xFFFFFFFF);
            
            // 渲染页面内容
            if (ofdDoc.PageDocs != null && ofdDoc.PageDocs.Count > pageIndex)
            {
                var pageDoc = ofdDoc.PageDocs[pageIndex];
                if (pageDoc != null && pageDoc.Page != null)
                {
                    // 设置资源管理器
                    renderContext.ResourceManager = new ResourceManager(ofdDoc, (int)pageDoc.PageId);
                    // 创建渲染上下文对象，封装共性参数
                    var renderCtxObj = new RenderContextObject
                    {
                        RenderContext = renderContext,
                        OfdDocument = ofdDoc,
                        CurrentPageDoc = pageDoc,
                        CurrentPage = pageDoc.Page,
                        PageIndex = pageIndex,
                        DocumentIndex = docIndex,
                        IsTemplate = false
                    };

                    // 渲染页面内容
                    RenderPageContent(renderCtxObj);

                    // 渲染注释
                    // 根据当前页的pageDoc.PageId，获取当前页的页注释对象PageAnnotDocument
                    var pageAnnotDoc = ofdDoc.PageAnnotDocs?.FirstOrDefault(a => a.PageId == pageDoc.PageId);
                    if (pageAnnotDoc != null && pageAnnotDoc.PageAnnot != null)
                    {
                        // 渲染注释
                        RenderPageAnnot(renderCtxObj, pageAnnotDoc.PageAnnot);
                    }
                    else
                    {
                        // 没有注释，跳过
                    }
                    
                }
            }
            
            // 返回渲染结果
            return renderContext.GetRenderResult();
        }

        

        #endregion

        #region 页面块渲染

        /// <summary>
        /// 渲染页面内容，包括模版页
        /// 渲染顺序：
        /// 最上层
        ///    ───────────
        ///    前景层
        ///        Content前景
        ///        Template前景
        ///    ───────────
        ///    正文层
        ///        Content正文
        ///        Template正文
        ///    ───────────
        ///    背景层
        ///        Content背景
        ///        Template背景
        ///    ───────────
        /// 最下层
        /// </summary>
        /// <param name="renderCtxObj">渲染上下文对象</param>
        private void RenderPageContent(RenderContextObject renderCtxObj)
        {
            if (renderCtxObj?.CurrentPage?.Content == null)
                return;

            var page = renderCtxObj.CurrentPage;
            var renderContext = renderCtxObj.RenderContext;

            // 1. 渲染背景层（最下层）
            // 1.1 渲染 Template 背景层（最先渲染）
            if (page.Template != null && page.Template.Count > 0)
            {
                // 选择所有 ZOrder="Background" 模版页
                var sortedTemplates = page.Template.Where(t => t.ZOrder == LayerType.Background);

                foreach (var template in sortedTemplates)
                {
                    // 获取模版页面对象
                    var templatePage = renderContext.ResourceManager.GetTemplatePage(template.TemplateID) as Page;
                    if (templatePage == null || templatePage.Content == null)
                        continue;

                    // 渲染模版背景层
                    foreach (var layer in templatePage.Content)
                    {
                        renderCtxObj.CurrentLayer = layer;
                        renderCtxObj.IsTemplate = true;
                        foreach (var blockItem in layer.PageBlockItems)
                        {
                            RenderPageBlock(renderCtxObj, blockItem);
                        }
                    }
                }
            }

            // 1.2 渲染 Content 背景层
            var backgroundLayers = page.Content.Where(l => l.Type == LayerType.Background);
            foreach (var backgroundLayer in backgroundLayers)
            {
                if (backgroundLayer != null && backgroundLayer.PageBlockItems != null)
                {
                    renderCtxObj.CurrentLayer = backgroundLayer;
                    renderCtxObj.IsTemplate = false;
                    foreach (var blockItem in backgroundLayer.PageBlockItems)
                    {
                        RenderPageBlock(renderCtxObj, blockItem);
                    }
                }
            }

            // 2. 渲染正文层
            // 2.1 渲染 Template 正文层
            if (page.Template != null && page.Template.Count > 0)
            {
                // 选择所有 ZOrder="Body" 模版页
                var sortedTemplates = page.Template.Where(t => t.ZOrder == LayerType.Body);

                foreach (var template in sortedTemplates)
                {
                    // 获取模版页面对象
                    var templatePage = GetTemplatePage(template.TemplateID);
                    if (templatePage == null || templatePage.Content == null)
                        continue;

                    // 渲染模版正文层
                    foreach (var layer in templatePage.Content)
                    {
                        renderCtxObj.CurrentLayer = layer;
                        renderCtxObj.IsTemplate = true;
                        foreach (var blockItem in layer.PageBlockItems)
                        {
                            RenderPageBlock(renderCtxObj, blockItem);
                        }
                    }
                }
            }

            // 2.2 渲染 Content 正文层
            var contentLayers = page.Content.Where(l => l.Type == LayerType.Body);
            foreach (var contentLayer in contentLayers)
            {
                if (contentLayer != null && contentLayer.PageBlockItems != null)
                {
                    renderCtxObj.CurrentLayer = contentLayer;
                    renderCtxObj.IsTemplate = false;
                    foreach (var blockItem in contentLayer.PageBlockItems)
                    {
                        RenderPageBlock(renderCtxObj, blockItem);
                    }
                }
            }

            // 3. 渲染前景层（最上层）
            // 3.1 渲染 Template 前景层
            if (page.Template != null && page.Template.Count > 0)
            {
                // 选择所有 ZOrder="Foreground" 模版页
                var sortedTemplates = page.Template.Where(t => t.ZOrder == LayerType.Foreground);

                foreach (var template in sortedTemplates)
                {
                    // 获取模版页面对象
                    var templatePage = GetTemplatePage(template.TemplateID);
                    if (templatePage == null || templatePage.Content == null)
                        continue;

                    // 渲染模版前景层
                    foreach (var layer in templatePage.Content)
                    {
                        renderCtxObj.CurrentLayer = layer;
                        renderCtxObj.IsTemplate = true;
                        foreach (var blockItem in layer.PageBlockItems)
                        {
                            RenderPageBlock(renderCtxObj, blockItem);
                        }
                    }
                }
            }

            // 3.2 渲染 Content 前景层（最后渲染）
            var foregroundLayers = page.Content.Where(l => l.Type == LayerType.Foreground);
            foreach (var foregroundLayer in foregroundLayers)
            {
                if (foregroundLayer != null && foregroundLayer.PageBlockItems != null)
                {
                    renderCtxObj.CurrentLayer = foregroundLayer;
                    renderCtxObj.IsTemplate = false;
                    foreach (var blockItem in foregroundLayer.PageBlockItems)
                    {
                        RenderPageBlock(renderCtxObj, blockItem);
                    }
                }
            }
        }

        /// <summary>
        /// 获取模版页面对象
        /// </summary>
        /// <param name="templateID">模版ID</param>
        /// <returns>模版页面对象，如果未找到返回null</returns>
        private Page GetTemplatePage(ST_RefID templateID)
        {
            if (templateID == ST_RefID.Invalid)
                return null;

            // 从 OFDDocument 中获取模版页面对象
            // 这里需要根据实际的实现进行调整
            // 当前仅作为框架示例
            return null;
        }


        
        /// <summary>
        /// 渲染页面块
        /// 根据页面块类型调用相应的渲染方法
        /// </summary>
        /// <param name="renderCtxObj">渲染上下文对象</param>
        /// <param name="blockItem">页面块对象</param>
        private void RenderPageBlock(RenderContextObject renderCtxObj, object blockItem)
        {
            if (renderCtxObj?.RenderContext == null || blockItem == null)
                return;

            // 根据页面块类型调用相应的渲染方法
            switch (blockItem)
            {
                case Models.BaseStructure.Pages.PageBlockItems.TextObject textObj:
                    {
                        RenderTextObject(renderCtxObj, textObj);
                        break;
                    }
                case Models.BaseStructure.Pages.PageBlockItems.PathObject pathObj:
                    {
                        RenderPathObject(renderCtxObj, pathObj);
                        break;
                    }

                case Models.BaseStructure.Pages.PageBlockItems.ImageObject imageObj:
                    {
                        RenderImageObject(renderCtxObj, imageObj);
                        break;
                    }

                case Models.BaseStructure.Pages.PageBlockItems.CompositeObject compositeObj:
                    {
                        RenderCompositeObject(renderCtxObj, compositeObj);
                        break;
                    }

                case Models.BaseStructure.Pages.PageBlockItems.PageBlock pageBlock:
                    {
                        RenderPageBlockObject(renderCtxObj, pageBlock);
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
        /// <param name="renderCtxObj">渲染上下文对象</param>
        /// <param name="textObject">文本对象</param>
        #endregion

        #region 文本渲染

        private void RenderTextObject(RenderContextObject renderCtxObj, TextObject textObject)
        {
            if (renderCtxObj?.RenderContext == null || textObject == null)
                return;

            var renderContext = renderCtxObj.RenderContext;

            // 获取图元外接矩形位置（页面坐标系，毫米转换为像素）
            float boundaryX = renderContext.MillimetersToPixels((float)textObject.Boundary.X);
            float boundaryY = renderContext.MillimetersToPixels((float)textObject.Boundary.Y);
            float boundaryWidth = renderContext.MillimetersToPixels((float)textObject.Boundary.Width);
            float boundaryHeight = renderContext.MillimetersToPixels((float)textObject.Boundary.Height);

            // 保存当前渲染状态
            renderContext.SaveState();

            // 设置裁剪区（使用图元的外接矩形作为默认裁剪区）
            renderContext.SetClipRect(boundaryX, boundaryY, boundaryWidth, boundaryHeight);

            // 转换文本样式
            var textStyle = ConvertToTextStyle(renderCtxObj, textObject);

            // 判断处理情况：
            // 1. 一个TextObject有一个或多个TextCode，但没有CGTransform - 直接按照多个TextCode处理
            // 2. 一个TextObject有一个TextCode，有一个或多个CGTransform - 使用CGTransform处理
            // 3. 一个TextObject有多个TextCode，并且有一个或多个CGTransform - 不处理CGTransform，直接按照多个TextCode处理
            bool hasCGTransforms = textObject.CGTransforms != null && textObject.CGTransforms.Count > 0;
            bool hasMultipleTextCodes = textObject.TextCodes != null && textObject.TextCodes.Count > 1;

            // 情况2：只有一个TextCode且有CGTransform，使用CGTransform处理
            if (!hasMultipleTextCodes && textObject.TextCodes.Count == 1 && hasCGTransforms)
            {
                var textCode = textObject.TextCodes[0];
                if (!string.IsNullOrEmpty(textCode.Text))
                {
                    RenderSingleTextCodeWithCGTransforms(renderCtxObj, textObject, textCode, boundaryX, boundaryY, textStyle);
                }
            }
            else
            {
                // 情况1和3：按照多个TextCode处理，不使用CGTransform
                foreach (var textCode in textObject.TextCodes)
                {
                    if (string.IsNullOrEmpty(textCode.Text))
                        continue;

                    RenderTextCodeWithoutCGTransforms(renderCtxObj, textCode, boundaryX, boundaryY, textStyle);
                }
            }

            // 恢复渲染状态
            renderContext.RestoreState();
        }

        /// <summary>
        /// 渲染单个TextCode带字符变换的文本（情况2）
        /// 优化：使用批量绘制提高性能，确保编码和字型一一对应，不要遗漏编码
        /// </summary>
        /// <param name="renderCtxObj">渲染上下文对象</param>
        /// <param name="textObject">文本对象</param>
        /// <param name="textCode">文本代码</param>
        /// <param name="boundaryX">边界X坐标</param>
        /// <param name="boundaryY">边界Y坐标</param>
        /// <param name="textStyle">文本样式</param>
        private void RenderSingleTextCodeWithCGTransforms(RenderContextObject renderCtxObj, TextObject textObject, TextCode textCode, float boundaryX, float boundaryY, TextStyle textStyle)
        {
            if (renderCtxObj?.RenderContext == null || textObject == null)
                return;

            var renderContext = renderCtxObj.RenderContext;
            var textRenderer = renderContext as ITextRenderer;
            if (textRenderer == null)
                return;

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
        /// <param name="renderCtxObj">渲染上下文对象</param>
        /// <param name="textCode">文本代码</param>
        /// <param name="boundaryX">边界X坐标</param>
        /// <param name="boundaryY">边界Y坐标</param>
        /// <param name="textStyle">文本样式</param>
        private void RenderTextCodeWithoutCGTransforms(RenderContextObject renderCtxObj, TextCode textCode, float boundaryX, float boundaryY, TextStyle textStyle)
        {
            if (renderCtxObj?.RenderContext == null || textCode == null)
                return;

            var renderContext = renderCtxObj.RenderContext;
            var textRenderer = renderContext as ITextRenderer;
            if (textRenderer == null)
                return;

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
        private TextStyle ConvertToTextStyle(RenderContextObject renderCtxObj, CT_Text textObject)
        {
            if (renderCtxObj?.RenderContext == null || textObject == null)
                return null;

            CT_Layer layer = renderCtxObj.CurrentLayer;
            IRenderContext renderContext = renderCtxObj.RenderContext;
            bool isTemplate = renderCtxObj.IsTemplate;

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

            // 获取绘制参数（按照就近原则）
            var drawParam = GetDrawParam(textObject, layer, renderContext, isTemplate);

            OFDFont oFDFont = isTemplate ? renderContext.ResourceManager.GetTemplateResource<OFDFont>(textObject.FontRefID)
                : renderContext.ResourceManager.GetResource<OFDFont>(textObject.FontRefID);
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

            // 填充颜色（按照就近原则：图元属性 > 图元DrawParam > 图层DrawParam > 默认黑色）
            if (textObject.FillColor != null)
            {
                style.Color = ConvertToARGB(textObject.FillColor, renderContext, isTemplate);
            }
            else if (drawParam != null && drawParam.FillColor != null)
            {
                style.Color = ConvertToARGB(drawParam.FillColor, renderContext, isTemplate);
            }
            else
            {
                style.Color = 0xFF000000;
            }

            // 是否描边（使用文本对象属性）
            style.Stroke = textObject.Stroke;

            // 描边颜色（按照就近原则：图元属性 > 图元DrawParam > 图层DrawParam > 默认透明）
            if (textObject.StrokeColor != null)
            {
                style.StrokeColor = ConvertToARGB(textObject.StrokeColor, renderContext, isTemplate);
            }
            else if (drawParam != null && drawParam.StrokeColor != null)
            {
                style.StrokeColor = ConvertToARGB(drawParam.StrokeColor, renderContext, isTemplate);
            }
            else
            {
                style.StrokeColor = 0x00000000;
            }

            // 透明度（使用文本对象属性，默认完全不透明）
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

        #endregion

        #region 路径渲染

        /// <summary>
        /// 检查图元对象是否定义了绘制属性
        /// </summary>
        /// <param name="graphicUnit">图元对象</param>
        /// <returns>如果定义了绘制属性返回true，否则返回false</returns>
        /// <summary>
        /// 渲染路径对象
        /// </summary>
        /// <param name="renderCtxObj">渲染上下文对象</param>
        /// <param name="pathObj">路径对象</param>
        private void RenderPathObject(RenderContextObject renderCtxObj, object pathObj)
        {
            if (renderCtxObj?.RenderContext == null || pathObj == null)
                return;

            var renderContext = renderCtxObj.RenderContext;
            var layer = renderCtxObj.CurrentLayer;
            var isTemplate = renderCtxObj.IsTemplate;

            // 获取渲染上下文
            var pathRenderer = renderContext as IPathRenderer;
            if (pathRenderer == null)
                return;

            var pathObject = pathObj as PathObject;
            if (pathObject == null)
                return;

            // 如果路径数据为空，跳过渲染
            if (string.IsNullOrEmpty(pathObject.AbbreviatedData))
                return;


            // 获取图元外接矩形位置（页面坐标系，毫米转换为像素）
            float boundaryX = renderContext.MillimetersToPixels((float)pathObject.Boundary.X);
            float boundaryY = renderContext.MillimetersToPixels((float)pathObject.Boundary.Y);
            float boundaryWidth = renderContext.MillimetersToPixels((float)pathObject.Boundary.Width);
            float boundaryHeight = renderContext.MillimetersToPixels((float)pathObject.Boundary.Height);

            // 转换图形样式
            var graphStyle = ConvertToGraphStyle(renderCtxObj, pathObject);

            // 保存当前渲染状态
            renderContext.SaveState();

            // 绘制边界矩形（用于调试，实际渲染时可以注释掉）
            // ((SkiaRenderContext)renderContext).DrawRectangle(boundaryX, boundaryY, boundaryWidth, boundaryHeight, graphStyle);
            // 设置裁剪区（在变换后的坐标系中，使用单位矩形作为裁剪区）
            renderContext.SetClipRect(boundaryX, boundaryY, boundaryWidth, boundaryHeight);

            // 应用CTM变换矩阵（使用SKMatrix进行2D仿射变换）
            if (pathObject.CTM != null && pathObject.CTM.Count >= 6)
            {
                // 先通过 Boundary 平移到页面空间，再通过 CTM 变换到对象空间
                // Skia 中矩阵变换是 "反向作用"，因此代码中先平移再乘 CTM，效果等价于 OFD 的先 CTM 再平移
                renderContext.Translate(boundaryX, boundaryY);
                // 应用变换矩阵
                ApplyVectorSpaceTransformMatrix(renderContext, pathObject.CTM);
                // 开始绘制路径
                pathRenderer.BeginPath();
                // 解析并绘制路径
                ParseAndRenderPath(pathRenderer, renderContext, pathObject.AbbreviatedData, 0, 0);
            }
            else
            {
                // 开始绘制路径
                pathRenderer.BeginPath();
                // 解析并绘制路径
                ParseAndRenderPath(pathRenderer, renderContext, pathObject.AbbreviatedData, boundaryX, boundaryY);
            }

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
            renderContext.RestoreState();
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
        private void ParseAndRenderPath(IPathRenderer pathRenderer, IRenderContext renderContext, string abbreviatedData, float boundaryX = 0, float boundaryY = 0)
        {
            if (string.IsNullOrEmpty(abbreviatedData))
                return;

            var tokens = abbreviatedData.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 0)
                return;

            int index = 0;
            while (index < tokens.Length)
            {
                string command = tokens[index++];

                switch (command.ToUpper())
                {
                    case "S":
                        if (index + 1 < tokens.Length)
                        {
                            float x = renderContext.MillimetersToPixels(float.Parse(tokens[index])) + boundaryX;
                            float y = renderContext.MillimetersToPixels(float.Parse(tokens[index + 1])) + boundaryY;
                            pathRenderer.MoveTo(x, y);
                            index += 2;
                        }
                        break;
                    case "M":
                        if (index + 1 < tokens.Length)
                        {
                            float x = renderContext.MillimetersToPixels(float.Parse(tokens[index])) + boundaryX;
                            float y = renderContext.MillimetersToPixels(float.Parse(tokens[index + 1])) + boundaryY;
                            pathRenderer.MoveTo(x, y);
                            index += 2;
                        }
                        break;
                    case "L":
                        if (index + 1 < tokens.Length)
                        {
                            float x = renderContext.MillimetersToPixels(float.Parse(tokens[index])) + boundaryX;
                            float y = renderContext.MillimetersToPixels(float.Parse(tokens[index + 1])) + boundaryY;
                            pathRenderer.LineTo(x, y);
                            index += 2;
                        }
                        break;
                    case "Q":
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
                    case "B":
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
                    case "A":
                        if (index + 6 < tokens.Length)
                        {
                            float rx = renderContext.MillimetersToPixels(float.Parse(tokens[index]));
                            float ry = renderContext.MillimetersToPixels(float.Parse(tokens[index + 1]));
                            float angle = float.Parse(tokens[index + 2]);
                            bool largeArc = int.Parse(tokens[index + 3]) == 1;
                            bool sweep = int.Parse(tokens[index + 4]) == 1;
                            float x = renderContext.MillimetersToPixels(float.Parse(tokens[index + 5])) + boundaryX;
                            float y = renderContext.MillimetersToPixels(float.Parse(tokens[index + 6])) + boundaryY;
                            pathRenderer.ArcTo(rx, ry, angle, largeArc, sweep, x, y);
                            index += 7;
                        }
                        break;
                    case "C":
                        pathRenderer.ClosePath();
                        break;
                }
            }
            // 对路径进行归一化处理
            //pathRenderer.NormalizePath();
        }

        /// <summary>
        /// 将OFD路径对象转换为图形样式
        /// </summary>
        /// <param name="pathObject">OFD路径对象</param>
        /// <param name="layer">图层对象</param>
        /// <param name="renderContext">渲染上下文</param>
        /// <param name="isTemplate">是否为模板页</param>
        /// <returns>图形样式</returns>
        private GraphStyle ConvertToGraphStyle(RenderContextObject renderCtxObj, CT_Path pathObject)
        {
            if (renderCtxObj?.RenderContext == null || pathObject == null)
                return null;

            CT_Layer layer = renderCtxObj.CurrentLayer;
            IRenderContext renderContext= renderCtxObj.RenderContext;
            bool isTemplate = renderCtxObj.IsTemplate;

            var style = new GraphStyle();

            // 获取绘制参数（按照就近原则）
            var drawParam = GetDrawParam(pathObject, layer, renderContext, isTemplate);

            // 填充颜色（按照就近原则：图元属性 > 图元DrawParam > 图层DrawParam > 默认透明色）
            if (pathObject.FillColor != null)
            {
                style.Color = ConvertToARGB(pathObject.FillColor, renderContext, isTemplate);
            }
            else if (drawParam != null && drawParam.FillColor != null)
            {
                style.Color = ConvertToARGB(drawParam.FillColor, renderContext, isTemplate);
            }
            else
            {
                style.Color = 0x00000000;
            }

            // 透明度（使用路径对象属性，默认完全不透明）
            style.Alpha = (byte)pathObject.Alpha;

            // 描边颜色（按照就近原则：图元属性 > 图元DrawParam > 图层DrawParam > 默认黑色）
            if (pathObject.StrokeColor != null)
            {
                style.StrokeColor = ConvertToARGB(pathObject.StrokeColor, renderContext, isTemplate);
            }
            else if (drawParam != null && drawParam.StrokeColor != null)
            {
                style.StrokeColor = ConvertToARGB(drawParam.StrokeColor, renderContext, isTemplate);
            }
            else
            {
                style.StrokeColor = 0xFF000000;
            }

            // 描边透明度（默认完全不透明）
            style.StrokeAlpha = 255;

            // 描边宽度（按照就近原则：图元属性 > 图元DrawParam > 图层DrawParam > 默认0）
            if (pathObject.LineWidth != 0)
            {
                style.StrokeWidth = (float)pathObject.LineWidth;
            }
            else if (drawParam != null && drawParam.LineWidth != 0.353)
            {
                style.StrokeWidth = (float)drawParam.LineWidth;
            }
            else
            {
                style.StrokeWidth = 0;
            }

            // 是否填充（使用路径对象属性）
            style.Fill = pathObject.Fill;

            // 是否描边（使用路径对象属性）
            style.Stroke = pathObject.Stroke;

            // 虚线样式（按照就近原则：图元DrawParam > 图层DrawParam）
            if (drawParam != null && drawParam.DashPattern != null)
            {
                var dashArray = drawParam.DashPattern.ToDoubleArray();
                if (dashArray != null)
                {
                    style.DashPattern = new float[dashArray.Length];
                    for (int i = 0; i < dashArray.Length; i++)
                    {
                        style.DashPattern[i] = (float)dashArray[i];
                    }
                }
                else
                {
                    style.DashPattern = null;
                }
            }
            else
            {
                style.DashPattern = null;
            }

            return style;
        }

        #endregion

        #region 图像渲染

        /// <summary>
        /// 渲染图像对象
        /// </summary>
        /// <param name="renderCtxObj">渲染上下文对象</param>
        /// <param name="imageObj">图像对象</param>
        private void RenderImageObject(RenderContextObject renderCtxObj, object imageObj)
        {
            if (renderCtxObj?.RenderContext == null || imageObj == null)
                return;

            var renderContext = renderCtxObj.RenderContext;
            var layer = renderCtxObj.CurrentLayer;
            var isTemplate = renderCtxObj.IsTemplate;

            // 获取渲染上下文
            var imageRenderer = renderContext as IImageRenderer;
            if (imageRenderer == null)
                return;

            var imageObject = imageObj as ImageObject;
            if (imageObject == null)
                return;

            // 转换图像样式
            var imageStyle = ConvertToImageStyle(renderCtxObj, imageObject);

            // 获取图像位置和大小（OFD坐标，单位：毫米）
            // 将毫米转换为像素
            float boundaryX = renderContext.MillimetersToPixels((float)imageObject.Boundary.X);
            float boundaryY = renderContext.MillimetersToPixels((float)imageObject.Boundary.Y);
            float width = renderContext.MillimetersToPixels((float)imageObject.Boundary.Width);
            float height = renderContext.MillimetersToPixels((float)imageObject.Boundary.Height);

            // 保存当前渲染状态
            renderContext.SaveState();

            // 设置裁剪区（使用图元的外接矩形作为默认裁剪区）
            renderContext.SetClipRect(boundaryX, boundaryY, width, height);

            // 应用CTM变换矩阵（使用SKMatrix进行2D仿射变换）
            if (imageObject.CTM != null && imageObject.CTM.Count >= 6)
            {
                // 先通过 Boundary 平移到页面空间，再通过 CTM 变换到对象空间
                // Skia 中矩阵变换是 "反向作用"，因此代码中先平移再乘 CTM，效果等价于 OFD 的先 CTM 再平移
                renderContext.Translate(boundaryX, boundaryY);
                // 应用变换矩阵
                ApplyDeviceSpaceTransformMatrix(renderContext, imageObject.CTM);
            }

            // 从资源管理器获取图像数据
            byte[] imageData = null;
            if (renderContext.ResourceManager != null && imageObject.ResourceID != null)
            {
                // 首先尝试获取多媒体资源对象
                var multiMedia = renderContext.ResourceManager.GetResource<MultiMedia>(imageObject.ResourceID.ToString());
                if (multiMedia != null && multiMedia.MediaFile != null)
                {
                    // 从资源文件获取图像数据
                    imageData = renderContext.ResourceManager.GetResourceFile(multiMedia.MediaFile.Path);

                    // SkiaSharp 不支持 TIFF,需要处理,
                    if (multiMedia.FormatString == "TIFF" || multiMedia.MediaFile.Path.ToUpper().Contains(".TIF"))
                    {
                        // 转换为 PNG 格式
                        imageData = ConvertTIFF2PNG(imageData);
                    }
                    // 处理 JB2 格式图片
                    else if (multiMedia.FormatString == "GBIG2" || multiMedia.MediaFile.Path.ToUpper().Contains(".JB2"))
                    {
                        // 转换为 PNG 格式
                        imageData = ConvertJB2ToPNG(imageData);
                    }
                    else
                    { }
                }
            }

            // 如果图像数据为空，跳过渲染
            if (imageData == null || imageData.Length == 0)
            {
                // 恢复渲染状态
                renderContext.RestoreState();
                return;
            }


            if (imageObject.CTM != null && imageObject.CTM.Count >= 6)
            {
                // 图元原始坐标：(0,0) 到 (1,1)（归一化尺寸），Skia 会自动应用矩阵变换
                imageRenderer.DrawImage(0, 0, 1, 1, imageData, imageStyle);
            }
            else
            {
                imageRenderer.DrawImage(boundaryX, boundaryY, width, height, imageData, imageStyle);
            }

            // 恢复渲染状态
            renderContext.RestoreState();
        }

        /// <summary>
        /// 将OFD图像对象转换为图像样式
        /// </summary>
        /// <param name="imageObject">OFD图像对象</param>
        /// <param name="layer">图层对象</param>
        /// <param name="renderContext">渲染上下文</param>
        /// <param name="isTemplate">是否为模板页</param>
        /// <returns>图像样式</returns>
        private ImageStyle ConvertToImageStyle(RenderContextObject renderCtxObj, CT_Image imageObject)
        {
            if (renderCtxObj?.RenderContext == null || imageObject == null)
                return null;

            IRenderContext renderContext = renderCtxObj.RenderContext; 
            bool isTemplate = renderCtxObj.IsTemplate;

            var style = new ImageStyle
            {
                // 图像插值模式
                InterpolationMode = ImageInterpolationMode.HighQuality,
                // 保持纵横比
                PreserveAspectRatio = true
            };

            // 获取绘制参数（按照就近原则）
            var drawParam = GetDrawParam(imageObject, null, renderContext, isTemplate) as CT_DrawParam;

            // 透明度（使用图像对象属性，默认完全不透明）
            style.Alpha = 255;

            return style;
        }

        #endregion

        #region 复合对象渲染

        /// <summary>
        /// 渲染复合对象
        /// </summary>
        /// <param name="renderCtxObj">渲染上下文对象</param>
        /// <param name="compositeObj">复合对象</param>
        private void RenderCompositeObject(RenderContextObject renderCtxObj, object compositeObj)
        {
            if (renderCtxObj?.RenderContext == null || compositeObj == null)
                return;

            var renderContext = renderCtxObj.RenderContext;
            var layer = renderCtxObj.CurrentLayer;
            var isTemplate = renderCtxObj.IsTemplate;

            var compositeObject = compositeObj as CompositeObject;
            if (compositeObject == null)
                return;

            // 从资源管理器获取复合对象内容
            if (renderContext.ResourceManager != null && compositeObject.ResourceID != null)
            {
                // 获取矢量图形资源
                var vectorGraphic = isTemplate ? renderContext.ResourceManager.GetTemplateResource<CompositeGraphicUnit>(compositeObject.ResourceID.ToString())
                    : renderContext.ResourceManager.GetResource<CompositeGraphicUnit>(compositeObject.ResourceID.ToString());
                if (vectorGraphic != null && vectorGraphic.Content != null)
                {
                    // 保存当前渲染状态
                    renderContext.SaveState();

                    // 获取复合对象位置和大小（OFD坐标，单位：毫米
                    float boundaryX = renderContext.MillimetersToPixels((float)compositeObject.Boundary.X);
                    float boundaryY = renderContext.MillimetersToPixels((float)compositeObject.Boundary.Y);
                    float width = renderContext.MillimetersToPixels((float)compositeObject.Boundary.Width);
                    float height = renderContext.MillimetersToPixels((float)compositeObject.Boundary.Height);

                    // 应用变换
                    renderContext.Translate(boundaryX, boundaryY);

                    // 递归渲染复合对象中的子对象
                    if (vectorGraphic.Content.PageBlockItems != null && vectorGraphic.Content.PageBlockItems.Count > 0)
                    {
                        foreach (var childBlock in vectorGraphic.Content.PageBlockItems)
                        {
                            RenderPageBlock(renderCtxObj, childBlock);
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
        /// <param name="renderCtxObj">渲染上下文对象</param>
        /// <param name="pageBlock">页面块对象</param>
        private void RenderPageBlockObject(RenderContextObject renderCtxObj, object pageBlock)
        {
            if (renderCtxObj?.RenderContext == null || pageBlock == null)
                return;

            var renderContext = renderCtxObj.RenderContext;
            var layer = renderCtxObj.CurrentLayer;
            var isTemplate = renderCtxObj.IsTemplate;

            var pageBlockObj = pageBlock as PageBlock;
            if (pageBlockObj == null)
                return;

            // 保存当前渲染状态
            renderContext.SaveState();

            // 遍历页面块中的所有子元素并递归渲染
            if (pageBlockObj.PageBlockItems != null && pageBlockObj.PageBlockItems.Count > 0)
            {
                foreach (var childBlock in pageBlockObj.PageBlockItems)
                {
                    RenderPageBlock(renderCtxObj, childBlock);
                }
            }

            // 恢复渲染状态
            renderContext.RestoreState();
        }

        #endregion

        #region 颜色处理

        /// <summary>
        /// 将OFD颜色转换为ColorARGB格式
        /// </summary>
        /// <param name="ofdColor">OFD颜色对象</param>
        /// <param name="renderContext">渲染上下文</param>
        /// <param name="isTemplate">是否为模板页</param>
        /// <returns>ColorARGB颜色值</returns>
        private ColorARGB ConvertToARGB(CT_Color ofdColor, IRenderContext renderContext, bool isTemplate = false)
        {
            // 默认颜色为黑色
            if (ofdColor == null)
                return ColorARGB.Black;
            
            // 获取透明度（0-255）
            byte alpha = (byte)(ofdColor.Alpha >= 0 && ofdColor.Alpha <= 255 ? ofdColor.Alpha : 255);
            
            // 获取颜色空间
            CT_ColorSpace colorSpace = GetColorSpace(ofdColor, renderContext, isTemplate);
            
            // 获取颜色值
            ST_Array colorValue = ofdColor.Value;
            if (colorValue == null || colorValue.Count == 0)
            {
                // 如果没有颜色值，使用黑色
                // todo:此属性不出现时, 应采用Index属性从颜色空间的调色板中的取值。 当二者都不出现时, 该颜色各通道的值全部为0
                return new ColorARGB(alpha, 0, 0, 0);
            }
            
            // 根据颜色空间类型转换颜色
            byte r = 0, g = 0, b = 0;
            switch (colorSpace.Type)
            {
                case ColorSpaceType.GRAY:
                    // 处理灰度颜色
                    r = g = b = ParseGrayColor(colorValue);
                    break;
                
                case ColorSpaceType.RGB:
                    // 处理RGB颜色
                    var rgb = ParseRGBColor(colorValue);
                    r = rgb[0];
                    g = rgb[1];
                    b = rgb[2];
                    break;
                
                case ColorSpaceType.CMYK:    
                    // 处理CMYK颜色
                    var cmyk = ParseCMYKColor(colorValue);
                    // CMYK转RGB
                    (r, g, b) = CMYKToRGB(cmyk[0], cmyk[1], cmyk[2], cmyk[3]);
                    break;
                
                default:
                    // 默认使用RGB
                    if (colorValue.Count >= 3)
                    {
                        r = ParseColorComponent(colorValue[0]);
                        g = ParseColorComponent(colorValue[1]);
                        b = ParseColorComponent(colorValue[2]);
                    }
                    break;
            }
            
            return new ColorARGB(alpha, r, g, b);
        }
        
        /// <summary>
        /// 获取颜色空间
        /// </summary>
        /// <param name="ofdColor">OFD颜色对象</param>
        /// <param name="renderContext">渲染上下文</param>
        /// <param name="isTemplate">是否为模板页</param>
        /// <returns>颜色空间对象</returns>
        private CT_ColorSpace GetColorSpace(CT_Color ofdColor, IRenderContext renderContext = null, bool isTemplate = false)
        {
            // 1. 首先尝试从ofdColor.ColorSpace获取颜色空间资源
            if (ofdColor.ColorSpace != null && ofdColor.ColorSpace != ST_RefID.Invalid && renderContext != null)
            {
                var colorSpace = isTemplate
                    ? renderContext.ResourceManager.GetTemplateResource<CT_ColorSpace>(ofdColor.ColorSpace.ToString())
                    : renderContext.ResourceManager.GetResource<CT_ColorSpace>(ofdColor.ColorSpace.ToString());
                
                if (colorSpace != null)
                {
                    return colorSpace;
                }
            }
            
            // 2. 尝试从文档的DefaultCS获取默认颜色空间
            if (RootDocument != null && RootDocument.DefaultOFDDocument != null 
                && RootDocument.DefaultOFDDocument.Document != null 
                && RootDocument.DefaultOFDDocument.Document.CommonData != null 
                && RootDocument.DefaultOFDDocument.Document.CommonData.DefaultCS != null 
                && RootDocument.DefaultOFDDocument.Document.CommonData.DefaultCS != ST_RefID.Invalid 
                && renderContext != null)
            {
                var defaultCSId = RootDocument.DefaultOFDDocument.Document.CommonData.DefaultCS.ToString();
                var colorSpace = isTemplate
                    ? renderContext.ResourceManager.GetTemplateResource<CT_ColorSpace>(defaultCSId)
                    : renderContext.ResourceManager.GetResource<CT_ColorSpace>(defaultCSId);
                
                if (colorSpace != null)
                {
                    return colorSpace;
                }
            }
            
            // 3. 如果为空，使用RGB作为默认颜色空间
            return new CT_ColorSpace { Type = ColorSpaceType.RGB };
        }
        
        /// <summary>
        /// 解析灰度颜色
        /// </summary>
        /// <param name="colorValue">颜色值</param>
        /// <returns>灰度值（0-255）</returns>
        private byte ParseGrayColor(ST_Array colorValue)
        {
            if (colorValue.Count == 0)
                return 0;
            
            return ParseColorComponent(colorValue[0]);
        }
        
        /// <summary>
        /// 解析RGB颜色
        /// </summary>
        /// <param name="colorValue">颜色值</param>
        /// <returns>RGB颜色数组（0-255）</returns>
        private byte[] ParseRGBColor(ST_Array colorValue)
        {
            byte[] rgb = new byte[3] { 0, 0, 0 };
            
            if (colorValue.Count >= 1)
                rgb[0] = ParseColorComponent(colorValue[0]);
            if (colorValue.Count >= 2)
                rgb[1] = ParseColorComponent(colorValue[1]);
            if (colorValue.Count >= 3)
                rgb[2] = ParseColorComponent(colorValue[2]);
            
            return rgb;
        }
        
        /// <summary>
        /// 解析CMYK颜色
        /// </summary>
        /// <param name="colorValue">颜色值</param>
        /// <returns>CMYK颜色数组（0-255）</returns>
        private byte[] ParseCMYKColor(ST_Array colorValue)
        {
            byte[] cmyk = new byte[4] { 0, 0, 0, 0 };
            
            if (colorValue.Count >= 1)
                cmyk[0] = ParseColorComponent(colorValue[0]);
            if (colorValue.Count >= 2)
                cmyk[1] = ParseColorComponent(colorValue[1]);
            if (colorValue.Count >= 3)
                cmyk[2] = ParseColorComponent(colorValue[2]);
            if (colorValue.Count >= 4)
                cmyk[3] = ParseColorComponent(colorValue[3]);
            
            return cmyk;
        }
        
        /// <summary>
        /// 解析颜色分量
        /// 支持十六进制（如 "#FF"）和十进制（如 "255"）格式
        /// </summary>
        /// <param name="component">颜色分量</param>
        /// <returns>颜色分量值（0-255）</returns>
        private byte ParseColorComponent(object component)
        {
            if (component == null)
                return 0;
            
            string value = component.ToString().Trim();
            
            // 处理十六进制格式
            if (value.StartsWith("#"))
            {
                // 移除 # 前缀
                value = value.Substring(1);
                
                // 解析十六进制值
                if (byte.TryParse(value, System.Globalization.NumberStyles.HexNumber, null, out byte result))
                {
                    return result;
                }
            }
            
            // 处理十进制格式
            if (byte.TryParse(value, out byte decimalResult))
            {
                return decimalResult;
            }
            
            return 0;
        }
        
        /// <summary>
        /// CMYK转RGB
        /// </summary>
        /// <param name="c">青（0-255）</param>
        /// <param name="m">品红（0-255）</param>
        /// <param name="y">黄（0-255）</param>
        /// <param name="k">黑（0-255）</param>
        /// <returns>RGB颜色（0-255）</returns>
        private (byte, byte, byte) CMYKToRGB(byte c, byte m, byte y, byte k)
        {
            // 将CMYK值转换为0-1范围
            double c1 = c / 255.0;
            double m1 = m / 255.0;
            double y1 = y / 255.0;
            double k1 = k / 255.0;
            
            // CMYK转RGB公式
            double r1 = 1 - Math.Min(1, c1 * (1 - k1) + k1);
            double g1 = 1 - Math.Min(1, m1 * (1 - k1) + k1);
            double b1 = 1 - Math.Min(1, y1 * (1 - k1) + k1);
            
            // 将RGB值转换为0-255范围
            byte r = (byte)(r1 * 255);
            byte g = (byte)(g1 * 255);
            byte b = (byte)(b1 * 255);
            
            return (r, g, b);
        }

        #endregion

        #region 变换矩阵处理

        /// <summary>
        /// 应用变换矩阵到渲染上下文（设备空间）
        /// </summary>
        /// <param name="renderContext">渲染上下文</param>
        /// <param name="ctm">CTM变换矩阵（OFD坐标系，单位：毫米）</param>
        private void ApplyDeviceSpaceTransformMatrix(IRenderContext renderContext, ST_Array ctm)
        {
            // 应用CTM变换矩阵（使用SKMatrix进行2D仿射变换）
            if (ctm != null && ctm.Count >= 6)
            {
                // OFD中的CTM矩阵形式（3x3仿射变换矩阵）最后一列为[0, 0, 1]：
                // | scaleX  skewX  0 |
                // | skewY   scaleY 0 |
                // | transX  transY 1 |

                // 其中：
                // scaleX: X轴缩放因子
                // skewX: X轴倾斜因子（Y轴方向的倾斜）
                // skewY: Y轴倾斜因子（X轴方向的倾斜）
                // scaleY: Y轴缩放因子
                // transX: X轴平移量
                // transY: Y轴平移量

                var ctmArray = ctm.ToDoubleArray();
                // 获取CTM矩阵参数
                float scaleX = renderContext.MillimetersToPixels((float)ctmArray[0]);
                float skewX = renderContext.MillimetersToPixels((float)ctmArray[1]);
                float skewY = renderContext.MillimetersToPixels((float)ctmArray[2]);
                float scaleY = renderContext.MillimetersToPixels((float)ctmArray[3]);
                float transX = renderContext.MillimetersToPixels((float)ctmArray[4]);
                float transY = renderContext.MillimetersToPixels((float)ctmArray[5]);

                // 创建SKMatrix（注意：SKMatrix的顺序与OFD矩阵不同）
                // SKMatrix的形式：
                // | ScaleX  SkewX  TransX |
                // | SkewY   ScaleY TransY |
                // | 0       0      1      |

                // 对应关系：
                // ScaleX = scaleX (X轴缩放)
                // SkewX = skewX (X轴倾斜)
                // SkewY = skewY (Y轴倾斜)
                // ScaleY = scaleY (Y轴缩放)
                // TransX = transX (X轴平移)
                // TransY = transY (Y轴平移)

                var matrix = new SkiaSharp.SKMatrix
                {
                    ScaleX = scaleX,
                    SkewX = skewX,
                    SkewY = skewY,
                    ScaleY = scaleY,
                    TransX = transX,
                    TransY = transY,
                    Persp0 = 0,
                    Persp1 = 0,
                    Persp2 = 1
                };

                // 直接获取SkiaRenderContext并应用矩阵变换
                var skiaContext = renderContext as SkiaRenderContext;
                if (skiaContext != null)
                {
                    skiaContext.ConcatMatrix(matrix);
                }
            }
        }


        /// <summary>
        /// 应用变换矩阵到渲染上下文（矢量空间）
        /// </summary>
        /// <param name="renderContext">渲染上下文</param>
        /// <param name="ctm">CTM变换矩阵（OFD坐标系，单位：毫米）</param>
        private void ApplyVectorSpaceTransformMatrix(IRenderContext renderContext, ST_Array ctm)
        {
            // 应用CTM变换矩阵（使用SKMatrix进行2D仿射变换）
            if (ctm != null && ctm.Count >= 6)
            {
                // OFD中的CTM矩阵形式（3x3仿射变换矩阵）最后一列为[0, 0, 1]：
                // | scaleX  skewX  0 |
                // | skewY   scaleY 0 |
                // | transX  transY 1 |

                // 其中：
                // scaleX: X轴缩放因子
                // skewX: X轴倾斜因子（Y轴方向的倾斜）
                // skewY: Y轴倾斜因子（X轴方向的倾斜）
                // scaleY: Y轴缩放因子
                // transX: X轴平移量
                // transY: Y轴平移量

                var ctmArray = ctm.ToDoubleArray();
                // 获取CTM矩阵参数
                float scaleX = (float)ctmArray[0];
                float skewX = (float)ctmArray[1];
                float skewY = (float)ctmArray[2];
                float scaleY = (float)ctmArray[3];
                float transX = (float)ctmArray[4];
                float transY = (float)ctmArray[5];

                // 创建SKMatrix（注意：SKMatrix的顺序与OFD矩阵不同）
                // SKMatrix的形式：
                // | ScaleX  SkewX  TransX |
                // | SkewY   ScaleY TransY |
                // | 0       0      1      |

                // 对应关系：
                // ScaleX = scaleX (X轴缩放)
                // SkewX = skewX (X轴倾斜)
                // SkewY = skewY (Y轴倾斜)
                // ScaleY = scaleY (Y轴缩放)
                // TransX = transX (X轴平移)
                // TransY = transY (Y轴平移)

                var matrix = new SkiaSharp.SKMatrix
                {
                    ScaleX = scaleX,
                    SkewX = skewX,
                    SkewY = skewY,
                    ScaleY = scaleY,
                    TransX = transX,
                    TransY = transY,
                    Persp0 = 0,
                    Persp1 = 0,
                    Persp2 = 1
                };

                // 直接获取SkiaRenderContext并应用矩阵变换
                var skiaContext = renderContext as SkiaRenderContext;
                if (skiaContext != null)
                {
                    skiaContext.ConcatMatrix(matrix);
                }
            }
        }


        #endregion

        #region 绘制参数处理

        /// <summary>
        /// 解析绘制参数的基础绘制参数
        /// 递归处理Relative属性，合并基础绘制参数的属性
        /// </summary>
        /// <param name="drawParam">绘制参数对象</param>
        /// <param name="renderContext">渲染上下文</param>
        /// <param name="isTemplate">是否为模板页</param>
        /// <returns>合并后的绘制参数对象</returns>
        private CT_DrawParam ResolveDrawParamWithRelative(CT_DrawParam drawParam, IRenderContext renderContext, bool isTemplate = false)
        {
            if (drawParam == null)
                return null;

            // 如果没有Relative属性，直接返回
            if (drawParam.Relative == null || drawParam.Relative == ST_RefID.Invalid)
                return drawParam;

            // 递归获取基础绘制参数
            CT_DrawParam baseDrawParam = isTemplate
                ? renderContext.ResourceManager.GetTemplateResource<CT_DrawParam>(drawParam.Relative.ToString())
                : renderContext.ResourceManager.GetResource<CT_DrawParam>(drawParam.Relative.ToString());

            if (baseDrawParam == null)
                return drawParam;

            // 递归处理基础绘制参数的基础绘制参数
            baseDrawParam = ResolveDrawParamWithRelative(baseDrawParam, renderContext, isTemplate);

            // 创建合并后的绘制参数
            var mergedDrawParam = new CT_DrawParam();

            // 合并填充颜色（基础绘制参数的属性会被当前绘制参数覆盖）
            mergedDrawParam.FillColor = drawParam.FillColor ?? baseDrawParam.FillColor;

            // 合并勾边颜色
            mergedDrawParam.StrokeColor = drawParam.StrokeColor ?? baseDrawParam.StrokeColor;

            // 合并线宽
            mergedDrawParam.LineWidth = drawParam.LineWidth != 0.353 ? drawParam.LineWidth : baseDrawParam.LineWidth;

            // 合并线条连接样式
            mergedDrawParam.Join = drawParam.Join != DrawParamJoinType.Miter ? drawParam.Join : baseDrawParam.Join;

            // 合并线端点样式
            mergedDrawParam.Cap = drawParam.Cap != DrawParamCapType.Butt ? drawParam.Cap : baseDrawParam.Cap;

            // 合并虚线偏移
            mergedDrawParam.DashOffset = drawParam.DashOffset != 0 ? drawParam.DashOffset : baseDrawParam.DashOffset;

            // 合并虚线样式
            mergedDrawParam.DashPattern = drawParam.DashPattern.Count > 0 ? drawParam.DashPattern : baseDrawParam.DashPattern;

            // 合并MiterLimit
            mergedDrawParam.MiterLimit = drawParam.MiterLimit != 4.234 ? drawParam.MiterLimit : baseDrawParam.MiterLimit;

            return mergedDrawParam;
        }

        /// <summary>
        /// 获取绘制参数（按照就近原则）
        /// </summary>
        /// <param name="graphicUnit">图元对象</param>
        /// <param name="layer">图层对象</param>
        /// <param name="renderContext">渲染上下文</param>
        /// <param name="isTemplate">是否为模板页</param>
        /// <returns>合并后的绘制参数对象，包含所有有效的属性</returns>
        /// <remarks>
        /// 绘制参数的作用顺序应采用就近原则，针对每个属性分别判断：
        /// 1. 图元对象（CT_GraphicUnit）已经定义该绘制属性时，使用图元定义的属性
        /// 2. 图元未定义该属性时，检查图元的DrawParam中是否有该属性
        /// 3. 图元的DrawParam中也没有该属性时，检查所在图层的DrawParam中是否有该属性
        /// 4. 都没有定义该属性时，使用该属性的默认值
        /// 
        /// 基础绘制参数处理：
        /// 如果绘制参数有Relative属性，需要递归获取基础绘制参数并合并属性
        /// </remarks>
        private CT_DrawParam GetDrawParam(CT_GraphicUnit graphicUnit, CT_Layer layer, IRenderContext renderContext, bool isTemplate = false)
        {
            var mergedDrawParam = new CT_DrawParam();

            // 获取图元的DrawParam
            CT_DrawParam graghicUnitDrawParam = null;
            if (graphicUnit.DrawParam != null && graphicUnit.DrawParam != ST_RefID.Invalid)
            {
                graghicUnitDrawParam = isTemplate 
                    ? renderContext.ResourceManager.GetTemplateResource<CT_DrawParam>(graphicUnit.DrawParam.ToString())
                    : renderContext.ResourceManager.GetResource<CT_DrawParam>(graphicUnit.DrawParam.ToString());
                
                // 处理基础绘制参数
                if (graghicUnitDrawParam != null)
                {
                    graghicUnitDrawParam = ResolveDrawParamWithRelative(graghicUnitDrawParam, renderContext, isTemplate);
                }
            }

            // 获取图层的DrawParam
            CT_DrawParam layerDrawParam = null;
            if (layer != null && layer.DrawParam != null && layer.DrawParam != ST_RefID.Invalid)
            {
                layerDrawParam = isTemplate
                    ? renderContext.ResourceManager.GetTemplateResource<Models.BaseStructure.Resources.ResItems.DrawParam>(layer.DrawParam.ToString())
                    : renderContext.ResourceManager.GetResource<Models.BaseStructure.Resources.ResItems.DrawParam>(layer.DrawParam.ToString());
                
                // 处理基础绘制参数
                if (layerDrawParam != null)
                {
                    layerDrawParam = ResolveDrawParamWithRelative(layerDrawParam, renderContext, isTemplate);
                }
            }

            // 合并填充颜色（按照就近原则）
            if (graghicUnitDrawParam != null && graghicUnitDrawParam.FillColor != null)
            {
                mergedDrawParam.FillColor = graghicUnitDrawParam.FillColor;
            }
            else if (layerDrawParam != null && layerDrawParam.FillColor != null)
            {
                mergedDrawParam.FillColor = layerDrawParam.FillColor;
            }

            // 合并勾边颜色（按照就近原则）
            if (graghicUnitDrawParam != null && graghicUnitDrawParam.StrokeColor != null)
            {
                mergedDrawParam.StrokeColor = graghicUnitDrawParam.StrokeColor;
            }
            else if (layerDrawParam != null && layerDrawParam.StrokeColor != null)
            {
                mergedDrawParam.StrokeColor = layerDrawParam.StrokeColor;
            }

            // 合并线宽（按照就近原则）
            if (graghicUnitDrawParam != null && graghicUnitDrawParam.LineWidth != 0.353)
            {
                mergedDrawParam.LineWidth = graghicUnitDrawParam.LineWidth;
            }
            else if (layerDrawParam != null && layerDrawParam.LineWidth != 0.353)
            {
                mergedDrawParam.LineWidth = layerDrawParam.LineWidth;
            }

            // 合并线条连接样式（按照就近原则）
            if (graghicUnitDrawParam != null && graghicUnitDrawParam.Join != DrawParamJoinType.Miter)
            {
                mergedDrawParam.Join = graghicUnitDrawParam.Join;
            }
            else if (layerDrawParam != null && layerDrawParam.Join != DrawParamJoinType.Miter)
            {
                mergedDrawParam.Join = layerDrawParam.Join;
            }

            // 合并线端点样式（按照就近原则）
            if (graghicUnitDrawParam != null && graghicUnitDrawParam.Cap != DrawParamCapType.Butt)
            {
                mergedDrawParam.Cap = graghicUnitDrawParam.Cap;
            }
            else if (layerDrawParam != null && layerDrawParam.Cap != DrawParamCapType.Butt)
            {
                mergedDrawParam.Cap = layerDrawParam.Cap;
            }

            // 合并虚线偏移（按照就近原则）
            if (graghicUnitDrawParam != null && graghicUnitDrawParam.DashOffset != 0)
            {
                mergedDrawParam.DashOffset = graghicUnitDrawParam.DashOffset;
            }
            else if (layerDrawParam != null && layerDrawParam.DashOffset != 0)
            {
                mergedDrawParam.DashOffset = layerDrawParam.DashOffset;
            }

            // 合并虚线样式（按照就近原则）
            if (graghicUnitDrawParam != null && graghicUnitDrawParam.DashPattern != null)
            {
                mergedDrawParam.DashPattern = graghicUnitDrawParam.DashPattern;
            }
            else if (layerDrawParam != null && layerDrawParam.DashPattern != null)
            {
                mergedDrawParam.DashPattern = layerDrawParam.DashPattern;
            }

            // 合并MiterLimit（按照就近原则）
            if (graghicUnitDrawParam != null && graghicUnitDrawParam.MiterLimit != 4.234)
            {
                mergedDrawParam.MiterLimit = graghicUnitDrawParam.MiterLimit;
            }
            else if (layerDrawParam != null && layerDrawParam.MiterLimit != 4.234)
            {
                mergedDrawParam.MiterLimit = layerDrawParam.MiterLimit;
            }

            return mergedDrawParam;
        }

        #endregion

        #region 文件输出方法

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

        #region 注释渲染

        /// <summary>
        /// 渲染页面注释
        /// 根据注释元素 Annot 的定义，渲染注释的外观（Appearance）
        /// </summary>
        /// <param name="renderCtxObj">渲染上下文对象</param>
        /// <param name="pageAnnot">页面注释文档对象</param>
        private void RenderPageAnnot(RenderContextObject renderCtxObj, Models.Annotation.PageAnnot pageAnnot)
        {
            if (pageAnnot.Annotations == null || pageAnnot.Annotations.Count == 0)
                return;

            var renderContext = renderCtxObj.RenderContext;

            // 遍历所有注释元素
            foreach (var annot in pageAnnot.Annotations)
            {
                // 检查注释是否可见
                if (!annot.Visible)
                    continue;

                // 检查注释的外观是否存在
                if (annot.Appearance == null)
                    continue;

                // 保存当前渲染状态
                renderContext.SaveState();

                try
                {
                    // 根据注释类型进行不同的处理
                    switch (annot.Type)
                    {
                        case Models.Annotation.AnnotationType.Link:
                            // 链接注释：渲染外观（如果有）
                            RenderAnnotAppearance(renderCtxObj, annot);
                            break;

                        case Models.Annotation.AnnotationType.Path:
                            // 路径注释：一般为图形对象，渲染外观
                            RenderAnnotAppearance(renderCtxObj, annot);
                            break;

                        case Models.Annotation.AnnotationType.Highlight:
                            // 高亮注释：渲染高亮效果
                            RenderHighlightAnnot(renderCtxObj, annot);
                            break;

                        case Models.Annotation.AnnotationType.Stamp:
                            // 签章注释：渲染电子印章
                            RenderStampAnnot(renderCtxObj, annot);
                            break;

                        case Models.Annotation.AnnotationType.Watermark:
                            // 水印注释：渲染水印
                            RenderAnnotAppearance(renderCtxObj, annot);
                            break;

                        default:
                            // 其他类型：默认渲染外观
                            RenderAnnotAppearance(renderCtxObj, annot);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    // 记录错误但继续渲染其他注释
                    System.Diagnostics.Debug.WriteLine($"渲染注释 {annot.ID} 失败: {ex.Message}");
                }
                finally
                {
                    // 恢复渲染状态
                    renderContext.RestoreState();
                }
            }
        }

        /// <summary>
        /// 渲染注释的外观内容
        /// 处理具有 CT_PageBlock 属性的注释类型
        /// </summary>
        /// <param name="renderCtxObj">渲染上下文对象</param>
        /// <param name="annot">注释元素</param>
        private void RenderAnnotAppearance(RenderContextObject renderCtxObj, Models.Annotation.Annot annot)
        {
            // 检查外观是否存在
            if (annot.Appearance == null)
                return;

            // 渲染外观内容
            // Appearance 继承自 CT_PageBlock，包含 PageBlockItems
            // 注意：边界框变换由 RenderPageBlock 内部处理
            if (annot.Appearance.PageBlockItems != null)
            {
                foreach (var blockItem in annot.Appearance.PageBlockItems)
                {
                    RenderPageBlock(renderCtxObj, blockItem);
                }
            }
        }

        /// <summary>
        /// 渲染高亮注释
        /// 高亮注释通常需要特殊的渲染处理，如半透明填充
        /// </summary>
        /// <param name="renderCtxObj">渲染上下文对象</param>
        /// <param name="annot">注释元素</param>
        private void RenderHighlightAnnot(RenderContextObject renderCtxObj, Models.Annotation.Annot annot)
        {
            var renderContext = renderCtxObj.RenderContext;

            // 如果有外观，先渲染外观
            if (annot.Appearance != null && annot.Appearance.PageBlockItems != null)
            {
                foreach (var blockItem in annot.Appearance.PageBlockItems)
                {
                    RenderPageBlock(renderCtxObj, blockItem);
                }
            }

            // 高亮注释的特殊处理：可以在这里添加半透明高亮效果
            // 例如，根据注释的参数或边界绘制半透明矩形
            // 这里预留扩展点，可以根据具体需求实现高亮效果
        }

        /// <summary>
        /// 渲染签章注释（电子印章）
        /// 从 SignDocument 中获取印章数据并渲染
        /// </summary>
        /// <param name="renderCtxObj">渲染上下文对象</param>
        /// <param name="annot">注释元素</param>
        private void RenderStampAnnot(RenderContextObject renderCtxObj, Models.Annotation.Annot annot)
        {
            if (annot == null)
                return;

            var renderContext = renderCtxObj.RenderContext;
            var ofdDoc = renderCtxObj.OfdDocument;

            // 1. 首先尝试渲染外观（如果存在）
            if (annot.Appearance != null && annot.Appearance.PageBlockItems != null)
            {
                foreach (var blockItem in annot.Appearance.PageBlockItems)
                {
                    RenderPageBlock(renderCtxObj, blockItem);
                }
            }

            // 2. 从 SignDocument 中获取印章数据并渲染
            if (ofdDoc?.SignDocs != null && ofdDoc.SignDocs.Count > 0)
            {
                // 查找匹配的签章文档
                foreach (var signDoc in ofdDoc.SignDocs)
                {
                    if (signDoc?.Signature?.SignedInfo?.StampAnnots == null)
                        continue;

                    // 查找与当前页面匹配的 StampAnnot
                    // StampAnnot.PageRef 是页面ID
                    uint currentPageId = renderCtxObj.CurrentPageDoc.PageId;
                    
                    // 查找匹配的 StampAnnot
                    Models.Signature.StampAnnot stampAnnot = FindMatchingStampAnnot(
                        signDoc.Signature.SignedInfo.StampAnnots, 
                        currentPageId);

                    if (stampAnnot == null)
                        continue;

                    // 获取印章数据
                    // 优先尝试 SignedValue.dat（国脉等深度封装格式）
                    byte[] sealData = signDoc.SignedValue;
                    bool isSignedValueFormat = true;

                    // 如果 SignedValue 为空，则尝试 Seal.esl
                    if (sealData == null || sealData.Length == 0)
                    {
                        sealData = signDoc.Seal;
                        isSignedValueFormat = false;
                    }

                    if (sealData == null || sealData.Length == 0)
                        continue;

                    try
                    {
                        // 解析印章数据并渲染
                        // 支持深度封装格式（SignedValue.dat）和标准格式（Seal.esl）
                        RenderSealData(renderContext, sealData, stampAnnot, annot, isSignedValueFormat);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"渲染印章失败: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// 渲染印章数据
        /// 使用电子印章解析库解析并渲染印章到指定位置
        /// </summary>
        /// <param name="renderContext">渲染上下文</param>
        /// <param name="sealData">印章二进制数据</param>
        /// <param name="stampAnnot">签章注释信息</param>
        /// <param name="annot">注释元素（可选，用于获取边界）</param>
        /// <param name="isSignedValueFormat">是否为深度封装格式（SignedValue.dat）</param>
        private async void RenderSealData(IRenderContext renderContext, byte[] sealData, Models.Signature.StampAnnot stampAnnot, Models.Annotation.Annot annot = null, bool isSignedValueFormat = false)
        {
            if (sealData == null || sealData.Length == 0)
                return;

            try
            {
                // 获取签章注释的边界位置
                ST_Box boundary = new ST_Box();
                bool hasBoundary = false;

                if (annot?.Appearance?.Boundary != null)
                {
                    boundary = annot.Appearance.Boundary;
                    hasBoundary = true;
                }
                else if (stampAnnot.Boundary != null)
                {
                    // 从 StampAnnot 的 Boundary 属性解析
                    // Boundary 格式通常是 "x y width height"
                    var boundaryStr = stampAnnot.Boundary.ToString();
                    if (!string.IsNullOrEmpty(boundaryStr))
                    {
                        boundary = ST_Box.Parse(boundaryStr);
                        hasBoundary = true;
                    }
                }

                if (!hasBoundary)
                    return;

                // 转换边界坐标为像素
                float x = renderContext.MillimetersToPixels((float)boundary.X);
                float y = renderContext.MillimetersToPixels((float)boundary.Y);
                float width = renderContext.MillimetersToPixels((float)boundary.Width);
                float height = renderContext.MillimetersToPixels((float)boundary.Height);

                // 使用电子印章解析库解析印章
                IEsealParser parser = null;
                Stream sealImageStream = null;

                try
                {
                    // 1. 记录签章数据格式类型
                    string formatType = isSignedValueFormat ? "SignedValue.dat（深度封装）" : "Seal.esl（标准格式）";
                    System.Diagnostics.Debug.WriteLine($"签章数据格式: {formatType}, 数据大小: {sealData.Length} 字节");

                    // 2. 自动检测并获取合适的解析器
                    parser = EsealParserFactory.GetParser(sealData);
                    System.Diagnostics.Debug.WriteLine($"使用解析器: {parser.ParserName}");

                    // 3. 提取印章图像（按目标尺寸）
                    sealImageStream = await parser.ExtractImageAsync(sealData, (int)width, (int)height);

                    if (sealImageStream == null || sealImageStream.Length == 0)
                    {
                        System.Diagnostics.Debug.WriteLine("无法从印章数据中提取图像");
                        return;
                    }

                    // 4. 可选：获取印章信息用于日志记录
                    try
                    {
                        var sealInfo = await parser.LoadAsync(sealData);
                        System.Diagnostics.Debug.WriteLine($"印章信息: {sealInfo?.SealName}, 类型: {sealInfo?.SealType}");
                    }
                    catch
                    {
                        // 获取信息失败不影响渲染
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"印章解析失败: {ex.Message}，尝试使用默认方式渲染");

                    // 如果解析失败，尝试直接作为图像渲染
                    sealImageStream = new MemoryStream(sealData);
                }

                // 4. 使用 IImageRenderer 绘制印章
                var imageRenderer = renderContext as IImageRenderer;
                if (imageRenderer != null && sealImageStream != null)
                {
                    try
                    {
                        // 创建图像样式
                        var imageStyle = new ImageStyle();

                        // 使用 DrawImage 方法绘制印章
                        imageRenderer.DrawImage(x, y, width, height, sealImageStream, imageStyle);
                    }
                    finally
                    {
                        // 确保释放图像流
                        sealImageStream?.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"渲染印章数据失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 查找匹配的签章注释
        /// </summary>
        /// <param name="stampAnnots">签章注释列表</param>
        /// <param name="pageId">页面ID</param>
        /// <param name="annotId">注释ID</param>
        /// <returns>匹配的签章注释，如果未找到返回null</returns>
        private Models.Signature.StampAnnot FindMatchingStampAnnot(
            List<Models.Signature.StampAnnot> stampAnnots,
            uint pageId)
        {
            if (stampAnnots == null)
                return null;

            foreach (var sa in stampAnnots)
            {
                // 使用 ReferencedId.RawValue 获取 uint 类型的页面ID
                if (sa.PageRef.ReferencedId.RawValue == pageId)
                {
                        return sa;
                }
            }

            return null;
        }

        #endregion

        #region 图像格式转换

        /// <summary>
        /// 将TIFF格式的图像数据转换为PNG格式
        /// 该方法使用LibTiff.Net库来处理TIFF图像，支持多种TIFF格式
        /// </summary>
        /// <param name="imageData">TIFF格式的图像数据</param>
        /// <returns>PNG格式的图像数据</returns>
        /// <exception cref="ArgumentNullException">当imageData为null或空时抛出</exception>
        /// <exception cref="InvalidOperationException">当无法打开或读取TIFF图像时抛出</exception>
        private byte[] ConvertTIFF2PNG(byte[] imageData)
        {
            // 验证输入参数
            if (imageData == null || imageData.Length == 0)
                throw new ArgumentNullException(nameof(imageData), "图像数据不能为空");

            try
            {
                // 创建TIFF流
                using var tiffStream = new MemoryStream(imageData);
                
                // 使用LibTiff.Net打开TIFF图像
                using var tiff = BitMiracle.LibTiff.Classic.Tiff.ClientOpen("TIFF", "r", tiffStream,
                    new BitMiracle.LibTiff.Classic.TiffStream());

                if (tiff == null)
                    throw new InvalidOperationException("无法打开TIFF图像，可能是损坏的TIFF文件或不支持的格式");

                // 获取图像尺寸
                int width = tiff.GetField(BitMiracle.LibTiff.Classic.TiffTag.IMAGEWIDTH)[0].ToInt();
                int height = tiff.GetField(BitMiracle.LibTiff.Classic.TiffTag.IMAGELENGTH)[0].ToInt();

                // 验证图像尺寸
                if (width <= 0 || height <= 0)
                    throw new InvalidOperationException("无效的图像尺寸");

                // 分配RGBA缓冲区（每个像素4个字节：R, G, B, A）
                int[] rgbaBuffer = new int[width * height];

                // 使用LibTiff.Net的RGBA接口直接读取为RGBA格式
                // 这个方法是关键，它会自动处理TIFF的各种格式和压缩
                if (!tiff.ReadRGBAImageOriented(width, height, rgbaBuffer,
                    BitMiracle.LibTiff.Classic.Orientation.TOPLEFT, true))
                {
                    throw new InvalidOperationException("无法读取TIFF图像为RGBA格式");
                }

                // 创建SkiaSharp位图
                using var bitmap = new SkiaSharp.SKBitmap(width, height,
                    SkiaSharp.SKColorType.Rgba8888, SkiaSharp.SKAlphaType.Unpremul);

                // 将RGBA数据拷贝到位图
                IntPtr pixels = bitmap.GetPixels();
                System.Runtime.InteropServices.Marshal.Copy(rgbaBuffer, 0, pixels, rgbaBuffer.Length);

                // 编码为PNG格式
                using var pngStream = new MemoryStream();
                bitmap.Encode(pngStream, SkiaSharp.SKEncodedImageFormat.Png, 100);

                // 返回PNG数据
                return pngStream.ToArray();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"图像格式转换失败：{ex.Message}", ex);
            }
        }

        /// <summary>
        /// 将JB2格式图像转换为PNG格式
        /// </summary>
        /// <param name="imageData">JB2格式的图像数据</param>
        /// <returns>转换后的PNG格式图像数据</returns>
        /// <exception cref="ArgumentNullException">当imageData为null或空时抛出</exception>
        /// <exception cref="InvalidOperationException">当无法打开或读取JB2图像时抛出</exception>
        private byte[] ConvertJB2ToPNG(byte[] imageData)
        {
            // 验证输入参数
            if (imageData == null || imageData.Length == 0)
                throw new ArgumentNullException(nameof(imageData), "图像数据不能为空");

            try
            {
                // 使用JBig2Decoder.NETStandard库解析JBIG2数据
                var jbig = new JBIG2StreamDecoder();
                int width = 0;
                int height = 0;
                // the resulting 'byte[] rgbBuffer' is a RGB array
                byte[] rgbBuffer = jbig.DecodeJBIG2(imageData, out width, out height);

                // 使用System.Drawing.Bitmap创建位图
                using var bitmap = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                // 锁定位图数据
                var bmpData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, width, height),
                    System.Drawing.Imaging.ImageLockMode.WriteOnly,
                    System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                try
                {
                    // 将RGB数据拷贝到位图
                    System.Runtime.InteropServices.Marshal.Copy(rgbBuffer, 0, bmpData.Scan0, rgbBuffer.Length);
                }
                finally
                {
                    // 解锁位图数据
                    bitmap.UnlockBits(bmpData);
                }

                // 创建SkiaSharp位图 SKColorType.Rgb888x 每个像素占用 4 字节 (R,G,B,X)，而rgbBuffer 是 3 字节/像素 的 RGB 数据
                //using var bitmap = new SkiaSharp.SKBitmap(width, height,
                //    SkiaSharp.SKColorType.Rgb888x, SkiaSharp.SKAlphaType.Opaque);

                //// 将RGBA数据拷贝到位图
                //IntPtr pixels = bitmap.GetPixels();
                //System.Runtime.InteropServices.Marshal.Copy(rgbBuffer, 0, pixels, rgbBuffer.Length);

                // 编码为PNG格式
                using var pngStream = new MemoryStream();
                bitmap.Save(pngStream, System.Drawing.Imaging.ImageFormat.Png);

                // 返回PNG数据
                return pngStream.ToArray();

            }
            catch (Exception ex)
            {
                // 记录错误并返回原始数据，避免整个渲染过程失败
                System.Diagnostics.Debug.WriteLine($"JB2转换失败: {ex.Message}");
                return imageData;
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