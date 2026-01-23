using OFDViewer.Models.BaseStructure.Resources;
using OFDViewer.Models.BaseStructure.Resources.ResItems;
using OFDViewer.Models.BaseType;
using OFDViewer.Parse;
using Xunit;

namespace OFDViewer.Tests
{
    /// <summary>
    /// OFDDocument 资源获取方法的单元测试
    /// </summary>
    public class OFDDocumentResourceTests
    {
        /// <summary>
        /// 测试目标：验证从文档资源中获取字体资源
        /// 测试场景：在 DocumentResource 中添加字体资源，调用 GetResource 方法获取
        /// 预期结果：成功获取到字体资源对象
        /// </summary>
        [Fact]
        public void GetResource_FromDocumentResource_ShouldReturnFont()
        {
            // 准备测试数据
            var ofdDoc = new OFDDocument();
            var fonts = new OFDFonts();
            var font = new OFDFont()
            {
                ID = 1,
                FontName = "TestFont",
                FontFile = new ST_Loc("font.ttf")
            };
            fonts.ofdFonts = new List<OFDFont> { font };
            
            ofdDoc.DocumentResource = new Res();
            ofdDoc.DocumentResource.AddResource(fonts);
            
            // 添加页面文档（必须设置，否则GetResource会返回null）
            ofdDoc.PageDocs = new List<PageDocument> { new PageDocument() };

            // 执行测试
            var result = ofdDoc.GetResource(0, "1", ResourceType.Font, ResourceLocation.Document);

            // 验证结果
            Assert.NotNull(result);
            Assert.IsType<OFDFont>(result);
            Assert.Equal((uint)1, ((OFDFont)result).ID);
        }

        /// <summary>
        /// 测试目标：验证从公共资源中获取颜色空间资源
        /// 测试场景：在 PublicResource 中添加颜色空间资源，调用 GetResource 方法获取
        /// 预期结果：成功获取到颜色空间资源对象
        /// </summary>
        [Fact]
        public void GetResource_FromPublicResource_ShouldReturnColorSpace()
        {
            // 准备测试数据
            var ofdDoc = new OFDDocument();
            var colorSpaces = new ColorSpaces();
            var colorSpace = new ColorSpace()
            {
                ID = 1
            };
            colorSpaces.colorSpaces = new List<ColorSpace> { colorSpace };
            
            ofdDoc.PublicResource = new Res();
            ofdDoc.PublicResource.AddResource(colorSpaces);
            
            // 添加页面文档（必须设置，否则GetResource会返回null）
            ofdDoc.PageDocs = new List<PageDocument> { new PageDocument() };

            // 执行测试
            var result = ofdDoc.GetResource(0, "1", ResourceType.ColorSpace, ResourceLocation.Public);

            // 验证结果
            Assert.NotNull(result);
            Assert.IsType<ColorSpace>(result);
            Assert.Equal((uint)1, ((ColorSpace)result).ID);
        }

        /// <summary>
        /// 测试目标：验证从页面资源中获取绘制参数资源
        /// 测试场景：在页面资源中添加绘制参数，调用 GetResource 方法获取
        /// 预期结果：成功获取到绘制参数资源对象
        /// </summary>
        [Fact]
        public void GetResource_FromPageResource_ShouldReturnDrawParam()
        {
            // 准备测试数据
            var ofdDoc = new OFDDocument();
            var drawParams = new DrawParams();
            var drawParam = new DrawParam()
            {
                ID = 1,
                LineWidth = 1.0
            };
            drawParams.drawParams = new List<DrawParam> { drawParam };
            
            var pageDoc = new PageDocument();
            pageDoc.PageRes = new Res();
            pageDoc.PageRes.AddResource(drawParams);
            
            ofdDoc.PageDocs = new List<PageDocument> { pageDoc };

            // 执行测试
            var result = ofdDoc.GetResource(0, "1", ResourceType.DrawParam, ResourceLocation.Page);

            // 验证结果
            Assert.NotNull(result);
            Assert.IsType<DrawParam>(result);
            Assert.Equal((uint)1, ((DrawParam)result).ID);
        }

