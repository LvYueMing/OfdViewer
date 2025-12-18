using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml;
using OFDViewer.OFDReader;
using Xunit;

namespace OFDViewer.Tests
{
    public class OfdArchiveTests : IDisposable
    {
        private readonly MemoryStream _zipStream;
        private readonly OfdArchive _archive;

        public OfdArchiveTests()
        {
            // 创建内存中的 zip 包，包含一个文本文件和一个 XML 文件
            _zipStream = new MemoryStream();
            using (var zip = new ZipArchive(_zipStream, ZipArchiveMode.Update, true))
            {
                var entry1 = zip.CreateEntry("test.txt");
                using (var writer = new StreamWriter(entry1.Open(), Encoding.UTF8, leaveOpen: true))
                {
                    writer.Write("hello world");
                }

                var entry2 = zip.CreateEntry("test.xml");
                using (var writer = new StreamWriter(entry2.Open(), Encoding.UTF8, leaveOpen: true))
                {
                    writer.Write("<?xml version=\"1.0\"?><root><child>abc</child></root>");
                }
            }
            _zipStream.Position = 0;
            _archive = OfdArchive.OpenFromStream(_zipStream, ZipArchiveMode.Read, leaveOpen: true);
        }

        [Fact]
        public void Open_FilePath_ShouldOpenArchive()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            File.WriteAllBytes(tempFile, _zipStream.ToArray());

            try
            {
                // Act
                using var archive = OfdArchive.OpenFromFile(tempFile);

                // Assert
                Assert.NotNull(archive);
            }
            finally
            {
                // 确保资源释放后再删除
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        public void Open_Stream_ShouldOpenArchive()
        {
            using var archive = OfdArchive.OpenFromStream(new MemoryStream(_zipStream.ToArray()));
            Assert.NotNull(archive);
        }

        [Fact]
        public void GetFileStream_ValidFile_ShouldReturnStream()
        {
            using var stream = _archive.GetFileStream("test.txt");
            Assert.NotNull(stream);
            using var reader = new StreamReader(stream);
            var content = reader.ReadToEnd();
            Assert.Equal("hello world", content);
        }

        [Fact]
        public void GetFileStream_InvalidFile_ShouldThrow()
        {
            Assert.Throws<FileNotFoundException>(() => _archive.GetFileStream("notfound.txt"));
        }

        [Fact]
        public void ReadTextFile_ValidFile_ShouldReturnContent()
        {
            var content = _archive.ReadTextFile("test.txt");
            Assert.Equal("hello world", content);
        }

        [Fact]
        public void ReadTextFile_InvalidFile_ShouldThrow()
        {
            Assert.Throws<FileNotFoundException>(() => _archive.ReadTextFile("notfound.txt"));
        }

        [Fact]
        public void ReadXmlFile_ValidFile_ShouldReturnXmlDocument()
        {
            var doc = _archive.ReadXmlFile("test.xml");
            Assert.NotNull(doc);
            Assert.Equal("root", doc.DocumentElement.Name);
            Assert.Equal("abc", doc.DocumentElement["child"].InnerText);
        }

        [Fact]
        public void ReadXmlFile_InvalidFile_ShouldThrow()
        {
            Assert.Throws<FileNotFoundException>(() => _archive.ReadXmlFile("notfound.xml"));
        }

        [Fact]
        public void ExtractToTempDirectory_ShouldExtractFiles()
        {
            var tempDir = _archive.ExtractToTempDirectory();
            try
            {
                var txtPath = Path.Combine(tempDir, "test.txt");
                var xmlPath = Path.Combine(tempDir, "test.xml");
                Assert.True(File.Exists(txtPath));
                Assert.True(File.Exists(xmlPath));
                Assert.Equal("hello world", File.ReadAllText(txtPath));
            }
            finally
            {
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void Dispose_ShouldNotThrow()
        {
            _archive.Dispose();
        }



        public void Dispose()
        {
            _archive.Dispose();
            _zipStream.Dispose();
        }

        [Fact]
        public void ExtractAndReadOFD_LocalFile()
        {
            string ofdPath = @"C:\Users\Administrator\Desktop\test.ofd"; // 替换为你的 OFD 文件路径
            using var archive = OfdArchive.OpenFromFile(ofdPath);

            // 解压
            var tempDir = archive.ExtractToTempDirectory();
            Assert.True(Directory.Exists(tempDir));
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine($"已解压到：{tempDir}");

            // 输出所有文件名
            var entryCache = archive.GetType()
                .GetField("_entryCache", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(archive) as System.Collections.IDictionary;
            foreach (var entry in entryCache)
            {
                Console.WriteLine(((System.Collections.DictionaryEntry)entry).Key);
            }

            // 读取并输出某个 XML 文件内容
            string xmlFile = "Doc_0/Document.xml"; // 替换为实际 OFD 包内的 XML 路径
            var xml = archive.ReadXmlFile(xmlFile);
            Console.WriteLine(xml.OuterXml);

            // 清理
            Directory.Delete(tempDir, true);
        }
    }
}
