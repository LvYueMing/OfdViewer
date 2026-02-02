using System;

namespace OfdViewer.Eseal.Abstractions.Models
{
    /// <summary>
    /// 签章人信息
    /// </summary>
    public class SignerInfo
    {
        /// <summary>
        /// 签章人名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 签章人证件类型
        /// </summary>
        public string IdType { get; set; }

        /// <summary>
        /// 签章人证件号码
        /// </summary>
        public string IdNumber { get; set; }

        /// <summary>
        /// 签章人单位名称
        /// </summary>
        public string Organization { get; set; }

        /// <summary>
        /// 签章人证书序列号
        /// </summary>
        public string CertSerialNumber { get; set; }

        /// <summary>
        /// 签章人证书颁发者
        /// </summary>
        public string CertIssuer { get; set; }

        /// <summary>
        /// 签章人证书有效期开始
        /// </summary>
        public DateTime? CertValidFrom { get; set; }

        /// <summary>
        /// 签章人证书有效期结束
        /// </summary>
        public DateTime? CertValidTo { get; set; }
    }
}