        /// <summary>
        /// 测试目标：验证自动搜索资源（从页面到文档再到公共资源）
        /// 测试场景：在文档资源中添加矢量图形，使用自动搜索模式获取
        /// 预期结果：成功获取到矢量图形资源对象
        /// </summary>
        [Fact]
        public void GetResource_AutoSearch_ShouldFindVectorGraphic()
        {
            // 准备测试数据
            var ofdDoc = new OFDDocument();
            var vectorGraphics = new CompositeGraphicUnits();
            var vectorGraphic = new CompositeGraphicUnit()
            {
                ID = 1
            };
            vectorGraphics.compositeGraphicUnits = new List<CompositeGraphicUnit> { vectorGraphic };
            
            ofdDoc.DocumentResource = new Res();
            ofdDoc.DocumentResource.AddResource(vectorGraphics);
            
            ofdDoc.PageDocs = new List<PageDocument> { new PageDocument() };

            // 执行测试（使用自动搜索模式）
            var result = ofdDoc.GetResource(0, "1", ResourceType.VectorGraphic, ResourceLocation.Auto);

            // 验证结果
            Assert.NotNull(result);
            Assert.IsType<CompositeGraphicUnit>(result);
            Assert.Equal((uint)1, ((CompositeGraphicUnit)result).ID);
        }

        /// <summary>
        /// 测试目标：验证获取多媒体资源
        /// 测试场景：添加多媒体资源，调用 GetResource 方法获取
        /// 预期结果：成功获取到多媒体资源对象
        /// </summary>
        [Fact]
        public void GetResource_ShouldReturnMultimedia()
        {
            // 准备测试数据
            var ofdDoc = new OFDDocument();
            var multiMedias = new MultiMedias();
            var multimedia = new MultiMedia()
            {
                ID = 1,
                MediaFile = new ST_Loc("image.png")
            };
            multiMedias.multiMedias = new List<MultiMedia> { multimedia };
            
            ofdDoc.DocumentResource = new Res();
            ofdDoc.DocumentResource.AddResource(multiMedias);
            
            // 添加页面文档（必须设置，否则GetResource会返回null）
            ofdDoc.PageDocs = new List<PageDocument> { new PageDocument() };

            // 执行测试
            var result = ofdDoc.GetResource(0, "1", ResourceType.Multimedia, ResourceLocation.Document);

            // 验证结果
            Assert.NotNull(result);
            Assert.IsType<MultiMedia>(result);
            Assert.Equal((uint)1, ((MultiMedia)result).ID);
        }

        /// <summary>
        /// 测试目标：验证资源不存在时返回null
        /// 测试场景：调用 GetResource 方法获取不存在的资源
        /// 预期结果：返回null
        /// </summary>
        [Fact]
        public void GetResource_WhenResourceNotFound_ShouldReturnNull()
        {
            // 准备测试数据
            var ofdDoc = new OFDDocument();
            ofdDoc.DocumentResource = new Res();
            ofdDoc.PageDocs = new List<PageDocument> { new PageDocument() };

            // 执行测试
            var result = ofdDoc.GetResource(0, "999", ResourceType.Font, ResourceLocation.Auto);

            // 验证结果
            Assert.Null(result);
        }

        /// <summary>
        /// 测试目标：验证使用 ResourceType.All 搜索所有类型资源
        /// 测试场景：添加字体资源，使用 ResourceType.All 搜索
        /// 预期结果：成功获取到资源对象
        /// </summary>
        [Fact]
        public void GetResource_WithResourceTypeAll_ShouldFindResource()
        {
            // 准备测试数据
            var ofdDoc = new OFDDocument();
            var fonts = new OFDFonts();
            var font = new OFDFont()
            {
                ID = 1,
                FontName = "TestFont"
            };
            fonts.ofdFonts = new List<OFDFont> { font };
            
            ofdDoc.DocumentResource = new Res();
            ofdDoc.DocumentResource.AddResource(fonts);
            
            // 添加页面文档（必须设置，否则GetResource会返回null）
            ofdDoc.PageDocs = new List<PageDocument> { new PageDocument() };

            // 执行测试（使用 ResourceType.All）
            var result = ofdDoc.GetResource(0, "1", ResourceType.All, ResourceLocation.Document);

            // 验证结果
            Assert.NotNull(result);
            Assert.IsType<OFDFont>(result);
        }

        /// <summary>
        /// 测试目标：验证页面索引超出范围时返回null
        /// 测试场景：调用 GetResource 方法时传入无效的页面索引
        /// 预期结果：返回null
        /// </summary>
        [Fact]
        public void GetResource_WithInvalidPageIndex_ShouldReturnNull()
        {
            // 准备测试数据
            var ofdDoc = new OFDDocument();
            ofdDoc.PageDocs = new List<PageDocument> { new PageDocument() };

            // 执行测试（使用无效的页面索引）
            var result = ofdDoc.GetResource(-1, "1", ResourceType.All, ResourceLocation.Auto);

            // 验证结果
            Assert.Null(result);
        }

