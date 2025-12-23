using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OFDViewer.PageDescription
{
    /// <summary>
    /// 线端点样式,枚举值,指定了一条线的端点样式。
    /// 可取值为:
    /// Butt
    /// Round
    /// Square
    /// 默认值为 Butt
    /// 线条端点样式取值与效果之间关系见表24
    /// </summary>
    public enum Cap
    {
        Butt,
        Round,
        Square
    }
}
