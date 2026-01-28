using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OFDViewer.Parse
{
    /// <summary>
    /// 资源获取位置枚举
    /// </summary>
    public enum ResourceLocation
    {
        /// <summary>
        /// 模板级资源
        /// </summary>       
        Template,
        /// <summary>
        /// 页面级资源
        /// </summary>
        Page,
        /// <summary>
        /// 文档级资源
        /// </summary>
        Document,
        /// <summary>
        /// 公共级资源
        /// </summary>
        Public,
        /// <summary>
        /// 自动搜索（Page -> Document -> Public）
        /// </summary>
        Auto
    }
}
