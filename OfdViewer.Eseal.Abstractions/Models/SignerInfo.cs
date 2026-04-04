using System;

namespace OfdViewer.ESeal.Abstractions.Models
{
    /// <summary>
    /// 签章人信息
    /// 符合 GM/T 0031-2014 安全电子签章密码技术规范
    /// </summary>
    public class SignerInfo
    {
        /// <summary>
        /// 签章人名称
        /// GM/T 0031-2014: signerName - 签章人名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 签章人证件类型
        /// GM/T 0031-2014: signerIDType - 签章人证件类型
        /// 例如：身份证、护照、军官证等
        /// </summary>
        public string IdType { get; set; } = string.Empty;

        /// <summary>
        /// 签章人证件号码
        /// GM/T 0031-2014: signerID - 签章人证件号码
        /// </summary>
        public string IdNumber { get; set; } = string.Empty;

        /// <summary>
        /// 签章人单位名称
        /// GM/T 0031-2014: signerOrg - 签章人所在单位
        /// </summary>
        public string Organization { get; set; } = string.Empty;

        /// <summary>
        /// 签章人单位统一社会信用代码
        /// </summary>
        public string OrganizationCode { get; set; } = string.Empty;

        /// <summary>
        /// 签章人证书序列号
        /// GM/T 0031-2014: certSerialNumber - 证书序列号
        /// </summary>
        public string CertSerialNumber { get; set; } = string.Empty;

        /// <summary>
        /// 签章人证书颁发者
        /// GM/T 0031-2014: certIssuer - 证书颁发者
        /// </summary>
        public string CertIssuer { get; set; } = string.Empty;

        /// <summary>
        /// 签章人证书有效期开始
        /// GM/T 0031-2014: certValidStart - 证书有效期起始
        /// </summary>
        public DateTime? CertValidFrom { get; set; }

        /// <summary>
        /// 签章人证书有效期结束
        /// GM/T 0031-2014: certValidEnd - 证书有效期结束
        /// </summary>
        public DateTime? CertValidTo { get; set; }

        /// <summary>
        /// 签章人证书主题（DN）
        /// GM/T 0031-2014: certSubject - 证书主题
        /// </summary>
        public string CertSubject { get; set; } = string.Empty;

        /// <summary>
        /// 签章人证书指纹（Thumbprint）
        /// </summary>
        public string CertThumbprint { get; set; } = string.Empty;

        /// <summary>
        /// 签章人证书公钥算法
        /// GM/T 0031-2014: 公钥算法标识（如 SM2、RSA 等）
        /// </summary>
        public string CertPublicKeyAlgorithm { get; set; } = string.Empty;

        /// <summary>
        /// 签章人证书签名算法
        /// GM/T 0031-2014: signatureAlgorithm - 签名算法标识
        /// </summary>
        public string CertSignatureAlgorithm { get; set; } = string.Empty;

        /// <summary>
        /// 签章人证书密钥用途
        /// </summary>
        public string CertKeyUsage { get; set; } = string.Empty;

        /// <summary>
        /// 签章人证书扩展密钥用途
        /// </summary>
        public string CertExtendedKeyUsage { get; set; } = string.Empty;

        /// <summary>
        /// 签章人证书是否有效
        /// </summary>
        public bool IsCertValid()
        {
            var now = DateTime.Now;
            return (!CertValidFrom.HasValue || now >= CertValidFrom.Value) &&
                   (!CertValidTo.HasValue || now <= CertValidTo.Value);
        }
    }
}
