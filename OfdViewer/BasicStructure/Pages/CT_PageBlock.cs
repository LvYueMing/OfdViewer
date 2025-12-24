using OFDViewer.BasicStructure.Pages.PageBlockItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OFDViewer.BasicStructure.Pages
{
    /// <summary>
    /// 页面块结构
    /// </summary>
    public class CT_PageBlock
    {
        /// <summary>
        /// 存储每个子元素的类型标识（与PageBlocks一一对应）
        /// </summary>
        [XmlIgnore]
        public List<PageBlockItemNum> PageBlockItemNames { get; set; } = new List<PageBlockItemNum>();


        /// <summary>
        /// 存储CT_PageBlock的子元素（与PageBlockNames一一对应）
        /// </summary>
        [XmlElement("TextObject", typeof(TextObject))]
        [XmlElement("PathObject", typeof(PathObject))]
        [XmlElement("ImageObject", typeof(ImageObject))]
        [XmlElement("CompositeObject", typeof(CompositeObject))]
        [XmlElement("PageBlock", typeof(PageBlock))]
        [XmlChoiceIdentifier("PageBlockItemNames")]
        public List<object> PageBlockItems { get; set; } = new List<object>();

    }
}
