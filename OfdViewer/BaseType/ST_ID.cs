using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfdViewer.BaseType
{
    /// <summary>
    /// ST_ID 标识，无符号整数，应在文档内唯一。0 表示无效标识
    /// </summary>
    public struct ST_ID : IEquatable<ST_ID>, IComparable<ST_ID>
    {
        private readonly uint _value;

        /// <summary>
        /// 无效标识的值
        /// </summary>
        public const uint InvalidValue = 0;

        /// <summary>
        /// 无效标识实例
        /// </summary>
        public static readonly ST_ID Invalid = new ST_ID(InvalidValue);

        /// <summary>
        /// 初始化ST_ID
        /// </summary>
        /// <param name="value">无符号整数值</param>
        public ST_ID(uint value)
        {
            _value = value;
        }

        /// <summary>
        /// 获取原始值
        /// </summary>
        public uint Value => _value;

        /// <summary>
        /// 是否为有效标识（不等于0）
        /// </summary>
        public bool IsValid => _value != InvalidValue;

        /// <summary>
        /// 从字符串解析ST_ID
        /// </summary>
        /// <param name="str">字符串格式的标识</param>
        public static ST_ID Parse(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return Invalid;

            if (uint.TryParse(str, out uint value))
                return new ST_ID(value);

            throw new FormatException($"无效的ST_ID格式: '{str}'");
        }

        public static bool TryParse(string str, out ST_ID result)
        {
            result = Invalid;

            if (string.IsNullOrWhiteSpace(str))
                return false;

            if (uint.TryParse(str, out uint value))
            {
                result = new ST_ID(value);
                return true;
            }

            return false;
        }

        public override string ToString() => _value.ToString();

        #region 接口实现和运算符重载
        public bool Equals(ST_ID other) => _value == other._value;
        public override bool Equals(object obj) => obj is ST_ID other && Equals(other);
        public override int GetHashCode() => _value.GetHashCode();
        public int CompareTo(ST_ID other) => _value.CompareTo(other._value);

        public static bool operator ==(ST_ID left, ST_ID right) => left.Equals(right);
        public static bool operator !=(ST_ID left, ST_ID right) => !left.Equals(right);
        public static bool operator <(ST_ID left, ST_ID right) => left._value < right._value;
        public static bool operator >(ST_ID left, ST_ID right) => left._value > right._value;
        public static implicit operator uint(ST_ID id) => id._value;
        public static explicit operator ST_ID(uint value) => new ST_ID(value);
        #endregion
    }
}
