using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using OFDViewer.OFDEnum;
using OFDViewer.Utils;

namespace OFDViewer.Actions
{
    /// <summary>
    /// 目标区域
    /// </summary>
    public class CT_Dest
    {
        /// <summary>
        /// 声明目标区域的描述方法,可取值列举如下:
        ///XYZ———目标区域由左上角位置(Left, Top)以及页面缩放比例
        ///(Zoom)确定;
        ///Fit———适合整个窗口区域;
        ///FitH———适合窗口宽度,目标区域仅由 Top确定;
        ///FitV———适合窗口高度,目标区域仅由 Left确定;
        ///FitR———适合窗口内的目标区域,目标区域为(Left、Top、Right、Bottom)所确定的矩形区域
        ///必选
        /// </summary>
        [XmlAttribute("Type")]
        public string TypeString
        {
            get => Type.ToString();
            set => Type = EnumHelper.ParseEnum<DestType>(value);
        }
        [XmlIgnore]
        public DestType Type { get; set; }

        /// <summary>
        /// 引用跳转目标页面的标识 必选
        /// </summary>
        [XmlAttribute("PageID")]
        public string PageID { get; set; } // 假设ST_RefID对应string类型，可根据实际定义调整

        /// <summary>
        /// 目标区域左上角x 坐标默认为0
        ///可选
        /// </summary>
        [XmlAttribute("Left")]
        public double Left { get; set; } = 0;


        /// <summary>
        /// 目标区域左上角y 坐标
        /// 默认为0
        /// 可选
        /// </summary>
        [XmlAttribute("Top")]
        public double Top { get; set; } = 0;



        /// <summary>
        /// 目标区域右下角x 坐标 可选
        /// </summary>
        [XmlAttribute("Right")]
        public double Right { get; set; }


        /// <summary>
        /// 目标区域右下角y 坐标 可选
        /// </summary>
        [XmlAttribute("Bottom")]
        public double Bottom { get; set; }



        /// <summary>
        /// 目标区域页面缩放比例,为0或不出现则按照当前缩放比例跳转,可取值范围[0.1,64.0]
        /// 可选
        /// </summary>
        [XmlAttribute("Zoom")]
        public double Zoom { get; set; }


        /// <summary>
        /// 标识Left属性是否序列化（处理可选属性）
        /// </summary>
        [XmlIgnore]
        public bool LeftSpecified { get; set; }

        /// <summary>
        /// 标识Top属性是否序列化（处理可选属性）
        /// </summary>
        [XmlIgnore]
        public bool TopSpecified { get; set; }

        /// <summary>
        /// 标识Right属性是否序列化（处理可选属性）
        /// </summary>
        [XmlIgnore]
        public bool RightSpecified { get; set; }

        /// <summary>
        /// 标识Bottom属性是否序列化（处理可选属性）
        /// </summary>
        [XmlIgnore]
        public bool BottomSpecified { get; set; }

        /// <summary>
        /// 标识Zoom属性是否序列化（处理可选属性）
        /// </summary>
        [XmlIgnore]
        public bool ZoomSpecified { get; set; }
    }
}
