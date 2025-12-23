using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OFDViewer.BaseType;
using System.Xml.Serialization;
using OFDViewer.Enums;
using OFDViewer.Utils;

namespace OFDViewer.PageDescription.Colors.ColorItems
{
    /// <summary>
    /// 底纹
    /// 底纹是复杂颜色的一种,用于图形和文字的填充以及勾边处理。底纹结构如图26所示。
    /// </summary>
    public class CT_Pattern
    {
        /// <summary>
        /// 底纹单元,用底纹填充目标区域时,所使用的单元对象 必选
        /// </summary>
        [XmlElement("CellContent", IsNullable = false)]
        public PatternCellContent CellContent { get; set; }

        /// <summary>
        /// 底纹单元的宽度 必选
        /// </summary>
        [XmlAttribute("Width")]
        public double Width { get; set; }

        /// <summary>
        /// 底纹单元的高度 必选
        /// </summary>
        [XmlAttribute("Height")]
        public double Height { get; set; }

        /// <summary>
        /// X 方向底纹单元间距,默认值为底纹单元的宽度。若设定值小于底
        /// 纹单元的宽度时,应按默认值处理
        /// 可选
        /// </summary>
        [XmlAttribute("XStep")]
        public double XStep { get; set; }

        // 标记XStep是否序列化（解决可选值的默认值问题）
        [XmlIgnore]
        public bool XStepSpecified { get; set; }

        /// <summary>
        /// Y 方向底纹单元间距,默认值底纹单元的高度。若设定值小于底纹
        /// 单元的高度时,应按默认值处理
        /// 可选
        /// </summary>
        [XmlAttribute("YStep")]
        public double YStep { get; set; }
        // 标记YStep是否序列化
        [XmlIgnore]
        public bool YStepSpecified { get; set; }

        /// <summary>
        /// 描述底纹单元的映像翻转方式,枚举值,默认值为 Normal 可选
        /// </summary>
        [XmlAttribute("ReflectMethod")]
        public string ReflectMethodString
        {
            get => ReflectMethod.ToString();
            set => ReflectMethod = EnumHelper.ParseEnum<PatternReflectMethod>(value);
        }

        [XmlIgnore]
        public PatternReflectMethod ReflectMethod { get; set; }


        /// <summary>
        /// 底纹单元起始绘制位置,可取值如下
        /// Page:相对于页面坐标系的原点
        /// Object:相对于对象坐标系的原点
        /// 默认值为 Object
        /// 可选
        /// </summary>
        [XmlAttribute("RelativeTo")]
        public string RelativeToString
        {
            get=> RelativeTo.ToString();
            set=> RelativeTo = EnumHelper.ParseEnum<PatternRelativeTo>(value);
        }

        [XmlIgnore]
        public PatternRelativeTo RelativeTo { get; set; }


        /// <summary>
        /// 底纹单元的变换矩阵,用于某些需要对底纹单元进行平移旋转变换
        /// 的场合,默认为单位矩阵;底纹呈现时先做 XStep、YStep排列,然后
        /// 一起做 CTM 处理
        /// 可选
        /// </summary>
        [XmlAttribute("CTM")]
        public string CTMString
        {
            get => CTM.ToString();
            set => CTM = ST_Array.Parse(value);
        }

        [XmlIgnore]
        public ST_Array CTM { get; set; }
    }
}
