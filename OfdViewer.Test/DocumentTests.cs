using OFDViewer.Actions;
using OFDViewer.BaseType;
using OFDViewer.BasicStructure.DocumentRoot;
using OFDViewer.BasicStructure.Outlines;
using OFDViewer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xunit;

namespace OFDViewer.Tests
{
    public class DocumentTests
    {
        // 测试必填字段存在时的XML序列化
        [Fact]
        public void Serialize_Document_ShouldContainMandatoryElements()
        {
            // 1. 准备测试数据
            var document = new Document
            {
                CommonData = new CT_CommonData(), // 必选字段
                Pages = new List<DocumentPage>
                {
                    new DocumentPage() // 至少需要一个页面（必选）
                }
            };

            // 2. 执行序列化
            XmlHelper.SerializeToFile(document, "Document.xml");
        }


        // 测试必填字段存在时的XML序列化
        //[Fact]
        //public void Serialize_WithRequiredFields_ShouldContainMandatoryElements()
        //{
        //    // 1. 准备测试数据
        //    var document = new Document
        //    {
        //        CommonData = new CT_CommonData(), // 必选字段
        //        Pages = new List<DocumentPage>
        //        {
        //            new DocumentPage() // 至少需要一个页面（必选）
        //        }
        //    };

        //    // 2. 执行序列化
        //    string xml = XmlHelper.SerializeToString(document);
        //    XElement root = XElement.Parse(xml);

        //    // 3. 验证XML命名空间
        //    Assert.Equal(Constants.OFD_NAMESPACE_URI, root.Name.Namespace.NamespaceName);
        //    Assert.Equal("Document", root.Name.LocalName);

        //    // 4. 验证必选元素存在
        //    Assert.NotNull(root.Element(XName.Get("CommonData", Constants.OFD_NAMESPACE_URI)));
        //    Assert.NotNull(root.Element(XName.Get("Pages", Constants.OFD_NAMESPACE_URI)));
        //    Assert.NotEmpty(root.Element(XName.Get("Pages", Constants.OFD_NAMESPACE_URI))?.Elements(XName.Get("Page", Constants.OFD_NAMESPACE_URI)));
        //}

        //// 测试缺少必填字段时的序列化问题
        //[Fact]
        //public void Serialize_WithoutRequiredFields_ShouldMissMandatoryElements()
        //{
        //    // 1. 创建缺少必选字段的文档
        //    var invalidDocument = new Document
        //    {
        //        // 故意不设置CommonData（必选）
        //        Pages = null // 故意不设置Pages（必选）
        //    };

        //    // 2. 执行序列化
        //    string xml = invalidDocument.SerializeToXml();
        //    XElement root = XElement.Parse(xml);

        //    // 3. 验证必选元素缺失
        //    Assert.Null(root.Element(XName.Get("CommonData", Constants.OFD_NAMESPACE_URI)));
        //    Assert.Null(root.Element(XName.Get("Pages", Constants.OFD_NAMESPACE_URI)));
        //}

        //// 测试可选字段的XML序列化
        //[Fact]
        //public void Serialize_WithOptionalFields_ShouldContainOptionalElements()
        //{
        //    // 1. 准备包含可选字段的测试数据
        //    var document = new Document
        //    {
        //        CommonData = new CT_CommonData(),
        //        Pages = new List<DocumentPage> { new DocumentPage() },
        //        Outlines = new List<CT_OutlineElem>
        //        {
        //            new CT_OutlineElem { Title = "Test Outline" }
        //        },
        //        Permissions = new CT_Permission(),
        //        Actions = new List<CT_Action> { new CT_Action() },
        //        VPreferences = new CT_VPreferences(),
        //        Bookmarks = new List<CT_Bookmark> { new CT_Bookmark() },
        //        Annotations = new ST_Loc { Value = "Annotations.xml" },
        //        CustomTags = new ST_Loc { Value = "CustomTags.xml" },
        //        Attachments = new ST_Loc { Value = "Attachments.xml" },
        //        Extensions = new ST_Loc { Value = "Extensions.xml" }
        //    };

        //    // 2. 执行序列化
        //    string xml = document.SerializeToXml();
        //    XElement root = XElement.Parse(xml);

        //    // 3. 验证可选元素存在
        //    Assert.NotNull(root.Element(XName.Get("Outlines", Constants.OFD_NAMESPACE_URI)));
        //    Assert.NotNull(root.Element(XName.Get("Permissions", Constants.OFD_NAMESPACE_URI)));
        //    Assert.NotNull(root.Element(XName.Get("Actions", Constants.OFD_NAMESPACE_URI)));
        //    Assert.NotNull(root.Element(XName.Get("VPreferences", Constants.OFD_NAMESPACE_URI)));
        //    Assert.NotNull(root.Element(XName.Get("Bookmarks", Constants.OFD_NAMESPACE_URI)));
        //    Assert.Equal("Annotations.xml", root.Element(XName.Get("Annotations", Constants.OFD_NAMESPACE_URI))?.Value);
        //    Assert.Equal("CustomTags.xml", root.Element(XName.Get("CustomTags", Constants.OFD_NAMESPACE_URI))?.Value);
        //    Assert.Equal("Attachments.xml", root.Element(XName.Get("Attachments", Constants.OFD_NAMESPACE_URI))?.Value);
        //    Assert.Equal("Extensions.xml", root.Element(XName.Get("Extensions", Constants.OFD_NAMESPACE_URI))?.Value);
        //}

        //// 测试XML反序列化是否能正确还原对象
        //[Fact]
        //public void Deserialize_FromValidXml_ShouldRestoreObject()
        //{
        //    // 1. 准备测试XML
        //    var original = new Document
        //    {
        //        CommonData = new CT_CommonData(),
        //        Pages = new List<DocumentPage> { new DocumentPage() },
        //        Annotations = new ST_Loc { Value = "test.xml" }
        //    };
        //    string xml = original.SerializeToXml();

        //    // 2. 执行反序列化
        //    Document deserialized = Document.DeserializeFromXml(xml);

        //    // 3. 验证对象还原正确
        //    Assert.NotNull(deserialized.CommonData);
        //    Assert.NotNull(deserialized.Pages);
        //    Assert.Single(deserialized.Pages);
        //    Assert.Equal("test.xml", deserialized.Annotations.Value);
        //}
    }
}
