using System;

namespace OfdViewer.Eseal.Abstractions.Models
{
    /// <summary>
    /// 证书信息
    /// </summary>
    public class CertificateInfo
    {
        /// <summary>
        /// 证书序列号
        /// </summary>
        public string SerialNumber { get; set; }

        /// <summary>
        /// 证书颁发者
        /// </summary>
        public string Issuer { get; set; }

        /// <summary>
        /// 证书主题
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// 证书有效期开始
        /// </summary>
        public DateTime? ValidFrom { get; set; }

        /// <summary>
        /// 证书有效期结束
        /// </summary>
        public DateTime? ValidTo { get; set; }

        /// <summary>
        /// 证书公钥
        /// </summary>
        public string PublicKey { get; set; }

        /// <summary>
        /// 证书算法
        /// </summary>
        public string Algorithm { get; set; }

        /// <summary>
        /// 证书指纹
        /// </summary>
        public string Thumbprint { get; set; }
    }
}
