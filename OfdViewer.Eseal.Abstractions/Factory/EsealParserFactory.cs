using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using OfdViewer.Eseal.Abstractions.Interfaces;

namespace OfdViewer.Eseal.Abstractions.Factory
{
    /// <summary>
    /// 电子印章解析器工厂
    /// 负责创建和管理各厂商的解析器实例
    /// 实现配置化选择厂商，避免硬编码
    /// </summary>
    public static class EsealParserFactory
    {
        /// <summary>
        /// 解析器注册表
        /// 存储厂商名称到解析器创建函数的映射
        /// </summary>
        private static readonly ConcurrentDictionary<string, Func<IEsealParser>> _parserRegistry = new();

        /// <summary>
        /// 解析器缓存
        /// 存储已创建的解析器实例（可重用）
        /// </summary>
        private static readonly ConcurrentDictionary<string, IEsealParser> _parserCache = new();

        /// <summary>
        /// 注册解析器
        /// </summary>
        /// <param name="vendorName">厂商名称</param>
        /// <param name="parserFactory">解析器创建函数</param>
        /// <exception cref="ArgumentException">当厂商名称为空或已注册时抛出</exception>
        public static void Register(string vendorName, Func<IEsealParser> parserFactory)
        {
            if (string.IsNullOrWhiteSpace(vendorName))
            {
                throw new ArgumentException("厂商名称不能为空", nameof(vendorName));
            }

            if (parserFactory == null)
            {
                throw new ArgumentNullException(nameof(parserFactory));
            }

            string normalizedName = NormalizeVendorName(vendorName);

            if (!_parserRegistry.TryAdd(normalizedName, parserFactory))
            {
                throw new ArgumentException($"厂商 '{vendorName}' 的解析器已经注册", nameof(vendorName));
            }
        }

        /// <summary>
        /// 获取解析器实例
        /// </summary>
        /// <param name="vendorName">厂商名称</param>
        /// <returns>解析器实例</returns>
        /// <exception cref="ArgumentException">当厂商名称为空时抛出</exception>
        /// <exception cref="KeyNotFoundException">当厂商未注册时抛出</exception>
        public static IEsealParser GetParser(string vendorName)
        {
            if (string.IsNullOrWhiteSpace(vendorName))
            {
                throw new ArgumentException("厂商名称不能为空", nameof(vendorName));
            }

            string normalizedName = NormalizeVendorName(vendorName);

            if (_parserRegistry.TryGetValue(normalizedName, out var parserFactory))
            {
                return parserFactory();
            }

            throw new KeyNotFoundException($"未找到厂商 '{vendorName}' 的解析器，请先注册");
        }

        /// <summary>
        /// 获取或创建解析器实例（带缓存）
        /// </summary>
        /// <param name="vendorName">厂商名称</param>
        /// <returns>解析器实例</returns>
        public static IEsealParser GetOrCreateParser(string vendorName)
        {
            if (string.IsNullOrWhiteSpace(vendorName))
            {
                throw new ArgumentException("厂商名称不能为空", nameof(vendorName));
            }

            string normalizedName = NormalizeVendorName(vendorName);

            return _parserCache.GetOrAdd(normalizedName, _ =>
            {
                if (_parserRegistry.TryGetValue(normalizedName, out var parserFactory))
                {
                    return parserFactory();
                }

                throw new KeyNotFoundException($"未找到厂商 '{vendorName}' 的解析器，请先注册");
            });
        }

