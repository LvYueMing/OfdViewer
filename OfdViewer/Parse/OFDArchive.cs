using System.Collections.Concurrent;
using System.IO.Compression;
using System.Text;
using System.Xml;

namespace OFDViewer.Parse
{
    /// <summary>
    /// OFD 归档处理类，负责 OFD 文档的 ZIP 压缩、解压、文件管理等底层操作
    /// </summary>
    /// <remarks>
    /// 支持两种模式：
    /// 1. 文件模式：通过文件路径打开或创建 OFD 文档
    /// 2. 流模式：通过 Stream 对象打开或创建 OFD 文档
    /// 提供 XML 文档缓存机制，提高处理效率
    /// </remarks>
    public class OFDArchive : IDisposable
    {
        /// <summary>
        /// ZIP 归档对象
        /// </summary>
        private ZipArchive _zipArchive;
        
        /// <summary>
        /// 归档对应的文件流（文件路径模式下使用）
        /// </summary>
        private Stream? _fileStream;
        
        /// <summary>
        /// 归档对应的自定义流（流模式下使用）
        /// </summary>
        private Stream? _customStream;
        
        /// <summary>
        /// ZIP 条目缓存，提高文件查找效率
        /// </summary>
        private readonly ConcurrentDictionary<string, ZipArchiveEntry> _entryCache;
        
        /// <summary>
        /// XML 文档缓存，避免重复解析 XML 文件
        /// </summary>
        private readonly ConcurrentDictionary<string, XmlDocument> _xmlCache;

        /// <summary>
        /// 读取不可信归档时使用的实例级资源限制。
        /// </summary>
        private readonly OFDArchiveReadLimits _readLimits;

        private readonly object _xmlCacheLock = new object();

        /// <summary>
        /// 已缓存 XML 的原始字节总量，在 <see cref="_xmlCacheLock"/> 内访问。
        /// </summary>
        private long _cachedXmlBytes;

        /// <summary>
        /// 归档是否已保存（避免重复保存）
        /// </summary>
        private bool _saved;
        
        /// <summary>
        /// 资源释放标记
        /// </summary>
        private bool _disposed;
        
        /// <summary>
        /// 是否保持自定义流打开（流模式专用）
        /// </summary>
        private bool _leaveOpen;


        #region 构造函数

        private OFDArchive(FileStream stream, ZipArchiveMode mode, bool leaveOpen, OFDArchiveReadLimits readLimits)
        {
            _readLimits = readLimits ?? throw new ArgumentNullException(nameof(readLimits));
            _fileStream = stream;
            _leaveOpen = leaveOpen;
            _zipArchive = new ZipArchive(stream, mode, leaveOpen);
            _entryCache = new ConcurrentDictionary<string, ZipArchiveEntry>();
            _xmlCache = new ConcurrentDictionary<string, XmlDocument>();

            // 预加载所有条目到缓存
            if (mode == ZipArchiveMode.Read)
            {
                try
                {
                    EnsureArchiveEntryCountWithinLimit();
                    foreach (var entry in _zipArchive.Entries)
                    {
                        _entryCache.TryAdd(NormalizePath(entry.FullName), entry);
                    }
                }
                catch
                {
                    _zipArchive.Dispose();
                    stream.Dispose();
                    throw;
                }
            }
        }

        private OFDArchive(Stream stream, ZipArchiveMode mode, bool leaveOpen, OFDArchiveReadLimits readLimits)
        {
            _readLimits = readLimits ?? throw new ArgumentNullException(nameof(readLimits));
            _customStream = stream;
            _leaveOpen = leaveOpen;
            _zipArchive = new ZipArchive(stream, mode, leaveOpen);
            _entryCache = new ConcurrentDictionary<string, ZipArchiveEntry>();
            _xmlCache = new ConcurrentDictionary<string, XmlDocument>();

            // 预加载所有条目到缓存
            if (mode == ZipArchiveMode.Read)
            {
                try
                {
                    EnsureArchiveEntryCountWithinLimit();
                    foreach (var entry in _zipArchive.Entries)
                    {
                        _entryCache.TryAdd(NormalizePath(entry.FullName), entry);
                    }
                }
                catch
                {
                    _zipArchive.Dispose();
                    throw;
                }
            }
        }

        #endregion


