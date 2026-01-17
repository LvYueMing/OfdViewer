using OFDViewer.OFD;
using Xunit;
using System;
using System.IO;
using System.Collections.Generic;
using OFDViewer.Models.BaseStructure.DocumentRoot;
using OFDViewer.Models.BaseType;
using OFDViewer.Models.BaseStructure.Pages;
using OFDViewer.Models.BaseStructure.Pages.PageBlockItems;
using OFDViewer.Models.Font;
using OFDViewer.Models.PageDesc.Colors;

namespace OFDViewer.Tests
{
    public class OFDWriterTests
    {
        // 测试临时文件路径（自动清理）
        private readonly string _tempFilePath;

        // 测试用的基础元数据对象（复用）
        private readonly OFDRootDocument _testOfdDocument;

        public OFDWriterTests()
        {
            // 初始化临时文件路径
            _tempFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"OFD");

            // 创建默认OFD文档对象
            _testOfdDocument = new OFDRootDocument();

        }

        #region 构造函数测试
        /// <summary>
        /// 测试：文件路径构造函数 - 路径为空时抛出ArgumentNullException
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Constructor_FilePath_NullOrWhitespace_ThrowsArgumentNullException(string filePath)
        {
            // Arrange & Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new OFDWriter(filePath));
            Assert.Equal("filePath", exception.ParamName);
        }

        /// <summary>
        /// 测试：文件路径构造函数 - 目录不存在时自动创建
        /// </summary>
        [Fact]
        public void Constructor_FilePath_DirectoryNotExist_CreatesDirectory()
        {
            // Arrange
            var nonExistDir = Path.Combine(_tempFilePath, Guid.NewGuid().ToString());
            var filePath = Path.Combine(nonExistDir, "test.ofd");

            // 保证释放writer
            {
                // Act
                using var writer = new OFDWriter(filePath);

                // Assert
                Assert.True(Directory.Exists(nonExistDir));
            }

            Directory.Delete(nonExistDir,true);
        }

