using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OFDViewer.Models.BaseType;
using System.Xml.Serialization;

namespace OFDViewer.Models.Extension.ExtensionItems
{
    /// <summary>
    /// 扩展数据文件路径元素
    /// </summary>
    public class ExtendData : ExtensionItem
    {
        /// <summary>
        /// 扩展数据文件路径
        /// </summary>
        [XmlText(DataType = "string")]
        public string Path
        {
            get => Loc?.ToString();
            set => Loc = string.IsNullOrEmpty(value) ? null : new ST_Loc(value);
        }

        [XmlIgnore]
        public ST_Loc? Loc { get; set; }
    }
}