        /// <summary>
        /// 测试目标：验证资源ID为空时返回null
        /// 测试场景：调用 GetResource 方法时传入空的资源ID
        /// 预期结果：返回null
        /// </summary>
        [Fact]
        public void GetResource_WithEmptyResourceId_ShouldReturnNull()
        {
            // 准备测试数据
            var ofdDoc = new OFDDocument();
            ofdDoc.DocumentResource = new Res();

            // 执行测试（使用空资源ID）
            var result = ofdDoc.GetResource(0, "", ResourceType.All, ResourceLocation.Document);

            // 验证结果
            Assert.Null(result);
        }

        /// <summary>
        /// 测试目标：验证从多个资源中获取指定ID的资源
        /// 测试场景：添加多个相同类型的资源，获取指定ID的资源
        /// 预期结果：成功获取到指定ID的资源对象
        /// </summary>
        [Fact]
        public void GetResource_FromMultipleResources_ShouldReturnSpecificResource()
        {
            // 准备测试数据
            var ofdDoc = new OFDDocument();
            var fonts = new OFDFonts();
            var font1 = new OFDFont() { ID = 1, FontName = "Font1" };
            var font2 = new OFDFont() { ID = 2, FontName = "Font2" };
            var font3 = new OFDFont() { ID = 3, FontName = "Font3" };
            fonts.ofdFonts = new List<OFDFont> { font1, font2, font3 };
            
            ofdDoc.DocumentResource = new Res();
            ofdDoc.DocumentResource.AddResource(fonts);
            
            // 添加页面文档（必须设置，否则GetResource会返回null）
            ofdDoc.PageDocs = new List<PageDocument> { new PageDocument() };

            // 执行测试（获取中间的资源）
            var result = ofdDoc.GetResource(0, "2", ResourceType.Font, ResourceLocation.Document);

            // 验证结果
            Assert.NotNull(result);
            Assert.IsType<OFDFont>(result);
            Assert.Equal((uint)2, ((OFDFont)result).ID);
            Assert.Equal("Font2", ((OFDFont)result).FontName);
        }

        /// <summary>
        /// 测试目标：验证资源搜索顺序（页面 -> 文档 -> 公共）
        /// 测试场景：在不同层级添加相同ID的资源，验证优先返回页面级资源
        /// 预期结果：返回页面级的资源对象
        /// </summary>
        [Fact]
        public void GetResource_AutoSearch_ShouldReturnPageLevelResourceFirst()
        {
            // 准备测试数据
            var ofdDoc = new OFDDocument();
            
            // 在公共资源中添加资源
            var publicFonts = new OFDFonts();
            publicFonts.ofdFonts = new List<OFDFont> { new OFDFont() { ID = 1, FontName = "PublicFont" } };
            ofdDoc.PublicResource = new Res();
            ofdDoc.PublicResource.AddResource(publicFonts);
            
            // 在文档资源中添加资源
            var docFonts = new OFDFonts();
            docFonts.ofdFonts = new List<OFDFont> { new OFDFont() { ID = 1, FontName = "DocumentFont" } };
            ofdDoc.DocumentResource = new Res();
            ofdDoc.DocumentResource.AddResource(docFonts);
            
            // 在页面资源中添加资源
            var pageDoc = new PageDocument();
            var pageFonts = new OFDFonts();
            pageFonts.ofdFonts = new List<OFDFont> { new OFDFont() { ID = 1, FontName = "PageFont" } };
            pageDoc.PageRes = new Res();
            pageDoc.PageRes.AddResource(pageFonts);
            ofdDoc.PageDocs = new List<PageDocument> { pageDoc };

            // 执行测试（使用自动搜索模式）
            var result = ofdDoc.GetResource(0, "1", ResourceType.Font, ResourceLocation.Auto);

            // 验证结果：应该返回页面级的资源
            Assert.NotNull(result);
            Assert.IsType<OFDFont>(result);
            Assert.Equal((uint)1, ((OFDFont)result).ID);
            Assert.Equal("PageFont", ((OFDFont)result).FontName);
        }

        #region 泛型版本 GetResource<T> 测试

