using OfdViewer.ESeal.Abstractions.Factory;
using OfdViewer.ESeal.Implementations.Common;
using Xunit;

namespace OFDViewer.Tests
{
    /// <summary>
    /// 电子签章解析器工厂测试。
    /// </summary>
    public class EsealParserFactoryTests
    {
        /// <summary>
        /// 注册名称应忽略首尾空格和大小写，并能创建对应解析器。
        /// </summary>
        [Fact]
        public void Register_NormalizedVendorName_CanResolveParser()
        {
            string vendorName = CreateVendorName();

            try
            {
                EsealParserFactory.Register($"  {vendorName.ToUpperInvariant()}  ", () => new DefaultEsealParser());

                Assert.True(EsealParserFactory.IsRegistered(vendorName));
                using var parser = EsealParserFactory.GetParser(vendorName.ToLowerInvariant());
                Assert.IsType<DefaultEsealParser>(parser);
            }
            finally
            {
                EsealParserFactory.Unregister(vendorName);
            }
        }

        /// <summary>
        /// 同一厂商名称重复注册时必须拒绝覆盖已有工厂。
        /// </summary>
        [Fact]
        public void Register_DuplicateVendor_ThrowsArgumentException()
        {
            string vendorName = CreateVendorName();

            try
            {
                EsealParserFactory.Register(vendorName, () => new DefaultEsealParser());

                Assert.Throws<ArgumentException>(() =>
                    EsealParserFactory.Register(vendorName.ToUpperInvariant(), () => new DefaultEsealParser()));
            }
            finally
            {
                EsealParserFactory.Unregister(vendorName);
            }
        }

        /// <summary>
        /// 缓存入口应返回同一实例，注销时必须同步释放缓存实例。
        /// </summary>
        [Fact]
        public async Task Unregister_CachedParser_DisposesParser()
        {
            string vendorName = CreateVendorName();

            try
            {
                EsealParserFactory.Register(vendorName, () => new DefaultEsealParser());

                var first = EsealParserFactory.GetOrCreateParser(vendorName);
                var second = EsealParserFactory.GetOrCreateParser(vendorName.ToUpperInvariant());

                Assert.Same(first, second);
                Assert.True(EsealParserFactory.Unregister(vendorName));
                await Assert.ThrowsAsync<ObjectDisposedException>(() => first.ValidateAsync(CreatePngData()));
            }
            finally
            {
                EsealParserFactory.Unregister(vendorName);
            }
        }

        /// <summary>
        /// 解析器构造失败时，TryGetParser 应返回 false 且不泄露异常。
        /// </summary>
        [Fact]
        public void TryGetParser_FactoryThrows_ReturnsFalse()
        {
            string vendorName = CreateVendorName();

            try
            {
                EsealParserFactory.Register(vendorName, () => throw new InvalidOperationException("测试工厂创建失败"));

                bool found = EsealParserFactory.TryGetParser(vendorName, out var parser);

                Assert.False(found);
                Assert.Null(parser);
            }
            finally
            {
                EsealParserFactory.Unregister(vendorName);
            }
        }

        /// <summary>
        /// 自动探测应返回能够识别输入数据的解析器实例。
        /// </summary>
        [Fact]
        public void TryGetParser_RecognizedImage_ReturnsParser()
        {
            string vendorName = CreateVendorName();

            try
            {
                EsealParserFactory.Register(vendorName, () => new DefaultEsealParser());

                bool found = EsealParserFactory.TryGetParser(CreatePngData(), out var parser);

                Assert.True(found);
                Assert.NotNull(parser);
                Assert.True(parser.CanParse(CreatePngData()));
                parser.Dispose();
            }
            finally
            {
                EsealParserFactory.Unregister(vendorName);
            }
        }

        /// <summary>
        /// 创建不会与其他测试冲突的厂商名称。
        /// </summary>
        private static string CreateVendorName()
        {
            return $"test-{Guid.NewGuid():N}";
        }

        /// <summary>
        /// 返回可被 SkiaSharp 解码的一像素 PNG 数据。
        /// </summary>
        private static byte[] CreatePngData()
        {
            return Convert.FromBase64String(
                "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=");
        }
    }
}
