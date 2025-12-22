using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OFDViewer.Enums
{
    /// <summary>
    /// 底纹单元起始绘制位置,可取值如下
    ///  Page:相对于页面坐标系的原点
    ///  Object:相对于对象坐标系的原点
    /// 默认值为 Object
    /// </summary>
    public enum PatternRelativeTo
    {
        [Description("Page")]
        Page,
        [Description("Object")]
        Object
    }
}
