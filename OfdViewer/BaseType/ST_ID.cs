using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OFDViewer.BaseType
{
/// <summary>
/// ST_ID 标识，无符号整数，应在文档内唯一。0 表示无效标识
/// 支持多线程环境下的唯一标识生成
/// </summary>
    public struct ST_ID : IEquatable<ST_ID>, IComparable<ST_ID>
    {
        private readonly uint _value;

        // 用于生成唯一标识的原子计数器
        private static long _idCounter = 0;

        // 用于标识生成器锁定的对象（用于较老版本.NET的线程安全）
        private static readonly object _syncLock = new object();

        /// <summary>
        /// 无效标识的值
        /// </summary>
        public const uint InvalidValue = 0;

        /// <summary>
        /// 无效标识实例
        /// </summary>
        public static readonly ST_ID Invalid = new ST_ID(InvalidValue);

        /// 获取原始值
        /// </summary>
        public uint Value => _value;

        /// <summary>
        /// 是否为有效标识（不等于0）
        /// </summary>
        public bool IsValid => _value != InvalidValue;

        /// <summary>
        /// 初始化ST_ID
        /// </summary>
        /// <param name="value">无符号整数值</param>
        public ST_ID(uint value)
        {
            _value = value;
        }

        /// <summary>

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

        /// <summary>
        /// 尝试从字符串解析ST_ID
        /// </summary>
        /// <param name="str">字符串格式的标识</param>
        /// <param name="result">解析结果</param>
        /// <returns>是否解析成功</returns>
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

        /// <summary>
        /// 生成一个新的唯一标识（线程安全）
        /// </summary>
        /// <returns>新的唯一ST_ID</returns>
        public static ST_ID CreateNew()
        {
            // 使用原子操作获取下一个ID
            long nextId = Interlocked.Increment(ref _idCounter);

            // 确保ID不为0（0是无效标识）
            if (nextId == 0)
            {
                nextId = Interlocked.Increment(ref _idCounter);
            }

            // 检查是否溢出（虽然uint.MaxValue很大，但安全起见）
            if (nextId > uint.MaxValue)
            {
                throw new OverflowException("ST_ID计数器已超过最大值");
            }

            return new ST_ID((uint)nextId);
        }

        /// <summary>
        /// 设置起始计数器值（用于分布式系统或需要特定起始值的场景）
        /// </summary>
        /// <param name="startValue">起始值（必须大于0）</param>
        public static void SetIdCounterStart(uint startValue)
        {
            if (startValue == 0)
            {
                throw new ArgumentException("起始值必须大于0", nameof(startValue));
            }

            Interlocked.Exchange(ref _idCounter, startValue - 1);
        }


        /// <summary>
        /// 批量生成多个唯一标识
        /// </summary>
        /// <param name="count">要生成的数量</param>
        /// <returns>唯一标识数组</returns>
        public static ST_ID[] CreateBatch(int count)
        {
            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "数量必须大于0");
            }

            ST_ID[] ids = new ST_ID[count];

            // 获取起始ID
            long startId = Interlocked.Add(ref _idCounter, count);
            startId = startId - count + 1; // 调整到实际起始值

            // 确保起始ID不为0
            if (startId <= 0)
            {
                // 重新计算
                Interlocked.Add(ref _idCounter, -count);
                return CreateBatch(count); // 递归调用（通常只发生一次）
            }

            for (int i = 0; i < count; i++)
            {
                uint idValue = (uint)(startId + i);
                if (idValue == 0) // 如果遇到0，跳过
                {
                    idValue = (uint)(startId + count);
                    Interlocked.Increment(ref _idCounter);
                }

                ids[i] = new ST_ID(idValue);

                // 检查溢出
                if (idValue == uint.MaxValue)
                {
                    throw new OverflowException("ST_ID计数器已超过最大值");
                }
            }

            return ids;
        }


        public override string ToString() => _value.ToString();

        #region 接口实现和运算符重载
        public bool Equals(ST_ID other) => _value == other._value;
        public override bool Equals(object obj) => obj is ST_ID other && Equals(other);
        public override int GetHashCode() => _value.GetHashCode();
        public int CompareTo(ST_ID other) => _value.CompareTo(other._value);

        public static bool operator ==(ST_ID left, ST_ID right) => left.Equals(right);
        public static bool operator !=(ST_ID left, ST_ID right) => !left.Equals(right);
        public static bool operator <(ST_ID left, ST_ID right) => left.CompareTo(right) < 0;
        public static bool operator >(ST_ID left, ST_ID right) => left.CompareTo(right) > 0;

        public static explicit operator uint(ST_ID id) => id._value;
        public static implicit operator ST_ID(uint value) => new ST_ID(value);
        #endregion
    }
}
