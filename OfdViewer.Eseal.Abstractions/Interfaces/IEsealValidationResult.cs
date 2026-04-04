using System;
using System.Collections.Generic;
using OfdViewer.ESeal.Abstractions.Models;

namespace OfdViewer.ESeal.Abstractions.Interfaces
{
    /// <summary>
    /// 印章验签结果接口
    /// </summary>
    public interface IEsealValidationResult
    {
        /// <summary>
        /// 验签是否成功
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// 验签状态码
        /// </summary>
        int StatusCode { get; }

        /// <summary>
        /// 验签结果消息
        /// </summary>
        string Message { get; }

        /// <summary>
        /// 验签时间
        /// </summary>
        DateTime ValidationTime { get; }

        /// <summary>
        /// 证书信息
        /// </summary>
        CertificateInfo Certificate { get; }

        /// <summary>
        /// 详细错误信息（如果有）
        /// </summary>
        IEnumerable<string> Errors { get; }
    }
}
