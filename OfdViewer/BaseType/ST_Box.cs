using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfdViewer.BaseType
{
    /// <summary>
    /// ST_Box 矩形区域，前两个值代表左上角坐标，后两个值依次表示宽和高
    /// </summary>
    public struct ST_Box : IEquatable<ST_Box>
    {
        private readonly ST_Pos _position;
        private readonly double _width;
        private readonly double _height;

        /// <summary>
        /// 初始化矩形区域
        /// </summary>
        /// <param name="x">左上角X坐标</param>
        /// <param name="y">左上角Y坐标</param>
        /// <param name="width">宽度（必须大于0）</param>
        /// <param name="height">高度（必须大于0）</param>
        public ST_Box(double x, double y, double width, double height)
        {
            if (width <= 0)
                throw new ArgumentOutOfRangeException(nameof(width), "宽度必须大于0");

            if (height <= 0)
                throw new ArgumentOutOfRangeException(nameof(height), "高度必须大于0");

            _position = new ST_Pos(x, y);
            _width = width;
            _height = height;
        }

        /// <summary>
        /// 初始化矩形区域
        /// </summary>
        /// <param name="position">左上角坐标</param>
        /// <param name="width">宽度（必须大于0）</param>
        /// <param name="height">高度（必须大于0）</param>
        public ST_Box(ST_Pos position, double width, double height)
            : this(position.X, position.Y, width, height)
        {
        }

        /// <summary>
        /// 左上角X坐标
        /// </summary>
        public double X => _position.X;

        /// <summary>
        /// 左上角Y坐标
        /// </summary>
        public double Y => _position.Y;

        /// <summary>
        /// 宽度（大于0）
        /// </summary>
        public double Width => _width;

        /// <summary>
        /// 高度（大于0）
        /// </summary>
        public double Height => _height;

        /// <summary>
        /// 左上角坐标
        /// </summary>
        public ST_Pos Position => _position;

        /// <summary>
        /// 右下角X坐标
        /// </summary>
        public double Right => X + Width;

        /// <summary>
        /// 右下角Y坐标
        /// </summary>
        public double Bottom => Y + Height;

        /// <summary>
        /// 从字符串解析矩形区域
        /// </summary>
        /// <param name="str">格式："x y width height"</param>
        public static ST_Box Parse(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                throw new ArgumentException("矩形字符串不能为空", nameof(str));

            var parts = str.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 4)
                throw new FormatException($"无效的ST_Box格式，应为四个数值用空格分隔: '{str}'");

            if (!double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double x))
                throw new FormatException($"无效的X坐标: '{parts[0]}'");

            if (!double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double y))
                throw new FormatException($"无效的Y坐标: '{parts[1]}'");

            if (!double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out double width))
                throw new FormatException($"无效的宽度: '{parts[2]}'");

            if (!double.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out double height))
                throw new FormatException($"无效的高度: '{parts[3]}'");

            return new ST_Box(x, y, width, height);
        }

        public static bool TryParse(string str, out ST_Box result)
        {
            result = default;

            if (string.IsNullOrWhiteSpace(str))
                return false;

            var parts = str.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 4)
                return false;

            if (!double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double x))
                return false;

            if (!double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double y))
                return false;

            if (!double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out double width))
                return false;

            if (!double.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out double height))
                return false;

            if (width <= 0 || height <= 0)
                return false;

            result = new ST_Box(x, y, width, height);
            return true;
        }

        /// <summary>
        /// 检查点是否在矩形内
        /// </summary>
        public bool Contains(ST_Pos point) =>
            point.X >= X && point.X <= Right &&
            point.Y >= Y && point.Y <= Bottom;

        /// <summary>
        /// 转换为字符串格式
        /// </summary>
        public override string ToString() =>
            $"{X.ToString(CultureInfo.InvariantCulture)} {Y.ToString(CultureInfo.InvariantCulture)} " +
            $"{_width.ToString(CultureInfo.InvariantCulture)} {_height.ToString(CultureInfo.InvariantCulture)}";

        #region 接口实现和运算符重载
        public bool Equals(ST_Box other) =>
            _position.Equals(other._position) &&
            Math.Abs(_width - other._width) < double.Epsilon &&
            Math.Abs(_height - other._height) < double.Epsilon;

        public override bool Equals(object obj) => obj is ST_Box other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(_position, _width, _height);

        public static bool operator ==(ST_Box left, ST_Box right) => left.Equals(right);
        public static bool operator !=(ST_Box left, ST_Box right) => !left.Equals(right);
        #endregion
    }
}
