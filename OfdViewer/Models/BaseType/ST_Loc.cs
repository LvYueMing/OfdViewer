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
        private readonly string _path;

        // C# 10+ 支持 struct 的无参数构造函数
        public ST_Loc()
        {
            _path = ".";
        }

        public ST_Loc(string path)
        {
            if (string.IsNullOrEmpty(path) || path == ".")
            {
                _path = ".";
            }
            else
            {
                // 先将所有 \ 替换为 /
                path = path.Replace('\\', '/');

                // OFD文件内部路径没有绝对路径概念，将所有路径统一处理为相对路径
                // 以/开头的路径视为从根目录开始的相对路径
                if (path.StartsWith("/"))
                {
                    // 移除开头的/，并规范化路径
                    path = path.Substring(1);
                }

                _path = NormalizePath(path);
            }
        }

        public string Path => _path;

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
        /// <returns>解析后的实际相对路径</returns>
        /// <summary>
        public ST_Loc Resolve(ST_Loc currentLoc)
        {
            // 如果当前路径为空或为当前路径，直接返回基准位置
            if (_path == "." || string.IsNullOrEmpty(_path))
                return currentLoc;


            // 确保基准路径始终基于根目录，这样才能正确解析多个..
            // 使用List来保持路径顺序，比Stack更容易处理
            var fullPath = new List<string>();

            // 首先处理基准路径
            if (currentLoc.Path != ".")
            {
                var baseParts = currentLoc.Path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var part in baseParts)
                {
                    // 基准路径中不应出现".."，若出现则直接忽略
                    if (part == ".")
                        continue;
                    if (part == "..")
                    {
                        // 基准路径已基于根目录，无需回退，直接跳过
                        continue;
                    }
                    else
                    {
                        fullPath.Add(part);
                    }
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
                        fullPath.RemoveAt(fullPath.Count - 1);
                    // 避免超出根目录的情况，不添加更多的..
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
        public static ST_Loc Resolve(string relativePath, string baseLoc)
        {
            return Resolve(new ST_Loc(relativePath), new ST_Loc(baseLoc));
        }

        /// <summary>
        /// 静态方法：根据基准位置解析相对路径，生成实际的相对路径
        /// </summary>
        /// <param name="relativePath">相对路径</param>
        /// <param name="baseLoc">基准位置路径</param>
        /// <returns>解析后的实际相对路径</returns>
        public static ST_Loc Resolve(ST_Loc relativePath, ST_Loc baseLoc)
        {
            return relativePath.Resolve(baseLoc);
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


        public override string ToString() => _path ?? ".";

        // 其余接口和运算符重载可按需保留
        public bool Equals(ST_Loc other) => string.Equals(_path, other._path, StringComparison.Ordinal);
        public override bool Equals(object obj) => obj is ST_Loc other && Equals(other);
        public override int GetHashCode() => _path?.GetHashCode() ?? 0;
        public static bool operator ==(ST_Loc left, ST_Loc right) => left.Equals(right);
        public static bool operator !=(ST_Loc left, ST_Loc right) => !left.Equals(right);
    }

}
