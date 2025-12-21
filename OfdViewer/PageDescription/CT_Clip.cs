using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OFDViewer.PageDescription
{
    /// <summary>
    /// 裁剪区
    /// <para>裁剪区由一组路径或文字构成,用以指定页面上的一个有效绘制区域,落在裁剪区以外的部分不受绘制指令的影响。</para>
    /// <para>一个裁剪区可由多个分路径(Area)组成,最终的裁剪范围是各个分路径的并集。裁剪区中的数据均相对于所修饰图元对象的外接矩形。</para>
    /// </summary>
    public class CT_Clip
    {
        /// <summary>
        ///裁剪区域,用一个图形对象或文字对象来描述裁剪区的一个组成部分,最终裁剪区是这些区域的并集
        ///必选
        /// </summary>
        [XmlElement("Area")]
        public List<ClipArea> Areas { get; set; }
    }
}
