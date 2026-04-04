using System;
using System.Collections.Generic;
using OfdViewer.ESeal.Abstractions.Interfaces;

namespace OfdViewer.ESeal.Abstractions.Models
{
    /// <summary>
    /// 验签结果数据模型
    /// 实现 IEsealValidationResult 接口
    /// 符合 GM/T 0031-2014 安全电子签章密码技术规范
    /// </summary>
    public class ValidationResult : IEsealValidationResult
    {
        /// <summary>
        /// 验签是否成功
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// 验签状态码
        /// 0: 验签成功
        /// -1: 验签失败
        /// -2: 证书已过期或尚未生效
        /// -3: 证书信息缺失
        /// -4: 数据完整性验证失败
        /// -5: 签名算法不支持
        /// -6: 印章已过期
        /// -7: 印章已吊销
        /// -8: 印章已冻结
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// 验签结果消息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 验签时间
        /// </summary>
        public DateTime ValidationTime { get; set; }

        /// <summary>
        /// 证书信息
        /// </summary>
        public CertificateInfo Certificate { get; set; } = new CertificateInfo();

        /// <summary>
        /// 详细错误信息列表
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();

        /// <summary>
        /// 详细错误信息（接口实现）
        /// </summary>
        IEnumerable<string> IEsealValidationResult.Errors => Errors;

        /// <summary>
        /// 签名算法标识
        /// GM/T 0031-2014: signatureAlgorithm - 签名算法标识
        /// </summary>
        public string SignatureAlgorithm { get; set; } = string.Empty;

        /// <summary>
        /// 签名时间
        /// GM/T 0031-2014: signingTime - 签名时间
        /// </summary>
        public DateTime? SigningTime { get; set; }

        /// <summary>
        /// 被签名数据的哈希值
        /// GM/T 0031-2014: toSignDigest - 待签数据摘要
        /// </summary>
        public byte[] SignedDataHash { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// 哈希算法标识
        /// GM/T 0031-2014: digestAlgorithm - 摘要算法标识
        /// </summary>
        public string HashAlgorithm { get; set; } = string.Empty;

        /// <summary>
        /// 印章图片哈希值验证结果
        /// GM/T 0031-2014: pictureHash 验证
        /// </summary>
        public bool ImageHashValid { get; set; }

        /// <summary>
        /// 证书链验证结果
        /// </summary>
        public bool CertificateChainValid { get; set; }

        /// <summary>
        /// 证书吊销状态验证结果
        /// </summary>
        public bool CertificateNotRevoked { get; set; }

        /// <summary>
        /// 时间戳验证结果
        /// GM/T 0031-2014: timeStamp - 时间戳
        /// </summary>
        public bool TimestampValid { get; set; }

        /// <summary>
        /// 时间戳信息
        /// </summary>
        public DateTime? Timestamp { get; set; }

        /// <summary>
        /// 扩展验证结果
        /// 用于存储厂商特定的验证信息
        /// </summary>
        public Dictionary<string, object> ExtendedResults { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// 创建一个成功的验签结果
        /// </summary>
        /// <param name="message">成功消息</param>
        /// <returns>验签结果</returns>
        public static ValidationResult Success(string message = "验签成功")
        {
            return new ValidationResult
            {
                IsValid = true,
                StatusCode = 0,
                Message = message,
                ValidationTime = DateTime.Now
            };
        }

        /// <summary>
        /// 创建一个失败的验签结果
        /// </summary>
        /// <param name="statusCode">状态码</param>
        /// <param name="message">错误消息</param>
        /// <returns>验签结果</returns>
        public static ValidationResult Failure(int statusCode, string message)
        {
            return new ValidationResult
            {
                IsValid = false,
                StatusCode = statusCode,
                Message = message,
                ValidationTime = DateTime.Now
            };
        }

        /// <summary>
        /// 添加错误信息
        /// </summary>
        /// <param name="error">错误信息</param>
        public void AddError(string error)
        {
            if (!string.IsNullOrEmpty(error))
            {
                Errors.Add(error);
            }
        }

        /// <summary>
        /// 添加多个错误信息
        /// </summary>
        /// <param name="errors">错误信息列表</param>
        public void AddErrors(IEnumerable<string> errors)
        {
            if (errors != null)
            {
                Errors.AddRange(errors);
            }
        }

        /// <summary>
        /// 获取完整错误信息
        /// </summary>
        /// <returns>完整错误信息</returns>
        public string GetFullErrorMessage()
        {
            if (Errors.Count == 0)
                return Message;

            return $"{Message}; 详细信息: {string.Join("; ", Errors)}";
        }
    }

    /// <summary>
    /// 验签状态码常量
    /// </summary>
    public static class ValidationStatusCodes
    {
        /// <summary>
        /// 验签成功
        /// </summary>
        public const int Success = 0;

        /// <summary>
        /// 验签失败（通用错误）
        /// </summary>
        public const int GeneralFailure = -1;

        /// <summary>
        /// 证书已过期或尚未生效
        /// </summary>
        public const int CertificateExpired = -2;

        /// <summary>
        /// 证书信息缺失
        /// </summary>
        public const int CertificateMissing = -3;

        /// <summary>
        /// 数据完整性验证失败
        /// </summary>
        public const int DataIntegrityFailure = -4;

        /// <summary>
        /// 签名算法不支持
        /// </summary>
        public const int UnsupportedAlgorithm = -5;

        /// <summary>
        /// 印章已过期
        /// </summary>
        public const int SealExpired = -6;

        /// <summary>
        /// 印章已吊销
        /// </summary>
        public const int SealRevoked = -7;

        /// <summary>
        /// 印章已冻结
        /// </summary>
        public const int SealFrozen = -8;

        /// <summary>
        /// 证书链验证失败
        /// </summary>
        public const int CertificateChainInvalid = -9;

        /// <summary>
        /// 证书已吊销
        /// </summary>
        public const int CertificateRevoked = -10;

        /// <summary>
        /// 时间戳验证失败
        /// </summary>
        public const int TimestampInvalid = -11;

        /// <summary>
        /// 印章图片哈希验证失败
        /// </summary>
        public const int ImageHashInvalid = -12;
    }
}
