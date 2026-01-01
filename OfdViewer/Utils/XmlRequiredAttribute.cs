using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OFDViewer.Utils
{
    /// <summary>
    /// 自定义特性：标记XML序列化时的必填属性/元素（对应xs:attribute use="required"）
    /// 支持普通属性非空验证 + 集合（List等）元素个数非空验证
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class XmlRequiredAttribute : Attribute
    {
        /// <summary>
        /// 校验失败提示信息
        /// </summary>
        public string ErrorMsg { get; set; }

        /// <summary>
        /// 对于集合类型，要求的最小元素个数（默认0）
        /// </summary>
        public int MinItemCount { get; set; }

        /// <summary>
        /// 对于集合类型，要求的最大元素个数（默认int.MaxValue，表示不限制）
        /// </summary>
        public int MaxItemCount { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="errorMsg">校验失败提示信息</param>
        /// <param name="minItemCount">集合最小元素个数</param>
        /// <param name="maxItemCount">集合最大元素个数</param>
        public XmlRequiredAttribute(string errorMsg = null, int minItemCount = 0, int maxItemCount = int.MaxValue)
        {
            ErrorMsg = errorMsg ?? "该属性是XML序列化必填项，不能为空或无效值";
            MinItemCount = Math.Max(minItemCount, 0); // 确保最小元素数非负
            MaxItemCount = Math.Max(maxItemCount, MinItemCount); // 确保最大元素数不小于最小元素数
        }
    }
}