        #region 打开归档（读取模式）
        /// <summary>
        /// 打开 OFD 文件
        /// </summary>
        /// <param name="filePath">OFD 文件路径</param>
        /// <param name="mode">打开模式</param>
        public static OFDArchive OpenFromFile(string filePath)
        {
            var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return new OFDArchive(stream, ZipArchiveMode.Read, leaveOpen: false, OFDArchiveReadLimits.Default);
        }

        /// <summary>
        /// 使用指定资源限制打开 OFD 文件。
        /// </summary>
        /// <param name="filePath">OFD 文件路径。</param>
        /// <param name="readLimits">读取不可信归档时应用的资源限制。</param>
        /// <returns>以只读模式打开的 OFD 归档。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="readLimits"/> 为 <see langword="null"/>。</exception>
        public static OFDArchive OpenFromFile(string filePath, OFDArchiveReadLimits readLimits)
        {
            ArgumentNullException.ThrowIfNull(readLimits);
            var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return new OFDArchive(stream, ZipArchiveMode.Read, leaveOpen: false, readLimits);
        }

        /// <summary>
        /// 打开 OFD 文件
        /// </summary>
        /// <param name="filePath">OFD 文件路径</param>
        /// <param name="mode">打开模式</param>
        /// <param name="leaveOpen">false(默认):释放ZipArchive时自动释放stream; true:释放ZipArchive时不自动释放stream </param>
        public static OFDArchive OpenFromFile(string filePath, ZipArchiveMode mode = ZipArchiveMode.Read, bool leaveOpen = false)
        {
            var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return new OFDArchive(stream, mode, leaveOpen, OFDArchiveReadLimits.Default);
        }


        /// <summary>
        /// 从流打开 OFD 文件
        /// </summary>
        public static OFDArchive OpenFromStream(Stream stream)
        {
            return new OFDArchive(stream, ZipArchiveMode.Read, leaveOpen: false, OFDArchiveReadLimits.Default);
        }

        /// <summary>
        /// 使用指定资源限制从流打开 OFD 文件。
        /// </summary>
        /// <param name="stream">包含 OFD ZIP 数据的可读流。</param>
        /// <param name="readLimits">读取不可信归档时应用的资源限制。</param>
        /// <param name="leaveOpen">释放归档后是否保持调用方流打开。</param>
        /// <returns>以只读模式打开的 OFD 归档。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="readLimits"/> 为 <see langword="null"/>。</exception>
        public static OFDArchive OpenFromStream(
            Stream stream,
            OFDArchiveReadLimits readLimits,
            bool leaveOpen = false)
        {
            return new OFDArchive(stream, ZipArchiveMode.Read, leaveOpen, readLimits);
        }

        /// <summary>
        /// 从流打开 OFD 文件
        /// </summary>
        /// <param name="mode">打开模式</param>
        /// <param name="leaveOpen">false(默认):释放ZipArchive时自动释放stream; true:释放ZipArchive时不自动释放stream </param>
        public static OFDArchive OpenFromStream(Stream stream, ZipArchiveMode mode = ZipArchiveMode.Read, bool leaveOpen = false)
        {
            return new OFDArchive(stream, mode, leaveOpen, OFDArchiveReadLimits.Default);
        }

        #endregion


        #region 创建归档（写入模式）
        /// <summary>
        /// 从文件路径创建OFD归档（写入模式）
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>OFD归档对象</returns>
        public static OFDArchive CreateFromFile(string filePath)
        {
            var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            return new OFDArchive(stream, ZipArchiveMode.Create, leaveOpen: false, OFDArchiveReadLimits.Default);
        }

        /// <summary>
        /// 从流创建OFD归档（写入模式）
        /// </summary>
        /// <param name="stream">输出流</param>
        /// <param name="leaveOpen">false(默认):释放ZipArchive时自动释放stream; true:释放ZipArchive时不自动释放stream </param>
        /// <returns>OFD归档对象</returns>
        public static OFDArchive CreateFromStream(Stream stream, bool leaveOpen)
        {
            return new OFDArchive(stream, ZipArchiveMode.Create, leaveOpen, OFDArchiveReadLimits.Default);
        }

        #endregion

        /// <summary>
        /// 检查文件是否存在于OFD归档内
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns>文件是否存在</returns>
        public bool FileExists(string path)
        {
            if (_zipArchive == null)
                return false;

            // 使用 DirectoryExists 检查路径是否为目录
            if (DirectoryExists(path))
            {
                return false;
            }

            var normalizedPath = NormalizePath(path);
            return _entryCache.ContainsKey(normalizedPath);
        }

