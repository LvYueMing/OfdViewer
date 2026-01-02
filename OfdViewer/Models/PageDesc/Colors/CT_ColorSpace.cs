using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OFDViewer.Utils;
using System.Xml.Serialization;
using OFDViewer.Models.BaseType;

namespace OFDViewer.Models.PageDesc.Colors
{
    /// <summary>
    /// 颜色空间
    /// 本标准支持 GRAY、RGB、CMYK 颜色空间。除通过设置各通道值使用颜色空间内的任意颜色之
    /// 外,还可在颜色空间内定义调色板或指定相应的颜色配置文件,通过设置索引值进行引用。颜色空间结构如图24所示。
    /// </summary>
    public class CT_ColorSpace
    {
        /// <summary>
        /// 调色板 
        /// 可选
        /// </summary>
        /// <remarks="调色板">
        /// CV
        /// 调色板中预定义颜色
        /// 调色板中颜色的索引编号从0开始
        /// 必选
        /// </remarks>
        [XmlArray("Palette")]
        [XmlArrayItem("CV")]
        public List<ST_Array> Palette { get; set; }

        // 枚举属性 Type
        [XmlIgnore]
        public ColorSpaceType Type { get; set; }

        /// <summary>
        /// 颜色空间的类型,可取值如下:GRAY、RGB、CMYK 必选
        /// </summary>
        [XmlAttribute("Type")]
        [XmlRequired(ErrorMsg = "Type 必选属性为必选项，且不能为空")]
        public string TypeString
        {
            get => Type.ToString();
            set => Type = EnumHelper.ParseEnum<ColorSpaceType>(value);
        }

        /// <summary>
        /// 每个颜色通道所使用的位数 有效取值为:1,2,4,8,16
        /// 默认值为8
        /// 可选
        /// </summary>
        [XmlAttribute("BitsPerComponent")]
        public int BitsPerComponent { get; set; } = 8;


        [XmlIgnore]
        public ST_Loc Profile { get; set; }

        /// <summary>
        /// 指向包内颜色配置文件 
        /// 可选
        /// </summary>
        [XmlAttribute("Profile")]
        public string ProfileString
        {
            get => Profile.ToString();
            set => Profile = value;
        }


    }
}
