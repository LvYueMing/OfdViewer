using OFDViewer.BaseType.DocumentStructure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OFDViewer.BaseType
{
    /// <summary>
    /// ST_Array 数组，以空格来分割元素。
    /// 元素可以是除 ST_Loc、ST_Array 外的数据类型，不可嵌套
    /// 使用 List<object> 存储，便于动态添加和操作
    /// </summary>
    public struct ST_Array : IEquatable<ST_Array>, IEnumerable<object>
    {
        private readonly List<object> _values;
        private static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;

        /// <summary>
        /// 数组长度
        /// </summary>
        public int Length => _values.Count;

        /// <summary>
        /// 元素个数（与 Length 相同）
        /// </summary>
        public int Count => _values.Count;

        /// <summary>
        /// 是否为空的数组
        /// </summary>
        public bool IsEmpty => _values.Count == 0;

        /// <summary>
        /// 索引器
        /// </summary>
        public object this[int index]
        {
            get
            {
                if (index < 0 || index >= _values.Count)
                    throw new IndexOutOfRangeException();
                return _values[index];
            }
        }

        /// <summary>
        /// 获取原始列表的只读副本
        /// </summary>
        public IReadOnlyList<object> Values => _values.AsReadOnly();

        /// <summary>
        /// 空数组实例
        /// </summary>
        public static readonly ST_Array Empty = new ST_Array();

        /// <summary>
        /// 初始化空数组
        /// </summary>
        public ST_Array()
        {
            _values = new List<object>();
        }

        /// <summary>
        /// 初始化数组
        /// </summary>
        /// <param name="values">数组元素</param>
        public ST_Array(params object[] values)
        {
            _values = new List<object>();
            if (values != null)
            {
                foreach (var value in values)
                {
                    AddInternal(value);
                }
            }
        }

        /// <summary>
        /// 初始化数组
        /// </summary>
        /// <param name="values">数组元素集合</param>
        public ST_Array(IEnumerable<object> values)
        {
            _values = new List<object>();
            if (values != null)
            {
                foreach (var value in values)
                {
                    AddInternal(value);
                }
            }
        }

        /// <summary>
        /// 内部添加方法，进行类型验证
        /// </summary>
        private void AddInternal(object value)
        {
            if (value == null)
            {
                _values.Add(null);
                return;
            }

            // 验证元素类型：不能是 ST_Loc 或 ST_Array
            var type = value.GetType();
            if (type == typeof(ST_Loc) || type == typeof(ST_Array))
            {
                throw new ArgumentException($"ST_Array 不能包含 {type.Name} 类型的元素", nameof(value));
            }

            _values.Add(value);
        }

        /// <summary>
        /// 从字符串解析数组
        /// 自动识别数值类型：整数、浮点数
        /// </summary>
        /// <param name="str">以空格分割的字符串</param>
        public static ST_Array Parse(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return Empty;

            var parts = str.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var values = new List<object>();

            foreach (var part in parts)
            {
                // 尝试解析为整数
                if (int.TryParse(part, NumberStyles.Integer, InvariantCulture, out int intValue))
                {
                    values.Add(intValue);
                }
                // 尝试解析为浮点数
                else if (double.TryParse(part, NumberStyles.Float, InvariantCulture, out double doubleValue))
                {
                    // 检查是否为整数的浮点数表示
                    if (doubleValue == Math.Floor(doubleValue) &&
                        doubleValue >= int.MinValue && doubleValue <= int.MaxValue)
                    {
                        values.Add((int)doubleValue);
                    }
                    else
                    {
                        values.Add(doubleValue);
                    }
                }
                // 否则作为字符串
                else
                {
                    values.Add(part);
                }
            }

            return new ST_Array(values);
        }

        public static bool TryParse(string str, out ST_Array result)
        {
            result = Empty;

            try
            {
                result = Parse(str);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 添加元素到数组
        /// </summary>
        public ST_Array Add(object value)
        {
            var newArray = new ST_Array(_values);
            newArray.AddInternal(value);
            return newArray;
        }

        /// <summary>
        /// 添加多个元素到数组
        /// </summary>
        public ST_Array AddRange(params object[] values)
        {
            var newArray = new ST_Array(_values);
            foreach (var value in values)
            {
                newArray.AddInternal(value);
            }
            return newArray;
        }

        /// <summary>
        /// 添加多个元素到数组
        /// </summary>
        public ST_Array AddRange(IEnumerable<object> values)
        {
            var newArray = new ST_Array(_values);
            foreach (var value in values)
            {
                newArray.AddInternal(value);
            }
            return newArray;
        }

        /// <summary>
        /// 在指定位置插入元素
        /// </summary>
        public ST_Array Insert(int index, object value)
        {
            if (index < 0 || index > _values.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            // 验证元素类型
            if (value != null)
            {
                var type = value.GetType();
                if (type == typeof(ST_Loc) || type == typeof(ST_Array))
                {
                    throw new ArgumentException($"ST_Array 不能包含 {type.Name} 类型的元素", nameof(value));
                }
            }

            var newList = new List<object>(_values);
            newList.Insert(index, value);
            return new ST_Array(newList);
        }

        /// <summary>
        /// 移除指定位置的元素
        /// </summary>
        public ST_Array RemoveAt(int index)
        {
            if (index < 0 || index >= _values.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            var newList = new List<object>(_values);
            newList.RemoveAt(index);
            return new ST_Array(newList);
        }

        /// <summary>
        /// 尝试获取指定索引的整数
        /// </summary>
        public bool TryGetInt(int index, out int value)
        {
            value = 0;
            if (index < 0 || index >= _values.Count)
                return false;

            var obj = _values[index];
            if (obj is int i)
            {
                value = i;
                return true;
            }

            if (obj is double d && d == Math.Floor(d) &&
                d >= int.MinValue && d <= int.MaxValue)
            {
                value = (int)d;
                return true;
            }

            if (obj is string s && int.TryParse(s, NumberStyles.Integer, InvariantCulture, out int parsed))
            {
                value = parsed;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 尝试获取指定索引的双精度浮点数
        /// </summary>
        public bool TryGetDouble(int index, out double value)
        {
            value = 0;
            if (index < 0 || index >= _values.Count)
                return false;

            var obj = _values[index];
            if (obj is double d)
            {
                value = d;
                return true;
            }

            if (obj is int i)
            {
                value = i;
                return true;
            }

            if (obj is string s && double.TryParse(s, NumberStyles.Float, InvariantCulture, out double parsed))
            {
                value = parsed;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 尝试获取指定索引的字符串
        /// </summary>
        public bool TryGetString(int index, out string value)
        {
            value = null;
            if (index < 0 || index >= _values.Count)
                return false;

            var obj = _values[index];
            if (obj == null)
                return false;

            value = obj.ToString();
            return true;
        }

        /// <summary>
        /// 获取指定索引的整数，如果无法转换则返回默认值
        /// </summary>
        public int GetInt(int index, int defaultValue = 0)
        {
            return TryGetInt(index, out int value) ? value : defaultValue;
        }

        /// <summary>
        /// 获取指定索引的双精度浮点数，如果无法转换则返回默认值
        /// </summary>
        public double GetDouble(int index, double defaultValue = 0)
        {
            return TryGetDouble(index, out double value) ? value : defaultValue;
        }

        /// <summary>
        /// 获取指定索引的字符串，如果无法转换则返回默认值
        /// </summary>
        public string GetString(int index, string defaultValue = "")
        {
            return TryGetString(index, out string value) ? value : defaultValue;
        }

        /// <summary>
        /// 将数组转换为字符串表示
        /// </summary>
        public override string ToString()
        {
            if (_values.Count == 0)
                return string.Empty;

            var sb = new StringBuilder();

            for (int i = 0; i < _values.Count; i++)
            {
                if (i > 0)
                    sb.Append(' ');

                var value = _values[i];
                if (value == null)
                {
                    sb.Append(string.Empty);
                }
                else if (value is IFormattable formattable)
                {
                    // 对于可格式化的数值类型，使用不变文化
                    if (value is double d)
                    {
                        sb.Append(d.ToString(InvariantCulture));
                    }
                    else if (value is float f)
                    {
                        sb.Append(f.ToString(InvariantCulture));
                    }
                    else if (value is decimal m)
                    {
                        sb.Append(m.ToString(InvariantCulture));
                    }
                    else
                    {
                        sb.Append(formattable.ToString(null, InvariantCulture));
                    }
                }
                else
                {
                    sb.Append(value.ToString());
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// 将数组转换为double数组（尽可能转换）
        /// </summary>
        public double[] ToDoubleArray()
        {
            var result = new double[_values.Count];
            for (int i = 0; i < _values.Count; i++)
            {
                if (TryGetDouble(i, out double value))
                {
                    result[i] = value;
                }
                else
                {
                    result[i] = 0;
                }
            }
            return result;
        }

        /// <summary>
        /// 将数组转换为int数组（尽可能转换）
        /// </summary>
        public int[] ToIntArray()
        {
            var result = new int[_values.Count];
            for (int i = 0; i < _values.Count; i++)
            {
                if (TryGetInt(i, out int value))
                {
                    result[i] = value;
                }
                else
                {
                    result[i] = 0;
                }
            }
            return result;
        }

        /// <summary>
        /// 将数组转换为字符串数组
        /// </summary>
        public string[] ToStringArray()
        {
            var result = new string[_values.Count];
            for (int i = 0; i < _values.Count; i++)
            {
                result[i] = _values[i]?.ToString() ?? string.Empty;
            }
            return result;
        }

        /// <summary>
        /// 过滤数组
        /// </summary>
        public ST_Array Where(Func<object, bool> predicate)
        {
            var newValues = _values.Where(predicate).ToList();
            return new ST_Array(newValues);
        }

        /// <summary>
        /// 映射数组
        /// </summary>
        public ST_Array Select(Func<object, object> selector)
        {
            var newValues = _values.Select(selector).ToList();
            return new ST_Array(newValues);
        }

        /// <summary>
        /// 查找第一个匹配的元素
        /// </summary>
        public object Find(Predicate<object> match)
        {
            return _values.Find(match);
        }

        /// <summary>
        /// 判断所有元素是否都能解析为数值
        /// </summary>
        public bool IsNumericArray()
        {
            foreach (var value in _values)
            {
                if (value == null)
                    return false;

                if (value is int || value is double || value is float || value is decimal)
                    continue;

                if (value is string s)
                {
                    if (!double.TryParse(s, NumberStyles.Float, InvariantCulture, out _))
                        return false;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 判断所有元素是否都能解析为整数
        /// </summary>
        public bool IsIntegerArray()
        {
            foreach (var value in _values)
            {
                if (value == null)
                    return false;

                if (value is int)
                    continue;

                if (value is double d && d == Math.Floor(d) &&
                    d >= int.MinValue && d <= int.MaxValue)
                    continue;

                if (value is string s)
                {
                    if (!int.TryParse(s, NumberStyles.Integer, InvariantCulture, out _))
                        return false;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 清空数组
        /// </summary>
        public ST_Array Clear()
        {
            return Empty;
        }


        #region 接口实现和运算符重载
        public bool Equals(ST_Array other)
        {
            if (_values.Count != other._values.Count)
                return false;

            for (int i = 0; i < _values.Count; i++)
            {
                var value1 = _values[i];
                var value2 = other._values[i];

                if (value1 == null && value2 == null)
                    continue;

                if (value1 == null || value2 == null)
                    return false;

                if (value1.GetType() != value2.GetType())
                    return false;

                if (!value1.Equals(value2))
                    return false;
            }

            return true;
        }

        public override bool Equals(object obj) => obj is ST_Array other && Equals(other);

        public override int GetHashCode()
        {
            var hash = new HashCode();
            foreach (var value in _values)
            {
                hash.Add(value);
            }
            return hash.ToHashCode();
        }

        public IEnumerator<object> GetEnumerator() => _values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _values.GetEnumerator();

        public static bool operator ==(ST_Array left, ST_Array right) => left.Equals(right);
        public static bool operator !=(ST_Array left, ST_Array right) => !left.Equals(right);
        #endregion
    }
}
