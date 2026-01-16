using System.Xml;
using OFDViewer.Models.BaseStructure.MainEntry;
using OFDViewer.OFD;
using OFDViewer.Utils;
using Xunit;

namespace OFDViewer.Tests
{
    public class OFDDocumentTests
    {
        /// <summary>
        /// 测试目标：验证默认构造函数是否正确初始化OFDDocument对象
        /// 测试场景：使用无参构造函数创建OFDDocument实例
        /// 预期结果：RootOfd和Docs属性被正确初始化，Docs包含一个默认文档
        /// </summary>
        [Fact]
        public void Constructor_Default_ShouldInitializeDefaultValues()
        {
            // 执行测试
            var ofdDocument = new OFDRootDocument();

            // 验证RootOfd属性
            Assert.NotNull(ofdDocument.RootOfd);
            Assert.Equal("1.0", ofdDocument.RootOfd.Version);
            Assert.Equal("OFD", ofdDocument.RootOfd.DocTypeString);

            // 验证Docs属性
            Assert.NotNull(ofdDocument.Docs);
            Assert.Single(ofdDocument.Docs);
            Assert.Equal(0, ofdDocument.Docs[0].DocIndex);

            // 验证DocCount属性
            Assert.Equal(1, ofdDocument.DocCount);

            // 验证RootOfdFilePath属性
            Assert.Equal(Constants.Root_OfdFile, ofdDocument.RootOfdFile);

            // 验证OFDRootDirectory属性
            Assert.Equal("/", ofdDocument.OFDRootDirectory);
        }

        /// <summary>
        /// 测试目标：验证AddNewDoc方法是否正确添加新文档
        /// 测试场景：创建OFDDocument实例并调用AddNewDoc方法
        /// 预期结果：Docs集合增加一个新文档，RootOfd.DocBodies同步增加对应条目
        /// </summary>
        [Fact]
        public void AddNewDoc_ShouldAddDocToCollection()
        {
            // 准备测试数据
            var ofdDocument = new OFDRootDocument();
            var initialDocCount = ofdDocument.DocCount;

            // 执行测试
            var newDoc = ofdDocument.AddNewDoc();

            // 验证Docs集合
            Assert.Equal(initialDocCount + 1, ofdDocument.DocCount);
            Assert.Contains(newDoc, ofdDocument.Docs);
            Assert.Equal(1, newDoc.DocIndex);

            // 验证RootOfd.DocBodies同步更新
            Assert.Equal(ofdDocument.DocCount, ofdDocument.RootOfd.DocBodies.Count);
            var docBody = ofdDocument.RootOfd.DocBodies.Last();
            Assert.Equal(Constants.GetFilePath(Constants.Doc_DocumentFile, 1), docBody.DocRootPath);
        }

        /// <summary>
        /// 测试目标：验证DefaultDoc属性是否正确返回第一个文档
        /// 测试场景：访问OFDDocument的DefaultDoc属性
        /// 预期结果：返回Docs集合中的第一个文档
        /// </summary>
        [Fact]
        public void DefaultDoc_ShouldReturnFirstDocument()
        {
            // 准备测试数据
            var ofdDocument = new OFDRootDocument();
            var firstDoc = ofdDocument.Docs[0];

            // 执行测试
            var defaultDoc = ofdDocument.DefaultOFDDoc;

            // 验证结果
            Assert.Same(firstDoc, defaultDoc);
        }

        /// <summary>
        /// 测试目标：验证DefaultDoc属性在Docs集合为空时自动创建文档
        /// 测试场景：创建OFDDocument实例后清空Docs集合，然后访问DefaultDoc属性
        /// 预期结果：Docs集合自动创建一个新文档并返回
        /// </summary>
        [Fact]
        public void DefaultDoc_WhenDocsEmpty_ShouldCreateNewDocument()
        {
            // 准备测试数据
            var ofdDocument = new OFDRootDocument();
            ofdDocument.Docs.Clear();

            // 执行测试
            var defaultDoc = ofdDocument.DefaultOFDDoc;

            // 验证结果
            Assert.Single(ofdDocument.Docs);
            Assert.Same(defaultDoc, ofdDocument.Docs[0]);
            Assert.Equal(0, defaultDoc.DocIndex);
        }

