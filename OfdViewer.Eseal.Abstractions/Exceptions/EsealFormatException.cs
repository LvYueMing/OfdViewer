using System;

namespace OfdViewer.ESeal.Abstractions.Exceptions
{
    /// <summary>
    /// 印章格式异常
    /// 当印章文件格式不正确或无法解析时抛出
    /// </summary>
    public class EsealFormatException : EsealParserException
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="message">异常消息</param>
        public EsealFormatException(string message) : base(message)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="message">异常消息</param>
        /// <param name="inner">内部异常</param>
        public EsealFormatException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
