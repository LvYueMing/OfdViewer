using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using OFDViewer.BaseType;
using OFDViewer.Fonts;
using OFDViewer.Graph;

namespace OFDViewer.PageDescription
{
    /// <summary>
    /// 裁剪区域,用一个图形对象或文字对象来描述裁剪区的一个组成部分,最终裁剪区是这些区域的并集
    /// 必选
    /// </summary>
    public class ClipArea
    {
        private CT_Path _path;
        private CT_Text _text;

        /// <summary>
        /// 用于裁剪的图形,见9.1图形对象 必选
        /// </summary>
        /// <remarks>Path、Text元素（xs:choice 二选一）</remarks>
        [XmlElement("Path", IsNullable = true)]
        public CT_Path Path
        {
            get => _path;
            set
            {
                // 如果设置了Path，则Text必须为null
                if (value != null && _text != null)
                {
                    throw new InvalidOperationException("Path和Text是互斥的，Text已存在，不能设置Path。");
                }
                _path = value;
            }
        }

        /// <summary>
        /// 用于裁剪的文本,见11.2文字对象 必选
        /// </summary>
        /// <remarks>Path、Text元素（xs:choice 二选一）</remarks>
        [XmlElement("Text", IsNullable = true)]
        public CT_Text Text
        {
            get => _text;
            set
            {
                // 如果设置了Text，则Path必须为null
                if (value != null && _path != null)
                {
                    throw new InvalidOperationException("Path和Text是互斥的，Path已存在，不能设置Text。");
                }
                _text = value;
            }
        }

        /// <summary>
        /// 引用资源文件中的绘制参数的标识,线宽、结合点和端点样式等绘制特性对裁剪效果会产生影响,有关绘制参数的描述见8.2
        /// 可选
        /// </summary>
        [XmlAttribute("DrawParam")]
        public ST_RefID DrawParam { get; set; }

        /// <summary>
        /// 针对对象坐标系,对 Area下包含的Path和 Text进行进一步的变换 可选
        /// </summary>
        [XmlAttribute("CTM")]
        public ST_Array CTM { get; set; }

        /// <summary>
        /// 用于控制Path/Text的XML序列化（解决xs:choice的序列化问题）
        /// </summary>
        [XmlIgnore]
        public bool PathSpecified => Path != null;

        /// <summary>
        /// 用于控制Path/Text的XML序列化（解决xs:choice的序列化问题）
        /// </summary>
        [XmlIgnore]
        public bool TextSpecified => Text != null;


        /// <summary>
        /// 设置Path值，同时将Text设为null
        /// </summary>
        /// <param name="path">路径对象</param>
        public void SetPath(CT_Path path)
        {
            _text = null;
            _path = path;
        }

        /// <summary>
        /// 设置Text值，同时将Path设为null
        /// </summary>
        /// <param name="text">文本对象</param>
        public void SetText(CT_Text text)
        {
            _path = null;
            _text = text;
        }
    }
}
