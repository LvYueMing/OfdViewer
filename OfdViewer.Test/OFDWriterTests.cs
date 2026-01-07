using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OFDViewer.Models.BaseStructure.MainEntry;
using OFDViewer.OFD;
using Xunit;

namespace OFDViewer.Tests
{
    public class OFDWriterTests
    {
        // 测试临时文件路径（自动清理）
        private readonly string _tempFilePath;

        // 测试用的基础元数据对象（复用）
        private readonly RootOFD _testRootOfd;
        private readonly OFDDoc _testDoc;
        private readonly OFDDocument _testOfdDocument;

        public OFDWriterTests()
        {
            // 初始化临时文件路径
            _tempFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{Guid.NewGuid()}.ofd");

            // 创建默认OFD文档对象
            _testOfdDocument = new OFDDocument();

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
            var nonExistDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Guid.NewGuid().ToString());
            var filePath = Path.Combine(nonExistDir, "test.ofd");

            // Act
            using var writer = new OFDWriter(filePath);

            // Assert
            Assert.True(Directory.Exists(nonExistDir));
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
            var exception = Assert.Throws<ArgumentNullException>(() => writer.WriteOFDDocument(null));
            Assert.Equal("ofdDocument", exception.ParamName);
        }

        /// <summary>
        /// 测试：WriteEntireDocument - 文档无Metadata时抛出ArgumentNullException
        /// </summary>
        [Fact]
        public void WriteEntireDocument_OfdMetadataNull_ThrowsArgumentNullException()
        {
            // Arrange
            var invalidDoc = new OFDDocument();
            invalidDoc.RootOfd = null;
            using var ms = new MemoryStream();
            using var writer = new OFDWriter(ms, leaveOpen: true);

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => writer.WriteOFDDocument(invalidDoc));
            Assert.Equal("OfdMetadata", exception.ParamName);
        }

        /// <summary>
        /// 测试：WriteEntireDocument - 文档无子文档时抛出InvalidOperationException
        /// </summary>
        [Fact]
        public void WriteEntireDocument_NoDocs_ThrowsInvalidOperationException()
        {
            // Arrange
            var invalidDoc = new OFDDocument();
            invalidDoc.Docs.Clear();
            using var ms = new MemoryStream();
            using var writer = new OFDWriter(ms, leaveOpen: true);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => writer.WriteOFDDocument(invalidDoc));
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
            writer.WriteRootOFD(_testRootOfd);
            
            // Act
            writer.Save();
            // Assert
            Assert.True(ms.Length > 0); // 保存后流有数据
        }

        // 测试：Save方法 - writer.WriteRootOFD保存为磁盘文件
        [Fact]
        public void Save_ToFile_AfterWriteRootOFD_SavesSuccessfully()
        {
            // Arrange
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{Guid.NewGuid()}.ofd");
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
        }

        // 测试：Save方法 - writer.WriteOFDDoc保存为磁盘文件
        [Fact]
        public void Save_ToFile_AfterWriteOFDDoc_SavesSuccessfully()
        {
            // Arrange
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{Guid.NewGuid()}.ofd");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            Assert.False(File.Exists(filePath)); // 保存前文件不存在
            // Act
            using var writer = new OFDWriter(filePath);
            writer.WriteRootOFD(_testOfdDocument.RootOfd);
            writer.WriteOFDDoc(_testOfdDocument.DefaultDoc);
            writer.Save();
            // Assert
            Assert.True(File.Exists(filePath)); // 保存后文件存在
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
            writer.WriteOFDDocument(_testOfdDocument);

            // Act
            writer.Save();
            writer.Save(); // 重复调用

            // Assert
            Assert.True(ms.Length > 0); // 保存后流有数据
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
            Assert.Throws<ObjectDisposedException>(() => writer.WriteOFDDocument(_testOfdDocument));
            Assert.Throws<ObjectDisposedException>(() => writer.WriteRootOFD(_testRootOfd));
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