        /// <summary>
        /// 测试目标：验证OFDDocument对象序列化后的XML格式正确性
        /// 测试场景：将OFDDocument对象序列化为XML字符串
        /// 预期结果：生成的XML包含正确的结构和命名空间
        /// </summary>
        [Fact]
        public void SerializeToXml_ShouldGenerateCorrectXml()
        {
            // 准备测试数据
            var ofdDocument = new OFDRootDocument();

            // 执行测试
            var xml = XmlHelper.SerializeToString(ofdDocument.RootOfd);

            // 使用XmlDocument验证XML结构
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            // 验证根元素
            var ofdElement = xmlDoc.DocumentElement;
            Assert.NotNull(ofdElement);
            Assert.Equal("ofd:OFD", ofdElement.Name);
            Assert.Equal(Constants.OFD_NAMESPACE_URI, ofdElement.NamespaceURI);

            // 验证命名空间
            var xmlnsOfd = ofdElement.Attributes["xmlns:ofd"];
            Assert.NotNull(xmlnsOfd);
            Assert.Equal(Constants.OFD_NAMESPACE_URI, xmlnsOfd.Value);

            // 验证属性
            Assert.Equal("1.0", ofdElement.GetAttribute("Version"));
            Assert.Equal("OFD", ofdElement.GetAttribute("DocType"));

            // 验证DocBody元素

            // // XPath 的路径定位语法，表示在整个 XML 文档中递归查找所有层级的节点（不局限于根节点的直接子节点，深层嵌套的节点也会被匹配）。
            // XPath 查询时不能直接使用节点的 “前缀 + 节点名”（如//ofd:DocBody）或仅节点名（如//DocBody）进行匹配
            // 本段代码使用local-name()+namespace-uri()的组合，正是为了绕过命名空间前缀的影响，精准匹配节点的本地名称和所属命名空间 URI，从而正确获取目标节点
            // [] 谓词（条件筛选）语法，表示只保留满足括号内条件的节点，相当于编程中的where筛选。
            // local-name()是 XPath 的内置函数，用于获取 XML 节点的本地名称（不包含命名空间前缀的节点名）。该条件表示：节点的本地名称必须严格等于DocBody。
            // namespace-uri()是 XPath 的内置函数，用于获取 XML 节点所属的XML 命名空间的 URI（统一资源标识符）。该条件表示：节点的命名空间 URI 必须严格等于Constants.OFD_NAMESPACE_URI常量存储的值
            var docBodyNodes = xmlDoc.SelectNodes($"//*[local-name()='DocBody' and namespace-uri()='{Constants.OFD_NAMESPACE_URI}']");
            Assert.NotNull(docBodyNodes);
            Assert.Single(docBodyNodes);

            // 验证DocRoot元素
            var docRootNode = docBodyNodes[0].SelectSingleNode($"*[local-name()='DocRoot' and namespace-uri()='{Constants.OFD_NAMESPACE_URI}']");
            Assert.NotNull(docRootNode);
            Assert.Equal("Doc_0/Document.xml", docRootNode.InnerText);
        }

        /// <summary>
        /// 测试目标：验证多文档OFD的XML序列化格式
        /// 测试场景：创建包含多个文档的OFDDocument对象并序列化
        /// 预期结果：生成的XML包含多个DocBody元素，每个DocBody对应一个文档
        /// </summary>
        [Fact]
        public void SerializeToXml_WithMultipleDocs_ShouldGenerateCorrectXml()
        {
            // 准备测试数据
            var ofdDocument = new OFDRootDocument();
            ofdDocument.AddNewDoc();
            ofdDocument.AddNewDoc();

            // 执行测试
            var xml = XmlHelper.SerializeToString(ofdDocument.RootOfd);

            // 使用XmlDocument验证XML结构
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            // 验证DocBody元素数量
            var docBodyNodes = xmlDoc.SelectNodes($"//*[local-name()='DocBody' and namespace-uri()='{Constants.OFD_NAMESPACE_URI}']");
            Assert.NotNull(docBodyNodes);
            Assert.Equal(3, docBodyNodes.Count);

            // 验证每个DocBody的DocRoot路径
            for (int i = 0; i < 3; i++)
            {
                var docRootNode = docBodyNodes[i].SelectSingleNode($"*[local-name()='DocRoot' and namespace-uri()='{Constants.OFD_NAMESPACE_URI}']");
                Assert.NotNull(docRootNode);
                Assert.Equal($"Doc_{i}/Document.xml", docRootNode.InnerText);
            }
        }

        /// <summary>
        /// 测试目标：验证序列化和反序列化的往返数据完整性
        /// 测试场景：将OFDDocument.RootOfd序列化为XML，再反序列化为新对象
        /// 预期结果：原始对象和反序列化后的对象属性值一致
        /// </summary>
        [Fact]
        public void SerializeAndDeserialize_ShouldMaintainDataIntegrity()
        {
            // 准备测试数据
            var ofdDocument = new OFDRootDocument();
            ofdDocument.AddNewDoc();
            var originalRootOfd = ofdDocument.RootOfd;
            originalRootOfd.Version = "1.1";

            // 执行序列化
            var xml = XmlHelper.SerializeToString(originalRootOfd);

            // 执行反序列化
            var deserializedRootOfd = XmlHelper.DeserializeFromString<RootOFD>(xml);

            // 验证数据完整性
            Assert.NotNull(deserializedRootOfd);
            Assert.Equal(originalRootOfd.Version, deserializedRootOfd.Version);
            Assert.Equal(originalRootOfd.DocTypeString, deserializedRootOfd.DocTypeString);
            Assert.Equal(originalRootOfd.DocBodies.Count, deserializedRootOfd.DocBodies.Count);

            // 验证DocBody路径
            for (int i = 0; i < originalRootOfd.DocBodies.Count; i++)
            {
                Assert.Equal(originalRootOfd.DocBodies[i].DocRootPath, deserializedRootOfd.DocBodies[i].DocRootPath);
            }
        }
    }
}