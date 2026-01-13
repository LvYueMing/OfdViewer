using System;
using System.Xml.Serialization;
using OFDViewer.Models.BaseType;
using OFDViewer.Models.Extension.ExtensionItems;

namespace OFDViewer.Models.Extension
{
    /// <summary>
    /// 扩展元素基类
    /// </summary>
    [XmlInclude(typeof(Property))]
    [XmlInclude(typeof(Data))]
    [XmlInclude(typeof(ExtendData))]
    public abstract class ExtensionItem
    {
    }
}