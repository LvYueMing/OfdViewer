using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OFDViewer.Graph.Shape
{
    // Choice元素类型枚举
    public enum ShapeTypeEnum
    {
        // 移动
        Move,
        //线段
        Line,
        // 二阶贝塞尔曲线
        QuadraticBezier,
        // 三阶贝塞尔曲线
        CubicBezier,
        // 圆弧
        Arc,
        // 关闭
        Close
    }
}
