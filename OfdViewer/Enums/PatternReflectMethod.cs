using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OFDViewer.Enums
{
    /// <summary>
    /// 描述底纹单元的映像翻转方式,枚举值,默认值为 Normal
    /// 图 28 翻转绘制效果
    /// </summary>
    public enum PatternReflectMethod
    {
        [Description("Normal")]
        Normal,
        [Description("Row")]
        Row,
        [Description("Column")]
        Column,
        [Description("RowAndColumn")]
        RowAndColumn
    }
}
