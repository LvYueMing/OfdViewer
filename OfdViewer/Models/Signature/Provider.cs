using System.Xml.Serialization;
using OFDViewer.Utils;

namespace OFDViewer.Models.Signature
{
    /// <summary>
    /// 创建签名时所用的签章组件提供者信息
    /// </summary>
    public class Provider
    {
        /// <summary>
        /// 创建签名时所用的签章组件的提供者名称 
        /// 必选
        /// </summary>
        [XmlAttribute("ProviderName")]
        [XmlRequired(errorMsg: "ProviderName为必选项，且不能为空")]
        public string ProviderName { get; set; }

        /// <summary>
        /// 创建签名时所用的签章组件的版本
        /// 可选
        /// </summary>
        [XmlAttribute("Version")]
        public string Version { get; set; }

        /// <summary>
        /// 创建签名时所用的签章组件的制造商 
        /// 可选
        /// </summary>
        [XmlAttribute("Company")]
        public string Company { get; set; }
    }
}
