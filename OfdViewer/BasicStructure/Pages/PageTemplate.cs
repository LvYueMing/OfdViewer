using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using OFDViewer.BaseType;
using OFDViewer.Enums;
using OFDViewer.Utils;

namespace OFDViewer.BasicStructure.Pages
{
    /// <summary>
    /// 对应XSD中的Template元素
    /// </summary>
    public class PageTemplate
    {
        /// <summary>
        /// 引用在文档公用数据(CommonData)中定义的模板页标识 
        /// 必选
        /// </summary>
        [XmlAttribute("TemplateID")]
        public ST_RefID TemplateID { get; set; }

        /// <summary>
        /// 控制模板在页面中的呈现顺序,其类型描述和呈现顺序与 Layer中Type的描述和处理一致如果多个图层的此属性相同, 
        /// 则应根据其出现的顺序来显示, 先出现者先绘制默认值为 Background
        /// 可选
        /// </summary>
        [XmlAttribute("ZOrder")]
        public string ZOrderString
        {
            get => ZOrder.ToString();
            set => ZOrder = EnumHelper.ParseEnum<LayerType>(value);
        }


        [XmlIgnore]
        public LayerType ZOrder { get; set; } = LayerType.Background;

    }
}
