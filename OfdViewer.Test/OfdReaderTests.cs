using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using OFDViewer.OFDReader;
using Xunit;
using OFDViewer.Utils;

namespace OFDViewer.Tests
{
    public class OfdReaderTests : IDisposable
    {
        private MemoryStream _ofdStream;
        private OfdArchive _archive;

        // 简单的 OFD.xml 内容，符合 OFD 根节点要求
        private const string OfdXmlContent = """
        <?xml version="1.0" encoding="utf-8"?>
        <OFD Version="1.0" DocType="OFD" xmlns="http://www.ofdspec.org/2016">
          <DocBody>
            <DocInfo />
            <DocRoot>Doc_0/Document.xml</DocRoot>
          </DocBody>
        </OFD>
        """;

        private void CreateOfdArchiveWithOfdXml()
        {
            _ofdStream = new MemoryStream();
            using (var zip = new System.IO.Compression.ZipArchive(_ofdStream, System.IO.Compression.ZipArchiveMode.Update, true))
            {
                var entry = zip.CreateEntry("OFD.xml");
                using var writer = new StreamWriter(entry.Open(), Encoding.UTF8, leaveOpen: true);
                writer.Write(OfdXmlContent);
            }
            _ofdStream.Position = 0;
            _archive = OfdArchive.OpenFromStream(_ofdStream, System.IO.Compression.ZipArchiveMode.Read, leaveOpen: true);
        }

        [Fact]
        public void Ctor_FilePath_ShouldThrow_WhenFilePathIsNullOrEmpty()
        {
            Assert.Throws<ArgumentNullException>(() => new OfdReader(null,false));
            Assert.Throws<ArgumentNullException>(() => new OfdReader(""));
            Assert.Throws<ArgumentNullException>(() => new OfdReader("   "));
        }

        [Fact]
        public void Ctor_FilePath_ShouldThrow_WhenFileNotExist()
        {
            string notExistPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".ofd");
            Assert.False(File.Exists(notExistPath));
            Assert.Throws<FileNotFoundException>(() => new OfdReader(notExistPath));
        }

        [Fact]
        public void Ctor_FilePath_ShouldSucceed_WhenFileExists()
        {
            CreateOfdArchiveWithOfdXml();
            string tempFile = Path.GetTempFileName();
            File.WriteAllBytes(tempFile, _ofdStream.ToArray());
            try
            {
                using var reader = new OfdReader(tempFile);
                Assert.NotNull(reader);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public void Ctor_Stream_ShouldThrow_WhenStreamIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new OfdReader((Stream)null, false));
        }

        [Fact]
        public void Ctor_Stream_ShouldSucceed()
        {
            CreateOfdArchiveWithOfdXml();
            using var reader = new OfdReader(new MemoryStream(_ofdStream.ToArray()), false);
            Assert.NotNull(reader);
        }

        [Fact]
        public void ParseOfdDocument_ShouldParseSuccessfully()
        {
            CreateOfdArchiveWithOfdXml();
            using var reader = new OfdReader(new MemoryStream(_ofdStream.ToArray()), false);
            var ofd = reader.ParseOfdDocument();
            Assert.NotNull(ofd);
            Assert.Equal("1.0", ofd.Version);
            Assert.Equal("OFD", ofd.DocTypeString);
            Assert.NotNull(ofd.DocBodies);
            Assert.Single(ofd.DocBodies);
        }

        [Fact]
        public void ParseOfdDocument_ShouldReturnCachedInstance()
        {
            CreateOfdArchiveWithOfdXml();
            using var reader = new OfdReader(new MemoryStream(_ofdStream.ToArray()), false);
            var ofd1 = reader.ParseOfdDocument();
            var ofd2 = reader.ParseOfdDocument();
            Assert.Same(ofd1, ofd2);
        }

        [Fact]
        public void ParseOfdDocument_ShouldThrow_WhenOFDXmlMissing()
        {
            // 构造没有 OFD.xml 的 zip
            _ofdStream = new MemoryStream();
            using (var zip = new System.IO.Compression.ZipArchive(_ofdStream, System.IO.Compression.ZipArchiveMode.Update, true))
            {
                var entry = zip.CreateEntry("other.txt");
                using var writer = new StreamWriter(entry.Open(), Encoding.UTF8, leaveOpen: true);
                writer.Write("test");
            }
            _ofdStream.Position = 0;

            using var reader = new OfdReader(new MemoryStream(_ofdStream.ToArray()), false);
            var ex = Assert.Throws<InvalidOperationException>(() => reader.ParseOfdDocument());
            Assert.Contains("解析 OFD 文档失败", ex.Message);
        }

        [Fact]
        public void Dispose_ShouldNotThrow_WhenCalledMultipleTimes()
        {
            CreateOfdArchiveWithOfdXml();
            var reader = new OfdReader(new MemoryStream(_ofdStream.ToArray()), false);
            reader.Dispose();
            reader.Dispose();
        }



        [Fact(DisplayName ="解析本地ofd文件")]
        public void ParseOfdDocument_ShouldParse()
        {
            var path = @"C:\Users\Administrator\Desktop\test.ofd";
            using var reader = new OfdReader(path);
            var ofd = reader.ParseOfdDocument();
            Assert.NotNull(ofd);
            Assert.Equal("1.1", ofd.Version);
            Assert.Equal("OFD", ofd.DocTypeString);
            Assert.NotNull(ofd.DocBodies);
            Assert.Single(ofd.DocBodies);
        }

        [Fact(DisplayName = "解析本地ofd文件,并序列化")]
        public void ParseOfdDocument_ParseAndSerializeToFile()
        {
            var path = @"C:\Users\Administrator\Desktop\";
            using var reader = new OfdReader(path + "test.ofd");
            var ofd = reader.ParseOfdDocument();
            Assert.NotNull(ofd);
            XmlHelper.SerializeToFile(ofd, path+ "test.xml");
        }


        public void Dispose()
        {
            _archive?.Dispose();
            _ofdStream?.Dispose();
        }
    }
}
