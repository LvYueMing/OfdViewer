using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfdViewer.BaseType
{
    /// <summary>
    /// ST_RefID 标识引用，此标识应为文档内已定义的标识
    /// </summary>
    public struct ST_RefID : IEquatable<ST_RefID>
    {
        private readonly ST_ID _referencedId;

        /// <summary>
        /// 无效引用实例
        /// </summary>
        public static readonly ST_RefID Invalid = new ST_RefID(ST_ID.Invalid);

        /// <summary>
        /// 初始化ST_RefID
        /// </summary>
        /// <param name="referencedId">引用的ST_ID</param>
        public ST_RefID(ST_ID referencedId)
        {
            _referencedId = referencedId;
        }

        /// <summary>
        /// 获取引用的标识
        /// </summary>
        public ST_ID ReferencedId => _referencedId;

        /// <summary>
        /// 是否为有效引用
        /// </summary>
        public bool IsValid => _referencedId.IsValid;

        /// <summary>
        /// 从字符串解析ST_RefID
        /// </summary>
        /// <param name="str">字符串格式的引用标识</param>
        public static ST_RefID Parse(string str)
        {
            var id = ST_ID.Parse(str);
            return new ST_RefID(id);
        }

        public static bool TryParse(string str, out ST_RefID result)
        {
            if (ST_ID.TryParse(str, out ST_ID id))
            {
                result = new ST_RefID(id);
                return true;
            }

            result = Invalid;
            return false;
        }

        public override string ToString() => _referencedId.ToString();

        #region 接口实现和运算符重载
        public bool Equals(ST_RefID other) => _referencedId.Equals(other._referencedId);
        public override bool Equals(object obj) => obj is ST_RefID other && Equals(other);
        public override int GetHashCode() => _referencedId.GetHashCode();

        public static bool operator ==(ST_RefID left, ST_RefID right) => left.Equals(right);
        public static bool operator !=(ST_RefID left, ST_RefID right) => !left.Equals(right);
        public static implicit operator ST_ID(ST_RefID refId) => refId._referencedId;
        public static explicit operator ST_RefID(ST_ID id) => new ST_RefID(id);
        #endregion
    }

    
}