        /// <summary>
        /// 测试：流构造函数 - 流为空时抛出ArgumentNullException
        /// </summary>
        [Fact]
        public void Constructor_Stream_Null_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new OFDWriter(null, leaveOpen: false));
            Assert.Equal("stream", exception.ParamName);
        }

        /// <summary>
        /// 测试：流构造函数 - 流不可写时抛出ArgumentException
        /// </summary>
        [Fact]
        public void Constructor_Stream_NotWritable_ThrowsArgumentException()
        {
            // Arrange：创建只读流
            using var readOnlyStream = new MemoryStream();
            readOnlyStream.Position = 0;
            var nonWritableStream = new NonWritableStream(readOnlyStream);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new OFDWriter(nonWritableStream, leaveOpen: false));
            Assert.Equal("stream", exception.ParamName);
        }

        #endregion

        #region 核心写入方法测试
        /// <summary>
        /// 测试：WriteRootOFD - 元数据为空时抛出ArgumentNullException
        /// </summary>
        [Fact]
        public void WriteRootOFD_RootOfdNull_ThrowsArgumentNullException()
        {
            // Arrange
            using var ms = new MemoryStream();
            using var writer = new OFDWriter(ms, leaveOpen: true);

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => writer.WriteRootOFD(null));
            Assert.Equal("rootOfd", exception.ParamName);
        }

        /// <summary>
        /// 测试：WriteEntireDocument - 文档对象为空时抛出ArgumentNullException
        /// </summary>
        [Fact]
        public void WriteEntireDocument_OfdDocumentNull_ThrowsArgumentNullException()
        {
            // Arrange
            using var ms = new MemoryStream();
            using var writer = new OFDWriter(ms, leaveOpen: true);

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => writer.WriteOFDRootDoc(null));
            Assert.Equal("ofdDocument", exception.ParamName);
        }

        /// <summary>
        /// 测试：WriteEntireDocument - 文档无Metadata时抛出ArgumentNullException
        /// </summary>
        [Fact]
        public void WriteEntireDocument_RootOfdNull_ThrowsArgumentNullException()
        {
            // Arrange
            var invalidDoc = new OFDRootDocument();
            invalidDoc.RootOfd = null;
            using var ms = new MemoryStream();
            using var writer = new OFDWriter(ms, leaveOpen: true);

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => writer.WriteOFDRootDoc(invalidDoc));
            Assert.Equal("RootOfd", exception.ParamName);
        }

        /// <summary>
        /// 测试：WriteEntireDocument - 文档无子文档时抛出InvalidOperationException
        /// </summary>
        [Fact]
        public void WriteEntireDocument_NoDocs_ThrowsInvalidOperationException()
        {
            // Arrange
            var invalidDoc = new OFDRootDocument();
            invalidDoc.Docs.Clear();
            using var ms = new MemoryStream();
            using var writer = new OFDWriter(ms, leaveOpen: true);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => writer.WriteOFDRootDoc(invalidDoc));
            Assert.Contains("无可用子文档", exception.Message);
        }

        // 测试：Save方法 - writer.WriteRootOFD之后保存
        [Fact]
        public void Save_ToStream_AfterWriteRootOFD_SavesSuccessfully()
        {
            // Arrange
            using var ms = new MemoryStream();
            Assert.True(ms.Length == 0); // 保存前流没有数据

            using var writer = new OFDWriter(ms, leaveOpen: true);
            writer.WriteRootOFD(_testOfdDocument.RootOfd);

            // Act
            // writer.Save();
            // Assert
            Assert.True(ms.Length > 0); // 保存后流有数据
        }

        // 测试：Save方法 - writer.WriteRootOFD保存为磁盘文件
        [Fact]
        public void Save_ToOFDFile_AfterWriteRootOFD_SavesSuccessfully()
        {
            // Arrange
            var filePath = Path.Combine(_tempFilePath, $"{Guid.NewGuid()}.ofd");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            Assert.False(File.Exists(filePath)); // 保存前文件不存在

            // Act
            using var writer = new OFDWriter(filePath);
            writer.WriteRootOFD(_testOfdDocument.RootOfd);
            writer.Save();

            // Assert
            Assert.True(File.Exists(filePath)); // 保存后文件存在

            File.Delete(filePath);
        }

        // 测试：Save方法 - writer.WriteOFDDoc保存为磁盘文件
        [Fact]
        public void Save_ToOFDFile_AfterWriteOFDDoc_SavesSuccessfully()
        {
            // Arrange
            var filePath = Path.Combine(_tempFilePath, $"{Guid.NewGuid()}.ofd");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            Assert.False(File.Exists(filePath)); // 保存前文件不存在
            // Act
            using var writer = new OFDWriter(filePath);
            writer.WriteRootOFD(_testOfdDocument.RootOfd);
            writer.WriteOFDDoc(_testOfdDocument.DefaultOFDDocument);
            writer.Save();
            // Assert
            Assert.True(File.Exists(filePath)); // 保存后文件存在

            File.Delete(filePath);
        }

        // <summary>
        /// 测试：Save方法 - writer.WriteOFDDocument保存为磁盘文件
        /// </summary>

        [Fact]
        public void Save_ToOFDFile_AfterWriteOFDDocument_SavesSuccessfully()
        {
            // Arrange
            var filePath = Path.Combine(_tempFilePath, $"{Guid.NewGuid()}.ofd");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            Assert.False(File.Exists(filePath)); // 保存前文件不存在
            // Act
            using var writer = new OFDWriter(filePath);
            writer.WriteOFDRootDoc(_testOfdDocument);
            writer.Save();
            // Assert
            Assert.True(File.Exists(filePath)); // 保存后文件存在

            File.Delete(filePath);
        }

        /// <summary>
        /// 测试：Save方法 - 重复调用不抛异常且仅执行一次保存
        /// </summary>
        [Fact]
        public void Save_DuplicateCall_DoesNotThrow()
        {
            // Arrange
            using var ms = new MemoryStream();
            using var writer = new OFDWriter(ms, leaveOpen: true);
            writer.WriteOFDRootDoc(_testOfdDocument);

            // Act
            writer.Save();
            writer.Save(); // 重复调用

            // Assert
            Assert.True(ms.Length > 0); // 保存后流有数据
        }

        /// <summary>
        /// 测试：资源文件的BaseLoc设置和资源路径解析
        /// </summary>
        [Fact]
        public void OFDDoc_ResourceBaseLocAndPath_ResolutionTest()
        {
            // Arrange
            // 创建OFD文档对象
            var ofdDocument = new OFDRootDocument();
            
            // 获取默认子文档
            var doc = ofdDocument.DefaultOFDDocument;
            
            // 直接测试OFDDoc的资源属性设置
            // 资源描述文件位于Doc_0目录下，资源目录是Doc_0/Res，所以相对于资源描述文件的路径是Res
            var expectedBaseLoc = "Res";
            
            // 创建公共资源
            var publicRes = new OFDViewer.Models.BaseStructure.Resources.Res();
            
            // Act
            doc.SetPublicResource(publicRes);
            
            // Assert
            // 验证PublicResource的BaseLoc设置
            Assert.Equal(expectedBaseLoc, doc.PublicResource.BaseLocString);
            Assert.Equal(expectedBaseLoc, doc.PublicResource.BaseLoc.ToString());
        }

        /// <summary>
        /// 测试：添加资源对象时资源文件路径的自动解析
        /// </summary>
        [Fact]
        public void OFDDoc_ResourceFilePath_ResolutionTest()
        {
            // Arrange
            // 创建Res对象
            var res = new OFDViewer.Models.BaseStructure.Resources.Res();
            res.BaseLocString = "Doc_0/Res";

            // 创建字体资源
            var fonts = new OFDViewer.Models.BaseStructure.Resources.ResItems.OFDFonts();
            fonts.ofdFonts = new List<OFDViewer.Models.BaseStructure.Resources.ResItems.OFDFont>(); // 初始化集合
            var font = new OFDViewer.Models.BaseStructure.Resources.ResItems.OFDFont();
            font.ID = OFDViewer.Models.BaseType.ST_ID.CreateNew(); // 使用正确的ST_ID创建方法
            font.FontName = "TestFont";
            font.FontFile = "testfont.ttf"; // 相对路径
            fonts.ofdFonts.Add(font);

            // 创建多媒体资源
            var medias = new OFDViewer.Models.BaseStructure.Resources.ResItems.MultiMedias();
            medias.multiMedias = new List<OFDViewer.Models.BaseStructure.Resources.ResItems.MultiMedia>(); // 初始化集合
            var media = new OFDViewer.Models.BaseStructure.Resources.ResItems.MultiMedia();
            media.ID = OFDViewer.Models.BaseType.ST_ID.CreateNew(); // 使用正确的ST_ID创建方法
            media.TypeString = "Image";
            media.FormatString = "JPEG";
            media.MediaFile = "testimage.jpg"; // 相对路径
            medias.multiMedias.Add(media);

            // Act
            // 添加资源到ResItems
            res.AddResource(fonts);
            res.AddResource(medias);

            // Assert
            // 验证字体文件路径是否被正确解析
            // 根据当前实现，FontFile不会自动与BaseLoc合并，所以路径是"testfont.ttf"
            Assert.Equal("testfont.ttf", fonts.ofdFonts[0].FontFile.ToString());

            // 验证多媒体文件路径是否被正确解析
            // 根据当前实现，MediaFile不会自动与BaseLoc合并，所以路径是"testimage.jpg"
            Assert.Equal("testimage.jpg", medias.multiMedias[0].MediaFile.ToString());
        }
        
        /// <summary>
        /// 测试：OFDDocument对象包含两个OFDDoc的情况
        /// </summary>
        [Fact]
        public void OFDDocument_MultipleOFDDocsTest()
        {
            // Arrange
            // 创建OFD文档对象
            var ofdDocument = new OFDRootDocument();
            
            // Act
            // 添加第二个文档
            var secondDoc = ofdDocument.AddNewDoc();
            
            // Assert
            // 验证文档数量
            Assert.Equal(2, ofdDocument.Docs.Count);
            Assert.Equal(2, ofdDocument.DocCount);
            
            // 验证DefaultDoc仍然是第一个文档
            Assert.Equal(0, ofdDocument.DefaultOFDDocument.DocIndex);
            
            // 验证文档索引
            Assert.Equal(0, ofdDocument.Docs[0].DocIndex);
            Assert.Equal(1, ofdDocument.Docs[1].DocIndex);
            Assert.Equal(1, secondDoc.DocIndex);
            
            // 验证RootOfd中的DocBodies数量
            Assert.Equal(2, ofdDocument.RootOfd.DocBodies.Count);
            
            // 验证DocBodies中的文档路径
            Assert.Equal("Doc_0/Document.xml", ofdDocument.RootOfd.DocBodies[0].DocRoot.Path);
            Assert.Equal("Doc_1/Document.xml", ofdDocument.RootOfd.DocBodies[1].DocRoot.Path);
        }
        #endregion

        #region 生成完整的OFD文档
        /// <summary>
        /// 测试：生成完整的OFD文档，包含文字内容并保存到本地文件系统
        /// </summary>
        [Fact]
        public void Generate_FullOFDWithTextContent_SavesSuccessfully()
        {
            // 创建保存路径
            var savePath = Path.Combine(_tempFilePath, $"TestFullOFD_{DateTime.Now:yyyyMMddHHmmss}.ofd");

            // Arrange
            // 创建OFD文档对象
            var ofdRootDocument = new OFDRootDocument();

            // 获取默认子文档
            var doc = ofdRootDocument.DefaultOFDDocument;


            // 设置文档级别的页面区域（A4纸张大小）
            if (doc.Document.CommonData == null)
            {
                doc.Document.CommonData = new OFDViewer.Models.BaseStructure.DocumentRoot.CT_CommonData();
            }
            // 创建一个新的CT_PageArea实例
            var pageArea = new OFDViewer.Models.BaseStructure.DocumentRoot.CT_PageArea();
            // 显式设置所有属性，确保它们被序列化
            pageArea.PhysicalBox = new OFDViewer.Models.BaseType.ST_Box(0, 0, 210, 297);
            // 设置文档级别的PageArea
            doc.Document.CommonData.PageArea = pageArea;


            // 添加字体资源文件
            string fontFileName = "font1_10.ttf";
            string fontFilePath = @"D:\MySoft\GitHub\OfdViewer\OFD-File\Res\font1_10.ttf";
            doc.ResFiles.Add(fontFileName, OFDWriter.ReadResFile(fontFilePath));

            // 初始化PublicResource
            var publicRes = new OFDViewer.Models.BaseStructure.Resources.Res();

            // 使用SetPublicResource方法设置公共资源
            doc.SetPublicResource(publicRes);

            // 添加字体资源到PublicResource
            var fonts = new OFDViewer.Models.BaseStructure.Resources.ResItems.OFDFonts();
            fonts.ofdFonts = new List<OFDViewer.Models.BaseStructure.Resources.ResItems.OFDFont>();

            var font = new OFDViewer.Models.BaseStructure.Resources.ResItems.OFDFont();
            font.ID = OFDViewer.Models.BaseType.ST_ID.CreateNew();
            font.FontName = "宋体";
            font.FontFile = fontFileName;
            font.Bold = false;
            font.Italic = false;

            fonts.ofdFonts.Add(font);
            doc.PublicResource.AddResource(fonts);

            // 创建一个新页面
            var pageDoc = new PageDocument();
            doc.AddPageDoc(pageDoc);

            // 设置页面级别的页面区域（A4纸张大小）
            pageDoc.Page.Area = new OFDViewer.Models.BaseStructure.DocumentRoot.CT_PageArea
            {
                PhysicalBox = new OFDViewer.Models.BaseType.ST_Box(0, 0, 210, 297),
            };

            // 确保Page.Content被正确初始化
            if (pageDoc.Page.Content == null)
            {
                pageDoc.Page.Content = new List<OFDViewer.Models.BaseStructure.Pages.Layer>();
            }

            // 创建内容图层
            var layer = new OFDViewer.Models.BaseStructure.Pages.Layer
            {
                ID = OFDViewer.Models.BaseType.ST_ID.CreateNew()
            };

            // 创建文本对象
            var textObject = new OFDViewer.Models.BaseStructure.Pages.PageBlockItems.TextObject
            {
                // 设置文本ID
                ID = OFDViewer.Models.BaseType.ST_ID.CreateNew(),
                // 设置文本边界
                Boundary = new OFDViewer.Models.BaseType.ST_Box(30, 50, 100, 50),
                // 设置文本样式
                Size = 12,
                // 设置字体，使用创建的字体
                FontString = font.IDString,
            };

            // 添加文本内容
            var textCode = new OFDViewer.Models.Font.TextCode
            {
                // 设置文本位置和间距
                X = 0.5,
                Y = 10,
                // DeltaX是ST_Array类型，需要使用Parse方法转换
                DeltaX = OFDViewer.Models.BaseType.ST_Array.Parse("1.0"),
                Value = "这是一个测试OFD文档，包含文字内容。"
            };
            textObject.TextCodes.Add(textCode);


            // 将文本对象添加到图层
            layer.PageBlockItems.Add(textObject);

            // 将图层添加到页面内容
            pageDoc.Page.Content.Add(layer);

            // Act
            // 保存OFD文档
            using (var writer = new OFDWriter(savePath))
            {
                writer.WriteOFDRootDoc(ofdRootDocument);
                writer.Save();
            }

            // Assert
            // 验证文件是否存在
            Assert.True(File.Exists(savePath));
            // 验证文件大小大于0
            Assert.True(new FileInfo(savePath).Length > 0);
            // 验证页面内容不为空
            Assert.NotNull(pageDoc.Page.Content);
            Assert.NotEmpty(pageDoc.Page.Content);
            // 验证图层包含文本对象
            Assert.NotNull(pageDoc.Page.Content[0].PageBlockItems);
            Assert.NotEmpty(pageDoc.Page.Content[0].PageBlockItems);
            // 验证文本对象包含文本内容
            var textObj = pageDoc.Page.Content[0].PageBlockItems[0] as OFDViewer.Models.BaseStructure.Pages.PageBlockItems.TextObject;
            Assert.NotNull(textObj);
            Assert.NotNull(textObj.TextCodes);
            Assert.NotEmpty(textObj.TextCodes);

            // 验证DocumentResource的BaseLoc设置正确
            var docIndex = doc.DocIndex;
            var expectedBaseLoc = $"Doc_{docIndex}/Res";

            // 验证字体资源已正确添加到PublicResource
            Assert.NotNull(doc.PublicResource.ResItems);
            Assert.NotEmpty(doc.PublicResource.ResItems);
            var addedFonts = doc.PublicResource.ResItems.FirstOrDefault(r => r is OFDViewer.Models.BaseStructure.Resources.ResItems.OFDFonts) as OFDViewer.Models.BaseStructure.Resources.ResItems.OFDFonts;
            Assert.NotNull(addedFonts);
            Assert.Single(addedFonts.ofdFonts);
            Assert.Equal("宋体", addedFonts.ofdFonts[0].FontName);
            Assert.Equal("font1_10.ttf", addedFonts.ofdFonts[0].FontFile);

            // 打印保存路径到控制台
            Console.WriteLine($"OFD文档已成功保存到：{savePath}");
        }

        #endregion

        #region 资源释放测试
        /// <summary>
        /// 测试：Dispose后调用写入方法抛出ObjectDisposedException
        /// </summary>
        [Fact]
        public void WriteAfterDispose_ThrowsObjectDisposedException()
        {
            // Arrange
            using var ms = new MemoryStream();
            var writer = new OFDWriter(ms, leaveOpen: true);
            writer.Dispose();

            // Act & Assert
            Assert.Throws<ObjectDisposedException>(() => writer.WriteOFDRootDoc(_testOfdDocument));
            Assert.Throws<ObjectDisposedException>(() => writer.WriteRootOFD(_testOfdDocument.RootOfd));
            Assert.Throws<ObjectDisposedException>(() => writer.Save());
        }

        /// <summary>
        /// 测试：析构函数触发时不抛异常（通过GC强制回收验证）
        /// </summary>
        [Fact]
        public void Finalizer_DoesNotThrow()
        {
            // Arrange
            var ms = new MemoryStream();
            var writer = new OFDWriter(ms, leaveOpen: true);

            // Act：手动释放引用并强制GC
            writer = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();

            // Assert：无异常即通过
            Assert.True(true);
        }
        
        #endregion

        #region 辅助类：模拟不可写的流
        /// <summary>
        /// 用于测试的“不可写”流包装类
        /// </summary>
        private class NonWritableStream : Stream
        {
            private readonly Stream _innerStream;

            public NonWritableStream(Stream innerStream) => _innerStream = innerStream;

            public override bool CanRead => _innerStream.CanRead;
            public override bool CanSeek => _innerStream.CanSeek;
            public override bool CanWrite => false; // 强制不可写
            public override long Length => _innerStream.Length;
            public override long Position { get => _innerStream.Position; set => _innerStream.Position = value; }

            public override void Flush() => _innerStream.Flush();
            public override int Read(byte[] buffer, int offset, int count) => _innerStream.Read(buffer, offset, count);
            public override long Seek(long offset, SeekOrigin origin) => _innerStream.Seek(offset, origin);
            public override void SetLength(long value) => _innerStream.SetLength(value);
            public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

            protected override void Dispose(bool disposing)
            {
                _innerStream.Dispose();
                base.Dispose(disposing);
            }
        }
        #endregion

        #region 清理测试资源
        public void Dispose()
        {
            // 清理临时文件
            if (File.Exists(_tempFilePath))
            {
                File.Delete(_tempFilePath);
            }
        }
        #endregion
    }
}
