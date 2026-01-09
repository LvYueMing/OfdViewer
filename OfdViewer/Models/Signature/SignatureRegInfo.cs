using System.Xml.Serialization;
using OFDViewer.Models.BaseType;
using OFDViewer.Utils;

namespace OFDViewer.Models.Signature
{
    /// <summary>
    /// 数字签名或安全签章在列表中的注册信息,一次签名或签章对应一个节点
    /// </summary>
    public class SignatureRegInfo
    {
        /// <summary>
        /// 签名或签章的标识 
        /// 必选
        /// </summary>
        [XmlAttribute(AttributeName = "ID")]// 仅[XmlAttribute("ID")] 标记，即可序列化为XML的 xs:ID类型属性
        [XmlRequired(ErrorMsg = "ID 必选属性为必选项，且不能为空")]
        public string ID { get; set; }

        /// <summary>
        /// 枚举属性Type（忽略XML序列化，默认值为Seal，对应XSD默认值）
        /// </summary>
        [XmlIgnore]
        public SignatureType Type { get; set; } = SignatureType.Seal;

        /// <summary>
        /// 签名节点的类型,目前规定了两个可选值,Seal表示是安全签章,Sign表示是纯数字签名
        /// 可选
        /// </summary>
        [XmlAttribute(AttributeName = "Type")]
        public string TypeString
        {
            get => Type.ToString();
            set => Type = EnumHelper.ParseEnum<SignatureType>(value);
        }

        /// <summary>
        /// 指向包内的签名描述文件 
        /// 必选
        /// </summary>
        [XmlAttribute(AttributeName = "BaseLoc")]
        [XmlRequired(ErrorMsg = "BaseLoc 必选属性为必选项，且不能为空")]
        public string BaseLocString
        {
            get => BaseLoc.ToString();
            set => BaseLoc = value;
        }
        [XmlIgnore]
        public ST_Loc BaseLoc { get; set; }
    }
}
