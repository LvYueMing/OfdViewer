using System;
using System.Collections.Generic;

namespace OfdViewer.Eseal.Abstractions.Models
{
    /// <summary>
    /// 印章元数据
    /// </summary>
    public class EsealMetadata
    {
        /// <summary>
        /// 印章标识
        /// </summary>
        public string SealId { get; set; }

        /// <summary>
        /// 印章版本
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// 印章创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// 印章生效时间
        /// </summary>
        public DateTime? ValidFrom { get; set; }

        /// <summary>
        /// 印章失效时间
        /// </summary>
        public DateTime? ValidTo { get; set; }

        /// <summary>
        /// 印章类型
        /// </summary>
        public string SealType { get; set; }

        /// <summary>
        /// 印章制作单位
        /// </summary>
        public string Maker { get; set; }

        /// <summary>
        /// 扩展属性
        /// </summary>
        public Dictionary<string, string> ExtendedProperties { get; set; } = new Dictionary<string, string>();
    }
}
