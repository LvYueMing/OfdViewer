namespace OFDViewer.Parse
{
    /// <summary>
    /// 控制读取不可信 OFD 归档时允许消耗的资源上限。
    /// </summary>
    public sealed class OFDArchiveReadLimits
    {
        /// <summary>
        /// 默认读取限制。该实例不可变，可安全复用。
        /// </summary>
        public static OFDArchiveReadLimits Default { get; } = new OFDArchiveReadLimits();

        /// <summary>
        /// 归档允许包含的最大条目数。
        /// </summary>
        public int MaxArchiveEntryCount { get; }

        /// <summary>
        /// 单个条目允许读取的最大未压缩字节数。
        /// </summary>
        public long MaxEntryBytes { get; }

        /// <summary>
        /// 显式解压允许写出的最大未压缩总字节数。
        /// </summary>
        public long MaxExtractedBytes { get; }

        /// <summary>
        /// 单个 XML 文档允许解析的最大字符数。
        /// </summary>
        public long MaxXmlCharacters { get; }

        /// <summary>
        /// 单个 XML 文档允许出现的最大元素嵌套深度。
        /// </summary>
        public int MaxXmlDepth { get; }

        /// <summary>
        /// 单个归档允许缓存的最大 XML 文档数。
        /// </summary>
        public int MaxCachedXmlDocuments { get; }

        /// <summary>
        /// 单个归档允许缓存的 XML 原始内容最大总字节数。
        /// </summary>
        public long MaxCachedXmlBytes { get; }

        /// <summary>
        /// 创建一组不可变的 OFD 归档读取限制。
        /// </summary>
        /// <param name="maxArchiveEntryCount">归档允许包含的最大条目数。</param>
        /// <param name="maxEntryBytes">单个条目允许读取的最大未压缩字节数。</param>
        /// <param name="maxExtractedBytes">显式解压允许写出的最大未压缩总字节数。</param>
        /// <param name="maxXmlCharacters">单个 XML 文档允许解析的最大字符数。</param>
        /// <param name="maxXmlDepth">单个 XML 文档允许出现的最大元素嵌套深度。</param>
        /// <param name="maxCachedXmlDocuments">单个归档允许缓存的最大 XML 文档数。</param>
        /// <param name="maxCachedXmlBytes">单个归档允许缓存的 XML 原始内容最大总字节数。</param>
        /// <exception cref="ArgumentOutOfRangeException">任一限制不是正数。</exception>
        public OFDArchiveReadLimits(
            int maxArchiveEntryCount = 10_000,
            long maxEntryBytes = 128L * 1024 * 1024,
            long maxExtractedBytes = 512L * 1024 * 1024,
            long maxXmlCharacters = 16L * 1024 * 1024,
            int maxXmlDepth = 256,
            int maxCachedXmlDocuments = 256,
            long maxCachedXmlBytes = 64L * 1024 * 1024)
        {
            if (maxArchiveEntryCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxArchiveEntryCount));
            if (maxEntryBytes <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxEntryBytes));
            if (maxExtractedBytes <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxExtractedBytes));
            if (maxXmlCharacters <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxXmlCharacters));
            if (maxXmlDepth <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxXmlDepth));
            if (maxCachedXmlDocuments <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxCachedXmlDocuments));
            if (maxCachedXmlBytes <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxCachedXmlBytes));

            MaxArchiveEntryCount = maxArchiveEntryCount;
            MaxEntryBytes = maxEntryBytes;
            MaxExtractedBytes = maxExtractedBytes;
            MaxXmlCharacters = maxXmlCharacters;
            MaxXmlDepth = maxXmlDepth;
            MaxCachedXmlDocuments = maxCachedXmlDocuments;
            MaxCachedXmlBytes = maxCachedXmlBytes;
        }
    }
}
