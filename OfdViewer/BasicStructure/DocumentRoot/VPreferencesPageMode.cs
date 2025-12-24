using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OFDViewer.BasicStructure.DocumentRoot
{
    /// <summary>
    /// 窗口模式, 可取值如下:
    ///   None———常规模式
    ///   FullScreen———打开后全文显示
    ///   UseOutlines———同时呈现文档大纲
    ///   UseThumbs———同时呈现缩略图
    ///   UseCustomTags———同时呈现语义结构
    ///   UseLayers———同时呈现图层
    ///   UseAttatchs———同时呈现附件
    ///   UseBookmarks———同时呈现书签
    /// 默认值为 None
    /// </summary>
    public enum VPreferencesPageMode
    {
        None,
        FullScreen,
        UseOutlines,
        UseThumbs,
        UseCustomTags,
        UseLayers,
        UseAttatchs,
        UseBookmarks
    }
}
