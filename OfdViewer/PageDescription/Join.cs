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
    /// 线条连接样式,指定了两个线的端点结合时采用的样式
    /// 可取值为:
    ///     Miter
    ///     Round
    ///     Bevel
    /// 默认值为 Miter
    /// 线条连接样式的取值和显示效果之间的关系见表22
    /// 可选
    /// </summary>
    public enum Join
    {
        Miter,
        Round,
        Bevel
    }
}
