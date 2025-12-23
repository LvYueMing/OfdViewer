using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OFDViewer.Enums
{
    /// <summary>
    /// 径向延长线方向是否继续绘制渐变。 可选值为0、1、2、3
    /// 0: 不向圆心联线两侧继续绘制渐变
    /// 1: 在结束点椭圆至起始点椭圆延长线方向绘制渐变
    /// 2: 在起始点椭圆至结束点椭圆延长线方向绘制渐变
    /// 3: 向两侧延长线方向绘制渐变
    /// </summary>
    public enum RadialShdExtend
    {
        [Description("0")]
        Value0,
        [Description("1")]
        Value1,
        [Description("2")]
        Value2,
        [Description("3")]
        Value3
    }
}
