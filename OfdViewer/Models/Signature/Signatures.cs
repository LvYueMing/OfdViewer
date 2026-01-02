using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OFDViewer.Models.Signature
{
    /// <summary>
    /// 签名列表根结点
    /// 签名列表文件的入口点在7.4主入口中定义。签名列表文件中可以包含多个签名(例如联合发文等情况),见图85。
    /// 当允许下次继续添加签名时,该文件不会被包含到本次签名的保护文件列表(References)中。
    /// </summary>
    public class Signatures
    {
        /// <summary>
        /// 安全标识的最大值,作用与文档入口文件 Document.xml中的 MaxID
        /// 相同,为了避免在签名时影响文档入口文件,采用了与 ST_ID 不一样
        /// 的ID编码方式。推荐使用“sNNN”的编码方式,NNN 从1开始
        /// 可选
        /// </summary>
        [XmlElement(ElementName = "MaxSignId")] //仅[XmlAttribute("MaxSignId")] 标记，即可序列化为xs:IDREF类型属性
        public string MaxSignId { get; set; }

        /// <summary>
        /// 数字签名或安全签章在列表中的注册信息,一次签名或签章对应一个节点
        /// 可选
        /// </summary>
        [XmlElement(ElementName = "Signature")]
        public List<SignatureRegInfo> SignatureList { get; set; } = Array.Empty<SignatureRegInfo>().ToList();
    }
}
