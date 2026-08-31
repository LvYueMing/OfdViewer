using System;
using System.Xml.Serialization;
using OFDViewer.Models.BaseType;
using OFDViewer.Utils;

namespace OFDViewer.Models.Attachment
{
    /// <summary>
    /// 附件元素
    /// </summary>
    public class CT_Attachment
    {
        /// <summary>
        /// 文件位置
        /// 必选
        /// </summary>
        [XmlElement("FileLoc", IsNullable = false)]
        [XmlRequired(ErrorMsg = "FileLoc 必选元素为必选项，且不能为空")]
        public string FileLocString
        {
            get => FileLoc.ToString();
            set => FileLoc = value;
        }

        [XmlIgnore]
        public ST_Loc FileLoc { get; set; }

        /// <summary>
        /// 附件标识
        /// 全局唯一的标识，相当于给这个节点贴一个 “身份证号”，确保在整个 XML 文档中不会重复
        /// 必选
        /// </summary>
        [XmlAttribute("ID", DataType = "string", AttributeName = "ID")]
        [XmlRequired(ErrorMsg = "ID 必选属性为必选项，且不能为空")]
        public string IDString
        {
            get => ID.ToString();
            set => ID = ST_ID.Parse(value);
        }

        [XmlIgnore]
        public ST_ID ID { get; set; }

        /// <summary>
        /// 附件名称
        /// 必选
        /// </summary>
        [XmlAttribute("Name", DataType = "string", AttributeName = "Name")]
        [XmlRequired(ErrorMsg = "Name 必选属性为必选项，且不能为空")]
        public string Name { get; set; }

        /// <summary>
        /// 附件格式
        /// 可选
        /// </summary>
        [XmlAttribute("Format", DataType = "string", AttributeName = "Format")]
        public string Format { get; set; }

        /// <summary>
        /// 创建日期
        /// 可选
        /// 兼容：部分文档该属性取空值或非标准日期格式（如 PDF 日期 D:...），需容错解析
        /// </summary>
        [XmlAttribute("CreationDate")]
        public string CreationDateString
        {
            get => CreationDate.ToString("s");
            set => CreationDate = DateParser.TryParse(value, out var parsed) ? parsed : DateTime.MinValue;
        }
        [XmlIgnore]
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// 控制CreationDate属性是否序列化
        /// </summary>
        public bool ShouldSerializeCreationDate()
        {
            // 当CreationDate为默认值DateTime.MinValue时不序列化
            return CreationDate != DateTime.MinValue;
        }

        /// <summary>
        /// 修改日期
        /// 可选
        /// 兼容：部分文档该属性取空值或非标准日期格式（如 PDF 日期 D:...），需容错解析
        /// </summary>
        [XmlAttribute("ModDate")]
        public string ModDateString
        {
            get => ModDate.ToString("s");
            set => ModDate = DateParser.TryParse(value, out var parsed) ? parsed : DateTime.MinValue;
        }
        [XmlIgnore]
        public DateTime ModDate { get; set; }

        /// <summary>
        /// 控制ModDate属性是否序列化
        /// </summary>
        public bool ShouldSerializeModDate()
        {
            // 当ModDate为默认值DateTime.MinValue时不序列化
            return ModDate != DateTime.MinValue;
        }

        /// <summary>
        /// 附件大小, 以 KB 为单位
        /// 可选
        /// </summary>
        [XmlAttribute("Size", DataType = "double", AttributeName = "Size")]
        public double Size { get; set; }

        /// <summary>
        /// 控制Size属性是否序列化
        /// </summary>
        public bool ShouldSerializeSize()
        {
            // 当Size为默认值0时不序列化
            return Size != 0;
        }

        /// <summary>
        /// 附件是否可见
        /// 可选，默认值为true
        /// </summary>
        [XmlAttribute("Visible", DataType = "boolean", AttributeName = "Visible")]
        public bool Visible
        {
            get { return _visible; }
            set { _visible = value; }
        }
        private bool _visible = true;

        /// <summary>
        /// 控制Visible属性是否序列化
        /// </summary>
        public bool ShouldSerializeVisible()
        {
            // 当Visible为默认值true时不序列化
            return Visible != true;
        }

        /// <summary>
        /// 附件用途
        /// 可选，默认值为none
        /// </summary>
        [XmlAttribute("Usage", DataType = "string", AttributeName = "Usage")]
        public string Usage
        {
            get { return _usage; }
            set { _usage = value; }
        }
        private string _usage = "none";

        /// <summary>
        /// 控制Usage属性是否序列化
        /// </summary>
        public bool ShouldSerializeUsage()
        {
            // 当Usage为默认值"none"时不序列化
            return Usage != "none";
        }

        // 无参构造函数，初始化必选属性
        public CT_Attachment()
        {
            ID = ST_ID.CreateNew();
            FileLoc = new ST_Loc();
        }
    }
}