        /// <summary>
        /// 自动检测并获取合适的解析器
        /// 遍历所有已注册的解析器，返回第一个能处理该印章数据的解析器
        /// </summary>
        /// <param name="sealData">印章数据</param>
        /// <returns>合适的解析器实例</returns>
        /// <exception cref="InvalidOperationException">当没有解析器能处理该数据时抛出</exception>
        public static IEsealParser GetParser(byte[] sealData)
        {
            if (sealData == null || sealData.Length == 0)
            {
                throw new ArgumentException("印章数据不能为空", nameof(sealData));
            }

            foreach (var kvp in _parserRegistry)
            {
                try
                {
                    var parser = kvp.Value();
                    if (parser.CanParse(sealData))
                    {
                        return parser;
                    }

                    // 如果不能解析，释放资源
                    parser.Dispose();
                }
                catch
                {
                    // 忽略单个解析器的异常，继续尝试下一个
                }
            }

            throw new InvalidOperationException("没有可用的解析器能处理该印章数据格式");
        }

        /// <summary>
        /// 尝试获取解析器
        /// </summary>
        /// <param name="vendorName">厂商名称</param>
        /// <param name="parser">输出参数，解析器实例</param>
        /// <returns>是否成功获取</returns>
        public static bool TryGetParser(string vendorName, out IEsealParser parser)
        {
            parser = null;

            if (string.IsNullOrWhiteSpace(vendorName))
            {
                return false;
            }

            string normalizedName = NormalizeVendorName(vendorName);

            if (_parserRegistry.TryGetValue(normalizedName, out var parserFactory))
            {
                try
                {
                    parser = parserFactory();
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// 尝试自动检测并获取合适的解析器
        /// </summary>
        /// <param name="sealData">印章数据</param>
        /// <param name="parser">输出参数，解析器实例</param>
        /// <returns>是否成功获取</returns>
        public static bool TryGetParser(byte[] sealData, out IEsealParser parser)
        {
            parser = null;

            if (sealData == null || sealData.Length == 0)
            {
                return false;
            }

            foreach (var kvp in _parserRegistry)
            {
                try
                {
                    var testParser = kvp.Value();
                    if (testParser.CanParse(sealData))
                    {
                        parser = testParser;
                        return true;
                    }

                    testParser.Dispose();
                }
                catch
                {
                    // 忽略异常，继续尝试下一个
                }
            }

            return false;
        }

        /// <summary>
        /// 获取所有已注册的厂商名称
        /// </summary>
        /// <returns>厂商名称列表</returns>
        public static IEnumerable<string> GetRegisteredVendors()
        {
            return _parserRegistry.Keys.ToList();
        }

        /// <summary>
        /// 注销解析器
        /// </summary>
        /// <param name="vendorName">厂商名称</param>
        /// <returns>是否成功注销</returns>
        public static bool Unregister(string vendorName)
        {
            if (string.IsNullOrWhiteSpace(vendorName))
            {
                return false;
            }

            string normalizedName = NormalizeVendorName(vendorName);

            // 从缓存中移除并释放资源
            if (_parserCache.TryRemove(normalizedName, out var cachedParser))
            {
                cachedParser?.Dispose();
            }

            return _parserRegistry.TryRemove(normalizedName, out _);
        }

        /// <summary>
        /// 清空所有注册的解析器
        /// </summary>
        public static void Clear()
        {
            // 释放缓存中的所有解析器
            foreach (var parser in _parserCache.Values)
            {
                parser?.Dispose();
            }

            _parserCache.Clear();
            _parserRegistry.Clear();
        }

        /// <summary>
        /// 检查是否已注册指定厂商的解析器
        /// </summary>
        /// <param name="vendorName">厂商名称</param>
        /// <returns>是否已注册</returns>
        public static bool IsRegistered(string vendorName)
        {
            if (string.IsNullOrWhiteSpace(vendorName))
            {
                return false;
            }

            string normalizedName = NormalizeVendorName(vendorName);
            return _parserRegistry.ContainsKey(normalizedName);
        }

        /// <summary>
        /// 规范化厂商名称
        /// 统一转换为小写并去除首尾空格
        /// </summary>
        /// <param name="vendorName">原始厂商名称</param>
        /// <returns>规范化后的厂商名称</returns>
        private static string NormalizeVendorName(string vendorName)
        {
            return vendorName?.Trim().ToLowerInvariant() ?? string.Empty;
        }
    }
}
