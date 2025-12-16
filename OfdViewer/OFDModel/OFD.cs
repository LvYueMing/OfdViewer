using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace OfdViewer.OFDModel
{
    /// <summary>
    /// OFD 根节点模型（符合 GB/T 33190-2016 规范）
    /// </summary>
    [XmlRoot("OFD", Namespace = Constants.OFD_NAMESPACE_URI)]
    public class OFD 
    {
        #region 常量定义（限定合法值，符合 OFD 标准）
        /// <summary>
        /// 合法的 OFD 版本号（仅支持 1.0，符合标准）
        /// </summary>
        public const string ValidVersion = "1.0";

        /// <summary>
        /// 合法的文档类型集合
        /// </summary>
        private static readonly HashSet<string> ValidDocTypes = new HashSet<string>
        {
            "OFD",   // 符合本标准
            "OFD-A"  // 符合 OFD 存档规范
        };
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
                if (!string.Equals(value, ValidVersion, StringComparison.Ordinal))
                {
                    throw new ArgumentException($"Version 必须为 \"{ValidVersion}\"，当前值：{value}", nameof(value));
                }
                _version = value;
            }
        }
        private string _version = ValidVersion; // 默认值，符合标准

        /// <summary>
        /// 文件格式子集类型
        /// <para>必选，取值范围："OFD"（标准）、"OFD-A"（存档规范）</para>
        /// </summary>
        [XmlAttribute("DocType")]
        public string DocType
        {
            get => _docType;
            set
            {
                // 校验文档类型合法性
                if (string.IsNullOrWhiteSpace(value) || !ValidDocTypes.Contains(value))
                {
                    throw new ArgumentException($"DocType 必须为 \"OFD\" 或 \"OFD-A\"，当前值：{value}", nameof(value));
                }
                _docType = value;
            }
        }
        private string _docType = "OFD"; // 默认值，符合标准

        /// <summary>
        /// 文件对象入口（必选，支持多个版式文档）
        /// <para>必选，至少包含一个 DocBody 节点</para>
        /// </summary>
        [XmlElement("DocBody", Namespace = Constants.OFD_NAMESPACE_URI)]
        public List<DocBody> DocBodies { get; set; } = new List<DocBody>(); // 初始化空列表，避免空引用

        #endregion



        #region 命名空间声明（简化写法，避免硬编码）
        /// <summary>
        /// XML 命名空间声明（自动绑定 ofd 前缀）
        /// </summary>
        [XmlNamespaceDeclarations]
        public XmlSerializerNamespaces XmlNamespaces { get; }

        #endregion


        #region 构造函数（保证默认值符合标准）
        /// <summary>
        /// 无参构造函数（XmlSerializer 必需）
        /// </summary>
        public OFD()
        {
            // 初始化命名空间，符合 OFD 标准命名空间声明
            XmlNamespaces = new XmlSerializerNamespaces();
            XmlNamespaces.Add(Constants.OFD_VALUE, Constants.OFD_NAMESPACE_URI);

            // 默认值符合标准，避免反序列化后空值
            Version = ValidVersion;
            DocType = "OFD";
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
            DocType = docType ?? throw new ArgumentNullException(nameof(docType), "DocType 不能为空");
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

        /// <summary>
        /// 检查是否为存档类型 OFD（OFD-A）
        /// </summary>
        /// <returns>true=存档类型，false=标准类型</returns>
        public bool IsArchiveType() => string.Equals(DocType, "OFD-A", StringComparison.Ordinal);
        #endregion
    }

}
