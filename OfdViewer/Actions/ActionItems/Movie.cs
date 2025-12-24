using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OFDViewer.BaseType;
using System.Xml.Serialization;
using OFDViewer.Enums;
using OFDViewer.Utils;

namespace OFDViewer.Actions.ActionItems
{
    /// <summary>
    /// 播放视频动作
    /// Movie动作用于播放视频。播放视频动作结构如图79所示。
    /// </summary>
    public class Movie
    {
        /// <summary>
        /// 引用资源文件中定义的视频资源标识 必选
        /// </summary>
        [XmlAttribute(AttributeName = "ResourceID")]
        public string ResourceIDString
        {
            get => ResourceID.ToString();
            set => ResourceID = ST_RefID.Parse(value);
        }
        [XmlIgnore]
        public ST_RefID ResourceID { get; set; }

        /// <summary>
        /// 放映参数,见表59
        ///默认值为 Play
        ///可选
        /// </summary>
        [XmlAttribute(AttributeName = "Operator")]
        public string OperatorString
        {
            get => Operator.ToString();

            set => Operator = EnumHelper.ParseEnum<MovieOperator>(value);
        }

        public MovieOperator Operator { get; set; } = MovieOperator.Play; // 默认值 Play
    }
}
