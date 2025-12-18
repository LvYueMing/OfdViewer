using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OFDViewer.BaseType
{
    /// <summary>
    /// ST_Loc 包结构内文件的路径
    /// "." 表示当前路径, ".." 表示父路径
    /// 约定:
    /// 1. "/"代表根节点;
    /// 2. 未显式指定时代表当前路径;
    /// 3. 路径区分大小写
    /// </summary>
    public struct ST_Loc : IEquatable<ST_Loc>
    {
        private readonly string _path;
        // 是否为绝对路径
        private readonly bool _isAbsolute;

        public ST_Loc(string path)
        {
            if (string.IsNullOrEmpty(path) || path == ".")
            {
                _path = ".";
                _isAbsolute = false;
            }
            else
            {
                // 先将所有 \ 替换为 /
                path = path.Replace('\\', '/');

                if (path.StartsWith("/"))
                {
                    _path = NormalizePath(path, true);
                    _isAbsolute = true;
                }
                else
                {
                    // 处理相对路径，确保以 ./ 开头（除非是以 ../ 开头）
                    if (!path.StartsWith("../"))
                    {
                        if (!path.StartsWith("./"))
                        {
                            path = "./" + path;
                        }
                    }
                    _path = NormalizePath(path, false);
                    _isAbsolute = false;
                }
            }
        }

        public string Path => _path;
        public bool IsAbsolute => _isAbsolute;
        public bool IsRelative => !_isAbsolute;

        // 规范化路径，自动处理 . 和 ..
        private static string NormalizePath(string path, bool isAbsolute)
        {
            var parts = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            var stack = new Stack<string>();

            // 记录是否以 ../ 开头
            bool startsWithParentRef = path.StartsWith("../");

            foreach (var part in parts)
            {
                if (part == ".")
                    continue;
                if (part == "..")
                {
                    if (stack.Count > 0 && stack.Peek() != "..")
                        stack.Pop();
                    else if (!isAbsolute)
                        stack.Push("..");
                    // 绝对路径下超出根目录的 .. 忽略
                }
                else
                {
                    stack.Push(part);
                }
            }

            var normalized = string.Join("/", stack.Reverse());
            if (isAbsolute)
                return "/" + normalized;

            // 对于相对路径，添加适当的前缀
            if (string.IsNullOrEmpty(normalized))
                return ".";
            else if (startsWithParentRef)
                return normalized.StartsWith("..") ? normalized : "../" + normalized;
            else
                return "./" + normalized;
        }


        /// <summary>
        /// 显示转换
        /// </summary>
        /// <param name="loc">值</param>
        public static explicit operator string(ST_Loc loc)
        {
            return loc.ToString();
        }
        /// <summary>
        /// 隐式转换
        /// </summary>
        /// <param name="aPath">值</param>
        public static implicit operator ST_Loc(string aPath)
        {
            return new ST_Loc(aPath);
        }


        public override string ToString() => _path;

        // 其余接口和运算符重载可按需保留
        public bool Equals(ST_Loc other) => string.Equals(_path, other._path, StringComparison.Ordinal);
        public override bool Equals(object obj) => obj is ST_Loc other && Equals(other);
        public override int GetHashCode() => _path?.GetHashCode() ?? 0;
        public static bool operator ==(ST_Loc left, ST_Loc right) => left.Equals(right);
        public static bool operator !=(ST_Loc left, ST_Loc right) => !left.Equals(right);
    }

}
