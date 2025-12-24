using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OFDViewer.BaseType;
using System.Xml.Serialization;

namespace OFDViewer.Actions.ActionItems
{
    /// <summary>
    /// 播放音频动作
    /// Sound动作表明播放一段音频。Sound动作结构如图78所示
    /// </summary>
    public class Sound
    {
        /// <summary>
        /// 引用资源文件中的音频资源标识 必选
        /// </summary>
        [XmlAttribute(AttributeName = "ResourceID")]
        public string ResourceIDString
        {
            get=> ResourceID.ToString();
            set=> ResourceID = ST_RefID.Parse(value);
        }
        [XmlIgnore]
        public ST_RefID ResourceID { get; set; }

        /// <summary>
        /// 播放的音量,取值范围[0100]
        ///默认值为100
        /// 可选
        /// </summary>
        [XmlAttribute(AttributeName = "Volume")]
        public int Volume { get; set; }


        /// <summary>
        /// 此音频是否需要循环播放
        /// 如果此属性为true,则Synchronous值无效
        /// 默认为false
        /// 可选
        /// </summary>
        [XmlAttribute(AttributeName = "Repeat")]
        public bool Repeat { get; set; }


        /// <summary>
        /// 是否同步播放
        /// true表示后续动作应等待此音频播放结束后才能开始,false表示立
        /// 刻返回并开始下一个动作
        /// 默认值为false
        /// 可选
        /// </summary>
        [XmlAttribute(AttributeName = "Synchronous")]
        public bool Synchronous { get; set; }
    }
}
