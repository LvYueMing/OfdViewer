using OfdViewer.OFDModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace OfdViewer.OfdReader
{
    public class OfdReader
    {
        private readonly OfdArchive _archive;

        public OfdReader(string filePath)
        {
            _archive = OfdArchive.Open(filePath);
        }

        /// <summary>
        /// 解析 OFD 主入口文件
        /// </summary>
        public OFD ParseOFD()
        {
            // 1. 读取 OFD.xml 入口文件
            var ofdXml = _archive.ReadXmlFile("OFD.xml");

            // 2. 解析文档根目录
            var docNode = ofdXml.SelectSingleNode("/OFD/Document");
            if (docNode == null)
                throw new InvalidOfdFormatException("无效的 OFD 文件格式");

            var docPath = docNode.Attributes?["BaseLoc"]?.Value ?? "Doc_0";
            if (!docPath.EndsWith("/"))
                docPath += "/";

            // 3. 读取文档结构文件
            var docXml = _archive.ReadXmlFile($"{docPath}Document.xml");

            // 4. 解析公共资源
            var publicResPath = $"{docPath}PublicRes.xml";
            var publicRes = _archive.FileExists(publicResPath)
                ? _archive.ReadXmlFile(publicResPath)
                : null;

            // 5. 构建文档信息
            return new OfdDocumentInfo
            {
                RootPath = docPath,
                DocumentXml = docXml,
                PublicResXml = publicRes,
                OfdVersion = ofdXml.DocumentElement?.GetAttribute("Version") ?? "1.0",
                DocType = ofdXml.DocumentElement?.GetAttribute("DocType") ?? "Document"
            };
        }

        /// <summary>
        /// 解析页面列表
        /// </summary>
        public List<PageInfo> ParsePages(OfdDocumentInfo docInfo)
        {
            var pages = new List<PageInfo>();
            var pageNodes = docInfo.DocumentXml.SelectNodes("/Document/Pages/Page");

            if (pageNodes == null || pageNodes.Count == 0)
                return pages;

            for (int i = 0; i < pageNodes.Count; i++)
            {
                var pageNode = pageNodes[i];
                var pageId = pageNode.Attributes?["ID"]?.Value;
                var baseLoc = pageNode.Attributes?["BaseLoc"]?.Value ?? $"Pages/Page_{i}.xml";

                // 构建完整路径
                var fullPath = Path.Combine(docInfo.RootPath, baseLoc)
                    .Replace('\\', '/');

                pages.Add(new PageInfo
                {
                    Id = pageId ?? $"Page_{i}",
                    Index = i,
                    FilePath = fullPath,
                    Size = ParsePageSize(fullPath)
                });
            }

            return pages;
        }

        /// <summary>
        /// 解析单页内容
        /// </summary>
        public PageContent ParsePage(string pageFilePath)
        {
            if (!_archive.FileExists(pageFilePath))
                throw new FileNotFoundException($"页面文件不存在: {pageFilePath}");

            var pageXml = _archive.ReadXmlFile(pageFilePath);
            var content = new PageContent
            {
                FilePath = pageFilePath,
                Layers = ParseLayers(pageXml),
                Resources = ParsePageResources(pageXml, pageFilePath)
            };

            return content;
        }

        /// <summary>
        /// 解析页面图层
        /// </summary>
        private List<PageLayer> ParseLayers(XmlDocument pageXml)
        {
            var layers = new List<PageLayer>();
            var layerNodes = pageXml.SelectNodes("/Page/Content/Layer");

            if (layerNodes == null)
                return layers;

            for (int i = 0; i < layerNodes.Count; i++)
            {
                var layerNode = layerNodes[i];
                var layer = new PageLayer
                {
                    Id = layerNode.Attributes?["ID"]?.Value ?? $"Layer_{i}",
                    Type = layerNode.Attributes?["Type"]?.Value ?? "Body",
                    Objects = ParseLayerObjects(layerNode)
                };

                layers.Add(layer);
            }

            return layers;
        }

        /// <summary>
        /// 解析图层中的图元对象
        /// </summary>
        private List<PageObject> ParseLayerObjects(XmlNode layerNode)
        {
            var objects = new List<PageObject>();

            // 解析路径对象
            var pathNodes = layerNode.SelectNodes("Path");
            foreach (XmlNode pathNode in pathNodes)
            {
                objects.Add(ParsePathObject(pathNode));
            }

            // 解析文本对象
            var textNodes = layerNode.SelectNodes("Text");
            foreach (XmlNode textNode in textNodes)
            {
                objects.Add(ParseTextObject(textNode));
            }

            // 解析图像对象
            var imageNodes = layerNode.SelectNodes("Image");
            foreach (XmlNode imageNode in imageNodes)
            {
                objects.Add(ParseImageObject(imageNode));
            }

            return objects;
        }
    }
}
