using System.Xml.Serialization;
using OFDViewer.Utils;

namespace OFDViewer.Models.Signature
{
    /// <summary>
    /// 签名要保护的原文及本次签名相关的信息
    /// SignedInfo 记录了当次数字签名保护的所有文件的二进制摘要信息,
    /// 同时将安全算法提供者、签名算法、签名时间和所应用的安全印章等信息也包含在此节点内
    /// </summary>
    public class SignedInfo
    {
        /// <summary>
        /// 创建签名时所用的签章组件提供者信息 
        /// 必选
        /// </summary>
        [XmlElement("Provider")]
        [XmlRequired(ErrorMsg = "Provider为必选项，且不能为空")]
        public Provider Provider { get; set; }

        /// <summary>
        /// 签名方法,记录安全模块返回的签名算法代码,以便验证时使用 
        /// 必选
        /// </summary>
        [XmlElement("SignatureMethod", IsNullable = false)]
        [XmlRequired(ErrorMsg = "SignatureMethod为必选项，且不能为空")]
        public string SignatureMethod { get; set; }

        /// <summary>
        /// 签名时间,记录安全模块返回的签名时间,以便验证时使用 
        /// 必选
        /// </summary>
        [XmlElement("SignatureDateTime", IsNullable = false)]
        [XmlRequired(ErrorMsg = "SignatureDateTime为必选项，且不能为空")]
        public string SignatureDateTime { get; set; }

        /// <summary>
        /// 包内文件计算所得的摘要记录列表
        /// 一个受本次签名保护的包内文件对应一个 Reference 节点
        /// 必选
        /// </summary>
        [XmlElement("References")]
        [XmlRequired(ErrorMsg = "References为必选项，且不能为空")]
        public References References { get; set; }

        /// <summary>
        /// 本签名关联的外观(用OFD中的注释来表示),该节点可出现多次
        /// 可选
        /// </summary>
        [XmlElement("StampAnnot", IsNullable = false)]
        public List<StampAnnot> StampAnnots { get; set; }

        /// <summary>
        /// 电子印章信息 
        /// 可选
        /// </summary>
        [XmlElement("Seal", IsNullable = false)]
        public Seal Seal { get; set; }
    }
}
