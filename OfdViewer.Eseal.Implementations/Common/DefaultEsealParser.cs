using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OfdViewer.ESeal.Abstractions.Exceptions;
using OfdViewer.ESeal.Abstractions.Interfaces;
using OfdViewer.ESeal.Abstractions.Models;
using OfdViewer.ESeal.Implementations.Base;
using SkiaSharp;

namespace OfdViewer.ESeal.Implementations.Common
{
    /// <summary>
    /// 默认电子印章解析器
    /// 尝试以通用方式解析印章文件，支持常见的图像格式
    /// 作为后备解析器使用
    /// </summary>
    public class DefaultEsealParser : BaseEsealParser
    {
        /// <summary>
        /// 解析器名称
        /// </summary>
        public override string ParserName => "Default";

        /// <summary>
        /// 支持的文件格式
        /// </summary>
        public override IEnumerable<string> SupportedFormats => new[] { ".esl", ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".svg" };

        /// <summary>
        /// 图像文件魔数映射表
        /// </summary>
        private static readonly Dictionary<string, byte[]> ImageMagicNumbers = new()
        {
            { "PNG", new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } },
            { "JPEG", new byte[] { 0xFF, 0xD8, 0xFF } },
            { "GIF", new byte[] { 0x47, 0x49, 0x46, 0x38 } },
            { "BMP", new byte[] { 0x42, 0x4D } },
            { "TIFF", new byte[] { 0x49, 0x49, 0x2A, 0x00 } },
            { "TIFF_BE", new byte[] { 0x4D, 0x4D, 0x00, 0x2A } }
        };

        /// <summary>
        /// 检查是否支持该格式的印章文件
        /// </summary>
        /// <param name="sealData">印章二进制数据</param>
        /// <returns>是否支持</returns>
        public override bool CanParse(byte[] sealData)
        {
            if (sealData == null || sealData.Length < 8)
            {
                return false;
            }

            // 检查是否为已知的图像格式
            foreach (var magic in ImageMagicNumbers)
            {
                if (sealData.Length >= magic.Value.Length)
                {
                    bool match = true;
                    for (int i = 0; i < magic.Value.Length; i++)
                    {
                        if (sealData[i] != magic.Value[i])
                        {
                            match = false;
                            break;
                        }
                    }

                    if (match)
                    {
                        return true;
                    }
                }
            }

            // 尝试用 SkiaSharp 解码验证
            try
            {
                using var stream = new MemoryStream(sealData);
                using var bitmap = SKBitmap.Decode(stream);
                return bitmap != null;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 内部加载印章信息实现
        /// </summary>
        /// <param name="sealData">印章二进制数据</param>
        /// <returns>印章信息</returns>
        protected override async Task<IEsealInfo> LoadSealInfoInternalAsync(byte[] sealData)
        {
            try
            {
                using var stream = new MemoryStream(sealData);
                using var bitmap = SKBitmap.Decode(stream);

                if (bitmap == null)
                {
                    throw new EsealFormatException("无法识别印章图像格式");
                }

                var sealInfo = new SealInfo
                {
                    SealId = Guid.NewGuid().ToString("N"),
                    SealName = "未知印章",
                    SealType = "Unknown",
                    ImageData = sealData,
                    ImageFormat = DetectImageFormat(sealData),
                    ImageWidth = bitmap.Width,
                    ImageHeight = bitmap.Height,
                    CreateTime = DateTime.Now
                };

                return sealInfo;
            }
            catch (Exception ex)
            {
                throw new EsealFormatException($"加载印章图像失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 内部验证印章实现
        /// </summary>
        /// <param name="sealData">印章二进制数据</param>
        /// <param name="signedData">签名数据</param>
        /// <returns>验签结果</returns>
        protected override async Task<IEsealValidationResult> ValidateSealInternalAsync(byte[] sealData, byte[] signedData)
        {
            var result = new ValidationResult
            {
                ValidationTime = DateTime.Now,
                IsValid = true,
                StatusCode = 0,
                Message = "默认解析器无法验证数字签名，仅验证图像格式正确"
            };

            try
            {
                // 仅验证图像格式是否正确
                using var stream = new MemoryStream(sealData);
                using var bitmap = SKBitmap.Decode(stream);

                if (bitmap == null)
                {
                    result.IsValid = false;
                    result.StatusCode = -1;
                    result.Message = "印章图像格式无效";
                    result.Errors.Add("无法解码印章图像数据");
                }
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.StatusCode = -1;
                result.Message = $"验证失败: {ex.Message}";
                result.Errors.Add(ex.Message);
            }

            return result;
        }

        /// <summary>
        /// 内部提取图像数据实现
        /// </summary>
        /// <param name="sealData">印章二进制数据</param>
        /// <returns>图像字节数据</returns>
        protected override Task<byte[]> ExtractImageDataInternalAsync(byte[] sealData)
        {
            // 默认解析器直接返回原始数据（假设已经是图像格式）
            return Task.FromResult(sealData);
        }

        /// <summary>
        /// 内部获取签章人信息实现
        /// </summary>
        /// <param name="sealData">印章二进制数据</param>
        /// <returns>签章人信息</returns>
        protected override Task<SignerInfo> GetSignerInfoInternalAsync(byte[] sealData)
        {
            // 默认解析器无法获取签章人信息
            return Task.FromResult<SignerInfo>(null);
        }

        /// <summary>
        /// 内部获取元数据实现
        /// </summary>
        /// <param name="sealData">印章二进制数据</param>
        /// <returns>印章元数据</returns>
        protected override async Task<EsealMetadata> GetMetadataInternalAsync(byte[] sealData)
        {
            var sealInfo = await LoadSealInfoInternalAsync(sealData);

            return new EsealMetadata
            {
                SealId = sealInfo?.SealId,
                Version = "1.0",
                CreateTime = sealInfo?.CreateTime,
                SealType = sealInfo?.SealType
            };
        }

        /// <summary>
        /// 内部获取证书信息实现
        /// </summary>
        /// <param name="sealData">印章二进制数据</param>
        /// <returns>证书信息</returns>
        protected override Task<CertificateInfo> GetCertificateInfoInternalAsync(byte[] sealData)
        {
            // 默认解析器无法获取证书信息
            return Task.FromResult<CertificateInfo>(null);
        }

        /// <summary>
        /// 内部验证印章图片哈希实现
        /// </summary>
        /// <param name="sealData">印章二进制数据</param>
        /// <returns>哈希验证结果</returns>
        protected override Task<bool> VerifyImageHashInternalAsync(byte[] sealData)
        {
            // 默认解析器无法验证哈希值
            return Task.FromResult(false);
        }

        /// <summary>
        /// 检测图像格式
        /// </summary>
        /// <param name="sealData">图像数据</param>
        /// <returns>图像格式字符串</returns>
        private string DetectImageFormat(byte[] sealData)
        {
            if (sealData == null || sealData.Length < 8)
            {
                return "Unknown";
            }

            foreach (var magic in ImageMagicNumbers)
            {
                if (sealData.Length >= magic.Value.Length)
                {
                    bool match = true;
                    for (int i = 0; i < magic.Value.Length; i++)
                    {
                        if (sealData[i] != magic.Value[i])
                        {
                            match = false;
                            break;
                        }
                    }

                    if (match)
                    {
                        return magic.Key.Replace("_BE", "");
                    }
                }
            }

            return "Unknown";
        }
    }
}
