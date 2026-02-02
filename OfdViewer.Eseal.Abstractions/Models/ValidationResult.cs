using System;
using System.Collections.Generic;
using OfdViewer.Eseal.Abstractions.Interfaces;

namespace OfdViewer.Eseal.Abstractions.Models
{
    /// <summary>
    /// 验签结果数据模型
    /// 实现 IEsealValidationResult 接口
    /// </summary>
    public class ValidationResult : IEsealValidationResult
    {
        /// <summary>
        /// 验签是否成功
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// 验签状态码
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// 验签结果消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 验签时间
        /// </summary>
        public DateTime ValidationTime { get; set; }

        /// <summary>
        /// 证书信息
        /// </summary>
        public CertificateInfo Certificate { get; set; }

        /// <summary>
        /// 详细错误信息列表
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();

        /// <summary>
        /// 详细错误信息（接口实现）
        /// </summary>
        IEnumerable<string> IEsealValidationResult.Errors => Errors;
    }
}
