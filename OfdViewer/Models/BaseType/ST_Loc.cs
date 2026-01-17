using System.Xml.Serialization;

namespace OFDViewer.Models.BaseType
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
        private string _path;

        /// <summary>
        /// XML文本序列化属性
        /// </summary>
        [XmlText]
        public string Path
        {
            get => _path;
            set
            {
                if (string.IsNullOrEmpty(value) || value == ".")
                {
                    _path = ".";
                }
                else
                {
                    // 先将所有 \ 替换为 /
                    value = value.Replace('\\', '/');

                    // OFD文件内部路径没有绝对路径概念，将所有路径统一处理为相对路径
                    // 以/开头的路径视为从根目录开始的相对路径
                    if (value.StartsWith("/"))
                    {
                        // 移除开头的/，并规范化路径
                        value = value.Substring(1);
                    }

                    _path = NormalizePath(value);
                }
            }
        }


        // C# 10+ 支持 struct 的无参数构造函数
        public ST_Loc()
        {
            Path = "";
        }

        public ST_Loc(string path)
        {
            Path = path;
        }



        // 规范化路径，自动处理 . 和 ..
        /// <summary>
        /// 将传入的相对路径字符串进行规范化处理，解析并消除其中出现的 "." 与 ".."
        /// 规则：
        /// 1. 以 "/" 分隔路径片段，忽略空片段；
        /// 2. "." 代表当前目录，直接跳过；
        /// 3. ".." 代表返回上一级目录：
        ///    - 若栈顶不是 ".."（即存在上级目录），则弹出栈顶，表示回退一级；
        ///    - 若栈顶也是 ".."（或栈为空），说明已无法继续回退，需保留该 ".."；
        /// 4. 普通目录名直接压入栈；
        /// 5. 最终按顺序拼接栈内剩余片段，得到不含 "." 与多余 ".." 的规范相对路径；
        /// 6. 若结果为空，则返回 "." 表示当前目录。
        /// </summary>
        /// <param name="path">待规范化的相对路径</param>
        /// <returns>规范化后的相对路径</returns>
        private static string NormalizePath(string path)
        {
            var parts = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            var stack = new Stack<string>();

            foreach (var part in parts)
            {               
                if (part == ".")
                    continue;
                if (part == "..")
                {
                    // 若栈顶不是 ".."（即存在上级目录），则弹出栈顶，表示回退一级；
                    if (stack.Count > 0 && stack.Peek() != "..")
                        stack.Pop();
                    else
                        // 若栈顶也是 ".."（或栈为空），说明已无法继续回退，需保留该 ".."；
                        stack.Push("..");
                }
                else
                {
                    stack.Push(part);
                }
            }

            if (stack.Count == 0)
                return ".";

            return string.Join("/", stack.Reverse());
        }


        /// 根据基准位置解析当前相对路径，生成实际的相对路径
        /// </summary>
        /// <param name="baseLoc">基准位置路径,即当前路径</param>
        /// <summary>
        /// 根据基准位置解析相对路径，生成实际的相对路径
        /// </summary>
        /// <param name="baseLoc">基准位置路径（不应包含.和..）</param>
        /// <returns>解析后的实际相对路径</returns>
        /// <exception cref="ArgumentException">当基准路径包含.或..，或者相对路径超出基准路径时抛出</exception>
        public ST_Loc GetAbsolutePath(ST_Loc baseLoc)
        {
            // 如果当前路径为空或为当前路径，直接返回基准位置
            if (_path == "." || string.IsNullOrEmpty(_path))
                return baseLoc;

            // 确保基准路径始终基于根目录，这样才能正确解析多个..
            // 使用List来保持路径顺序，比Stack更容易处理
            var fullPath = new List<string>();

            // 首先处理基准路径
            if (baseLoc.Path != ".")
            {
                var baseParts = baseLoc.Path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var part in baseParts)
                {
                    // 基准路径中不应出现"."或".."，若出现则抛出异常
                    if (part == "." || part == "..")
                    {
                        throw new ArgumentException("基准路径不应包含.或..", nameof(baseLoc));
                    }
                    fullPath.Add(part);
                }
            }

            // 然后处理当前路径
            var currentParts = _path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in currentParts)
            {
                if (part == ".")
                    continue;
                if (part == "..")
                {
                    if (fullPath.Count > 0)
                    {
                        fullPath.RemoveAt(fullPath.Count - 1);
                    }
                    else
                    {
                        // 相对路径超出基准路径，抛出异常
                        throw new ArgumentException("相对路径超出基准路径范围", nameof(_path));
                    }
                }
                else
                {
                    fullPath.Add(part);
                }
            }

            // 构建最终路径
            if (fullPath.Count == 0)
                return new ST_Loc(".");

            return new ST_Loc(string.Join("/", fullPath));
        }

        /// <summary>
        /// 静态方法：根据基准位置解析相对路径，生成实际的相对路径
        /// </summary>
        /// <param name="relativePath">相对路径</param>
        /// <param name="baseLoc">基准位置路径</param>
        /// <returns>解析后的实际相对路径</returns>
        public static ST_Loc GetAbsolutePath(string relativePath, string baseLoc)
        {
            return GetAbsolutePath(new ST_Loc(relativePath), new ST_Loc(baseLoc));
        }

        /// <summary>
        /// 静态方法：根据基准位置解析相对路径，生成实际的相对路径
        /// </summary>
        /// <param name="relativePath">相对路径</param>
        /// <param name="baseLoc">基准位置路径</param>
        /// <returns>解析后的实际相对路径</returns>
        public static ST_Loc GetAbsolutePath(ST_Loc relativePath, ST_Loc baseLoc)
        {
            return relativePath.GetAbsolutePath(baseLoc);
        }

        /// <summary>
        /// 根据基准路径获取当前路径的相对路径
        /// </summary>
        /// <param name="basePath">基准路径</param>
        /// <returns>相对于基准路径的相对路径</returns>
        /// <exception cref="ArgumentException">当目标路径和基准路径没有共同父路径时抛出</exception>
        public ST_Loc GetRelativePath(ST_Loc basePath)
        {
            // 如果基准路径为空或为当前目录，直接返回当前路径
            if (basePath._path == "." || string.IsNullOrEmpty(basePath._path))
            {
                return this;
            }

            // 如果当前路径为空或为当前目录，表示与基准路径在同一目录
            if (_path == "." || string.IsNullOrEmpty(_path))
            {
                return new ST_Loc(".");
            }

            // 检查基准路径是否是文件路径（包含扩展名）
            var basePathStr2 = basePath._path;
            var baseExtension2 = System.IO.Path.GetExtension(basePathStr2);
            if (!string.IsNullOrEmpty(baseExtension2))
            {
                // 如果是文件路径，获取其目录部分
                basePathStr2 = System.IO.Path.GetDirectoryName(basePathStr2) ?? string.Empty;
            }

            // 分解路径为部分
            string[] baseParts2 = basePathStr2.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            string[] currentParts = _path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            // 找到共同的父目录
            int commonIndex = 0;
            int minLength = Math.Min(baseParts2.Length, currentParts.Length);
            while (commonIndex < minLength && string.Equals(baseParts2[commonIndex], currentParts[commonIndex], StringComparison.Ordinal))
            {
                commonIndex++;
            }

            // 检查是否有共同父路径
            // 情况1：两个路径都有实际内容但没有共同父路径
            // 情况2：基准路径是文件（baseParts2为空）且目标路径有实际内容且两者不同
            // 情况3：两个路径都是文件且不同
            bool isBaseFile = !string.IsNullOrEmpty(System.IO.Path.GetExtension(basePathStr2));
            bool isCurrentFile = !string.IsNullOrEmpty(System.IO.Path.GetExtension(_path));
            
            if ((commonIndex == 0 && baseParts2.Length > 0 && currentParts.Length > 0) ||
                (baseParts2.Length == 0 && currentParts.Length > 0 && !string.IsNullOrEmpty(basePathStr2)) ||
                (isBaseFile && isCurrentFile && _path != basePath._path))
            {
                throw new ArgumentException("目标路径和基准路径没有共同父路径，无法计算相对路径", nameof(basePath));
            }

            // 构建相对路径
            List<string> relativeParts = new List<string>();
            
            // 添加基准路径中超出共同部分的部分对应的 ".."
            for (int i = commonIndex; i < baseParts2.Length; i++)
            {
                relativeParts.Add("..");
            }
            
            // 添加当前路径中超出共同部分的部分
            for (int i = commonIndex; i < currentParts.Length; i++)
            {
                relativeParts.Add(currentParts[i]);
            }

            // 构建最终路径
            if (relativeParts.Count == 0)
            {
                return new ST_Loc(".");
            }
            
            return new ST_Loc(string.Join("/", relativeParts));
        }

        /// <summary>
        /// 静态方法：根据基准路径获取相对路径
        /// </summary>
        /// <param name="targetPath">目标路径</param>
        /// <param name="basePath">基准路径</param>
        /// <returns>相对于基准路径的相对路径</returns>
        /// <exception cref="ArgumentException">当目标路径和基准路径没有共同父路径时抛出</exception>
        public static ST_Loc GetRelativePath(ST_Loc targetPath, ST_Loc basePath)
        {
            return targetPath.GetRelativePath(basePath);
        }

        /// <summary>
        /// 静态方法：根据基准路径获取相对路径
        /// </summary>
        /// <param name="targetPath">目标路径</param>
        /// <param name="basePath">基准路径</param>
        /// <returns>相对于基准路径的相对路径</returns>
        public static ST_Loc GetRelativePath(string targetPath, string basePath)
        {
            return new ST_Loc(targetPath).GetRelativePath(new ST_Loc(basePath));
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


        public override string ToString() => _path ?? "";

        // 其余接口和运算符重载可按需保留
        public bool Equals(ST_Loc other) => string.Equals(_path, other._path, StringComparison.Ordinal);
        public override bool Equals(object obj) => obj is ST_Loc other && Equals(other);
        public override int GetHashCode() => _path?.GetHashCode() ?? 0;
        public static bool operator ==(ST_Loc left, ST_Loc right) => left.Equals(right);
        public static bool operator !=(ST_Loc left, ST_Loc right) => !left.Equals(right);
    }

}
