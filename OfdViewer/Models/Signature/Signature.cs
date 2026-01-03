using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using OFDViewer.Models.BaseType;
using OFDViewer.Utils;

namespace OFDViewer.Models.Signature
{
    /// <summary>
    /// 签名描述文件
    /// OFD的数字签名通过对签名描述文件的保护间接实现对 OFD 原文的保护。签名结构中的签名信
    /// 息(SignedInfo)是这一过程中的关键节点,其中记录了当次数字签名保护的所有文件的二进制摘要信
    /// 息, 同时将安全算法提供者、签名算法、签名时间和所应用的安全印章等信息也包含在此节点内。签名
    /// 描述文件同时包含了签名值将要存放的包内位置, 一旦对该文件实施签名保护, 则其对应的包内文件原
    /// 文以及本次签名对应的附加信息都将不可改动,从而实现一次数字签名对整个原文内容的保护。签名
    /// 描述文件的主要结构描述见图86。
    /// </summary>
    public class Signature
    {
        /// <summary>
        /// 签名要保护的原文及本次签名相关的信息 
        /// 必选
        /// </summary>
        [XmlElement("SignedInfo")]
        [XmlRequired(errorMsg: "SignedInfo为必选项，且不能为空")]
        public SignedInfo SignedInfo { get; set; }

        /// <summary>
        /// 指向安全签名提供者所返回的针对签名描述文件计算所得的签名值文件
        /// 必选
        /// </summary>
        [XmlElement("SignedValue")]
        [XmlRequired(errorMsg: "SignedValue为必选项，且不能为空")]
        public string SignedValueString
        {
            get => SignedValue.ToString();
            set => SignedValue = value;
        }
        [XmlIgnore]
        public ST_Loc SignedValue { get; set; }
    }
}
