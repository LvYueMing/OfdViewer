using System.Xml.Serialization;

namespace OFDViewer.Models.CustomTag
{
    /// <summary>
    /// 自定义标引索引文件
    /// 标引文件中通过ID引用与被标引对象发生"非接触式(分离式)"关联
    /// 标引内容可任意扩展，但建议给出扩展内容的规范约束文件(schema)或命名空间
    /// </summary>
    [XmlRoot("CustomTags", Namespace = Constants.OFD_NAMESPACE_URI)]
    public class CustomTags
    {
        /// <summary>
        /// 自定义标引对象集合
        /// 可选 0..n
        /// </summary>
        [XmlElement("CustomTag")]
        public List<CustomTag> Tags { get; set; } = new List<CustomTag>();

        /// <summary>
        /// 控制Tags属性是否序列化
        /// </summary>
        public bool ShouldSerializeTags()
        {
            return Tags != null && Tags.Count > 0;
        }
    }
}
