using System.Xml;
using System.Xml.Schema;
using System.Net;
using OFDViewer.Parse;
using Xunit;

namespace OFDViewer.Tests
{
    /// <summary>
    /// OFD 写入、读取以及标准结构校验的集成回归测试。
    /// </summary>
    public class OFDReadWriteIntegrationTests
    {
        /// <summary>
        /// 最小单页文档写入内存后应能重新读取，并保留核心文档结构。
        /// </summary>
        [Fact]
        public void WriteThenRead_MinimalDocument_PreservesStructure()
        {
            var source = CreateMinimalDocument();
            using var stream = WriteToStream(source);

            using var reader = new OFDReader(stream, leaveOpen: true);
            var result = reader.ParseOFDDocument();

            Assert.Single(result.Docs);
            Assert.Equal(source.RootOfd.DocBodies[0].DocInfo.DocID,
                result.RootOfd.DocBodies[0].DocInfo.DocID);
            Assert.Single(result.DefaultOFDDocument.Document.Pages);
            Assert.Single(result.DefaultOFDDocument.PageDocs);
            Assert.Equal("Doc_0/Document.xml", result.DefaultOFDDocument.DocumentFilePath.Replace('\\', '/'));
        }

        /// <summary>
        /// 仓库携带的核心 XSD 应可在任意机器上通过相对路径加载并校验写出的核心 XML。
        /// </summary>
        [Fact]
        public void GeneratedCoreXml_ValidatesAgainstBundledSchemas()
        {
            var source = CreateMinimalDocument();
            using var stream = WriteToStream(source);
            using var archive = OFDArchive.OpenFromStream(stream, leaveOpen: true);

            ValidateXml(archive, "OFD.xml", "OFD.xsd");
            ValidateXml(archive, "Doc_0/Document.xml", "Document.xsd");
            ValidateXml(archive, "Doc_0/Pages/Page_0/Content.xml", "Page.xsd");
        }

        private static OFDRootDocument CreateMinimalDocument()
        {
            var document = new OFDRootDocument();
            document.DefaultOFDDocument.NewPageDoc();
            return document;
        }

        private static MemoryStream WriteToStream(OFDRootDocument document)
        {
            var stream = new MemoryStream();
            using (var writer = new OFDWriter(stream, leaveOpen: true))
            {
                writer.WriteOFDRootDoc(document);
                writer.Save();
            }

            stream.Position = 0;
            return stream;
        }

        private static void ValidateXml(OFDArchive archive, string entryPath, string schemaFileName)
        {
            string schemaPath = Path.Combine(AppContext.BaseDirectory, "Schemas", schemaFileName);
            var schemas = new XmlSchemaSet
            {
                // 旧版 OFD.xsd 带有开发机绝对 include 路径，仅允许解析到测试输出中的 XSD。
                XmlResolver = new LocalSchemaResolver(Path.GetDirectoryName(schemaPath)!)
            };
            schemas.Add(null, schemaPath);

            var errors = new List<string>();
            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null,
                Schemas = schemas,
                ValidationType = ValidationType.Schema
            };
            settings.ValidationEventHandler += (_, args) => errors.Add(args.Message);

            using var xmlStream = archive.OpenFileStream(entryPath);
            using var xmlReader = XmlReader.Create(xmlStream, settings);
            while (xmlReader.Read())
            {
            }

            Assert.True(errors.Count == 0,
                $"{entryPath} 未通过 {schemaFileName} 校验：{string.Join(Environment.NewLine, errors)}");
        }

        /// <summary>
        /// 将 XSD include 严格限制在随测试复制的 Schemas 目录，避免访问任意本机或网络位置。
        /// </summary>
        private sealed class LocalSchemaResolver : XmlResolver
        {
            private readonly string _schemaDirectory;

            public LocalSchemaResolver(string schemaDirectory)
            {
                _schemaDirectory = Path.GetFullPath(schemaDirectory);
            }

            public override ICredentials? Credentials
            {
                set { }
            }

            public override object GetEntity(Uri absoluteUri, string? role, Type? ofObjectToReturn)
            {
                string schemaPath = Path.GetFullPath(
                    Path.Combine(_schemaDirectory, Path.GetFileName(absoluteUri.LocalPath)));
                string allowedPrefix = _schemaDirectory.TrimEnd(Path.DirectorySeparatorChar)
                    + Path.DirectorySeparatorChar;

                if (!schemaPath.StartsWith(allowedPrefix, StringComparison.OrdinalIgnoreCase))
                    throw new XmlException($"XSD 引用超出允许目录：{absoluteUri}");

                return File.OpenRead(schemaPath);
            }
        }
    }
}
