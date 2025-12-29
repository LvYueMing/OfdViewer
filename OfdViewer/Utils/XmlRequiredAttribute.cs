using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OFDViewer.Utils
{
    /// <summary>
    /// 自定义特性：标记XML序列化时的必填属性/元素（对应xs:attribute use="required"）
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class XmlRequiredAttribute : Attribute
    {
        /// <summary>
        /// 校验失败提示信息
        /// </summary>
        public string ErrorMsg { get; set; }

        public XmlRequiredAttribute(string errorMessage = null)
        {
            ErrorMsg = errorMessage ?? "该属性是XML序列化必填项，不能为空或无效值";
        }
    }
}
