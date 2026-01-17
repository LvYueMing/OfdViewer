using System.Globalization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace OFDViewer.Models.BaseType
{
    /// <summary>
    /// ST_Box 矩形区域，前两个值代表左上角坐标，后两个值依次表示宽和高
    /// </summary>
    public struct ST_Box : IEquatable<ST_Box>
    {
        // 坐标判断容差，可按需调整
        private const double PositionTolerance = 1e-6;
        private ST_Pos _position;
        private double _width;
        private double _height;

        /// <summary>
        /// 无效坐标实例
        /// </summary>
        public static readonly ST_Box InvalidValue = new ST_Box();

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
        /// 是否为有效标识
        /// </summary>
        public bool IsValid => !Equals(InvalidValue);

        /// <summary>
        /// XML文本序列化属性
        /// </summary>
        [XmlText]
        public string Value
        {
            get => ToString();
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    ST_Box parsedBox = Parse(value);
                    _position = parsedBox._position;
                    _width = parsedBox._width;
                    _height = parsedBox._height;
                }
            }
        }

        /// <summary>
        /// 初始化矩形区域,创建无效矩形区域(-1,-1,0,0)
        /// </summary>
        public ST_Box()
        {
            _position = ST_Pos.InvalidValue;
            _width = 0;
            _height = 0;
        }


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
        {
            if (width <= 0)
                throw new ArgumentOutOfRangeException(nameof(width), "宽度必须大于0");

            if (height <= 0)
                throw new ArgumentOutOfRangeException(nameof(height), "高度必须大于0");

            _position = position;
            _width = width;
            _height = height;
        }

        /// <summary>
        /// 解析字符串为ST_Box对象
        /// </summary>
        public static ST_Box Parse(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("输入字符串不能为空或只包含空格", nameof(value));

            string[] parts = value.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 4)
                throw new FormatException($"输入格式错误：{value}。正确格式应为：x y width height");

            double x = double.Parse(parts[0], CultureInfo.InvariantCulture);
            double y = double.Parse(parts[1], CultureInfo.InvariantCulture);
            double width = double.Parse(parts[2], CultureInfo.InvariantCulture);
            double height = double.Parse(parts[3], CultureInfo.InvariantCulture);

            // 如果宽度或高度为0，则返回无效的ST_Box值
            if (width <= 0 || height <= 0)
            {
                return InvalidValue;
            }

            return new ST_Box(x, y, width, height);
        }

        /// <summary>
        /// 尝试解析字符串为ST_Box对象
        /// </summary>
        public static bool TryParse(string value, out ST_Box result)
        {
            try
            {
                result = Parse(value);
                return true;
            }
            catch
            {
                result = InvalidValue;
                return false;
            }
        }

        /// <summary>
        /// 转换为字符串格式
        /// </summary>
        public override string ToString() =>
            $"{X.ToString(CultureInfo.InvariantCulture)} {Y.ToString(CultureInfo.InvariantCulture)} " +
            $"{_width.ToString(CultureInfo.InvariantCulture)} {_height.ToString(CultureInfo.InvariantCulture)}";

        #region 接口实现和运算符重载
        public bool Equals(ST_Box other) =>
            _position.Equals(other._position) &&
            Math.Abs(_width - other._width) < PositionTolerance &&
            Math.Abs(_height - other._height) < PositionTolerance;

        public override bool Equals(object obj) => obj is ST_Box other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(_position, _width, _height);

        public static bool operator ==(ST_Box left, ST_Box right) => left.Equals(right);
        public static bool operator !=(ST_Box left, ST_Box right) => !left.Equals(right);

        public static explicit operator ST_Box(string box) => Parse(box);

        public static explicit operator string(ST_Box pos) => pos.ToString();
        #endregion
    }
}