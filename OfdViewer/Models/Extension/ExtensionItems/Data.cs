using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OFDViewer.Models.Extension
{
    /// <summary>
    /// 扩展数据元素
    /// </summary>
    public class Data : ExtensionItem
    {
        /// <summary>
        /// 数据内容
        /// </summary>
        [XmlText(DataType = "anyType")]
        public object Content { get; set; }
    }
}
