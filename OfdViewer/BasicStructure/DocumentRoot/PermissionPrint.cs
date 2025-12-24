using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OFDViewer.BasicStructure.DocumentRoot
{
    /// <summary>
    /// 打印权限
    /// 其具体的权限和份数设置由其属性 Printable 及 Copies
    /// 控制, 若不设置 Print 节点, 则默认为可以打印, 并且打印份数不受限制
    /// </summary>
    public class PermissionPrint
    {
        /// <summary>
        /// 文档是否允许被打印
        /// 默认为true
        /// 可选
        /// </summary>
        [XmlAttribute(AttributeName = "Printable")]
        public bool Printable { get; set; }

        /// <summary>
        /// 打印份数, 在 Printable 为true 时有效, 若 Printable 为true 并且不设
        /// 置 Copies 则打印份数不受限, 若 Copies 的值为负值时, 打印份数不
        /// 受限, 当 Copies 的值为0 时, 不允许打印, 当 Copies 的值大于0 时,
        /// 则代表实际可打印的份数值
        /// 可选
        /// </summary>
        [XmlAttribute(AttributeName = "Copies")]
        public int Copies { get; set; } = -1;

        /// <summary>
        /// 仅当 Copies 不等于默认值 -1 时才序列化
        /// </summary>
        public bool ShouldSerializeCopies() => Copies != -1;
    }
}
