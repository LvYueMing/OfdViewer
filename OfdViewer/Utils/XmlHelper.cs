using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using System.Reflection;

namespace OFDViewer.Utils
{
    /// <summary>
    /// XML序列化辅助类
    /// </summary>
    public static class XmlHelper
    {
        private static readonly XmlWriterSettings DefaultSettings = new XmlWriterSettings
        {
            Indent = true, // 控制缩进格式
            OmitXmlDeclaration = false,// 省略 XML 声明（<?xml version="1.0" encoding="utf-8"?>）
            Encoding = Encoding.UTF8, // 设置编码格式
            NamespaceHandling = NamespaceHandling.OmitDuplicates // 禁止生成 xsi 和 xsd 命名空间声明
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
            // 校验参数（避免空引用）
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("文件路径不能为空", nameof(filePath));

            // 创建目录（避免路径不存在）
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            SerializeToStream(obj, fs);
        }

        /// <summary>
        /// 序列化到流
        /// </summary>
        public static void SerializeToStream<T>(T obj, Stream stream)
        {
            // 校验参数
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            var serializer = new XmlSerializer(typeof(T));
            using var writer = XmlWriter.Create(stream, DefaultSettings);

            // 创建 XmlSerializerNamespaces 并添加命名空间前缀
            var ns = new XmlSerializerNamespaces();

            // 检查类型是否有 XmlRootAttribute
            var xmlRootAttr = typeof(T).GetCustomAttribute<XmlRootAttribute>();
            if (xmlRootAttr != null && !string.IsNullOrEmpty(xmlRootAttr.Namespace))
            {
                // 使用 XmlRootAttribute 中定义的命名空间和指定前缀
                ns.Add(Constants.OFD_VALUE, xmlRootAttr.Namespace);
            }
            else
            {
                // 如果没有 XmlRootAttribute，使用默认命名空间
                ns.Add(Constants.OFD_VALUE, Constants.OFD_NAMESPACE_URI);
            }

            serializer.Serialize(writer, obj, ns);
        }

        /// <summary>
        /// 序列化到字符串
        /// </summary>
        public static string SerializeToString<T>(T obj)
        {
            // 校验参数
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            using var sw = new StringWriter();
            var serializer = new XmlSerializer(typeof(T));
            using var writer = XmlWriter.Create(sw, DefaultSettings);

            // 创建 XmlSerializerNamespaces 并添加命名空间前缀
            var ns = new XmlSerializerNamespaces();

            // 检查类型是否有 XmlRootAttribute
            var xmlRootAttr = typeof(T).GetCustomAttribute<XmlRootAttribute>();
            if (xmlRootAttr != null && !string.IsNullOrEmpty(xmlRootAttr.Namespace))
            {
                // 使用 XmlRootAttribute 中定义的命名空间和指定前缀
                ns.Add(Constants.OFD_VALUE, xmlRootAttr.Namespace);
            }
            else
            {
                // 如果没有 XmlRootAttribute，使用默认命名空间
                ns.Add(Constants.OFD_VALUE, Constants.OFD_NAMESPACE_URI);
            }
            serializer.Serialize(writer, obj);
            return sw.ToString();
        }
    }
}
