using System.Xml.Serialization;

namespace OFDViewer.Models.PageDesc.Colors.ColorItems
{
    /// <summary>
    /// 渐变段类型
    /// </summary>
    public class AxialShdSegment
    {
        /// <summary>
        /// 段颜色
        /// </summary>
        [XmlElement("Color")]
        public CT_Color Color { get; set; }

        /// <summary>
        /// 段位置
        /// </summary>
        [XmlAttribute("Position")]
        public double Position { get; set; }
    }
}
