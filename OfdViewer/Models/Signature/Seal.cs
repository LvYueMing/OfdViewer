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
    public class Seal
    {
        /// <summary>
        /// 指向包内的安全电子印章文件,遵循密码领域的相关规范 
        /// 必选
        /// </summary>
        [XmlElement("BaseLoc")]
        [XmlRequired(ErrorMsg = "BaseLoc为必选项，且不能为空")]
        public string BaseLocString
        {
            get => BaseLoc.ToString();
            set => BaseLoc = value;
        }
        [XmlIgnore]
        public ST_Loc BaseLoc { get; set; }
    }
}
