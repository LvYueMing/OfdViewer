using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OFDViewer.OFDEnum
{
    /// <summary>
    /// 文档分类,可取值如下:
    /// <para>Normal———普通文档</para>
    /// <para>EBook———电子书</para>
    /// <para>ENewsPaper———电子报纸</para>
    /// <para>EMagzine———电子期刊杂志</para>
    /// <para>默认值为 Normal</para>
    /// </summary>
    public enum DocumentUsage
    {
        /// <summary>
        /// 普通文档
        /// </summary>
        [Description("普通文档")]
        Normal = 0,
        /// <summary>
        /// 电子书
        /// </summary>
        [Description("电子书")]
        EBook = 1,
        /// <summary>
        /// 电子报纸
        /// </summary>
        [Description("电子报纸")]
        ENewsPaper = 2,
        /// <summary>
        /// 电子期刊杂志
        /// </summary>
        [Description("电子期刊杂志")]
        EMagzine = 3
    }
}
