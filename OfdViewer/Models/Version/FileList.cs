using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OFDViewer.Models.Version
{
    /// <summary>
    /// FileList元素
    /// </summary>
    public class FileList
    {
        /// <summary>
        /// 文件元素集合
        /// 0..*
        /// </summary>
        [XmlElement("File", IsNullable = false)]
        public List<File> Files { get; set; } = new List<File>();
    }
}
