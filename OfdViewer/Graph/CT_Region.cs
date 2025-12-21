using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OFDViewer.Graph
{
    /// <summary>
    /// 区域结构
    /// 图形也可采用 XML复杂类型的方式进行描述,这种方式主要用于区域(Region)。
    /// 区域由一系列的分路径(Area)组成,每个分路径都是闭合的,其结构如图49所示。
    /// </summary>
    public class CT_Region
    {
        // Area元素可重复（maxOccurs="unbounded"）
        [XmlElement("Area")]
        public List<RegionArea> Area { get; set; }
    }
}
