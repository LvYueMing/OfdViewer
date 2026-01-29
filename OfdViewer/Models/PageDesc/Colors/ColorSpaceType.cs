namespace OFDViewer.Models.PageDesc.Colors
{
    /// <summary>
    /// 颜色空间的类型,可取值如下:GRAY、RGB、CMYK
    /// BitsPerComponent(简称 BPC) 有效时, 颜色通道值的取值下限是0, 上
    /// 限由BitsPerComponent 决定, 即取区间[0, 2BPC - 1]内的整数, 采用10 进制或16 进制的形式表示, 采用
    /// 16 进制表示时, 应以“# ”加以标识。 当颜色通道的取值超出了相应的区间, 则按照默认颜色来处理。
    /// </summary>
    public enum ColorSpaceType
    {
        /// <summary>
        /// 只包含一个通道来表明灰度值
        /// 例如:" #FF" 、"255"
        /// </summary>
        GRAY,
        /// <summary>
        /// 包含三个通道, 依次是红、 绿、 蓝
        /// 例如:" #11 #22 #33" 、"17 34 51"
        /// </summary>
        RGB,
        /// <summary>
        /// 包含四个通道, 依次是青、 黄、 品红、 黑
        /// 例如:" #11 #22 #33 #44" 、"17 34 51 68"
        /// </summary>
        CMYK
    }
}
