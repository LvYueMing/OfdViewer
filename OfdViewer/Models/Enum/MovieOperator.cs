using System.ComponentModel;

namespace OFDViewer.Models.Enums
{
    /// <summary>
    /// 放映参数
    /// </summary>
    public enum MovieOperator
    {
        /// <summary>
        /// 播放
        /// </summary>
        [Description("Play")]
        Play,

        /// <summary>
        /// 停止
        /// </summary>
        [Description("Stop")]
        Stop,

        /// <summary>
        /// 暂停
        /// </summary>
        [Description("Pause")]
        Pause,


        /// <summary>
        /// 继续
        /// </summary>
        [Description("Resume")]
        Resume
    }
}
