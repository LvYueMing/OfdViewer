using System.Xml.Serialization;

namespace OFDViewer.Models.Annotation
{
    /// <summary>
    /// 注释
    /// 注释是版式文档形成后附加的图文信息,用户可通过鼠标或键盘与其进行交互。本标准中,页面内
    /// 容与注释内容是分文件描述的。文档的注释在注释列表文件中按照页面进行组织索引,注释的内容在
    /// 分页注释文件中描述,注释列表结构如图80所示。
    /// </summary>
    [XmlRoot("Annotations", Namespace = "http://www.ofdspec.org/2016")]
    public class Annotations
    {
        /// <summary>
        /// 注释所在页 
        /// 可选
        /// </summary>
        [XmlElement("Page", IsNullable = false)]
        public List<Page> Pages { get; set; } = new List<Page>();

        /// <summary>
        /// 控制Pages属性是否序列化
        /// </summary>
        public bool ShouldSerializePages()
        {
            // 当Pages为空时不序列化
            return Pages != null && Pages.Count > 0;
        }
    }
}