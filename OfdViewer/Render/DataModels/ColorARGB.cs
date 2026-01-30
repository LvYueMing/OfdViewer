using System;

namespace OFDViewer.Render.DataModels
{
    /// <summary>
    /// ARGB颜色结构体
    /// 用于存储和操作ARGB颜色值
    /// </summary>
    public struct ColorARGB
    {
        /// <summary>
        /// Alpha通道值（0-255）
        /// </summary>
        public byte A { get; set; }

        /// <summary>
        /// 红色通道值（0-255）
        /// </summary>
        public byte R { get; set; }

        /// <summary>
        /// 绿色通道值（0-255）
        /// </summary>
        public byte G { get; set; }

        /// <summary>
        /// 蓝色通道值（0-255）
        /// </summary>
        public byte B { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="a">Alpha通道值</param>
        /// <param name="r">红色通道值</param>
        /// <param name="g">绿色通道值</param>
        /// <param name="b">蓝色通道值</param>
        public ColorARGB(byte a, byte r, byte g, byte b)
        {
            A = a;
            R = r;
            G = g;
            B = b;
        }

        /// <summary>
        /// 构造函数（使用uint ARGB值）
        /// </summary>
        /// <param name="argb">ARGB格式的uint值</param>
        public ColorARGB(uint argb)
        {
            A = (byte)((argb >> 24) & 0xFF);
            R = (byte)((argb >> 16) & 0xFF);
            G = (byte)((argb >> 8) & 0xFF);
            B = (byte)(argb & 0xFF);
        }

        /// <summary>
        /// 转换为uint ARGB格式
        /// </summary>
        /// <returns>ARGB格式的uint值</returns>
        public uint ToUInt32()
        {
            return (uint)((A << 24) | (R << 16) | (G << 8) | B);
        }

        /// <summary>
        /// 隐式转换为uint ARGB格式
        /// </summary>
        /// <param name="color">ColorARGB颜色</param>
        public static implicit operator uint(ColorARGB color)
            => color.ToUInt32();

        /// <summary>
        /// 隐式从uint ARGB格式转换
        /// </summary>
        /// <param name="argb">ARGB格式的uint值</param>
        public static implicit operator ColorARGB(uint argb)
            => new ColorARGB(argb);

        /// <summary>
        /// 预定义颜色：黑色
        /// </summary>
        public static readonly ColorARGB Black = new ColorARGB(255, 0, 0, 0);

        /// <summary>
        /// 预定义颜色：白色
        /// </summary>
        public static readonly ColorARGB White = new ColorARGB(255, 255, 255, 255);

        /// <summary>
        /// 预定义颜色：红色
        /// </summary>
        public static readonly ColorARGB Red = new ColorARGB(255, 255, 0, 0);

        /// <summary>
        /// 预定义颜色：绿色
        /// </summary>
        public static readonly ColorARGB Green = new ColorARGB(255, 0, 255, 0);

        /// <summary>
        /// 预定义颜色：蓝色
        /// </summary>
        public static readonly ColorARGB Blue = new ColorARGB(255, 0, 0, 255);

        /// <summary>
        /// 预定义颜色：透明
        /// </summary>
        public static readonly ColorARGB Transparent = new ColorARGB(0, 0, 0, 0);
    }
}
