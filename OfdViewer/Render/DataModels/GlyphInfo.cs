using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OFDViewer.Render.DataModels
{
    /// <summary>
    /// 字形信息结构
    /// 用于批量绘制字形
    /// </summary>
    public struct GlyphInfo
    {
        /// <summary>
        /// 字形X坐标
        /// </summary>
        public float X;

        /// <summary>
        /// 字形Y坐标
        /// </summary>
        public float Y;

        /// <summary>
        /// 字形内容
        /// </summary>
        public string Glyph;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        /// <param name="glyph">字形内容</param>
        public GlyphInfo(float x, float y, string glyph)
        {
            X = x;
            Y = y;
            Glyph = glyph;
        }
    }
}