        /// <summary>
        /// 检查目录是否存在于OFD归档内
        /// </summary>
        /// <param name="path">目录路径</param>
        /// <returns>目录是否存在</returns>
        public bool DirectoryExists(string path)
        {
            if (_zipArchive == null)
                return false;
            var normalizedPath = NormalizePath(path).TrimEnd('/') + '/';
            return _entryCache.Keys.Any(key => key.StartsWith(normalizedPath, StringComparison.OrdinalIgnoreCase));
        }


        /// <summary>
        /// 获取指定目录下的【直接子文件/子文件夹】（不递归）
        /// </summary>
        /// <param name="path">ZIP 内的目标目录路径</param>
        /// <returns>直接子项的名称列表（文件夹以 / 结尾）</returns>
        public List<string> GetDirectEntryNamesInDirectory(string path)
        {
            // 校验 ZIP 归档是否有效
            if (_zipArchive == null)
                throw new InvalidOperationException("ZIP归档已释放，无法获取目录内容");

            // 规范化路径，确保以 / 结尾（如 "docs" → "docs/"）
            var normalizedPath = NormalizePath(path);
            var targetPrefix = string.IsNullOrEmpty(normalizedPath) ? string.Empty : $"{normalizedPath}/";

            var entries = new HashSet<string>(); // 使用 HashSet 自动去重，避免重复添加子目录


            foreach (var entry in _zipArchive.Entries)
            {
                var entryFullName = entry.FullName;

                // 跳过非目标目录下的条目
                if (!entryFullName.StartsWith(targetPrefix, StringComparison.Ordinal))
                    continue;

                // 获取相对路径，即去掉目录前缀，得到文件或子目录名称
                // 例： docs/test.txt => test.txt
                // 例： docs/subdir/file.txt => subdir/file.txt
                // 例： docs/subdir/signs => subdir/signs
                var relativePath = entryFullName.Substring(targetPrefix.Length);
                if (string.IsNullOrEmpty(relativePath))
                    continue;

                // 查找第一个 / 的位置，判断是直接子项还是深层子项
                var firstSeparatorIndex = relativePath.IndexOf('/');
                if (firstSeparatorIndex == 0)
                    continue; // 排除空路径（理论上不会出现）


                if (firstSeparatorIndex > 0)
                {
                    // 是子目录
                    // 例： docs/subdir/file.txt => subdir
                    // 例： docs/subdir/signs => subdir
                    var dirName = relativePath.Substring(0, firstSeparatorIndex);
                    entries.Add(dirName);
                }
                else
                {
                    // 是直接文件
                    // 如  docs/test.txt => test.txt
                    entries.Add(relativePath);
                }
            }

            // 转换为 List 并返回（保持顺序，也可按需排序）
            return entries.ToList();
        }


        /// <summary>
        /// 获取指定目录下的【所有层级文件/文件夹】（递归）
        /// </summary>
        /// <param name="path">ZIP 内的目标目录路径</param>
        /// <returns>所有子项的完整相对路径列表</returns>
        public List<string> GetAllEntryNamesInDirectory(string path)
        {
            // 校验 ZIP 归档是否有效
            if (_zipArchive == null)
                throw new InvalidOperationException("ZIP归档已释放，无法获取目录内容");

            // 规范化路径，确保以 / 结尾
            var normalizedPath = NormalizePath(path);
            var targetPrefix = string.IsNullOrEmpty(normalizedPath) ? string.Empty : $"{normalizedPath}/";

            var entries = new List<string>();

            foreach (var entry in _zipArchive.Entries)
            {
                var entryFullName = entry.FullName;

                // 跳过非目标目录下的条目
                if (!entryFullName.StartsWith(targetPrefix, StringComparison.Ordinal))
                    continue;

                // 获取相对路径，即去掉目录前缀，得到文件或子目录名称
                var relativePath = entryFullName.Substring(targetPrefix.Length);
                if (string.IsNullOrEmpty(relativePath))
                    continue;

                entries.Add(relativePath);
            }

            return entries;
        }


