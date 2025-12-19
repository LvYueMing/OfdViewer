using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OFDViewer.OFDEnum
{
    /// <summary>
    /// 图层类型描述
    /// </summary>
    public enum LayerType
    {
        /// <summary>
        /// 正文层
        /// </summary>
        [Description("正文层")]
        Body = 0,
        /// <summary>
        /// 前景层
        /// </summary>
        [Description("前景层")]
        Foreground = 1,
        /// <summary>
        /// 背景层
        /// </summary>
        [Description("背景层")]
        Background = 2
    }
}
