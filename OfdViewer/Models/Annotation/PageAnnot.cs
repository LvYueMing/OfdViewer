using System.Xml.Serialization;
using OFDViewer.Utils;

namespace OFDViewer.Models.Annotation
{
    /// <summary>
    /// 页面注释根元素
    /// </summary>
    [XmlRoot("PageAnnot", Namespace = "http://www.ofdspec.org/2016")]
    public class PageAnnot
    {
        /// <summary>
        /// 注释元素集合
        /// 1..n
        /// </summary>
        [XmlElement("Annot", IsNullable = false)]
        [XmlRequired(MinItemCount = 1)]
        public List<Annot> Annotations { get; set; } = new List<Annot>();

        /// <summary>
        /// 控制Annotations属性是否序列化
        /// </summary>
        public bool ShouldSerializeAnnotations()
        {
            // 当Annotations为空时不序列化
            return Annotations != null && Annotations.Count > 0;
        }
    }
}