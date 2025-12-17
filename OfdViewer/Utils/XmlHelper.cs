using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;

namespace OFDViewer.Utils
{
    /// <summary>
    /// XML序列化辅助类
    /// </summary>
    public static class XmlHelper
    {
        private static readonly XmlSerializerNamespaces EmptyNamespaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
        private static readonly XmlWriterSettings DefaultSettings = new XmlWriterSettings
        {
            Indent = true,
            OmitXmlDeclaration = true,
            Encoding = Encoding.UTF8
        };

        /// <summary>
        /// 从文件反序列化
        /// </summary>
        public static T DeserializeFromFile<T>(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("XML 文件不存在", filePath);

            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return DeserializeFromStream<T>(fs);
        }

        /// <summary>
        /// 从字符串反序列化
        /// </summary>
        public static T DeserializeFromString<T>(string xmlStr)
        {
            if (string.IsNullOrWhiteSpace(xmlStr))
                throw new ArgumentNullException(nameof(xmlStr), "XML 字符串不能为空");

            using var sr = new StringReader(xmlStr);
            var serializer = new XmlSerializer(typeof(T));
            return (T)serializer.Deserialize(sr);
        }

        /// <summary>
        /// 从流反序列化
        /// </summary>
        public static T DeserializeFromStream<T>(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream), "输入流不能为空");

            var serializer = new XmlSerializer(typeof(T));
            return (T)serializer.Deserialize(stream);
        }

        /// <summary>
        /// 序列化到文件
        /// </summary>
        public static void SerializeToFile<T>(T obj, string filePath)
        {
            using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            SerializeToStream(obj, fs);
        }

        /// <summary>
        /// 序列化到字符串
        /// </summary>
        public static string SerializeToString<T>(T obj)
        {
            using var sw = new StringWriter();
            var serializer = new XmlSerializer(typeof(T));
            using var writer = XmlWriter.Create(sw, DefaultSettings);
            serializer.Serialize(writer, obj, EmptyNamespaces);
            return sw.ToString();
        }

        /// <summary>
        /// 序列化到流
        /// </summary>
        public static void SerializeToStream<T>(T obj, Stream stream)
        {
            var serializer = new XmlSerializer(typeof(T));
            using var writer = XmlWriter.Create(stream, DefaultSettings);
            serializer.Serialize(writer, obj, EmptyNamespaces);
        }
    }
}
