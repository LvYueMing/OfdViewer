using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using OFDViewer.Models.BaseStructure.MainEntry;
using OFDViewer.Utils;
using Xunit;

namespace OFDViewer.Tests
{

    public class RootOFDTests
    {
        /// <summary>
        /// 测试目标：验证默认构造函数是否设置正确的默认值
        /// 测试场景：使用无参构造函数创建 OFD 对象
        /// 预期结果：Version 为 "1.0"，DocTypeString 为 "OFD"，DocBodies 为空列表
        /// </summary>
        [Fact]
        public void Constructor_Default_ShouldSetDefaultValues()
        {
            // 准备测试数据
            var ofd = new RootOFD();

            // 验证默认值设置
            Assert.Equal("1.0", ofd.Version);
            Assert.Equal("OFD", ofd.DocTypeString);
            Assert.NotNull(ofd.DocBodies);
            Assert.Empty(ofd.DocBodies);
        }

        /// <summary>
        /// 测试目标：验证带参构造函数是否正确设置属性
        /// 测试场景：使用合法参数创建 OFD 对象
        /// 预期结果：属性值与传入参数一致，Version 保持默认值
        /// </summary>
        [Fact]
        public void Constructor_WithParameters_ShouldSetProperties()
        {
            // 准备测试数据
            var docBodies = new List<DocBody> { new DocBody() };

            // 执行测试
            var ofd = new RootOFD("OFD", docBodies);

            // 验证属性设置
            Assert.Equal("1.0", ofd.Version);
            Assert.Equal("OFD", ofd.DocTypeString);
            Assert.Single(ofd.DocBodies);
            Assert.Equal(docBodies, ofd.DocBodies);
        }

        /// <summary>
        /// 测试目标：验证构造函数支持 OFD-A 文档类型
        /// 测试场景：使用 "OFD-A" 作为 DocType 参数
        /// 预期结果：DocTypeString 正确设置为 "OFD-A"
        /// </summary>
        [Fact]
        public void Constructor_With_OFD_A_ShouldSetDocType()
        {
            // 准备测试数据
            var docBodies = new List<DocBody> { new DocBody() };

            // 执行测试
            var ofd = new RootOFD("OFD-A", docBodies);

            // 验证文档类型设置
            Assert.Equal("OFD-A", ofd.DocTypeString);
        }

        /// <summary>
        /// 测试目标：验证构造函数对空 DocBodies 列表的异常处理
        /// 测试场景：传入空的 DocBody 列表
        /// 预期结果：抛出 ArgumentException 异常，提示至少包含一个元素
        /// </summary>
        [Fact]
        public void Constructor_WithEmptyDocBodies_ShouldThrowArgumentException()
        {
            // 准备测试数据
            var emptyDocBodies = new List<DocBody>();

            // 验证异常抛出
            var exception = Assert.Throws<ArgumentException>(() => new RootOFD("OFD", emptyDocBodies));
            Assert.Contains("至少包含一个元素", exception.Message);
        }

        /// <summary>
        /// 测试目标：验证构造函数对空 DocBodies 参数的异常处理
        /// 测试场景：传入 null 作为 DocBodies 参数
        /// 预期结果：抛出 ArgumentNullException 异常
        /// </summary>
        [Fact]
        public void Constructor_WithNullDocBodies_ShouldThrowArgumentNullException()
        {
            // 验证空参数异常
            var exception = Assert.Throws<ArgumentNullException>(() => new RootOFD("OFD", null));
            Assert.Equal("docBodies", exception.ParamName);
        }

        /// <summary>
        /// 测试目标：验证 Version 属性设置有效值
        /// 测试场景：将 Version 属性设置为合法值 "1.1"
        /// 预期结果：属性值成功更新为 "1.1"
        /// </summary>
        [Fact]
        public void Version_SetValidValue_ShouldUpdateProperty()
        {
            // 准备测试数据
            var ofd = new RootOFD();

            // 执行属性设置
            ofd.Version = "1.1";

            // 验证属性更新
            Assert.Equal("1.1", ofd.Version);
        }

        /// <summary>
        /// 测试目标：验证 Version 属性设置无效值的异常处理
        /// 测试场景：使用多个无效的版本号进行测试
        /// 预期结果：所有无效值都应抛出 ArgumentException 异常
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("2.0")]
        [InlineData("invalid")]
        public void Version_SetInvalidValue_ShouldThrowArgumentException(string invalidVersion)
        {
            // 准备测试数据
            var ofd = new RootOFD();

            // 验证异常抛出
            var exception = Assert.Throws<ArgumentException>(() => ofd.Version = invalidVersion);
            Assert.Equal("value", exception.ParamName);
            Assert.Contains("必须为有效的版本号", exception.Message);
        }

