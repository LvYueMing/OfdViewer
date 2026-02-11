using System;

namespace OfdViewer.ESeal.Abstractions.Exceptions
{
    /// <summary>
    /// 电子印章解析异常基类
    /// </summary>
    public class EsealParserException : Exception
    {
        /// <summary>
        /// 厂商名称
        /// </summary>
        public string VendorName { get; }

        /// <summary>
        /// 错误代码
        /// </summary>
        public int ErrorCode { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="message">异常消息</param>
        public EsealParserException(string message) : base(message)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="message">异常消息</param>
        /// <param name="inner">内部异常</param>
        public EsealParserException(string message, Exception inner) : base(message, inner)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="vendorName">厂商名称</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="message">异常消息</param>
        public EsealParserException(string vendorName, int errorCode, string message) : base(message)
        {
            VendorName = vendorName;
            ErrorCode = errorCode;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="vendorName">厂商名称</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="message">异常消息</param>
        /// <param name="inner">内部异常</param>
        public EsealParserException(string vendorName, int errorCode, string message, Exception inner) : base(message, inner)
        {
            VendorName = vendorName;
            ErrorCode = errorCode;
        }
    }
}