        /// <summary>
        /// 测试目标：验证泛型版本从文档资源中获取字体资源
        /// 测试场景：在 DocumentResource 中添加字体资源，调用泛型 GetResource<T> 方法获取
        /// 预期结果：成功获取到指定类型的字体资源对象
        /// </summary>
        [Fact]
        public void GetResourceGeneric_FromDocumentResource_ShouldReturnFont()
        {
            // 准备测试数据
            var ofdDoc = new OFDDocument();
            var fonts = new OFDFonts();
            var font = new OFDFont()
            {
                ID = 1,
                FontName = "TestFont",
                FontFile = new ST_Loc("font.ttf")
            };
            fonts.ofdFonts = new List<OFDFont> { font };
            
            ofdDoc.DocumentResource = new Res();
            ofdDoc.DocumentResource.AddResource(fonts);
            
            // 添加页面文档（必须设置，否则GetResource会返回null）
            ofdDoc.PageDocs = new List<PageDocument> { new PageDocument() };

            // 执行测试（使用泛型版本）
            var result = ofdDoc.GetResource<OFDFont>(0, "1", ResourceLocation.Document);

            // 验证结果
            Assert.NotNull(result);
            Assert.IsType<OFDFont>(result);
            Assert.Equal((uint)1, result.ID);
            Assert.Equal("TestFont", result.FontName);
        }

        /// <summary>
        /// 测试目标：验证泛型版本从公共资源中获取颜色空间资源
        /// 测试场景：在 PublicResource 中添加颜色空间资源，调用泛型 GetResource<T> 方法获取
        /// 预期结果：成功获取到指定类型的颜色空间资源对象
        /// </summary>
        [Fact]
        public void GetResourceGeneric_FromPublicResource_ShouldReturnColorSpace()
        {
            // 准备测试数据
            var ofdDoc = new OFDDocument();
            var colorSpaces = new ColorSpaces();
            var colorSpace = new ColorSpace()
            {
                ID = 1
            };
            colorSpaces.colorSpaces = new List<ColorSpace> { colorSpace };
            
            ofdDoc.PublicResource = new Res();
            ofdDoc.PublicResource.AddResource(colorSpaces);
            
            // 添加页面文档（必须设置，否则GetResource会返回null）
            ofdDoc.PageDocs = new List<PageDocument> { new PageDocument() };

            // 执行测试（使用泛型版本）
            var result = ofdDoc.GetResource<ColorSpace>(0, "1", ResourceLocation.Public);

            // 验证结果
            Assert.NotNull(result);
            Assert.IsType<ColorSpace>(result);
            Assert.Equal((uint)1, result.ID);
        }

        /// <summary>
        /// 测试目标：验证泛型版本从页面资源中获取绘制参数资源
        /// 测试场景：在页面资源中添加绘制参数，调用泛型 GetResource<T> 方法获取
        /// 预期结果：成功获取到指定类型的绘制参数资源对象
        /// </summary>
        [Fact]
        public void GetResourceGeneric_FromPageResource_ShouldReturnDrawParam()
        {
            // 准备测试数据
            var ofdDoc = new OFDDocument();
            var drawParams = new DrawParams();
            var drawParam = new DrawParam()
            {
                ID = 1,
                LineWidth = 1.0
            };
            drawParams.drawParams = new List<DrawParam> { drawParam };
            
            var pageDoc = new PageDocument();
            pageDoc.PageRes = new Res();
            pageDoc.PageRes.AddResource(drawParams);
            
            ofdDoc.PageDocs = new List<PageDocument> { pageDoc };

            // 执行测试（使用泛型版本）
            var result = ofdDoc.GetResource<DrawParam>(0, "1", ResourceLocation.Page);

            // 验证结果
            Assert.NotNull(result);
            Assert.IsType<DrawParam>(result);
            Assert.Equal((uint)1, result.ID);
            Assert.Equal(1.0, result.LineWidth);
        }

        /// <summary>
        /// 测试目标：验证泛型版本获取多媒体资源
        /// 测试场景：添加多媒体资源，调用泛型 GetResource<T> 方法获取
        /// 预期结果：成功获取到指定类型的多媒体资源对象
        /// </summary>
        [Fact]
        public void GetResourceGeneric_ShouldReturnMultimedia()
        {
            // 准备测试数据
            var ofdDoc = new OFDDocument();
            var multiMedias = new MultiMedias();
            var multimedia = new MultiMedia()
            {
                ID = 1,
                MediaFile = new ST_Loc("image.png")
            };
            multiMedias.multiMedias = new List<MultiMedia> { multimedia };
            
            ofdDoc.DocumentResource = new Res();
            ofdDoc.DocumentResource.AddResource(multiMedias);
            
            // 添加页面文档（必须设置，否则GetResource会返回null）
            ofdDoc.PageDocs = new List<PageDocument> { new PageDocument() };

            // 执行测试（使用泛型版本）
            var result = ofdDoc.GetResource<MultiMedia>(0, "1", ResourceLocation.Document);

            // 验证结果
            Assert.NotNull(result);
            Assert.IsType<MultiMedia>(result);
            Assert.Equal((uint)1, result.ID);
            Assert.Equal("image.png", result.MediaFile.Path);
        }