        /// <summary>
        /// 获取指定目录下的【直接子文件,包含路径】（不递归）
        /// </summary>
        /// <param name="path">ZIP 内的目标目录路径</param>
        /// <returns>直接子文件的完整路径列表</returns>
        public List<string> GetDirectFilePathsInDirectory(string path)
        {
            // 校验 ZIP 归档是否有效
            if (_zipArchive == null)
                throw new InvalidOperationException("ZIP归档已释放，无法获取目录内容");

            // 规范化路径，确保以 / 结尾（如 "docs" → "docs/"）
            var normalizedPath = NormalizePath(path);
            var targetPrefix = string.IsNullOrEmpty(normalizedPath) ? string.Empty : $"{normalizedPath}/";

            var entries = new HashSet<string>(); // 使用 HashSet 自动去重，避免重复添加子目录

            foreach (var entryName in _entryCache.Keys)
            {
                // 跳过非目标目录下的条目
                if (!entryName.StartsWith(targetPrefix, StringComparison.Ordinal))
                    continue;

                // 获取相对路径，即去掉目录前缀，得到文件或子目录名称
                // 例： docs/test.txt => test.txt
                // 例： docs/subdir/file.txt => subdir/file.txt
                // 例： docs/subdir/signs => subdir/signs
                var relativePath = entryName.Substring(targetPrefix.Length);
                if (string.IsNullOrEmpty(relativePath))
                    continue;

                // 查找第一个 / 的位置，判断是直接子项还是深层子项
                var firstSeparatorIndex = relativePath.IndexOf('/');
                if (firstSeparatorIndex == 0)
                    continue; // 排除空路径（理论上不会出现）

                // 是直接文件
                // 如  docs/test.txt => test.txt
                if (firstSeparatorIndex < 0)
                {
                    entries.Add(entryName);
                }
            }

            // 转换为 List 并返回（保持顺序，也可按需排序）
            return entries.ToList();
        }



        /// <summary>
        /// 创建目录（OFD归档内虚拟目录）
        /// </summary>
        /// <param name="path"></param>
        /// <exception cref="InvalidOperationException"></exception>
        private void CreateDirectory(string path)
        {
            if (_zipArchive == null)
                throw new InvalidOperationException("OFD归档已释放，无法创建目录");

            var entry = _zipArchive.CreateEntry(path);
            entry.Open();
        }


        /// <summary>
        /// 创建OFD文件写入流
        /// </summary>
        /// <param name="fileName">文件名称（相对路径）</param>
        /// <returns>写入流</returns>
        public Stream CreateFileStream(string fileName)
        {
            if (_zipArchive == null)
                throw new InvalidOperationException("OFD归档已释放，无法创建文件流");

            var entry = _zipArchive.CreateEntry(fileName);
            return entry.Open();
        }


        /// <summary>
        /// 获取文件内容流
        /// </summary>
        public Stream OpenFileStream(string filePath)
        {
            CheckDisposed();
            if (_entryCache.TryGetValue(NormalizePath(filePath), out var entry))
            {
                if (entry.Length > _readLimits.MaxEntryBytes)
                {
                    throw new InvalidDataException(
                        $"归档条目 {filePath} 的未压缩大小 {entry.Length} 字节超过限制：{_readLimits.MaxEntryBytes} 字节");
                }

                return new LimitedReadStream(entry.Open(), _readLimits.MaxEntryBytes);
            }
            throw new FileNotFoundException($"文件未找到: {filePath}");
        }

