using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace OFDViewer.OFD
{
    public class OFDArchive : IDisposable
    {
        private ZipArchive _zipArchive;
        // 归档对应的文件流（文件路径模式下使用）
        private Stream _fileStream;
        // 归档对应的自定义流（流模式下使用）
        private Stream _customStream;
        private readonly ConcurrentDictionary<string, ZipArchiveEntry> _entryCache;
        private readonly ConcurrentDictionary<string, XmlDocument> _xmlCache;
        private readonly string _tempExtractPath;

        // 归档是否已保存（避免重复保存）
        private bool _saved;
        // 资源释放标记
        private bool _disposed;
        // 是否保持自定义流打开（流模式专用）
        private bool _leaveOpen;


        #region 打开归档（读取模式）
        /// <summary>
        /// 打开 OFD 文件
        /// </summary>
        /// <param name="filePath">OFD 文件路径</param>
        /// <param name="mode">打开模式</param>
        public static OFDArchive OpenFromFile(string filePath)
        {
            var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return new OFDArchive(stream, ZipArchiveMode.Read, leaveOpen: false);
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
            return new OFDArchive(stream, mode, leaveOpen);
        }


        /// <summary>
        /// 从流打开 OFD 文件
        /// </summary>
        public static OFDArchive OpenFromStream(Stream stream)
        {
            return new OFDArchive(stream, ZipArchiveMode.Read, leaveOpen: false);
        }

        /// <summary>
        /// 从流打开 OFD 文件
        /// </summary>
        /// <param name="mode">打开模式</param>
        /// <param name="leaveOpen">false(默认):释放ZipArchive时自动释放stream; true:释放ZipArchive时不自动释放stream </param>
        public static OFDArchive OpenFromStream(Stream stream, ZipArchiveMode mode = ZipArchiveMode.Read, bool leaveOpen = false)
        {
            return new OFDArchive(stream, mode, leaveOpen);
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
            return new OFDArchive(stream, ZipArchiveMode.Create, leaveOpen: false);
        }

        /// <summary>
        /// 从流创建OFD归档（写入模式）
        /// </summary>
        /// <param name="stream">输出流</param>
        /// <param name="leaveOpen">false(默认):释放ZipArchive时自动释放stream; true:释放ZipArchive时不自动释放stream </param>
        /// <returns>OFD归档对象</returns>
        public static OFDArchive CreateFromStream(Stream stream, bool leaveOpen)
        {
            return new OFDArchive(stream, ZipArchiveMode.Create, leaveOpen);
        }
        #endregion


        private OFDArchive(FileStream stream, ZipArchiveMode mode, bool leaveOpen)
        {
            _fileStream = stream;
            _leaveOpen= leaveOpen;
            _zipArchive = new ZipArchive(stream, mode, leaveOpen);
            _entryCache = new ConcurrentDictionary<string, ZipArchiveEntry>();
            _xmlCache = new ConcurrentDictionary<string, XmlDocument>();

            // 预加载所有条目到缓存
            if (mode == ZipArchiveMode.Read)
            {
                foreach (var entry in _zipArchive.Entries)
                {
                    _entryCache.TryAdd(NormalizePath(entry.FullName), entry);
                }
            }
        }

        private OFDArchive(Stream stream, ZipArchiveMode mode, bool leaveOpen)
        {
            _customStream = stream;
            _leaveOpen = leaveOpen;
            _zipArchive = new ZipArchive(stream, mode, leaveOpen);
            _entryCache = new ConcurrentDictionary<string, ZipArchiveEntry>();
            _xmlCache = new ConcurrentDictionary<string, XmlDocument>();

            // 预加载所有条目到缓存
            if (mode == ZipArchiveMode.Read)
            {
                foreach (var entry in _zipArchive.Entries)
                {
                    _entryCache.TryAdd(NormalizePath(entry.FullName), entry);
                }
            }
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
        public Stream GetFileStream(string filePath)
        {
            if (_entryCache.TryGetValue(NormalizePath(filePath), out var entry))
            {
                return entry.Open();
            }
            throw new FileNotFoundException($"文件未找到: {filePath}");
        }

        /// <summary>
        /// 读取文本文件内容
        /// </summary>
        public string ReadTextFile(string filePath, Encoding encoding = null)
        {
            using (var stream = GetFileStream(filePath))
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
            return _xmlCache.GetOrAdd(filePath, path =>
            {
                var xmlDoc = new XmlDocument();
                using (var stream = GetFileStream(path))
                {
                    var settings = new XmlReaderSettings
                    {
                        DtdProcessing = DtdProcessing.Ignore,
                        XmlResolver = null, // 禁用外部解析，提高安全性
                        IgnoreComments = true,
                        IgnoreWhitespace = true
                    };

                    using (var reader = XmlReader.Create(stream, settings))
                    {
                        xmlDoc.Load(reader);
                    }
                }
                return xmlDoc;
            });
        }

        /// <summary>
        /// 解压整个 OFD 文件到临时目录（用于调试或完整分析）
        /// </summary>
        public string ExtractToTempDirectory()
        {
            var tempPath = _tempExtractPath ?? Path.Combine(Path.GetTempPath(), $"OFD_{Guid.NewGuid():N}");
            Directory.CreateDirectory(tempPath);
            foreach (var entry in _zipArchive.Entries)
            {
                var targetPath = Path.Combine(tempPath, entry.FullName);
                var targetDir = Path.GetDirectoryName(targetPath);

                if (!string.IsNullOrEmpty(targetDir) && !Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }

                if (!entry.FullName.EndsWith("/")) // 不是目录
                {
                    entry.ExtractToFile(targetPath, overwrite: true);
                }
            }

            return tempPath;
        }

        /// <summary>
        /// 规范化路径，统一使用正斜杠
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private string NormalizePath(string path)
        {
            return path.Replace('\\', '/').TrimStart('/');
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
                    // 托管资源释放：如果未手动Save，自动调用Save确保数据不丢失
                    if (!_saved)
                    {
                        try
                        {
                            Save();
                        }
                        catch
                        {
                            // 捕获异常，避免Dispose抛异常导致程序崩溃
                        }
                    }

                    // 释放剩余资源
                    _zipArchive?.Dispose();
                    _fileStream?.Dispose();
                    if (!_leaveOpen)
                        _customStream?.Dispose();
                    _entryCache.Clear();
                    _xmlCache.Clear();
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
