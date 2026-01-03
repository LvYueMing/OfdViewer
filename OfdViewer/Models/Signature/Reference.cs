using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using OFDViewer.Models.BaseType;
using OFDViewer.Utils;

namespace OFDViewer.Models.Signature
{
    /// <summary>
    /// 针对一个文件的摘要节点
    /// </summary>
    public class Reference
    {
        /// <summary>
        /// 对包内文件进行摘要计算,对所得的二进制摘要值进行base64编码所得结果
        /// 必选
        /// </summary>
        [XmlElement("CheckValue")]
        [XmlRequired(ErrorMsg = "CheckValue为必选项，且不能为空")]
        public string CheckValue { get; set; }

        /// <summary>
        /// 指向包内的文件,使用绝对路径 
        /// 必选
        /// </summary>
        [XmlAttribute("FileRef")]
        [XmlRequired(ErrorMsg = "FileRef为必选项，且不能为空")]
        public string FileRefString
        {
            get => FileRef.ToString();
            set => FileRef = value;
        }

        [XmlIgnore]
        public ST_Loc FileRef { get; set; }
    }
}
