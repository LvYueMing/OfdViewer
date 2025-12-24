using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OFDViewer.BasicStructure.DocumentRoot
{
    /// <summary>
    /// 标题栏显示模式, 可取值如下:
    ///   FileName———文件名称
    ///   DocTitle———呈现元数据中的 Title 属性
    ///  默认值为 FileName, 当设置为 DocTitle 但不存在 Title 属性时, 按照FileName 处理
    /// </summary>
    public enum VPreferencesTabDisplay
    {
        DocTitle,
        FileName
    }
}
