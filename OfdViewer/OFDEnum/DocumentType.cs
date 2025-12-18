using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OFDViewer.OFDEnum
{
    /// <summary>
    /// 文件格式子集类型
    /// </summary>
    public enum DocumentType
    {
        /// <summary>
        /// 符合当前《GB/T 33190-2016 电子文件存储与交换格式版式文档》标准
        /// </summary>
        [Description("OFD")]
        OFD = 0,
        /// <summary>
        /// 符合OFD存档规范
        /// </summary>
        [Description("OFD-A")]
        OFD_A = 1,
        /// <summary>
        /// 电子病历版式文档
        /// </summary>
        [Description("OFD-H")] 
        OFD_H = 2
    }
}
