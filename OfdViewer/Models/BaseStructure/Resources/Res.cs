using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using OFDViewer.Models.BaseType;
using OFDViewer.Models.BaseStructure.Resources.ResItems;
using OFDViewer.Models.PageDesc.Colors;
using OFDViewer.Utils;

namespace OFDViewer.Models.BaseStructure.Resources
{
    /// <summary>
    /// 资源是绘制图元时所需数据(如绘制参数、颜色空间、字型、图像、音视频等)的集合
    /// </summary>
    public class Res
    {
        /// <summary>
        /// 混合路径节点集合：匹配 xs:choice minOccurs="0" maxOccurs="unbounded"
        /// </summary>
        [XmlElement("ColorSpaces", typeof(ColorSpaces))]
        [XmlElement("DrawParams", typeof(DrawParams))]
        [XmlElement("Fonts", typeof(ResItems.Fonts))]
        [XmlElement("MultiMedias", typeof(MultiMedias))]
        [XmlElement("CompositeGraphicUnits", typeof(CompositeGraphicUnits))]
        public List<BaseRes> ResItems { get; set; }


        /// <summary>
        /// 定义此资源文件的通用数据存储路径,BaseLoc属性的意义在于明确资源文件存储的位置,比如 R1.xml中可以指定 BaseLoc
        /// 为“./Res”, 表明该资源文件中所有数据文件的默认存储位置在当前路径的 Res 目录下
        /// 必选
        /// </summary>
        [XmlAttribute("BaseLoc")]
        [XmlRequired(ErrorMsg = "BaseLoc 属性为必选项，且不能为空")]
        public string BaseLocString
        {
            get { return BaseLoc.ToString(); }
            set { BaseLoc = value; }
        }
        [XmlIgnore]
        public ST_Loc BaseLoc { get; set; }
    }
}
