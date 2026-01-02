using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using OFDViewer.Utils;
using OFDViewer.Models.BaseType;

namespace OFDViewer.Models.BasicStructure.DocumentRoot
{
    /// <summary>
    /// 页面区域结构 图 7
    /// </summary>
    public class CT_PageArea
    {
        /// <summary>
        /// 物理区域
        /// 页面物理区域,左上角的坐标为页面空间坐标系的原点 
        /// 必选
        /// </summary>
        [XmlElement("PhysicalBox")]
        [XmlRequired(ErrorMsg = "物理区域为必选属性，不能为空")]
        public string PhysicalBoxString
        {
            get => PhysicalBox.ToString();
            set => PhysicalBox = ST_Box.Parse(value);
        }

        [XmlIgnore]
        public ST_Box PhysicalBox { get; set; }

        /// <summary>
        /// 显示区域
        /// 页面内容实际显示或打印输出的区域,位于页面物理区域内,包含页眉、页脚、版心等内容
        /// [例外处理] 如果显示区域不完全位于页面物理区域内, 页面物理区域外的部分则被忽略。如果显示
        /// 区域完全位于页面物理区域外,则该页为空白页
        /// 可选
        /// </summary>
        [XmlElement("ApplicationBox")]
        public string ApplicationBoxString
        {
            get => ApplicationBox.IsValid ? ApplicationBox.ToString() : null;
            set => ApplicationBox = ST_Box.Parse(value);
        }


        [XmlIgnore]
        public ST_Box ApplicationBox { get; set; } = ST_Box.InvalidValue;

        /// <summary>
        /// 版心区域
        /// 即文件的正文区域,位于显示区域内。左上角的坐标决定了其在显示区域内的位置
        /// [例外处理] 如果版心区域不完全位于显示区域内, 显示区域外的部分则被忽略。如果版心区域完全
        /// 位于显示区域外,则版心内容不被绘制
        /// 可选
        /// </summary>
        [XmlElement("ContentBox")]
        public string ContentBoxString
        {
            get => ContentBox.IsValid ? ContentBox.ToString() : null;
            set => ContentBox = ST_Box.Parse(value);
        }

        [XmlIgnore]
        public ST_Box ContentBox { get; set; } = ST_Box.InvalidValue;

        /// <summary>
        /// 出血区域
        /// 即超出设备性能限制的额外出血区域,位于页面物理区域外。不出现时,默认值为页面物理区域
        /// [例外处理] 如果出血区域不完全位于页面物理区域外, 页面物理区域内的部分则被忽略。如果出血区域
        /// 完全位于页面物理区域内,出血区域无效
        /// 可选
        /// </summary>
        [XmlElement("BleedBox")]
        public string BleedBoxString
        {
            get => BleedBox.IsValid ? BleedBox.ToString() : null;
            set => BleedBox = ST_Box.Parse(value);
        }

        [XmlIgnore]
        public ST_Box BleedBox { get; set; } = ST_Box.InvalidValue;


        /// <summary>
        /// 无参构造函数，必选属性初始化
        /// </summary>
        public CT_PageArea()
        {
            // A4纸张大小，单位：点
            PhysicalBox = new ST_Box(0, 0, 595.32, 841.92);
        }


    }
}
