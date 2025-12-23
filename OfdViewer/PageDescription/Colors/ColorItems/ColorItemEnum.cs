using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OFDViewer.PageDescription.Colors.ColorItems
{
    /// <summary>
    /// 颜色结构
    /// 本标准中定义的颜色是一个广义的概念,包括基本颜色、底纹和渐变,颜色结构如图25所示。
    /// </summary>
    public enum ColorItemEnum
    {
        None,       // 无元素选中（对应minOccurs="0"）
        Pattern,
        AxialShd,
        RadialShd,
        GouraudShd,
        LaGourandShd
    }
}
