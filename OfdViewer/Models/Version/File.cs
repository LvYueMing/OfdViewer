using OFDViewer.Models.BaseType;
using OFDViewer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OFDViewer.Models.Version
{
    /// <summary>
    /// File元素（扩展自ST_Loc）
    /// </summary>
    public class File
    {
        /// <summary>
        /// 文件路径
        /// </summary>
        [XmlIgnore]
        public ST_Loc Loc { get; set; }

        /// <summary>
        /// 文件路径字符串表示
        /// </summary>
        [XmlText]
        [XmlRequired(ErrorMsg = "文件路径为必选项，且不能为空")]
        public string LocString
        {
            get => Loc.ToString();
            set => Loc = value;
        }

        /// <summary>
        /// 文件标识
        /// 必选
        /// </summary>
        [XmlIgnore]
        public ST_ID ID { get; set; }

        [XmlAttribute("ID")]
        [XmlRequired(ErrorMsg = "ID 必选属性为必选项，且不能为空")]
        public string IDString
        {
            get => ID.ToString();
            set => ID = ST_ID.Parse(value);
        }

        /// <summary>
        /// 无参构造函数
        /// </summary>
        public File()
        {
            ID = ST_ID.CreateNew();
            Loc = new ST_Loc();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="loc">文件路径</param>
        public File(ST_Loc loc)
        {
            ID = ST_ID.CreateNew();
            Loc = loc;
        }
    }
}
