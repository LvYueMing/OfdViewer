using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OFDViewer.BaseType;
using System.Xml.Serialization;
using OFDViewer.Enums;
using OFDViewer.Utils;

namespace OFDViewer.BasicStructure.Pages
{
    /// <summary>
    /// 模板页结构
    /// </summary>
    public class CT_TemplatePage
    {
        /// <summary>
        /// 模板页的标识,不能与已有标识重复 
        /// 必选
        /// </summary>
        [XmlAttribute("ID")] 
        public ST_ID ID { get; set; }

        /// <summary>
        /// 模板页名称 
        /// 可选
        /// </summary>
        [XmlAttribute("Name")]
        public string Name { get; set; }

        /// <summary>
        /// 模板页的默认图层类型,其类型描述和呈现顺序与 Layer中 Type的描述和处理一致,见表15
        /// 如果页面引用的多个模板的此属性相同, 则应根据引用的顺序来显示,先引用者先绘制默认值为 Background
        /// 可选
        /// </summary>
        [XmlAttribute("ZOrder")]
        public string ZOrderString
        {
            get => ZOrder.ToString();
            set => ZOrder = EnumHelper.ParseEnum<LayerType>(value);
        }
        public LayerType ZOrder { get; set; }= LayerType.Background;

        /// <summary>
        /// 指向模板页内容描述文件 
        /// 必选 IsNullable=false 体现required约束
        /// </summary>
        [XmlAttribute("BaseLoc")]
        public string BaseLocPath
        {
            get => BaseLoc.ToString();
            set => BaseLoc = value;
        }

        [XmlIgnore]
        public ST_Loc BaseLoc { get; set; }
    }
}
