using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OFDViewer.BaseType
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    namespace DocumentStructure
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
            private readonly PathType _pathType;

            /// <summary>
            /// 路径类型
            /// </summary>
            private enum PathType
            {
                Absolute,    // 绝对路径，以"/"开头
                Relative,    // 相对路径（相对于当前文档位置）
                Current,     // 当前路径("."或空字符串)
                Parent       // 父路径("..")
            }

            /// <summary>
            /// 根路径 - 表示OFD文件解压包所在路径
            /// </summary>
            public static readonly ST_Loc Root = new ST_Loc("/");

            /// <summary>
            /// 当前路径 - 表示当前文档所在的目录路径
            /// 这是一个相对概念，需要结合上下文使用
            /// </summary>
            public static readonly ST_Loc Current = new ST_Loc(".");

            /// <summary>
            /// 父路径 - 表示当前文档所在目录的父目录
            /// </summary>
            public static readonly ST_Loc Parent = new ST_Loc("..");

            /// <summary>
            /// 初始化ST_Loc
            /// </summary>
            /// <param name="path">路径字符串</param>
            public ST_Loc(string path)
            {
                if (string.IsNullOrEmpty(path))
                {
                    _path = ".";
                    _pathType = PathType.Current;
                }
                else if (path == "/")
                {
                    _path = "/";
                    _pathType = PathType.Absolute;
                }
                else if (path == ".")
                {
                    _path = ".";
                    _pathType = PathType.Current;
                }
                else if (path == "..")
                {
                    _path = "..";
                    _pathType = PathType.Parent;
                }
                else if (path.StartsWith("/"))
                {
                    _path = NormalizeAbsolutePath(path);
                    _pathType = PathType.Absolute;
                }
                else
                {
                    _path = NormalizeRelativePath(path);
                    _pathType = PathType.Relative;
                }
            }

            /// <summary>
            /// 基于当前文档路径创建ST_Loc
            /// </summary>
            /// <param name="currentDocumentPath">当前文档的绝对路径</param>
            /// <param name="relativePath">相对路径</param>
            public ST_Loc(ST_Loc currentDocumentPath, string relativePath)
            {
                if (!currentDocumentPath.IsAbsolute)
                    throw new ArgumentException("currentDocumentPath 必须是绝对路径", nameof(currentDocumentPath));

                if (string.IsNullOrEmpty(relativePath))
                {
                    _path = ".";
                    _pathType = PathType.Current;
                }
                else if (relativePath == ".")
                {
                    _path = ".";
                    _pathType = PathType.Current;
                }
                else if (relativePath == "..")
                {
                    _path = "..";
                    _pathType = PathType.Parent;
                }
                else if (relativePath.StartsWith("/"))
                {
                    _path = NormalizeAbsolutePath(relativePath);
                    _pathType = PathType.Absolute;
                }
                else
                {
                    // 解析相对于当前文档路径的相对路径
                    var resolvedPath = ResolveRelativePath(currentDocumentPath._path, relativePath);
                    _path = NormalizeAbsolutePath(resolvedPath);
                    _pathType = PathType.Absolute;
                }
            }

            /// <summary>
            /// 获取原始路径字符串
            /// </summary>
            public string Path => _path;

            /// <summary>
            /// 是否为绝对路径（从包根目录开始）
            /// </summary>
            public bool IsAbsolute => _pathType == PathType.Absolute;

            /// <summary>
            /// 是否为相对路径（相对于当前文档位置）
            /// </summary>
            public bool IsRelative => _pathType == PathType.Relative;

            /// <summary>
            /// 是否为当前路径（表示当前文档所在目录）
            /// </summary>
            public bool IsCurrent => _pathType == PathType.Current;

            /// <summary>
            /// 是否为父路径（表示当前文档所在目录的父目录）
            /// </summary>
            public bool IsParent => _pathType == PathType.Parent;

            /// <summary>
            /// 获取路径的父目录（如果是绝对路径）
            /// </summary>
            public ST_Loc ParentDirectory
            {
                get
                {
                    if (_pathType != PathType.Absolute)
                        throw new InvalidOperationException("只有绝对路径才能获取父目录");

                    if (_path == "/")
                        return Root; // 根目录的父目录还是根目录

                    var parts = _path.Split('/').Where(p => !string.IsNullOrEmpty(p)).ToList();
                    if (parts.Count == 0)
                        return Root;

                    parts.RemoveAt(parts.Count - 1);

                    if (parts.Count == 0)
                        return Root;
                    else
                        return new ST_Loc("/" + string.Join("/", parts));
                }
            }

            /// <summary>
            /// 获取文件名（如果有的话）
            /// </summary>
            public string FileName
            {
                get
                {
                    if (_pathType != PathType.Absolute ||
                        _path == "/" ||
                        string.IsNullOrEmpty(_path))
                        return string.Empty;

                    var parts = _path.Split('/');
                    return parts[parts.Length - 1];
                }
            }

            /// <summary>
            /// 获取文件扩展名（如果有的话）
            /// </summary>
            public string Extension
            {
                get
                {
                    var fileName = FileName;
                    if (string.IsNullOrEmpty(fileName))
                        return string.Empty;

                    var extension = System.IO.Path.GetExtension(fileName);
                    return extension ?? string.Empty;
                }
            }

            /// <summary>
            /// 获取目录名（不包含文件名）
            /// </summary>
            public ST_Loc DirectoryName
            {
                get
                {
                    if (_pathType != PathType.Absolute)
                        throw new InvalidOperationException("只有绝对路径才能获取目录名");

                    if (_path == "/")
                        return Root;

                    var lastSlash = _path.LastIndexOf('/');
                    if (lastSlash <= 0)
                        return Root;

                    return new ST_Loc(_path.Substring(0, lastSlash));
                }
            }

            /// <summary>
            /// 组合路径（类似于 System.IO.Path.Combine）
            /// </summary>
            public ST_Loc Combine(string relativePath)
            {
                if (string.IsNullOrEmpty(relativePath))
                    return this;

                if (relativePath.StartsWith("/"))
                    return new ST_Loc(relativePath);

                if (IsCurrent)
                    return new ST_Loc(relativePath);

                if (IsParent)
                    return new ST_Loc($"../{relativePath}");

                if (IsAbsolute)
                {
                    if (_path.EndsWith("/"))
                        return new ST_Loc($"{_path}{relativePath}");
                    else
                        return new ST_Loc($"{_path}/{relativePath}");
                }
                else
                {
                    // 相对路径的组合
                    if (string.IsNullOrEmpty(_path) || _path == ".")
                        return new ST_Loc(relativePath);
                    else
                        return new ST_Loc($"{_path}/{relativePath}");
                }
            }

            /// <summary>
            /// 组合路径
            /// </summary>
            public ST_Loc Combine(ST_Loc relativePath)
            {
                return Combine(relativePath._path);
            }

            /// <summary>
            /// 解析相对路径为绝对路径（基于当前文档路径）
            /// </summary>
            /// <param name="currentDocumentPath">当前文档的绝对路径</param>
            /// <returns>绝对路径</returns>
            public ST_Loc Resolve(ST_Loc currentDocumentPath)
            {
                if (!currentDocumentPath.IsAbsolute)
                    throw new ArgumentException("currentDocumentPath 必须是绝对路径", nameof(currentDocumentPath));

                if (IsAbsolute)
                    return this;

                if (IsCurrent)
                    return currentDocumentPath.DirectoryName;

                if (IsParent)
                    return currentDocumentPath.DirectoryName.ParentDirectory;

                // 解析相对路径
                var resolvedPath = ResolveRelativePath(currentDocumentPath._path, _path);
                return new ST_Loc(resolvedPath);
            }

            /// <summary>
            /// 将绝对路径转换为相对于指定基路径的相对路径
            /// </summary>
            /// <param name="basePath">基路径（必须是绝对路径）</param>
            /// <returns>相对路径</returns>
            public ST_Loc MakeRelativeTo(ST_Loc basePath)
            {
                if (!IsAbsolute)
                    throw new InvalidOperationException("只有绝对路径才能转换为相对路径");

                if (!basePath.IsAbsolute)
                    throw new ArgumentException("basePath 必须是绝对路径", nameof(basePath));

                if (_path == basePath._path)
                    return Current;

                // 确保两个路径都以"/"开头
                var targetPath = _path;
                var baseDir = basePath._path;

                // 如果basePath是文件，取其目录
                if (!baseDir.EndsWith("/"))
                {
                    baseDir = basePath.DirectoryName._path;
                }

                // 确保baseDir以"/"结尾
                if (!baseDir.EndsWith("/"))
                    baseDir += "/";

                if (targetPath.StartsWith(baseDir))
                {
                    // 目标路径在基路径下
                    var relative = targetPath.Substring(baseDir.Length);
                    return string.IsNullOrEmpty(relative) ? Current : new ST_Loc(relative);
                }
                else
                {
                    // 需要向上回溯
                    var targetParts = targetPath.Split('/').Where(p => !string.IsNullOrEmpty(p)).ToList();
                    var baseParts = baseDir.Split('/').Where(p => !string.IsNullOrEmpty(p)).ToList();

                    // 找到公共前缀
                    int commonLength = 0;
                    for (int i = 0; i < Math.Min(targetParts.Count, baseParts.Count); i++)
                    {
                        if (string.Equals(targetParts[i], baseParts[i], StringComparison.Ordinal))
                            commonLength++;
                        else
                            break;
                    }

                    // 计算需要向上回溯的层级
                    var upLevels = baseParts.Count - commonLength;
                    var relativeParts = new List<string>();

                    // 添加向上回溯的部分
                    for (int i = 0; i < upLevels; i++)
                    {
                        relativeParts.Add("..");
                    }

                    // 添加目标路径的剩余部分
                    for (int i = commonLength; i < targetParts.Count; i++)
                    {
                        relativeParts.Add(targetParts[i]);
                    }

                    if (relativeParts.Count == 0)
                        return Current;
                    else
                        return new ST_Loc(string.Join("/", relativeParts));
                }
            }

            /// <summary>
            /// 解析相对路径为绝对路径
            /// </summary>
            private static string ResolveRelativePath(string basePath, string relativePath)
            {
                // 确保basePath以"/"结尾（表示目录）
                var baseDir = basePath;
                if (!baseDir.EndsWith("/"))
                {
                    // 如果是文件路径，取目录部分
                    var lastSlash = baseDir.LastIndexOf('/');
                    if (lastSlash > 0)
                        baseDir = baseDir.Substring(0, lastSlash + 1);
                    else
                        baseDir = "/";
                }

                // 组合路径
                var combined = baseDir + relativePath;
                return NormalizeAbsolutePath(combined);
            }

            /// <summary>
            /// 规范化绝对路径
            /// </summary>
            private static string NormalizeAbsolutePath(string path)
            {
                if (string.IsNullOrEmpty(path))
                    return "/";

                // 确保以"/"开头
                if (!path.StartsWith("/"))
                    path = "/" + path;

                // 处理多个连续斜杠
                path = Regex.Replace(path, "/+", "/");

                // 移除末尾斜杠（除非是根路径）
                if (path.Length > 1 && path.EndsWith("/"))
                    path = path.Substring(0, path.Length - 1);

                // 处理 "." 和 ".."
                var parts = path.Split('/').Where(p => !string.IsNullOrEmpty(p)).ToList();
                var normalizedParts = new List<string>();

                foreach (var part in parts)
                {
                    if (part == ".")
                    {
                        // 忽略当前目录
                        continue;
                    }
                    else if (part == "..")
                    {
                        // 回退到父目录
                        if (normalizedParts.Count > 0)
                        {
                            normalizedParts.RemoveAt(normalizedParts.Count - 1);
                        }
                        // 对于绝对路径，忽略超出根目录的 ".."
                    }
                    else
                    {
                        normalizedParts.Add(part);
                    }
                }

                // 重建路径
                var result = "/" + string.Join("/", normalizedParts);
                return result.Length == 0 ? "/" : result;
            }

            /// <summary>
            /// 规范化相对路径
            /// </summary>
            private static string NormalizeRelativePath(string path)
            {
                if (string.IsNullOrEmpty(path))
                    return ".";

                // 处理多个连续斜杠
                path = Regex.Replace(path, "/+", "/");

                // 移除末尾斜杠
                if (path.EndsWith("/"))
                    path = path.Substring(0, path.Length - 1);

                // 处理 "." 和 ".." 路径段
                var parts = path.Split('/').Where(p => !string.IsNullOrEmpty(p)).ToList();
                var normalizedParts = new List<string>();

                foreach (var part in parts)
                {
                    if (part == ".")
                    {
                        // 忽略当前目录
                        continue;
                    }
                    else if (part == "..")
                    {
                        // 回退到父目录
                        if (normalizedParts.Count > 0 && normalizedParts[^1] != "..")
                        {
                            normalizedParts.RemoveAt(normalizedParts.Count - 1);
                        }
                        else
                        {
                            // 对于相对路径，保留 ".."
                            normalizedParts.Add("..");
                        }
                    }
                    else
                    {
                        normalizedParts.Add(part);
                    }
                }

                // 重建路径
                var result = string.Join("/", normalizedParts);
                return string.IsNullOrEmpty(result) ? "." : result;
            }

            public static ST_Loc Parse(string str)
            {
                return new ST_Loc(str);
            }

            public static bool TryParse(string str, out ST_Loc result)
            {
                try
                {
                    result = new ST_Loc(str);
                    return true;
                }
                catch
                {
                    result = default;
                    return false;
                }
            }

            public override string ToString() => _path;

            #region 接口实现和运算符重载
            public bool Equals(ST_Loc other) =>
                string.Equals(_path, other._path, StringComparison.Ordinal);

            public override bool Equals(object obj) => obj is ST_Loc other && Equals(other);

            public override int GetHashCode() => _path?.GetHashCode() ?? 0;

            public static bool operator ==(ST_Loc left, ST_Loc right) => left.Equals(right);
            public static bool operator !=(ST_Loc left, ST_Loc right) => !left.Equals(right);

            public static ST_Loc operator /(ST_Loc left, string right) => left.Combine(right);
            public static ST_Loc operator /(ST_Loc left, ST_Loc right) => left.Combine(right);
            #endregion
        }


    }
}
