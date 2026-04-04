using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using OfdViewer.ESeal.Abstractions.Exceptions;
using OfdViewer.ESeal.Abstractions.Interfaces;
using OfdViewer.ESeal.Abstractions.Models;
using SkiaSharp;

namespace OfdViewer.ESeal.Implementations.Base
{
    /// <summary>
    /// 电子印章解析器抽象基类
    /// 提供通用的实现逻辑，各厂商只需实现特定方法
    /// 符合 GM/T 0031-2014 安全电子签章密码技术规范
    /// </summary>
    public abstract class BaseEsealParser : IEsealParser
    {
        /// <summary>
        /// 解析器名称（厂商标识）
        /// </summary>
        public abstract string ParserName { get; }

        /// <summary>
        /// 支持的文件格式
        /// </summary>
        public abstract IEnumerable<string> SupportedFormats { get; }

        /// <summary>
        /// 是否已释放
        /// </summary>
        protected bool _isDisposed;

        /// <summary>
        /// 加载印章文件
        /// </summary>
        /// <param name="sealData">印章二进制数据</param>
        /// <returns>印章信息对象</returns>
        public virtual async Task<IEsealInfo> LoadAsync(byte[] sealData)
        {
            ThrowIfDisposed();
            ValidateSealData(sealData);

            try
            {
                return await LoadSealInfoInternalAsync(sealData);
            }
            catch (Exception ex) when (!(ex is EsealParserException))
            {
                throw new EsealParserException(ParserName, 0, $"加载印章失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 验证印章有效性
        /// </summary>
        /// <param name="sealData">印章二进制数据</param>
        /// <param name="signedData">签名数据（可选）</param>
        /// <returns>验签结果</returns>
        public virtual async Task<IEsealValidationResult> ValidateAsync(byte[] sealData, byte[] signedData = null)
        {
            ThrowIfDisposed();
            ValidateSealData(sealData);

            try
            {
                return await ValidateSealInternalAsync(sealData, signedData);
            }
            catch (Exception ex) when (!(ex is EsealParserException))
            {
                throw new EsealValidationException($"验证印章失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 提取印章图像
        /// </summary>
        /// <param name="sealData">印章二进制数据</param>
        /// <returns>印章图像流（PNG格式）</returns>
        public virtual async Task<Stream> ExtractImageAsync(byte[] sealData)
        {
            ThrowIfDisposed();
            ValidateSealData(sealData);

            try
            {
                var imageData = await ExtractImageDataInternalAsync(sealData);
                if (imageData == null || imageData.Length == 0)
                {
                    throw new EsealFormatException("无法从印章数据中提取图像");
                }

                return new MemoryStream(imageData);
            }
            catch (Exception ex) when (!(ex is EsealParserException))
            {
                throw new EsealParserException(ParserName, 0, $"提取印章图像失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 提取印章图像（带指定尺寸）
        /// </summary>
        /// <param name="sealData">印章二进制数据</param>
        /// <param name="width">目标宽度（像素）</param>
        /// <param name="height">目标高度（像素）</param>
        /// <returns>印章图像流</returns>
        public virtual async Task<Stream> ExtractImageAsync(byte[] sealData, int width, int height)
        {
            ThrowIfDisposed();
            ValidateSealData(sealData);

            if (width <= 0 || height <= 0)
            {
                throw new ArgumentException("图像尺寸必须大于0");
            }

            try
            {
                var imageData = await ExtractImageDataInternalAsync(sealData);
                if (imageData == null || imageData.Length == 0)
                {
                    throw new EsealFormatException("无法从印章数据中提取图像");
                }

                // 使用 SkiaSharp 调整图像尺寸
                using var inputStream = new MemoryStream(imageData);
                using var bitmap = SKBitmap.Decode(inputStream);

                if (bitmap == null)
                {
                    throw new EsealFormatException("无法解码印章图像数据");
                }

                // 缩放图像
                using var resized = bitmap.Resize(new SKImageInfo(width, height), SKFilterQuality.High);
                using var image = SKImage.FromBitmap(resized);
                using var data = image.Encode(SKEncodedImageFormat.Png, 100);

                var outputStream = new MemoryStream();
                data.SaveTo(outputStream);
                outputStream.Position = 0;

                return outputStream;
            }
            catch (Exception ex) when (!(ex is EsealParserException))
            {
                throw new EsealParserException(ParserName, 0, $"提取印章图像失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 获取签章人信息
        /// </summary>
        /// <param name="sealData">印章二进制数据</param>
        /// <returns>签章人信息</returns>
        public virtual async Task<SignerInfo> GetSignerInfoAsync(byte[] sealData)
        {
            ThrowIfDisposed();
            ValidateSealData(sealData);

            try
            {
                return await GetSignerInfoInternalAsync(sealData);
            }
            catch (Exception ex) when (!(ex is EsealParserException))
            {
                throw new EsealParserException(ParserName, 0, $"获取签章人信息失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 获取印章元数据
        /// </summary>
        /// <param name="sealData">印章二进制数据</param>
        /// <returns>印章元数据</returns>
        public virtual async Task<EsealMetadata> GetMetadataAsync(byte[] sealData)
        {
            ThrowIfDisposed();
            ValidateSealData(sealData);

            try
            {
                return await GetMetadataInternalAsync(sealData);
            }
            catch (Exception ex) when (!(ex is EsealParserException))
            {
                throw new EsealParserException(ParserName, 0, $"获取印章元数据失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 获取证书信息
        /// </summary>
        /// <param name="sealData">印章二进制数据</param>
        /// <returns>证书信息</returns>
        public virtual async Task<CertificateInfo> GetCertificateInfoAsync(byte[] sealData)
        {
            ThrowIfDisposed();
            ValidateSealData(sealData);

            try
            {
                return await GetCertificateInfoInternalAsync(sealData);
            }
            catch (Exception ex) when (!(ex is EsealParserException))
            {
                throw new EsealParserException(ParserName, 0, $"获取证书信息失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 验证印章图片哈希值
        /// GM/T 0031-2014: pictureHash 验证
        /// </summary>
        /// <param name="sealData">印章二进制数据</param>
        /// <returns>哈希验证结果</returns>
        public virtual async Task<bool> VerifyImageHashAsync(byte[] sealData)
        {
            ThrowIfDisposed();
            ValidateSealData(sealData);

            try
            {
                return await VerifyImageHashInternalAsync(sealData);
            }
            catch (Exception ex) when (!(ex is EsealParserException))
            {
                throw new EsealParserException(ParserName, 0, $"验证印章图片哈希失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 检查是否支持该格式的印章文件
        /// </summary>
        /// <param name="sealData">印章二进制数据</param>
        /// <returns>是否支持</returns>
        public abstract bool CanParse(byte[] sealData);

        /// <summary>
        /// 获取支持的签名算法列表
        /// GM/T 0031-2014: signatureAlgorithm - 签名算法标识
        /// </summary>
        /// <returns>签名算法标识列表</returns>
        public virtual IEnumerable<string> GetSupportedSignatureAlgorithms()
        {
            // 默认返回国密算法
            return new[]
            {
                "1.2.156.10197.1.501",  // SM2withSM3
                "SM2withSM3",
                "1.2.840.10045.4.3.2",  // ECDSAwithSHA256
                "SHA256withRSA",
                "SHA256withECDSA"
            };
        }

        /// <summary>
        /// 获取支持的哈希算法列表
        /// GM/T 0031-2014: digestAlgorithm - 摘要算法标识
        /// </summary>
        /// <returns>哈希算法标识列表</returns>
        public virtual IEnumerable<string> GetSupportedHashAlgorithms()
        {
            // 默认返回国密哈希算法
            return new[]
            {
                "1.2.156.10197.1.401",  // SM3
                "SM3",
                "2.16.840.1.101.3.4.2.1", // SHA-256
                "SHA-256",
                "SHA-1"
            };
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public virtual void Dispose()
        {
            if (!_isDisposed)
            {
                DisposeInternal();
                _isDisposed = true;
            }

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 内部加载印章信息实现（由子类实现）
        /// </summary>
        /// <param name="sealData">印章二进制数据</param>
        /// <returns>印章信息</returns>
        protected abstract Task<IEsealInfo> LoadSealInfoInternalAsync(byte[] sealData);

        /// <summary>
        /// 内部验证印章实现（由子类实现）
        /// </summary>
        /// <param name="sealData">印章二进制数据</param>
        /// <param name="signedData">签名数据</param>
        /// <returns>验签结果</returns>
        protected abstract Task<IEsealValidationResult> ValidateSealInternalAsync(byte[] sealData, byte[] signedData);

        /// <summary>
        /// 内部提取图像数据实现（由子类实现）
        /// </summary>
        /// <param name="sealData">印章二进制数据</param>
        /// <returns>图像字节数据</returns>
        protected abstract Task<byte[]> ExtractImageDataInternalAsync(byte[] sealData);

        /// <summary>
        /// 内部获取签章人信息实现（由子类实现）
        /// </summary>
        /// <param name="sealData">印章二进制数据</param>
        /// <returns>签章人信息</returns>
        protected abstract Task<SignerInfo> GetSignerInfoInternalAsync(byte[] sealData);

        /// <summary>
        /// 内部获取元数据实现（由子类实现）
        /// </summary>
        /// <param name="sealData">印章二进制数据</param>
        /// <returns>印章元数据</returns>
        protected abstract Task<EsealMetadata> GetMetadataInternalAsync(byte[] sealData);

        /// <summary>
        /// 内部获取证书信息实现（由子类实现）
        /// </summary>
        /// <param name="sealData">印章二进制数据</param>
        /// <returns>证书信息</returns>
        protected abstract Task<CertificateInfo> GetCertificateInfoInternalAsync(byte[] sealData);

        /// <summary>
        /// 内部验证印章图片哈希实现（由子类实现）
        /// </summary>
        /// <param name="sealData">印章二进制数据</param>
        /// <returns>哈希验证结果</returns>
        protected abstract Task<bool> VerifyImageHashInternalAsync(byte[] sealData);

        /// <summary>
        /// 释放内部资源（由子类重写）
        /// </summary>
        protected virtual void DisposeInternal()
        {
        }

        /// <summary>
        /// 验证印章数据有效性
        /// </summary>
        /// <param name="sealData">印章二进制数据</param>
        protected virtual void ValidateSealData(byte[] sealData)
        {
            if (sealData == null || sealData.Length == 0)
            {
                throw new ArgumentException("印章数据不能为空", nameof(sealData));
            }
        }

        /// <summary>
        /// 检查是否已释放
        /// </summary>
        protected virtual void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }
    }
}
