using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OFDViewer.Models.Font
{
    public enum FontCharset
    {
        [Description("symbol")]
        symbol,
        [Description("prc")]
        prc,
        [Description("big5")]
        big5,
        [Description("shift-jis")]
        shift_jis,
        [Description("wansung")]
        wansung,
        [Description("johab")]
        johab,
        [Description("unicode")]
        unicode
    }
}
