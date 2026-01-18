using System;
using System.IO;
using OFDViewer.Parse;
using OFDViewer.Render.Abstractions;
using OFDViewer.Render.DataModels;
using OFDViewer.Render.Implementation;
using OFDViewer.Models.BaseStructure.Pages;

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
        
        #endregion

        #region 属性
        
        /// <summary>
        /// 当前OFD文档
        /// </summary>
        public OFDRootDocument CurrentDocument { get; private set; }
        
        /// <summary>
        /// 文档总页数
        /// </summary>
        public int PageCount { get; private set; }
        
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
            CurrentDocument = _ofdReader.ReadOFDDocument();
            PageCount = CalculatePageCount();
        }
        
        /// <summary>
        /// 计算文档总页数
        /// </summary>
        /// <returns>总页数</returns>
        private int CalculatePageCount()
        {
            if (CurrentDocument == null || CurrentDocument.Docs == null || CurrentDocument.Docs.Count == 0)
                return 0;
            
            var firstDoc = CurrentDocument.Docs[0];
            return firstDoc.PageDocs?.Count ?? 0;
        }
        
        /// <summary>
        /// 渲染指定页面到内存位图
        /// </summary>
        /// <param name="pageIndex">页面索引，默认为第1页</param>
        /// <returns>渲染结果（PNG格式字节数组）</returns>
        public byte[] RenderPageToBitmap(int pageIndex = 0)
        {
            if (CurrentDocument == null || CurrentDocument.Docs == null || CurrentDocument.Docs.Count == 0)
                throw new InvalidOperationException("OFD文档未加载或为空");
            
            var ofdDoc = CurrentDocument.Docs[0];
            if (ofdDoc == null || ofdDoc.Document == null)
                throw new InvalidOperationException("OFD文档结构无效");
            
            // 验证页面索引
            if (pageIndex < 0 || pageIndex >= PageCount)
                throw new ArgumentOutOfRangeException(nameof(pageIndex), "页面索引超出范围");
            
            // 获取页面尺寸（OFD标准单位：毫米）
            var pageWidth = (float)ofdDoc.Document.CommonData.PageArea.PhysicalBox.Width;
            var pageHeight = (float)ofdDoc.Document.CommonData.PageArea.PhysicalBox.Height;
            
            // 计算渲染尺寸（像素）
            var renderSize = new RenderSize
            {
                Width = pageWidth,
                Height = pageHeight,
                Dpi = _renderConfig.Dpi
            };
            
            int renderWidth = (int)renderSize.MillimetersToPixels(pageWidth);
            int renderHeight = (int)renderSize.MillimetersToPixels(pageHeight);
            
            // 创建渲染上下文
            using var renderContext = new SkiaRenderContext();
            renderContext.Config = _renderConfig;
            renderContext.Initialize(renderWidth, renderHeight);
            
            // 设置背景色为白色
            renderContext.SetBackgroundColor(0xFFFFFFFF);
            
            // 渲染页面内容
            if (ofdDoc.PageDocs != null && ofdDoc.PageDocs.Count > pageIndex)
            {
                var pageDoc = ofdDoc.PageDocs[pageIndex];
                if (pageDoc != null && pageDoc.Page != null)
                {
                    RenderPageContent(renderContext, pageDoc.Page);
                }
            }
            
            // 返回渲染结果
            return renderContext.GetRenderResult();
        }
        
        /// <summary>
        /// 渲染页面内容
        /// 遍历页面元素并调用渲染上下文的绘制方法
        /// </summary>
        /// <param name="renderContext">渲染上下文</param>
        /// <param name="page">页面对象</param>
        private void RenderPageContent(IRenderContext renderContext, Page page)
        {
            if (page == null || page.Content == null)
                return;
            
            // 遍历所有图层
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
        private void RenderPageBlock(IRenderContext renderContext, object blockItem)
        {
            if (renderContext == null || blockItem == null)
                return;
            
            // 检查渲染上下文是否实现了相应的渲染接口
            var graphicRenderer = renderContext as IGraphicRenderer;
            var textRenderer = renderContext as ITextRenderer;
            var imageRenderer = renderContext as IImageRenderer;
            var pathRenderer = renderContext as IPathRenderer;
            
            // 根据页面块类型调用相应的渲染方法
            switch (blockItem)
            {
                case Models.BaseStructure.Pages.PageBlockItems.TextObject textObj:
                    RenderTextObject(textRenderer, textObj);
                    break;
                    
                case Models.BaseStructure.Pages.PageBlockItems.PathObject pathObj:
                    RenderPathObject(pathRenderer, pathObj);
                    break;
                    
                case Models.BaseStructure.Pages.PageBlockItems.ImageObject imageObj:
                    RenderImageObject(imageRenderer, imageObj);
                    break;
                    
                case Models.BaseStructure.Pages.PageBlockItems.CompositeObject compositeObj:
                    RenderCompositeObject(renderContext, compositeObj);
                    break;
                    
                case Models.BaseStructure.Pages.PageBlockItems.PageBlock pageBlock:
                    RenderPageBlockObject(renderContext, pageBlock);
                    break;
                    
                default:
                    // 未知类型，跳过
                    break;
            }
        }
        
        /// <summary>
        /// 渲染文本对象
        /// </summary>
        /// <param name="textRenderer">文本渲染器</param>
        /// <param name="textObj">文本对象</param>
        private void RenderTextObject(ITextRenderer textRenderer, object textObj)
        {
            if (textRenderer == null || textObj == null)
                return;
            
            var textObject = textObj as Models.BaseStructure.Pages.PageBlockItems.TextObject;
            if (textObject == null)
                return;
            
            // 遍历文本内容列表
            foreach (var textCode in textObject.TextCodes)
            {
                if (string.IsNullOrEmpty(textCode.Value))
                    continue;
                
                // 转换文本样式
                var textStyle = ConvertToTextStyle(textObject);
                
                // 获取文本位置（OFD坐标，单位：毫米）
                float x = (float)textObject.Boundary.X;
                float y = (float)textObject.Boundary.Y;
                
                // 绘制文本
                textRenderer.DrawText(x, y, textCode.Value, textStyle);
            }
        }
        
        /// <summary>
        /// 将OFD文本对象转换为文本样式
        /// </summary>
        /// <param name="textObject">OFD文本对象</param>
        /// <returns>文本样式</returns>
        private TextStyle ConvertToTextStyle(Models.Font.CT_Text textObject)
        {
            var style = new TextStyle
            {
                // 字体名称（暂时使用默认字体，后续需要从资源中获取）
                FontFamily = "SimSun",
                // 字号转换：OFD毫米单位转换为像素
                FontSize = (float)(textObject.Size * _renderConfig.Dpi / 25.4f),
                // 字体粗细
                FontWeight = textObject.Weight,
                // 是否斜体
                Italic = textObject.Italic,
                // 水平缩放比例
                HScale = (float)textObject.HScale,
                // 填充颜色
                Color = ConvertToARGB(textObject.FillColor),
                // 透明度（默认完全不透明）
                Alpha = 255
            };
            
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
                return (uint)(0xFF000000 | ((uint)r << 16) | ((uint)g << 8) | b);
            }
            
            return 0xFF000000;
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
            
            // 转换图形样式
            var graphicStyle = ConvertToGraphicStyle(pathObject);
            
            // 开始绘制路径
            pathRenderer.BeginPath();
            
            // 解析并绘制路径
            ParseAndRenderPath(pathRenderer, pathObject.AbbreviatedData);
            
            // 根据样式绘制路径
            if (graphicStyle.Fill && graphicStyle.Stroke)
            {
                pathRenderer.FillAndStrokePath(graphicStyle);
            }
            else if (graphicStyle.Fill)
            {
                pathRenderer.FillPath(graphicStyle);
            }
            else if (graphicStyle.Stroke)
            {
                pathRenderer.StrokePath(graphicStyle);
            }
        }
        
        /// <summary>
        /// 解析OFD路径数据并调用路径渲染器绘制
        /// </summary>
        /// <param name="pathRenderer">路径渲染器</param>
        /// <param name="abbreviatedData">OFD路径数据</param>
        private void ParseAndRenderPath(IPathRenderer pathRenderer, string abbreviatedData)
        {
            if (string.IsNullOrEmpty(abbreviatedData))
                return;
            
            // 简单的路径解析实现
            // OFD路径数据格式：操作符+空格+参数+空格+...
            // 例如："M 100 100 L 200 200 Z"
            var tokens = abbreviatedData.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 0)
                return;
            
            int index = 0;
            while (index < tokens.Length)
            {
                string command = tokens[index++];
                
                switch (command.ToUpper())
                {
                    case "M":// 移动到
                        if (index + 1 < tokens.Length)
                        {
                            float x = float.Parse(tokens[index]);
                            float y = float.Parse(tokens[index + 1]);
                            pathRenderer.MoveTo(x, y);
                            index += 2;
                        }
                        break;
                    
                    case "L":// 绘制直线
                        if (index + 1 < tokens.Length)
                        {
                            float x = float.Parse(tokens[index]);
                            float y = float.Parse(tokens[index + 1]);
                            pathRenderer.LineTo(x, y);
                            index += 2;
                        }
                        break;
                    
                    case "C":// 三次贝塞尔曲线
                        if (index + 5 < tokens.Length)
                        {
                            float cp1x = float.Parse(tokens[index]);
                            float cp1y = float.Parse(tokens[index + 1]);
                            float cp2x = float.Parse(tokens[index + 2]);
                            float cp2y = float.Parse(tokens[index + 3]);
                            float x = float.Parse(tokens[index + 4]);
                            float y = float.Parse(tokens[index + 5]);
                            pathRenderer.CubicTo(cp1x, cp1y, cp2x, cp2y, x, y);
                            index += 6;
                        }
                        break;
                    
                    case "Q":// 二次贝塞尔曲线
                        if (index + 3 < tokens.Length)
                        {
                            float cpx = float.Parse(tokens[index]);
                            float cpy = float.Parse(tokens[index + 1]);
                            float x = float.Parse(tokens[index + 2]);
                            float y = float.Parse(tokens[index + 3]);
                            pathRenderer.QuadTo(cpx, cpy, x, y);
                            index += 4;
                        }
                        break;
                    
                    case "Z":// 闭合路径
                        pathRenderer.ClosePath();
                        break;
                    
                    default:
                        // 未知命令，跳过
                        break;
                }
            }
        }
        
        /// <summary>
        /// 将OFD路径对象转换为图形样式
        /// </summary>
        /// <param name="pathObject">OFD路径对象</param>
        /// <returns>图形样式</returns>
        private GraphicStyle ConvertToGraphicStyle(Models.Graph.CT_Path pathObject)
        {
            var style = new GraphicStyle
            {
                // 填充颜色
                Color = ConvertToARGB(pathObject.FillColor),
                Alpha = 255,
                
                // 描边颜色
                StrokeColor = ConvertToARGB(pathObject.StrokeColor),
                StrokeAlpha = 255,
                
                // 描边宽度
                StrokeWidth = 1.0f,
                
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
            
            // 图像数据（暂时使用占位符，后续需要从资源中获取实际图像数据）
            byte[] imageData = null;
            
            // TODO: 实现从OFD资源中获取图像数据的逻辑
            // 需要根据imageObject.ResourceID从资源中加载图像数据
            
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
        private void RenderCompositeObject(IRenderContext renderContext, object compositeObj)
        {
            if (renderContext == null || compositeObj == null)
                return;
            
            var compositeObject = compositeObj as Models.BaseStructure.Pages.PageBlockItems.CompositeObject;
            if (compositeObject == null)
                return;
            
            // TODO: 实现从OFD资源中获取复合对象内容的逻辑
            // 需要根据compositeObject.ResourceID从资源中加载复合对象内容
            // 复合对象的内容是CT_VectorG类型，包含多个子图元
            
            // 保存当前渲染状态
            renderContext.SaveState();
            
            // 获取复合对象位置和大小（OFD坐标，单位：毫米）
            float x = (float)compositeObject.Boundary.X;
            float y = (float)compositeObject.Boundary.Y;
            float width = (float)compositeObject.Boundary.Width;
            float height = (float)compositeObject.Boundary.Height;
            
            // 应用变换
            renderContext.Translate(x, y);
            
            // TODO: 实现递归渲染复合对象中的子对象
            // 需要遍历复合对象中的子图元，并调用相应的渲染方法
            
            // 恢复渲染状态
            renderContext.RestoreState();
        }
        
        /// <summary>
        /// 渲染页面块对象
        /// </summary>
        /// <param name="renderContext">渲染上下文</param>
        /// <param name="pageBlock">页面块对象</param>
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
            _ofdReader?.Dispose();
        }
        
        #endregion
    }
}