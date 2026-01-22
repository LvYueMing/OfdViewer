using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OFDViewer.Parse
{
    /// <summary>
    /// 资源类型枚举
    /// </summary>
    public enum ResourceType
    {
        /// <summary>
        /// 字型
        /// </summary>
        Font,
        /// <summary>
        /// 颜色空间
        /// </summary>
        ColorSpace,
        /// <summary>
        /// 绘制参数
        /// </summary>
        DrawParam,
        /// <summary>
        /// 矢量图像
        /// </summary>
        VectorGraphic,
        /// <summary>
        /// 多媒体
        /// </summary>
        Multimedia,
        /// <summary>
        /// 所有类型
        /// </summary>
        All
    }
}
