using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OFDViewer.Utils
{
    /// <summary>
    /// 自定义异常：XML序列化必填项校验失败
    /// </summary>
    public class XmlRequiredValidationException : Exception
    {
        /// <summary>
        /// 失败的属性名称
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// 失败的对象类型
        /// </summary>
        public Type TargetType { get; }

        public XmlRequiredValidationException(string message, string propertyName, Type targetType) : base(message)
        {
            PropertyName = propertyName;
            TargetType = targetType;
        }

        // 重写ToString，输出更详细的异常信息
        public override string ToString()
        {
            return $"【XML必填项校验失败】\n对象类型：{TargetType.FullName}\n属性名称：{PropertyName}\n错误信息：{Message}";
        }
    }
}
