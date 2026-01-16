using System;using System.Xml.Serialization;
using OFDViewer.Models.BaseStructure.Pages;
using OFDViewer.Models.BaseType;

namespace OFDViewer.Models.Annotation
{
    /// <summary>
    /// 注释元素
    /// </summary>
    public class Annot
    {
        /// <summary>
        /// 注释说明内容
        /// 可选
        /// </summary>
        [XmlElement("Remark", IsNullable = true)]
        public string Remark { get; set; }

        /// <summary>
        /// 注释参数集合
        /// Parameters 可选
        /// Parameter 1..n
        /// </summary>
        [XmlArray("Parameters", IsNullable = true)]
        [XmlArrayItem("Parameter", IsNullable = false)]
        public List<Parameter> ParameterList { get; set; } = new List<Parameter>();

        /// <summary>
        /// 控制ParameterList属性是否序列化
        /// </summary>
        public bool ShouldSerializeParameterList()
        {
            // 当ParameterList为空时不序列化
            return ParameterList != null && ParameterList.Count > 0;
        }

        /// <summary>
        /// 注释的静态呈现效果,使用页面块定义来描述
        /// 必选
        /// </summary>
        [XmlElement("Appearance", IsNullable = false)]
        public Appearance Appearance { get; set; }

        /// <summary>
        /// 注释标识
        /// 必选
        /// </summary>
        [XmlAttribute("ID", DataType = "string")]
        public string IDString
        {
            get => ID.ToString();
            set => ID = ST_ID.Parse(value);
        }

        [XmlIgnore]
        public ST_ID ID { get; set; }

        /// <summary>
        /// 注释类型,具体取值请见表62
        /// 必选
        /// </summary>
        [XmlAttribute("Type", DataType = "string")]
        public string TypeString
        {
            get => Type.ToString();
            set => Type = Enum.Parse<AnnotationType>(value);
        }

        [XmlIgnore]
        public AnnotationType Type { get; set; }

        /// <summary>
        /// 注释创建者
        /// 必选
        /// </summary>
        [XmlAttribute("Creator", DataType = "string")]
        public string Creator { get; set; }

        /// <summary>
        /// 最近一次修改的时间
        /// 必选
        /// </summary>
        [XmlAttribute("LastModDate", DataType = "date")]
        public DateTime LastModDate { get; set; }

        /// <summary>
        /// 控制LastModDate属性是否序列化
        /// </summary>
        public bool ShouldSerializeLastModDate()
        {
            // 当LastModDate为默认值DateTime.MinValue时不序列化
            return LastModDate != DateTime.MinValue;
        }

        /// <summary>
        /// 表示该注释对象是否显示
        /// 可选，默认值：true
        /// </summary>
        [XmlAttribute("Visible", DataType = "boolean")]
        public bool Visible { get; set; } = true;

        /// <summary>
        /// 控制Visible属性是否序列化
        /// </summary>
        public bool ShouldSerializeVisible()
        {
            // 当Visible为默认值true时不序列化
            return Visible != true;
        }

        /// <summary>
        /// 注释子类型
        /// 可选
        /// </summary>
        [XmlAttribute("Subtype", DataType = "string")]
        public string Subtype { get; set; }

        /// <summary>
        /// 对象的 Remark 信息是否随页面一起打印
        /// 可选，默认值：true
        /// </summary>
        [XmlAttribute("Print", DataType = "boolean")]
        public bool Print { get; set; } = true;

        /// <summary>
        /// 控制Print属性是否序列化
        /// </summary>
        public bool ShouldSerializePrint()
        {
            // 当Print为默认值true时不序列化
            return Print != true;
        }

        /// <summary>
        /// 对象的 Remark 信息是否不随页面缩放而同步缩放
        /// 可选，默认值：false
        /// </summary>
        [XmlAttribute("NoZoom", DataType = "boolean")]
        public bool NoZoom { get; set; } = false;

        /// <summary>
        /// 控制NoZoom属性是否序列化
        /// </summary>
        public bool ShouldSerializeNoZoom()
        {
            // 当NoZoom为默认值false时不序列化
            return NoZoom != false;
        }

        /// <summary>
        /// 对象的 Remark信息是否不随页面旋转而同步旋转
        /// 可选，默认值：false
        /// </summary>
        [XmlAttribute("NoRotate", DataType = "boolean")]
        public bool NoRotate { get; set; } = false;

        /// <summary>
        /// 控制NoRotate属性是否序列化
        /// </summary>
        public bool ShouldSerializeNoRotate()
        {
            // 当NoRotate为默认值false时不序列化
            return NoRotate != false;
        }

        /// <summary>
        /// 对象的 Remark信息是否不能被用户更改
        /// 可选，默认值：true
        /// </summary>
        [XmlAttribute("ReadOnly", DataType = "boolean")]
        public bool ReadOnly { get; set; } = true;

        /// <summary>
        /// 控制ReadOnly属性是否序列化
        /// </summary>
        public bool ShouldSerializeReadOnly()
        {
            // 当ReadOnly为默认值true时不序列化
            return ReadOnly != true;
        }
    }
}