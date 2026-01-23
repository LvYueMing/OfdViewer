using System.Xml.Serialization;
using OFDViewer.Models.BaseType;

namespace OFDViewer.Models.Font
{
    /// <summary>
    /// 字形转换
    /// 当存在字形变换时,TextCode 对象中使用字形变换节点(CGTransform) 描述字符编码和字形索引
    /// 之间的关系, 该节点结构如图66 所示
    /// </summary>
    public class CT_CGTransform
    {
        /// <summary>
        /// 变换后的字形索引列表 必选
        /// </summary>
        [XmlElement(ElementName = "Glyphs")]
        public ST_Array Glyphs { get; set; }

        /// <summary>
        /// TextCode 中字符编码的起始位置, 从0 开始 必选
        /// </summary>
        [XmlAttribute(AttributeName = "CodePosition")]
        public int CodePosition { get; set; }

        /// <summary>
        /// 变换关系中字符的数量, 该数值应大于或等于1, 否则属于错误描述,
        /// 默认为1
        /// 可选
        /// </summary>
        [XmlAttribute(AttributeName = "CodeCount")]
        public int CodeCount { get; set; } = 1;


        //控制 CodeCount 属性是否序列化
        public bool ShouldSerializeCodeCount()
        {
            return CodeCount != 1;
        }

        /// <summary>
        /// 变换关系中字形索引的个数, 该数值应大于或等于1, 否则属于错误描述, 默认为1
        /// 可选
        /// </summary>
        [XmlAttribute(AttributeName = "GlyphCount")]

        public int GlyphCount { get; set; } = 1;
        //控制 GlyphCount 属性是否序列化
        public bool ShouldSerializeGlyphCount()
        {
            return GlyphCount != 1;
        }

    }
}
