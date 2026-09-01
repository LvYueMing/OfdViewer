using System.Xml.Serialization;
using OFDViewer.Utils;

namespace OFDViewer.Models.BaseStructure.DocumentRoot
{
    /// <summary>
    /// 有效期
    /// 即此文档允许访问的期限, 其具体期限取决于开始日期和
    /// 结束日期, 其中开始日期不能晚于结束日期, 并且开始日期和结束
    /// 日期至少出现一个。 当不设置开始日期时, 代表不限定开始日期,
    /// 当不设置结束日期时代表不限定结束日期; 当此不设置此节点时,
    /// 表示开始日期和结束日期均不受限
    /// </summary>
    public class PermissionValidPeriod
    {
        /// <summary>
        /// 有效期开始日期 可选
        /// </summary>
        [XmlAttribute(AttributeName = "StartDate")]
        public string StartDateString
        {
            get => StartDate?.ToString("yyyy-MM-dd HH:mm:ss");
            // 容错解析：兼容 PDF 日期（D:...）等非标准格式，空值/解析失败置 null
            set => StartDate = string.IsNullOrEmpty(value) ? null
                : (DateParser.TryParse(value, out var parsed) ? parsed : (DateTime?)null);
        }
        [XmlIgnore]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// 有效期结束日期 可选
        /// </summary>
        [XmlAttribute(AttributeName = "EndDate")]
        public string EndDateString
        {
            get => EndDate?.ToString("yyyy-MM-dd HH:mm:ss");
            // 容错解析：兼容 PDF 日期（D:...）等非标准格式，空值/解析失败置 null
            set => EndDate = string.IsNullOrEmpty(value) ? null
                : (DateParser.TryParse(value, out var parsed) ? parsed : (DateTime?)null);
        }

        [XmlIgnore]
        public DateTime? EndDate { get; set; }


        /// <summary>
        /// 仅当 StartDate 有值时才序列化
        /// </summary>
        public bool ShouldSerializeStartDate() => StartDate.HasValue;

        /// <summary>
        /// 仅当 EndDate 有值时才序列化
        /// </summary>
        public bool ShouldSerializeEndDate() => EndDate.HasValue;
    }
}
