using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OFDViewer.BasicStructure.DocumentRoot
{
    /// <summary>
    /// 页面布局模式, 可取值如下:
    ///   OnePage———单页模式
    ///   OneColumn———单列模式
    ///   TwoPageL———对开模式
    ///   TwoColumnL———对开连续模式
    ///   TwoPageR———对开靠右模式
    ///   TwoColumnR———对开连续靠右模式
    ///  默认值为 OneColumn
    /// </summary>
    public enum VPreferencesPageLayout
    {
        OnePage,
        OneColumn,
        TwoPageL,
        TwoColumnL,
        TwoPageR,
        TwoColumnR
    }
}
