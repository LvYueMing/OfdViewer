using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OFDViewer.Enums
{
    /// <summary>
    /// 图形的填充规则,当 Fill属性存在时出现
    /// 可选值为 NonZero和 Even-Odd
    /// 默认值为 NonZero
    /// </summary>
    public enum PathFillRule
    {
        /// <summary>
        /// 正文层
        /// </summary>
        [Description("NonZero")]
        NonZero =0,


        /// <summary>
        /// 正文层
        /// </summary>
        [Description("Even-Odd")]
        EvenOdd=1 // C# 枚举不允许包含连字符，因此映射为 EvenOdd
    }
}
