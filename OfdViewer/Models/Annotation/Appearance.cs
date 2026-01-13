using System.Xml.Schema;
using System.Xml.Serialization;
using OFDViewer.Models.BaseStructure.Pages;
using OFDViewer.Models.BaseType;

namespace OFDViewer.Models.Annotation
{
    /// <summary>
    /// 注释的静态呈现效果,使用页面块定义来描述
    /// </summary>
    public class Appearance : CT_PageBlock
    {
        /// <summary>
        /// 边界框
        /// 可选
        /// </summary>
        [XmlAttribute("Boundary", DataType = "string")]
        public string BoundaryString
        {
            get => Boundary.ToString();
            set => Boundary = ST_Box.Parse(value);
        }

        [XmlIgnore]
        public ST_Box Boundary { get; set; }
    }
}