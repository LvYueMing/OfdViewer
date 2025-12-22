using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OFDViewer.Actions.ActionItems
{
    /// <summary>
    /// 动作类型
    /// </summary>
    public enum ActionItemEnum
    {
        /// <summary>
        /// 跳转动作
        /// </summary>
        [Description("Goto")]
        Goto,

        /// <summary>
        /// URI动作
        /// </summary>
        [Description("URI")]
        URI,

        /// <summary>
        /// 附件动作
        /// </summary>
        [Description("GotoA")]
        GotoA,

        /// <summary>
        /// 播放音频动作
        /// </summary>
        [Description("Sound")]
        Sound,

        /// <summary>
        /// 播放视频动作
        /// </summary>
        [Description("Movie")]
        Movie
    }
}