        /// <summary>
        /// 测试目标：验证 DocTypeString 属性设置有效值
        /// 测试场景：将 DocTypeString 设置为 "OFD-A"
        /// 预期结果：属性值成功更新为 "OFD-A"
        /// </summary>
        [Fact]
        public void DocTypeString_SetValidValue_ShouldUpdateDocType()
        {
            // 准备测试数据
            var ofd = new RootOFD();

            // 执行属性设置
            ofd.DocTypeString = "OFD-A";

            // 验证属性更新
            Assert.Equal("OFD-A", ofd.DocTypeString);
        }

        /// <summary>
        /// 测试目标：验证 DocTypeString 属性设置无效值的异常处理
        /// 测试场景：使用多个无效的文档类型进行测试
        /// 预期结果：所有无效值都应抛出 ArgumentException 异常
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("invalid")]
        [InlineData("OFD-B")]
        public void DocTypeString_SetInvalidValue_ShouldThrowArgumentException(string invalidDocType)
        {
            // 准备测试数据
            var ofd = new RootOFD();

            // 验证异常抛出
            var exception = Assert.Throws<ArgumentException>(() => ofd.DocTypeString = invalidDocType);
            Assert.Equal("value", exception.ParamName);
            Assert.Contains("必须为 \"OFD\" 或 \"OFD-A\"", exception.Message);
        }

        /// <summary>
        /// 测试目标：验证 AddDocBody 方法添加有效对象
        /// 测试场景：向空的 DocBodies 列表添加一个 DocBody 对象
        /// 预期结果：列表包含一个元素，且是该添加的对象
        /// </summary>
        [Fact]
        public void AddDocBody_ValidDocBody_ShouldAddToList()
        {
            // 准备测试数据
            var ofd = new RootOFD();
            var docBody = new DocBody();

            // 执行添加操作
            ofd.AddDocBody(docBody);

            //验证ofd.DocBodies 中有一个元素
            Assert.Equal(1, ofd.DocBodies.Count);
            Assert.Contains(docBody, ofd.DocBodies);
        }

        /// <summary>
        /// 测试目标：验证 AddDocBody 方法对空参数的异常处理
        /// 测试场景：尝试添加 null 到 DocBodies 列表
        /// 预期结果：抛出 ArgumentNullException 异常
        /// </summary>
        [Fact]
        public void AddDocBody_NullDocBody_ShouldThrowArgumentNullException()
        {
            // 准备测试数据
            var ofd = new RootOFD();

            // 验证异常抛出
            var exception = Assert.Throws<ArgumentNullException>(() => ofd.AddDocBody(null));
            Assert.Equal("docBody", exception.ParamName);
        }

        /// <summary>
        /// 测试目标：验证默认 OFD 对象序列化为 XML 的正确性
        /// 测试场景：使用默认 OFD 对象进行 XML 序列化
        /// 预期结果：抛出 XmlRequiredValidationException 异常
        /// </summary>
        [Fact]
        public void SerializeToXml_DefaultOFD_ShouldThrowXmlRequiredValidationException()
        {
            // 准备测试数据和序列化器
            var ofd = new RootOFD();

            //XmlHelper.SerializeToString 序列化时，特性校验list数量，会异常，断言异常
            var exception = Assert.Throws<XmlRequiredValidationException>(() => XmlHelper.SerializeToString<RootOFD>(ofd));
            Assert.Equal("OFDViewer.Models.BaseStructure.MainEntry.RootOFD.DocBodies", exception.PropertyName);
            Assert.Contains("元素个数无效", exception.Message);
        }

        /// <summary>
        /// 测试目标：验证默认 OFD 对象添加一个 DocBody 序列化为 XML 的正确性
        /// 测试场景：使用默认 OFD 对象进行 XML 序列化
        /// 预期结果：生成的 XML 包含正确的版本、文档类型、命名空间和 DocBody 元素,
        ///         元素节点应使用命名空间标识 ofd ,元素属性不使用命名空间标识
        /// </summary>
        [Fact]
        public void SerializeToXml_WithSingleDocBody_ShouldGenerateCorrectXml()
        {
            // 准备测试数据和序列化器
            var ofd = new RootOFD();
            ofd.AddDocBody(new DocBody());

            var xml = XmlHelper.SerializeToString(ofd);
            XmlHelper.SerializeToFile(ofd, "OFD.xml");

            // 使用 XmlDocument 进行详细检查
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            // 验证根元素
            var ofdElement = xmlDoc.DocumentElement;
            Assert.NotNull(ofdElement);
            Assert.Equal("ofd:OFD", ofdElement.Name);

            //验证命名空间 URI 和 命名空间标识符 xmlns:ofd
            Assert.Equal(Constants.OFD_NAMESPACE_URI, ofdElement.NamespaceURI);
            var xmlnsOFD = ofdElement.Attributes["xmlns:ofd"];
            Assert.NotNull(xmlnsOFD);
            Assert.Equal(Constants.OFD_NAMESPACE_URI, xmlnsOFD.Value);


            // 验证属性
            Assert.Equal("1.0", ofdElement.GetAttribute("Version"));
            Assert.Equal("OFD", ofdElement.GetAttribute("DocType"));

            // 验证 DocBody 元素
            var docBodyNodes = xmlDoc.SelectNodes($"//*[local-name()='DocBody' and namespace-uri()='{Constants.OFD_NAMESPACE_URI}']");
            Assert.NotNull(docBodyNodes);
            Assert.Equal(1, docBodyNodes.Count);

        }

        /// <summary>
        /// 测试目标：验证包含多个 DocBody 的 OFD 对象序列化为 XML 的正确性
        /// 测试场景：使用包含两个 DocBody 的 OFD 对象进行 XML 序列化
        /// 预期结果：生成的 XML 包含两个 DocBody 元素
        /// </summary>
        [Fact]
        public void SerializeToXml_WithMultipleDocBodies_ShouldIncludeAllDocBodies()
        {
            // 准备测试数据
            var docBodies = new List<DocBody> { new DocBody(), new DocBody() };
            var ofd = new RootOFD("OFD", docBodies);
            var serializer = new XmlSerializer(typeof(RootOFD));

            // 执行序列化
            var xml = XmlHelper.SerializeToString(ofd);
            //XmlHelper.SerializeToFile(ofd, "OFD.xml");

            // 验证 XML 结构
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);


            var docBodyNodes = xmlDoc.SelectNodes($"//*[local-name()='DocBody' and namespace-uri()='{Constants.OFD_NAMESPACE_URI}']");
            Assert.NotNull(docBodyNodes);
            Assert.Equal(2, docBodyNodes.Count);
        }

        /// <summary>
        /// 测试目标：验证 OFD-A 文档类型在 XML 序列化中的正确表示
        /// 测试场景：使用 OFD-A 文档类型的 OFD 对象进行 XML 序列化
        /// 预期结果：生成的 XML 中 DocType 属性值为 "OFD-A"
        /// </summary>
        [Fact]
        public void SerializeToXml_WithOFD_A_DocType_ShouldIncludeCorrectDocType()
        {
            // 准备测试数据
            var docBodies = new List<DocBody> { new DocBody() };
            var ofd = new RootOFD("OFD-A", docBodies);

            // 执行序列化
            var xml = XmlHelper.SerializeToString(ofd);
            //XmlHelper.SerializeToFile(ofd, "OFD.xml");

            // 验证 XML 结构
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            var ofdElement = xmlDoc.DocumentElement;

            // 验证文档类型属性
            Assert.Equal("OFD-A", ofdElement.GetAttribute("DocType"));
        }

        /// <summary>
        /// 测试目标：验证非默认版本号在 XML 序列化中的正确表示
        /// 测试场景：使用版本号为 "1.1" 的 OFD 对象进行 XML 序列化
        /// 预期结果：生成的 XML 中 Version 属性值为 "1.1"
        /// </summary>
        [Fact]
        public void SerializeToXml_WithVersion_1_1_ShouldIncludeCorrectVersion()
        {
            // 准备测试数据
            var docBodies = new List<DocBody> { new DocBody() };
            var ofd = new RootOFD("OFD", docBodies)
            {
                Version = "1.1"
            };

            // 执行序列化
            var xml = XmlHelper.SerializeToString(ofd);
            //XmlHelper.SerializeToFile(ofd, "OFD.xml");

            // 验证 XML 结构
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            var ofdElement = xmlDoc.DocumentElement;

            // 验证版本属性
            Assert.Equal("1.1", ofdElement.GetAttribute("Version"));
        }


        /// <summary>
        /// 测试目标：验证序列化和反序列化的往返数据完整性
        /// 测试场景：将 OFD 对象序列化为 XML，再反序列化为新对象
        /// 预期结果：原始对象和反序列化后的对象属性值一致
        /// </summary>
        [Fact]
        public void SerializeAndDeserialize_ShouldMaintainDataIntegrity()
        {
            // 准备原始测试数据
            var original = new RootOFD("OFD-A", new List<DocBody> { new DocBody() })
            {
                Version = "1.1"
            };

            // 执行序列化
            var xml = XmlHelper.SerializeToString(original);

            // 执行反序列化
            var deserialized = XmlHelper.DeserializeFromString<RootOFD>(xml);

            // 验证数据完整性
            Assert.NotNull(deserialized);
            Assert.Equal(original.Version, deserialized.Version);
            Assert.Equal(original.DocTypeString, deserialized.DocTypeString);
            Assert.Equal(original.DocBodies.Count, deserialized.DocBodies.Count);
        }


        /// <summary>
        /// 测试目标：验证 XML 反序列化功能
        /// 测试场景：从有效的 XML 字符串反序列化为 OFD 对象
        /// 预期结果：成功创建 OFD 实例，属性值与 XML 一致
        /// </summary>
        [Fact]
        public void DeserializeFromXml_ValidXml_ShouldCreateOFDInstance()
        {
            // 准备测试 XML
            var xml = $@"<OFD Version=""1.0"" DocType=""OFD"" xmlns=""{Constants.OFD_NAMESPACE_URI}"">
                      <DocBody />
                    </OFD>";
            var serializer = new XmlSerializer(typeof(RootOFD));

            // 执行反序列化
            using var stringReader = new StringReader(xml);
            var ofd = (RootOFD)serializer.Deserialize(stringReader);

            // 验证反序列化结果
            Assert.NotNull(ofd);
            Assert.Equal("1.0", ofd.Version);
            Assert.Equal("OFD", ofd.DocTypeString);
            Assert.Single(ofd.DocBodies);
        }

        /// <summary>
        /// 测试目标：验证 XmlRoot 特性配置的正确性
        /// 测试场景：检查 OFD 类的 XmlRootAttribute 特性
        /// 预期结果：特性正确设置了元素名和命名空间
        /// </summary>
        [Fact]
        public void XmlRootAttribute_ShouldHaveCorrectNamespace()
        {
            // 获取 XmlRootAttribute 特性
            var xmlRootAttribute = typeof(RootOFD).GetCustomAttribute<XmlRootAttribute>();

            // 验证特性配置
            Assert.NotNull(xmlRootAttribute);
            Assert.Equal("OFD", xmlRootAttribute.ElementName);
            Assert.Equal(Constants.OFD_NAMESPACE_URI, xmlRootAttribute.Namespace);
        }

        /// <summary>
        /// 测试目标：验证 DocBodies 属性的 XmlElement 特性配置的正确性
        /// 测试场景：检查 DocBodies 属性的 XmlElementAttribute 特性
        /// 预期结果：特性正确设置了元素名和命名空间
        /// </summary>
        [Fact]
        public void XmlElementAttribute_ForDocBodies_ShouldHaveCorrectNamespace()
        {
            // 获取属性信息
            var propertyInfo = typeof(RootOFD).GetProperty("DocBodies");
            var xmlElementAttribute = propertyInfo.GetCustomAttribute<XmlElementAttribute>();

            // 验证特性配置
            Assert.NotNull(xmlElementAttribute);
            Assert.Equal("DocBody", xmlElementAttribute.ElementName);
        }

        // 测试目标：验证 DocBody.DocRoot 属性设置值的正确性
        // 测试场景：将 DocRoot 属性设置为通过 Constants.GetOfdFilePath 构造的路径
        // 预期结果：DocRoot 属性值正确反映构造的路径
        [Fact]
        public void SerializeToXml_WithDocRoot_SetValue_ShouldConstructCorrectPath()
        {
            var ofd = new RootOFD();
            var docBody = new DocBody();
            docBody.DocRoot = Constants.GetFilePath(Constants.Doc_DocumentFile, 0);
            ofd.AddDocBody(docBody);

            var xml = XmlHelper.SerializeToString(ofd);
            XmlHelper.SerializeToFile(ofd, "OFD.xml");

            // 使用 XmlDocument 进行详细检查
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            // 创建XmlNamespaceManager实例，关联XmlDocument的NameTable
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(xmlDoc.NameTable);

            // 添加命名空间前缀映射（ofd为前缀，对应OFD标准的命名空间URI）
            // 注意：此处的命名空间URI需要与你OFD.xml中实际定义的ofd命名空间一致（通常为以下URI，可从XML根节点验证）
            nsMgr.AddNamespace("ofd", Constants.OFD_NAMESPACE_URI);

            // 在SelectSingleNode方法中传入XmlNamespaceManager实例
            XmlNode docRootNode = xmlDoc.DocumentElement.SelectSingleNode("ofd:DocBody/ofd:DocRoot", nsMgr);
            var rootPath = docRootNode?.InnerText;

            // 验证 DocRoot 属性是否正确设置
            Assert.Equal("Doc_0/Document.xml", rootPath);
        }
    }
}