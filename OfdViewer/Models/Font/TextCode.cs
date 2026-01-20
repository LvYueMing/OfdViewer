using System.Xml.Serialization;
using OFDViewer.Models.BaseType;

namespace OFDViewer.Models.Font
{
    /// <summary>
    /// 文字定位
    /// 文字对象使用严格的文字定位信息进行定位, 文字定位结构如图 61 所示
    /// </summary>
    public class TextCode
    {
        /// <summary>
        /// 第一个文字的字型原点在对象坐标系下的 X 坐标
        /// 当 X 不出现, 则采用上一个 TextCode 的 X 值, 文字对象中的第一个 TextCode 的 X 属性必选
        /// 可选
        /// </summary>
        [XmlAttribute(AttributeName = "X")]
        public double X { get; set; }

        /// <summary>
        /// 第一个文字的字型原点在对象坐标系下的Y 坐标
        /// 当Y 不出现, 则采用上一个 TextCode 的Y 值, 文字对象中的第一个TextCode 的Y 属性必选
        /// 可选
        /// </summary>
        [XmlAttribute(AttributeName = "Y")]
        public double Y { get; set; }

        /// <summary>
        /// double 型数值队列, 队列中的每个值代表后一个文字与前一个文字之间在 X 方向的偏移值
        /// DeltaX 不出现时, 表示文字的绘制点在 X 方向不做偏移
        /// 可选
        /// </summary>
        [XmlAttribute(AttributeName = "DeltaX")]
        public string DeltaXString
        {
            get => DeltaX.ToString();
            set => DeltaX = ST_Array.Parse(value);
        }
        [XmlIgnore]
        public ST_Array DeltaX { get; set; }

        /// <summary>
        /// 控制 DeltaX 属性是否序列化
        /// </summary>
        public bool ShouldSerializeDeltaYString()
        {
            return DeltaY != null && DeltaY.Count > 0;
        }

        /// <summary>
        /// double 型数值队列, 队列中的每个值代表后一个文字与前一个文字之间在Y 方向的偏移值
        /// DeltaY 不出现时, 表示文字的绘制点在 Y 方向不做偏移
        /// 可选
        /// </summary>
        [XmlAttribute(AttributeName = "DeltaY")]
        public string DeltaYString
        {
            get => DeltaY.ToString();
            set => DeltaY = ST_Array.Parse(value);
        }
        [XmlIgnore]
        public ST_Array DeltaY { get; set; }

        /// <summary>
        /// 控制 DeltaY 属性是否序列化
        /// </summary>
        /// <returns></returns>
        public bool ShouldSerializeDeltaXString()
        {
            return DeltaX != null && DeltaX.Count > 0;
        }


        // TextCode 的文本内容（xs:string 基类型）
        [XmlText]
        public string Text { get; set; }
    }
}