        /// <summary>
        /// 测试目标：验证泛型版本自动搜索资源
        /// 测试场景：在文档资源中添加矢量图形，使用泛型 GetResource<T> 方法自动搜索
        /// 预期结果：成功获取到指定类型的矢量图形资源对象
        /// </summary>
        [Fact]
        public void GetResourceGeneric_AutoSearch_ShouldFindVectorGraphic()
        {
            // 准备测试数据
            var ofdDoc = new OFDDocument();
            var vectorGraphics = new CompositeGraphicUnits();
            var vectorGraphic = new CompositeGraphicUnit()
            {
                ID = 1
            };
            vectorGraphics.compositeGraphicUnits = new List<CompositeGraphicUnit> { vectorGraphic };
            
            ofdDoc.DocumentResource = new Res();
            ofdDoc.DocumentResource.AddResource(vectorGraphics);
            
            ofdDoc.PageDocs = new List<PageDocument> { new PageDocument() };

            // 执行测试（使用泛型版本，自动搜索模式）
            var result = ofdDoc.GetResource<CompositeGraphicUnit>(0, "1", ResourceLocation.Auto);

            // 验证结果
            Assert.NotNull(result);
            Assert.IsType<CompositeGraphicUnit>(result);
            Assert.Equal((uint)1, result.ID);
        }

        /// <summary>
        /// 测试目标：验证泛型版本资源不存在时返回default(T)
        /// 测试场景：调用泛型 GetResource<T> 方法获取不存在的资源
        /// 预期结果：返回default(T)（对于引用类型为null）
        /// </summary>
        [Fact]
        public void GetResourceGeneric_WhenResourceNotFound_ShouldReturnDefault()
        {
            // 准备测试数据
            var ofdDoc = new OFDDocument();
            ofdDoc.DocumentResource = new Res();
            ofdDoc.PageDocs = new List<PageDocument> { new PageDocument() };

            // 执行测试（使用泛型版本，获取不存在的资源）
            var result = ofdDoc.GetResource<OFDFont>(0, "999", ResourceLocation.Auto);

            // 验证结果：对于引用类型，default(T)为null
            Assert.Null(result);
        }

        /// <summary>
        /// 测试目标：验证泛型版本页面索引无效时返回default(T)
        /// 测试场景：调用泛型 GetResource<T> 方法时传入无效的页面索引
        /// 预期结果：返回default(T)（对于引用类型为null）
        /// </summary>
        [Fact]
        public void GetResourceGeneric_WithInvalidPageIndex_ShouldReturnDefault()
        {
            // 准备测试数据
            var ofdDoc = new OFDDocument();
            ofdDoc.PageDocs = new List<PageDocument> { new PageDocument() };

            // 执行测试（使用泛型版本，传入无效的页面索引）
            var result = ofdDoc.GetResource<OFDFont>(-1, "1", ResourceLocation.Auto);

            // 验证结果：对于引用类型，default(T)为null
            Assert.Null(result);
        }

        /// <summary>
        /// 测试目标：验证泛型版本从多个资源中获取指定ID的资源
        /// 测试场景：添加多个相同类型的资源，调用泛型 GetResource<T> 方法获取指定ID的资源
        /// 预期结果：成功获取到指定类型和ID的资源对象
        /// </summary>
        [Fact]
        public void GetResourceGeneric_FromMultipleResources_ShouldReturnSpecificResource()
        {
            // 准备测试数据
            var ofdDoc = new OFDDocument();
            var fonts = new OFDFonts();
            var font1 = new OFDFont() { ID = 1, FontName = "Font1" };
            var font2 = new OFDFont() { ID = 2, FontName = "Font2" };
            var font3 = new OFDFont() { ID = 3, FontName = "Font3" };
            fonts.ofdFonts = new List<OFDFont> { font1, font2, font3 };
            
            ofdDoc.DocumentResource = new Res();
            ofdDoc.DocumentResource.AddResource(fonts);
            
            // 添加页面文档（必须设置，否则GetResource会返回null）
            ofdDoc.PageDocs = new List<PageDocument> { new PageDocument() };

            // 执行测试（使用泛型版本，获取中间的资源）
            var result = ofdDoc.GetResource<OFDFont>(0, "2", ResourceLocation.Document);

            // 验证结果
            Assert.NotNull(result);
            Assert.IsType<OFDFont>(result);
            Assert.Equal((uint)2, result.ID);
            Assert.Equal("Font2", result.FontName);
        }

        #endregion
    }
}
