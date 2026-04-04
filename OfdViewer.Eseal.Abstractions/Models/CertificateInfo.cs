using System;

namespace OfdViewer.ESeal.Abstractions.Models
{
    /// <summary>
    /// 证书信息
    /// 符合 GM/T 0031-2014 安全电子签章密码技术规范
    /// </summary>
    public class CertificateInfo
    {
        /// <summary>
        /// 证书序列号
        /// GM/T 0031-2014: certSerialNumber - 证书序列号
        /// </summary>
        public string SerialNumber { get; set; } = string.Empty;

        /// <summary>
        /// 证书颁发者
        /// GM/T 0031-2014: certIssuer - 证书颁发者
        /// </summary>
        public string Issuer { get; set; } = string.Empty;

        /// <summary>
        /// 证书主题（DN）
        /// GM/T 0031-2014: certSubject - 证书主题
        /// </summary>
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// 证书有效期开始
        /// GM/T 0031-2014: certValidStart - 证书有效期起始
        /// </summary>
        public DateTime? ValidFrom { get; set; }

        /// <summary>
        /// 证书有效期结束
        /// GM/T 0031-2014: certValidEnd - 证书有效期结束
        /// </summary>
        public DateTime? ValidTo { get; set; }

        /// <summary>
        /// 证书公钥
        /// GM/T 0031-2014: publicKey - 公钥
        /// </summary>
        public string PublicKey { get; set; } = string.Empty;

        /// <summary>
        /// 证书公钥算法
        /// GM/T 0031-2014: 公钥算法标识（如 SM2、RSA 等）
        /// </summary>
        public string PublicKeyAlgorithm { get; set; } = string.Empty;

        /// <summary>
        /// 证书签名算法
        /// GM/T 0031-2014: signatureAlgorithm - 签名算法标识
        /// </summary>
        public string SignatureAlgorithm { get; set; } = string.Empty;

        /// <summary>
        /// 证书指纹（Thumbprint）
        /// </summary>
        public string Thumbprint { get; set; } = string.Empty;

        /// <summary>
        /// 证书指纹算法
        /// </summary>
        public string ThumbprintAlgorithm { get; set; } = "SHA-1";

        /// <summary>
        /// 密钥用途
        /// </summary>
        public string KeyUsage { get; set; } = string.Empty;

        /// <summary>
        /// 扩展密钥用途
        /// </summary>
        public string ExtendedKeyUsage { get; set; } = string.Empty;

        /// <summary>
        /// 证书原始数据（DER编码）
        /// </summary>
        public byte[] RawData { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// 检查证书是否在有效期内
        /// </summary>
        /// <returns>是否在有效期内</returns>
        public bool IsValid()
        {
            var now = DateTime.Now;
            return (!ValidFrom.HasValue || now >= ValidFrom.Value) &&
                   (!ValidTo.HasValue || now <= ValidTo.Value);
        }

        /// <summary>
        /// 获取证书有效期剩余天数
        /// </summary>
        /// <returns>剩余天数，如果已过期返回负数</returns>
        public int GetRemainingDays()
        {
            if (!ValidTo.HasValue)
                return int.MaxValue;

            return (ValidTo.Value - DateTime.Now).Days;
        }

        /// <summary>
        /// 从证书主题中提取通用名称（CN）
        /// </summary>
        /// <returns>通用名称</returns>
        public string GetCommonName()
        {
            return ExtractDnComponent(Subject, "CN");
        }

        /// <summary>
        /// 从证书主题中提取组织名称（O）
        /// </summary>
        /// <returns>组织名称</returns>
        public string GetOrganization()
        {
            return ExtractDnComponent(Subject, "O");
        }

        /// <summary>
        /// 从证书主题中提取组织单位（OU）
        /// </summary>
        /// <returns>组织单位</returns>
        public string GetOrganizationalUnit()
        {
            return ExtractDnComponent(Subject, "OU");
        }

        /// <summary>
        /// 从DN字符串中提取指定组件
        /// </summary>
        /// <param name="dn">DN字符串</param>
        /// <param name="component">组件名称</param>
        /// <returns>组件值</returns>
        private string ExtractDnComponent(string dn, string component)
        {
            if (string.IsNullOrEmpty(dn))
                return string.Empty;

            var prefix = $"{component}=";
            var index = dn.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
            if (index < 0)
                return string.Empty;

            var start = index + prefix.Length;
            var end = dn.IndexOf(",", start);
            if (end < 0)
                end = dn.Length;

            return dn.Substring(start, end - start).Trim();
        }
    }
}