        /// <summary>
        /// 读取文本文件内容
        /// </summary>
        /// <param name="filePath">归档内文件路径。</param>
        /// <param name="encoding">文本编码；未指定时使用 UTF-8。</param>
        /// <returns>完整的文本内容。</returns>
        public string ReadTextFile(string filePath, Encoding? encoding = null)
        {
            using (var stream = OpenFileStream(filePath))
            using (var reader = new StreamReader(stream, encoding ?? Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        /// <summary>
        /// 读取并解析 XML 文件
        /// </summary>
        public XmlDocument ReadXmlFile(string filePath)
        {
            string normalizedPath = NormalizePath(filePath);
            if (_xmlCache.TryGetValue(normalizedPath, out var cachedDocument))
                return cachedDocument;

            var xmlDoc = new XmlDocument();
            long xmlBytes;
            using (var stream = OpenFileStream(normalizedPath))
            using (var xmlBuffer = new MemoryStream())
            {
                stream.CopyTo(xmlBuffer);
                xmlBytes = xmlBuffer.Length;
                xmlBuffer.Position = 0;

                var settings = CreateSecureXmlReaderSettings();
                using (var depthReader = XmlReader.Create(xmlBuffer, settings))
                {
                    while (depthReader.Read())
                    {
                        if (depthReader.Depth > _readLimits.MaxXmlDepth)
                        {
                            throw new XmlException(
                                $"XML 元素嵌套深度超过限制：{_readLimits.MaxXmlDepth}");
                        }
                    }
                }

                xmlBuffer.Position = 0;
                using var reader = XmlReader.Create(xmlBuffer, settings);
                xmlDoc.Load(reader);
            }

            lock (_xmlCacheLock)
            {
                if (_xmlCache.TryGetValue(normalizedPath, out cachedDocument))
                    return cachedDocument;
                if (_xmlCache.Count >= _readLimits.MaxCachedXmlDocuments)
                {
                    throw new InvalidDataException(
                        $"XML 缓存文档数超过限制：{_readLimits.MaxCachedXmlDocuments}");
                }
                if (xmlBytes > _readLimits.MaxCachedXmlBytes - _cachedXmlBytes)
                {
                    throw new InvalidDataException(
                        $"XML 缓存总字节数超过限制：{_readLimits.MaxCachedXmlBytes} 字节");
                }

                _xmlCache[normalizedPath] = xmlDoc;
                _cachedXmlBytes += xmlBytes;
                return xmlDoc;
            }
        }

        private XmlReaderSettings CreateSecureXmlReaderSettings()
        {
            return new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null,
                MaxCharactersInDocument = _readLimits.MaxXmlCharacters,
                IgnoreComments = true,
                IgnoreWhitespace = true
            };
        }

        /// <summary>
        /// 解压整个 OFD 文件到临时目录（用于调试或完整分析）
        /// </summary>
        public string ExtractToTempDirectory()
        {
            return ExtractToTempDirectory(
                _readLimits.MaxExtractedBytes,
                _readLimits.MaxArchiveEntryCount);
        }

        /// <summary>
        /// 在条目数量和解压总字节数限制内，将 OFD 内容安全解压到新的临时目录。
        /// </summary>
        /// <param name="maxExtractedBytes">允许写出的最大未压缩字节数。</param>
        /// <param name="maxEntryCount">允许处理的最大 ZIP 条目数。</param>
        /// <exception cref="ArgumentOutOfRangeException">限制参数不是正数。</exception>
        /// <exception cref="InvalidDataException">条目越界或归档超过资源限制。</exception>
        public string ExtractToTempDirectory(long maxExtractedBytes, int maxEntryCount)
        {
            CheckDisposed();
            if (maxExtractedBytes <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxExtractedBytes));
            if (maxEntryCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxEntryCount));

            var archive = _zipArchive ?? throw new InvalidOperationException("ZIP 归档已释放，无法解压");
            if (archive.Entries.Count > maxEntryCount)
                throw new InvalidDataException($"ZIP 条目数超过限制：{maxEntryCount}");

            string tempPath = Path.GetFullPath(
                Path.Combine(Path.GetTempPath(), $"OFD_{Guid.NewGuid():N}"));
            string allowedPrefix = tempPath.TrimEnd(Path.DirectorySeparatorChar)
                + Path.DirectorySeparatorChar;
            long extractedBytes = 0;

            Directory.CreateDirectory(tempPath);
            try
            {
                foreach (var entry in archive.Entries)
                {
                    string relativePath = entry.FullName
                        .Replace('/', Path.DirectorySeparatorChar)
                        .Replace('\\', Path.DirectorySeparatorChar);
                    if (string.IsNullOrWhiteSpace(relativePath) || Path.IsPathRooted(relativePath))
                        throw new InvalidDataException($"ZIP 条目路径无效：{entry.FullName}");

                    string targetPath = Path.GetFullPath(Path.Combine(tempPath, relativePath));
                    if (!targetPath.StartsWith(allowedPrefix, GetPathComparison()))
                        throw new InvalidDataException($"ZIP 条目试图写出解压目录：{entry.FullName}");

                    bool isDirectory = entry.FullName.EndsWith("/", StringComparison.Ordinal)
                        || entry.FullName.EndsWith("\\", StringComparison.Ordinal);
                    if (isDirectory)
                    {
                        Directory.CreateDirectory(targetPath);
                        continue;
                    }

                    if (entry.Length > maxExtractedBytes - extractedBytes)
                        throw new InvalidDataException($"ZIP 解压总大小超过限制：{maxExtractedBytes} 字节");

                    string? targetDir = Path.GetDirectoryName(targetPath);
                    if (!string.IsNullOrEmpty(targetDir))
                        Directory.CreateDirectory(targetDir);

                    using var source = entry.Open();
                    using var destination = new FileStream(targetPath, FileMode.Create, FileAccess.Write, FileShare.None);
                    extractedBytes = CopyWithLimit(source, destination, extractedBytes, maxExtractedBytes);
                }

                return tempPath;
            }
            catch
            {
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, recursive: true);
                throw;
            }
        }

