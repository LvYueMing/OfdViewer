using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OFDViewer.Enums
{
    /// <summary>
    /// 声明目标区域的描述方法,可取值列举如下:
    /// XYZ———目标区域由左上角位置(Left, Top)以及页面缩放比例(Zoom)确定;
    /// Fit———适合整个窗口区域;
    /// FitH———适合窗口宽度,目标区域仅由 Top确定;
    /// FitV———适合窗口高度,目标区域仅由 Left确定;
    /// FitR———适合窗口内的目标区域,目标区域为(Left、Top、Right、Bottom)所确定的矩形区域
    /// </summary>
    public enum DestType
    {
        /// <summary>
        /// 目标区域由左上角位置(Left, Top)以及页面缩放比例(Zoom)确定
        /// </summary>
        [Description("XYZ")]
        XYZ,

        /// <summary>
        /// 适合整个窗口区域
        /// </summary>
        [Description("XYZ")]
        Fit,

        /// <summary>
        /// 适合窗口宽度,目标区域仅由 Top确定
        /// </summary>
        [Description("XYZ")]
        FitH,

        /// <summary>
        /// 适合窗口高度,目标区域仅由 Left确定
        /// </summary>
        [Description("XYZ")]
        FitV,

        /// <summary>
        /// 适合窗口内的目标区域,目标区域为(Left、Top、Right、Bottom)所确定的矩形区域
        /// </summary>
        [Description("XYZ")]
        FitR
    }
}
