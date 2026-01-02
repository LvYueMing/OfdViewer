using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using OFDViewer.Utils;

namespace OFDViewer.Models.BaseStructure.MainEntry
{
    /// <summary>
    /// 用户自定义元数据, 可以指定一个名称及其对应的值 
    /// 必选
    /// </summary>
    public class CustomData
    {
        // 用户自定义元数据名称 必选
        [XmlAttribute("Name")]
        [XmlRequired(ErrorMsg = "Name 属性为必选项，且不能为空")]
        public string Name { get; set; }

        // type="xs:string"（元素值）
        [XmlText]
        public string Value { get; set; }
    }
}