        private static long CopyWithLimit(
            Stream source,
            Stream destination,
            long extractedBytes,
            long maxExtractedBytes)
        {
            byte[] buffer = new byte[81920];
            int read;
            while ((read = source.Read(buffer, 0, buffer.Length)) > 0)
            {
                if (read > maxExtractedBytes - extractedBytes)
                    throw new InvalidDataException($"ZIP 解压总大小超过限制：{maxExtractedBytes} 字节");

                destination.Write(buffer, 0, read);
                extractedBytes += read;
            }

            return extractedBytes;
        }

        private static StringComparison GetPathComparison()
        {
            return OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;
        }

        private void EnsureArchiveEntryCountWithinLimit()
        {
            if (_zipArchive.Entries.Count > _readLimits.MaxArchiveEntryCount)
            {
                throw new InvalidDataException(
                    $"ZIP 条目数 {_zipArchive.Entries.Count} 超过限制：{_readLimits.MaxArchiveEntryCount}");
            }
        }

        /// <summary>
        /// 路径规范化（确保统一的路径分隔符为 /）
        /// </summary>
        /// <param name="path">原始路径</param>
        /// <returns>规范化后的路径</returns>
        private string NormalizePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return string.Empty;

            // 将 \ 替换为 /，并去除首尾多余的 /
            return path.Replace('\\', '/').Trim('/');
        }


        /// <summary>
        /// 保存归档（核心方法）：将内存中所有ZIP条目刷入文件/流，完成物理文件生成
        /// </summary>
        public void Save()
        {
            CheckDisposed();

            // 避免重复保存
            if (_saved)
                return;

            try
            {

                // 核心逻辑：释放ZIPArchive会自动将缓存数据刷入底层流
                // 注意：ZipArchive的Dispose会触发数据刷写，这是.NET内置逻辑
                if (_zipArchive != null)
                {
                    _zipArchive?.Dispose();
                    _zipArchive = null;
                }

                // 处理流：仅当 leaveOpen=true 时，才手动刷流/释放（避免操作已关闭的流）
                if (_leaveOpen)
                {
                    // 刷文件流（仅leaveOpen=true时流未关闭）
                    if (_fileStream != null && _fileStream.CanWrite)
                    {
                        _fileStream.Flush();
                        if (!_leaveOpen) _fileStream.Dispose();
                        _fileStream = null;
                    }
                    // 刷自定义流
                    if (_customStream != null && _customStream.CanWrite)
                    {
                        _customStream.Flush();
                        if (!_leaveOpen) _customStream.Dispose();
                        _customStream = null;
                    }
                }

                // 标记为已保存
                _saved = true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("保存OFD归档失败", ex);
            }
        }


        #region 辅助方法
        /// <summary>
        /// 检查归档是否已释放，若已释放则抛异常
        /// </summary>
        private void CheckDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(OFDArchive), "OFD归档已释放，无法执行操作");
        }
        #endregion


        #region IDisposable实现
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // 释放剩余资源（不自动保存，避免在渲染解析时不必要地保存文档）
                    _zipArchive?.Dispose();
                    _fileStream?.Dispose();
                    if (!_leaveOpen)
                        _customStream?.Dispose();
                    _entryCache.Clear();
                    lock (_xmlCacheLock)
                    {
                        _xmlCache.Clear();
                        _cachedXmlBytes = 0;
                    }
                }

                // 标记资源已释放
                _disposed = true;
                _saved = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~OFDArchive()
        {
            Dispose(false);
        }
        #endregion
    }
}
