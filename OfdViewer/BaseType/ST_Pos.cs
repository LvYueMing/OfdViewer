using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OFDViewer.BaseType
{
    /// <summary>
    /// ST_Pos 点坐标，以空格分割，前者为x值，后者为y值
    /// </summary>
    public struct ST_Pos : IEquatable<ST_Pos>
    {
        private readonly double _x;
        private readonly double _y;

        /// <summary>
        /// 原点坐标实例
        /// </summary>
        public static readonly ST_Pos Zero = new ST_Pos(0, 0);

        /// <summary>
        /// 初始化点坐标
        /// </summary>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        public ST_Pos(double x, double y)
        {
            _x = x;
            _y = y;
        }

        /// <summary>
        /// X坐标
        /// </summary>
        public double X => _x;

        /// <summary>
        /// Y坐标
        /// </summary>
        public double Y => _y;

        /// <summary>
        /// 从字符串解析点坐标
        /// </summary>
        /// <param name="str">格式："x y"</param>
        public static ST_Pos Parse(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                throw new ArgumentException("坐标字符串不能为空", nameof(str));

            var parts = str.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 2)
                throw new FormatException($"无效的ST_Pos格式，应为两个数值用空格分隔: '{str}'");

            if (!double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double x))
                throw new FormatException($"无效的X坐标: '{parts[0]}'");

            if (!double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double y))
                throw new FormatException($"无效的Y坐标: '{parts[1]}'");

            return new ST_Pos(x, y);
        }

        public static bool TryParse(string str, out ST_Pos result)
        {
            result = Zero;

            if (string.IsNullOrWhiteSpace(str))
                return false;

            var parts = str.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
                return false;

            if (!double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double x))
                return false;

            if (!double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double y))
                return false;

            result = new ST_Pos(x, y);
            return true;
        }

        /// <summary>
        /// 转换为字符串格式
        /// </summary>
        public override string ToString() =>
            $"{_x.ToString(CultureInfo.InvariantCulture)} {_y.ToString(CultureInfo.InvariantCulture)}";

        #region 接口实现和运算符重载
        public bool Equals(ST_Pos other) =>
            Math.Abs(_x - other._x) < double.Epsilon &&
            Math.Abs(_y - other._y) < double.Epsilon;

        public override bool Equals(object obj) => obj is ST_Pos other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(_x, _y);

        public static bool operator ==(ST_Pos left, ST_Pos right) => left.Equals(right);
        public static bool operator !=(ST_Pos left, ST_Pos right) => !left.Equals(right);

        public static ST_Pos operator +(ST_Pos left, ST_Pos right) =>
            new ST_Pos(left._x + right._x, left._y + right._y);

        public static ST_Pos operator -(ST_Pos left, ST_Pos right) =>
            new ST_Pos(left._x - right._x, left._y - right._y);
        #endregion
    }
}
