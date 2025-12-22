using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OFDViewer.Actions
{
    /// <summary>
    /// 动作由事件触发,事件类型限定于 DO、PO、CLICK 三种,分别对应于文档打开动作、页面打开动作和区域内单击动作,事件类型说明见表52
    /// </summary>
    public enum ActionEventEnum
    {
        /// <summary>
        /// 文档打开
        /// </summary>
        [Description("DO")]
        DO,

        /// <summary>
        /// 页面打开
        /// </summary>
        [Description("PO")]
        PO,

        /// <summary>
        /// 单击区域
        /// </summary>
        [Description("CLICK")]
        CLICK

    }
}
