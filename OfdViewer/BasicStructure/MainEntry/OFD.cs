using OFDViewer.Enums;
using OFDViewer.Utils;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace OFDViewer.BasicStructure.MainEntry
{
    /// <summary>
    /// OFD 根节点模型（符合 GB/T 33190-2016 规范）
    /// </summary>
    [XmlRoot("OFD", Namespace = Constants.OFD_NAMESPACE_URI)]
    public class OFD
    {
        #region 常量定义（限定合法值，符合 OFD 标准）
        /// <summary>
        /// 合法的 OFD 版本号集合
        /// </summary>
        private static readonly HashSet<string> ValidVersions = new HashSet<string>
        {
            "1.0",  // 基础版本
            "1.1"   // 扩展版本（后续可继续添加）
        };
        /// <summary>
        /// 默认 OFD 版本号
        /// </summary>
        public const string DefaultVersion = "1.0";
        #endregion

        #region 命名空间声明（简化写法，避免硬编码）
        /// <summary>
        /// XML 命名空间声明（确保序列化时使用 ofd 前缀）
        /// </summary>
        //[XmlNamespaceDeclarations]
        //public XmlSerializerNamespaces XmlNamespaces
        //{
        //    get
        //    {
        //        var ns = new XmlSerializerNamespaces();
        //        ns.Add("ofd", Constants.OFD_NAMESPACE_URI);
        //        return ns;
        //    }
        //    set { /* 反序列化时不需要设置 */ }
        //}

        #endregion

        #region 必选属性
        /// <summary>
        /// 文件格式的版本号
        /// <para>必选，取值固定为 "1.0"（符合 GB/T 33190-2016）</para>
        /// </summary>
        [XmlAttribute("Version")]
        public string Version
        {
            get => _version;
            set
            {
                // 校验版本号合法性
                if (string.IsNullOrWhiteSpace(value) || !ValidVersions.Contains(value))
                {
                    throw new ArgumentException($"Version 必须为有效的版本号 ({string.Join(", ", ValidVersions)})，当前值：{value ?? "null"}", nameof(value));
                }
                _version = value;
            }
        }
        private string _version = DefaultVersion; // 默认值，符合标准

        /// <summary>
        /// 文件格式子集类型
        /// <para>必选，取值范围：OFD（标准）、OFDA（存档规范）、OFDH（红头文件）</para>
        /// </summary>
        [XmlIgnore]
        public DocumentType DocType { get; set; }

        /// <summary>
        /// 文件格式子集类型
        /// <para>必选，取值范围："OFD"（标准）、"OFD-A"（存档规范）</para>
        /// </summary>
        [XmlAttribute("DocType")]
        public string DocTypeString
        {
            get => EnumHelper.GetEnumDesc(DocType);
            set
            {
                if (!EnumHelper.TryParseEnum<DocumentType>(value, out var docType))
                {
                    throw new ArgumentException($"DocType 必须为 \"OFD\" 或 \"OFD-A\"，当前值：{value}", nameof(value));
                }
                DocType = docType;
            }
        }

        /// <summary>
        /// 文件对象入口（必选，支持多个版式文档）
        /// <para>必选，至少包含一个 DocBody 节点</para>
        /// </summary>
        [XmlElement("DocBody", Namespace = Constants.OFD_NAMESPACE_URI)]
        public List<DocBody> DocBodies { get; set; } = new List<DocBody>(); // 初始化空列表，避免空引用

        #endregion


        #region 构造函数（保证默认值符合标准）
        /// <summary>
        /// 无参构造函数（XmlSerializer 必需）
        /// </summary>
        public OFD()
        {
            // 默认值符合标准，避免反序列化后空值
            Version = DefaultVersion;
            DocTypeString = "OFD";
            DocBodies = new List<DocBody>();
        }

        /// <summary>
        /// 带参构造函数（推荐使用，强制必选参数）
        /// </summary>
        /// <param name="docType">文档类型（OFD/OFD-A）</param>
        /// <param name="docBodies">文件对象入口列表</param>
        /// <exception cref="ArgumentNullException">文档入口列表为空时抛出</exception>
        public OFD(string docType, List<DocBody> docBodies) : this()
        {
            DocTypeString = docType ?? throw new ArgumentNullException(nameof(docType), "DocType 不能为空");
            DocBodies = docBodies ?? throw new ArgumentNullException(nameof(docBodies), "DocBody 列表不能为空");

            // 校验 DocBody 列表非空
            if (docBodies.Count == 0)
            {
                throw new ArgumentException("DocBody 列表至少包含一个元素", nameof(docBodies));
            }
        }
        #endregion


        #region 便捷方法（提升易用性）
        /// <summary>
        /// 添加文件对象入口
        /// </summary>
        /// <param name="docBody">文件对象入口</param>
        /// <exception cref="ArgumentNullException">docBody 为空时抛出</exception>
        public void AddDocBody(DocBody docBody)
        {
            if (docBody == null)
            {
                throw new ArgumentNullException(nameof(docBody), "添加的 DocBody 不能为空");
            }
            DocBodies.Add(docBody);
        }

        #endregion
    }

}
