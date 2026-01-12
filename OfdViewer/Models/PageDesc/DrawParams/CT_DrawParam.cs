using System.Xml.Serialization;
using OFDViewer.Models.BaseType;
using OFDViewer.Models.PageDesc.Colors;
using OFDViewer.Utils;

namespace OFDViewer.Models.PageDesc.DrawParams
{
    /// <summary>
    /// 绘制参数
    /// 绘制参数是一组用于控制绘制渲染效果的修饰参数的集合。绘制参数可以被不同的图元对象所共享。
    /// 绘制参数可以继承已有的绘制参数,被继承的绘制参数称为该参数的“基础绘制参数”。绘制参数结构如图22所示。
    /// </summary>
    public class CT_DrawParam
    {
        /// <summary>
        /// 填充颜色,用以填充路径形成的区域以及文字轮廓内的区域,默认值为透明色。关于颜色的描述见8.3
        /// 可选
        /// </summary>
        [XmlElement("FillColor")]
        public CT_Color FillColor { get; set; }

        /// <summary>
        /// 勾边颜色,指定路径绘制的颜色以及文字轮廓的颜色,默认值为黑色。颜色的描述见8.3
        /// 可选
        /// </summary>
        [XmlElement("StrokeColor")]
        public CT_Color StrokeColor { get; set; }

        /// <summary>
        /// 基础绘制参数,引用资源文件中的绘制参数的标识 
        /// 可选
        /// </summary>
        [XmlAttribute("Relative")]
        public string RelativeString
        {
            get => Relative.ToString(); 
            set => Relative = ST_RefID.Parse(value);
        }
            [XmlIgnore]
        public ST_RefID Relative { get; set; }

        /// <summary>
        /// 线宽,非负浮点数,指定了路径绘制时线的宽度。由于某些设备不
        /// 能输出一个像素宽度的线,因此强制规定当线宽大于0时,无论多
        /// 小都最少要绘制两个像素的宽度; 当线宽为0时,绘制一个像素的
        /// 宽度。由于线宽0的定义与设备相关,所以不推荐使用线宽0。
        /// 默认值为0.353mm
        /// 可选
        /// </summary>
        [XmlAttribute("LineWidth")]
        public double LineWidth { get; set; } = 0.353;

        /// <summary>
        /// 线条连接样式,指定了两个线的端点结合时采用的样式 可取值为:
        ///  Miter
        ///  Round
        ///  Bevel
        /// 默认值为 Miter
        /// 线条连接样式的取值和显示效果之间的关系见表22
        /// 可选
        /// </summary>
        [XmlAttribute("Join")]
        public string JoinString
        {
            get => Join.ToString();
            set => Join = EnumHelper.ParseEnum<DrawParamJoinType>(value);
        }
        [XmlIgnore]
        public DrawParamJoinType Join { get; set; } = DrawParamJoinType.Miter;

        /// <summary>
        /// 线端点样式,枚举值,指定了一条线的端点样式。 可取值为:
        ///   Butt
        ///   Round
        ///   Square
        /// 默认值为 Butt
        /// 线条端点样式取值与效果之间关系见表24
        /// 可选
        /// </summary>
        [XmlAttribute("Cap")]
        public string CapString
        {
            get => Cap.ToString();
            set => Cap = EnumHelper.ParseEnum<DrawParamCapType>(value);
        }
        [XmlIgnore]
        public DrawParamCapType Cap { get; set; }

        /// <summary>
        /// 线条虚线样式开始的位置,默认值为0。当 DashPattern不出现时,该参数无效
        /// 可选
        /// </summary>
        [XmlAttribute("DashOffset")]
        public double DashOffset { get; set; } = 0;

        /// <summary>
        /// 线条虚线的重复样式,数组中共含两个值,第一个值代表虚线线段
        /// 的长度,第二个值代表虚线间隔的长度。默认值为空。线条虚线样式的控制效果见表23
        ///可选
        /// </summary>
        [XmlAttribute("DashPattern")]
        public string DashPatternString

        {
            get => DashPattern.ToString();
            set => DashPattern = ST_Array.Parse(value);
        }
        [XmlIgnore]
        public ST_Array DashPattern { get; set; }

        /// <summary>
        /// Join为 Miter时小角度结合点长度的截断值,默认值为 3.528。当Join不等于 Miter时该参数无效
        ///可选
        /// </summary>
        [XmlAttribute("MiterLimit")]
        public double MiterLimit { get; set; } = 4.234;
    }
}
