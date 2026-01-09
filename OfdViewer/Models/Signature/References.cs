using System.Xml.Serialization;
using OFDViewer.Utils;

namespace OFDViewer.Models.Signature
{
    /// <summary>
    /// 签名的范围
    /// 包内文件计算所得的摘要记录列表
    /// 一个受本次签名保护的包内文件对应一个 Reference 节点
    /// </summary>
    public class References
    {
        /// <summary>
        /// Reference元素集合（可重复，maxOccurs="unbounded"）
        /// </summary>
        [XmlElement("Reference")]
        [XmlRequired(MinItemCount = 1)]
        public List<Reference> Refers { get; set; }

        /// <summary>
        /// 校验方法枚举（忽略XML序列化，带默认值MD5）
        /// </summary>
        [XmlIgnore]
        public CheckMethodEnum CheckMethod { get; set; } = CheckMethodEnum.MD5;

        /// <summary>
        /// 摘要方法,视应用场景的不同使用不同的摘要方法。
        /// 用于各行业应用时,应使用符合该行业安全标准的算法
        /// 可选
        /// </summary>
        [XmlAttribute("CheckMethod")]
        public string CheckMethodString
        {
            get => CheckMethod.ToString();
            set => CheckMethod = EnumHelper.ParseEnum<CheckMethodEnum>(value);
        }
    }
}
