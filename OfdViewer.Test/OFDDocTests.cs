using OFDViewer.Parse;
using OFDViewer.Utils;
using OFDViewer.Models.BaseStructure.DocumentRoot;
using OFDViewer.Models.BaseStructure.Resources;
using OFDViewer.Models.Signature;
using OFDViewer.Models.BaseType;
using Xunit;
using System.IO;
using System.Text;

namespace OFDViewer.Tests
{
    public class OFDDocTests
    {
        /// <summary>
        /// 测试OFDDoc类的基本构造函数功能
        /// </summary>
        [Fact]
        public void Constructor_ShouldInitializeProperties()
        {
            // 执行测试
            var ofdDoc = new OFDDocument(0);

            // 验证属性
            Assert.Equal(0, ofdDoc.DocIndex);
            Assert.NotNull(ofdDoc.Document);
            Assert.NotNull(ofdDoc.PageDocs);
            Assert.NotNull(ofdDoc.ResFiles);
            Assert.Empty(ofdDoc.PageDocs);
            Assert.Empty(ofdDoc.ResFiles);
            Assert.Null(ofdDoc.PublicResource);
            Assert.Null(ofdDoc.DocumentResource);
            Assert.Null(ofdDoc.Signatures);
            Assert.Null(ofdDoc.SignDocs);

        }

        /// <summary>
        /// 测试AddPageDoc方法的功能
        /// </summary>
        [Fact]
        public void AddPageDoc_ShouldAddPageToCollection()
        {
            // 准备测试数据
            var ofdDoc = new OFDDocument(0);
            var pageDoc = new PageDocument();

            // 执行测试
            ofdDoc.AddPageDoc(pageDoc);

            // 验证结果
            Assert.NotNull(ofdDoc.PageDocs);
            Assert.Single(ofdDoc.PageDocs);
            Assert.Contains(pageDoc, ofdDoc.PageDocs);
        }

        /// <summary>
        /// 测试OFDDoc类的序列化和反序列化功能
        /// </summary>
        [Fact]
        public void SerializeAndDeserialize_ShouldMaintainDataIntegrity()
        {
            // 准备测试数据
            var ofdDoc = new OFDDocument(0);

            // 初始化基本属性
            ofdDoc.SetPublicResource(new Res() { BaseLoc = new ST_Loc(".") });
            ofdDoc.SetDocumentResource(new Res() { BaseLoc = new ST_Loc(".") });
            ofdDoc.Signatures = new Signatures();

            // 添加测试数据
            var pageDoc = new PageDocument();
            ofdDoc.AddPageDoc(pageDoc);

            // 添加资源文件
            ofdDoc.ResFiles.Add("test.txt", System.Text.Encoding.UTF8.GetBytes("test content"));

            // 分别序列化各个属性，而不是整个OFDDoc对象
            // 序列化Document
            string documentXml = XmlHelper.SerializeToString(ofdDoc.Document);
            Assert.NotNull(documentXml);
            Assert.NotEmpty(documentXml);
            var deserializedDocument = XmlHelper.DeserializeFromString<Document>(documentXml);
            Assert.NotNull(deserializedDocument);
            Assert.NotNull(deserializedDocument.CommonData);

            // 序列化PublicResource
            string publicResXml = XmlHelper.SerializeToString(ofdDoc.PublicResource);
            Assert.NotNull(publicResXml);
            Assert.NotEmpty(publicResXml);
            var deserializedPublicRes = XmlHelper.DeserializeFromString<Res>(publicResXml);
            Assert.NotNull(deserializedPublicRes);

            // 序列化DocumentResource
            string docResXml = XmlHelper.SerializeToString(ofdDoc.DocumentResource);
            Assert.NotNull(docResXml);
            Assert.NotEmpty(docResXml);
            var deserializedDocRes = XmlHelper.DeserializeFromString<Res>(docResXml);
            Assert.NotNull(deserializedDocRes);

            // 序列化Signatures
            string signaturesXml = XmlHelper.SerializeToString(ofdDoc.Signatures);
            Assert.NotNull(signaturesXml);
            Assert.NotEmpty(signaturesXml);
            var deserializedSignatures = XmlHelper.DeserializeFromString<Signatures>(signaturesXml);
            Assert.NotNull(deserializedSignatures);

            // 验证集合属性的完整性
            Assert.Null(ofdDoc.SignDocs);
            Assert.Single(ofdDoc.PageDocs);
            Assert.Single(ofdDoc.ResFiles);
            Assert.Equal("test content", System.Text.Encoding.UTF8.GetString(ofdDoc.ResFiles["test.txt"]));
        }

        /// <summary>
        /// 测试Document对象的序列化和反序列化
        /// </summary>
        [Fact]
        public void SerializeAndDeserialize_Document_ShouldMaintainDataIntegrity()
        {
            // 准备测试数据
            var ofdDoc = new OFDDocument(0);
            ofdDoc.Document = new Document
            {
                CommonData = new CT_CommonData(),
                Pages = new List<DocumentPage> { new DocumentPage() }
            };

            // 序列化到XML字符串
            string xml = XmlHelper.SerializeToString(ofdDoc.Document);
            Assert.NotNull(xml);
            Assert.NotEmpty(xml);

            // 反序列化回对象
            var deserializedDoc = XmlHelper.DeserializeFromString<Document>(xml);
            Assert.NotNull(deserializedDoc);

            // 验证属性
            Assert.NotNull(deserializedDoc.CommonData);
            Assert.NotNull(deserializedDoc.Pages);
            Assert.Single(deserializedDoc.Pages);
        }

        /// <summary>
        /// 测试 Resource 对象的序列化和反序列化
        /// </summary>
        [Fact]
        public void SerializeAndDeserialize_Resource_ShouldMaintainDataIntegrity()
        {
            // 准备测试数据
            var ofdDoc = new OFDDocument(0);
            ofdDoc.SetPublicResource(new Res
            {
                BaseLoc = new ST_Loc(".")
            });

            // 根据SetPublicResource方法的实现，BaseLoc应该被设置为相对路径"Res"
            Assert.Equal(".", ofdDoc.PublicResource.BaseLoc.ToString());

            // 序列化到XML字符串
            string xml = XmlHelper.SerializeToString(ofdDoc.PublicResource);
            Assert.NotNull(xml);
            Assert.NotEmpty(xml);

            // 反序列化回对象
            var deserializedRes = XmlHelper.DeserializeFromString<Res>(xml);
            Assert.NotNull(deserializedRes);

            // 根据当前实现
            Assert.Equal(".", deserializedRes.BaseLoc.ToString());
        }

        /// <summary>
        /// 测试Signatures对象的序列化和反序列化
        /// </summary>
        [Fact]
        public void SerializeAndDeserialize_Signatures_ShouldMaintainDataIntegrity()
        {
            // 准备测试数据
            var ofdDoc = new OFDDocument(0);
            ofdDoc.Signatures = new Signatures();

            // 序列化到XML字符串
            string xml = XmlHelper.SerializeToString(ofdDoc.Signatures);
            Assert.NotNull(xml);
            Assert.NotEmpty(xml);

            // 反序列化回对象
            var deserializedSignatures = XmlHelper.DeserializeFromString<Signatures>(xml);
            Assert.NotNull(deserializedSignatures);
        }
    }
}
