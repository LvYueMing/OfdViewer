using System;

namespace OfdViewer.ESeal.Abstractions.Exceptions
{
    /// <summary>
    /// 印章验签异常
    /// 当印章验证失败时抛出
    /// </summary>
    public class EsealValidationException : EsealParserException
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="message">异常消息</param>
        public EsealValidationException(string message) : base(message)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="message">异常消息</param>
        /// <param name="inner">内部异常</param>
        public EsealValidationException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
