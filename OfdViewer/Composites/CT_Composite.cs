using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OFDViewer.BaseType;
using OFDViewer.PageDescription;
using System.Xml.Serialization;

namespace OFDViewer.Composites
{
    /// <summary>
    /// 复合对象
    /// 复合对象是一种特殊的图元对象,拥有图元对象的一切特性,但其内容在 ResourceID 指向的矢量
    /// 图像资源中进行描述,一个资源可以被多个复合对象所引用,通过这种方式可实现对文档内矢量图文内
    /// 容的复用。复合对象的描述如图71所示。
    /// </summary>
    public class CT_Composite : CT_GraphicUnit
    {
        /// <summary>
        /// 引用资源文件中定义的矢量图像的标识 必选
        /// 复合对象引用的资源是 Res中的矢量图像(CompositeGraphUnit),其类型为 CT_VectorG,其结构如图72所示。
        /// </summary>
        [XmlAttribute("ResourceID")]
        public string ResourceIDString
        {
            get => ResourceID.ToString();
            set => ResourceID = ST_RefID.Parse(value);
        }

        [XmlIgnore]
        public ST_RefID ResourceID { get; set; }
    }
}
