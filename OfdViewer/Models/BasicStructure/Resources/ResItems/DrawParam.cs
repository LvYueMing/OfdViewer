using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OFDViewer.Models.PageDesc.DrawParams;
using System.Xml.Serialization;
using OFDViewer.Models.BaseType;
using OFDViewer.Utils;

namespace OFDViewer.Models.BasicStructure.Resources.ResItems
{
    public class DrawParam : CT_DrawParam
    {
        [XmlAttribute("ID")]
        [XmlRequired(ErrorMsg = "ID 必选属性为必选项，且不能为空")]
        public string IDString
        {
            get { return ID.ToString(); }
            set { ID = ST_ID.Parse(value); }
        }
        [XmlIgnore]
        public ST_ID ID { get; set; }
    }
}
