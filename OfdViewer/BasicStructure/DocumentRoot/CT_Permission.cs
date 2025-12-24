using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OFDViewer.BasicStructure.DocumentRoot
{
    /// <summary>
    /// 文档权限
    /// </summary>
    public class CT_Permission
    {
        /// <summary>
        /// 是否允许编辑
        /// 默认值为true
        /// 可选
        /// </summary>
        [XmlElement(ElementName = "Edit", IsNullable = false)]
        public bool Edit { get; set; } = true;

        /// <summary>
        /// 是否允许添加或修改标注
        /// 默认值:true
        /// 可选
        /// </summary>
        [XmlElement(ElementName = "Annot", IsNullable = false)]
        public bool Annot { get; set; } = true;

        /// <summary>
        /// 是否允许导出
        /// 默认值为true
        /// 可选
        /// </summary>
        [XmlElement(ElementName = "Export", IsNullable = false)]
        public bool Export { get; set; } = true;

        /// <summary>
        /// 是否允许进行数字签名
        /// 默认值:true
        /// 可选
        /// </summary>
        [XmlElement(ElementName = "Signature", IsNullable = false)]
        public bool Signature { get; set; } = true;

        /// <summary>
        /// 是否允许添加水印
        /// 默认为true
        /// 可选
        /// </summary>
        [XmlElement(ElementName = "Watermark", IsNullable = false)]
        public bool Watermark { get; set; } = true;

        /// <summary>
        /// 是否允许截屏
        /// 默认为true
        /// 可选
        /// </summary>
        [XmlElement(ElementName = "PrintScreen", IsNullable = false)]
        public bool PrintScreen { get; set; } = true;

        /// <summary>
        /// 打印权限, 其具体的权限和份数设置由其属性 Printable 及 Copies
        /// 控制, 若不设置 Print 节点, 则默认为可以打印, 并且打印份数不受限制
        /// 可选
        /// </summary>
        [XmlElement(ElementName = "Print", IsNullable = false)]
        public PermissionPrint Print { get; set; }

        /// <summary>
        /// 有效期, 即此文档允许访问的期限, 其具体期限取决于开始日期和
        /// 结束日期, 其中开始日期不能晚于结束日期, 并且开始日期和结束
        /// 日期至少出现一个。 当不设置开始日期时, 代表不限定开始日期,
        /// 当不设置结束日期时代表不限定结束日期; 当此不设置此节点时,
        /// 表示开始日期和结束日期均不受限
        /// 可选
        /// </summary>
        [XmlElement(ElementName = "ValidPeriod", IsNullable = false)]
        public PermissionValidPeriod ValidPeriod { get; set; }


        #region ShouldSerialize 方法 - 控制默认值不序列化
        /// <summary>
        /// 仅当 Edit 不等于默认值 true 时才序列化
        /// </summary>
        public bool ShouldSerializeEdit() => Edit != true;

        /// <summary>
        /// 仅当 Annot 不等于默认值 true 时才序列化
        /// </summary>
        public bool ShouldSerializeAnnot() => Annot != true;

        /// <summary>
        /// 仅当 Export 不等于默认值 true 时才序列化
        /// </summary>
        public bool ShouldSerializeExport() => Export != true;

        /// <summary>
        /// 仅当 Signature 不等于默认值 true 时才序列化
        /// </summary>
        public bool ShouldSerializeSignature() => Signature != true;

        /// <summary>
        /// 仅当 Watermark 不等于默认值 true 时才序列化
        /// </summary>
        public bool ShouldSerializeWatermark() => Watermark != true;

        /// <summary>
        /// 仅当 PrintScreen 不等于默认值 true 时才序列化
        /// </summary>
        public bool ShouldSerializePrintScreen() => PrintScreen != true;
        #endregion
    }
}
