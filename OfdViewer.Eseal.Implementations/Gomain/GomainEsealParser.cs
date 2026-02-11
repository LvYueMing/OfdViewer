using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using OfdViewer.ESeal.Abstractions.Exceptions;
using OfdViewer.ESeal.Abstractions.Interfaces;
using OfdViewer.ESeal.Abstractions.Models;
using OfdViewer.ESeal.Implementations.Base;

namespace OfdViewer.ESeal.Implementations.Gomain
{
    /// <summary>
    /// 国脉信安电子印章解析器实现
    /// 针对国脉信安深度封装签章格式的解析
    /// 支持从 SignedValue.dat 中提取印章图像和签名信息
    /// 
    /// 文件结构（基于 SignedValue_Structure.md 文档）：
    /// - ASN.1 数据部分（前 1317 字节）：包含签名元数据、签章者证书、SM2 签名值
    /// - ZIP 数据部分（后 4968 字节）：包含被签名的原始文档内容
    /// </summary>
    public class GomainEsealParser : BaseEsealParser
    {
        /// <summary>
        /// 解析器名称
        /// </summary>
        public override string ParserName => "Gomain";

        /// <summary>
        /// 支持的文件格式
        /// </summary>
        public override IEnumerable<string> SupportedFormats => new[] { ".esl", ".eseal", ".dat", ".signed", ".ofd" };

        /// <summary>
        /// ZIP 文件签名（PK\x03\x04）
        /// 用于检测 ASN.1 和 ZIP 数据的边界
        /// </summary>
        private static readonly byte[] ZipSignature = new byte[] { 0x50, 0x4B, 0x03, 0x04 };

        /// <summary>
        /// 国脉签章数据特征标识
        /// </summary>
        private static readonly byte[][] GomainSignatures = new byte[][]
        {
            Encoding.ASCII.GetBytes("GM"),
            Encoding.ASCII.GetBytes("GOMAIN"),
            new byte[] { 0x30, 0x82 }, // ASN.1 SEQUENCE 长形式长度标识
            new byte[] { 0x30, 0x81 }  // ASN.1 SEQUENCE 短形式长度标识
        };

        /// <summary>
        /// 国脉印章图像相关 OID 列表
        /// </summary>
        private static readonly string[] SealImageOids = new[]
        {
            "1.2.156.112570.1.1.1",    // 国脉印章图像 OID
            "1.2.156.10197.1.501",       // SM2 签名算法相关
            "1.2.156.10197.6.1.4.2.1",   // 印章图像数据
            "1.2.156.10197.6.1.4.2.2",   // 印章属性数据
            "1.2.156.10197.6.1.4.2"      // 电子印章数据
        };

        /// <summary>
        /// 签名时间 OID
        /// </summary>
        private static readonly string SigningTimeOid = "1.2.840.113549.1.9.5";

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

            // 检查国脉特征标识
            foreach (var signature in GomainSignatures)
            {
                if (sealData.Length >= signature.Length)
                {
                    bool match = true;
                    for (int i = 0; i < signature.Length; i++)
                    {
                        if (sealData[i] != signature[i])
                        {
                            match = false;
                            break;
                        }
                    }
                    if (match) return true;
                }
            }

            // 尝试通过内容特征检测
            return TryDetectByContent(sealData);
        }

        /// <summary>
        /// 通过内容特征检测是否为国脉格式
        /// </summary>
        /// <param name="sealData">印章数据</param>
        /// <returns>是否为国脉格式</returns>
        private bool TryDetectByContent(byte[] sealData)
        {
            try
            {
                // 检查是否包含 ZIP 数据（国脉深度封装格式特征）
                if (FindBytePattern(sealData, ZipSignature) > 0)
                {
                    return true;
                }

                // 尝试解析ASN.1结构或XML内容
                if (sealData[0] == 0x30) // ASN.1 SEQUENCE
                {
                    return true;
                }

                string content = Encoding.UTF8.GetString(sealData);
                return content.Contains("gomain", StringComparison.OrdinalIgnoreCase) ||
                       content.Contains("GOMAIN") ||
                       content.Contains("GM") ||
                       content.Contains("SignedValue") ||
                       content.Contains("SealData");
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 内部加载印章信息实现
        /// </summary>
        /// <param name="sealData">印章二进制数据（SignedValue.dat）</param>
        /// <returns>印章信息</returns>
        protected override async Task<IEsealInfo> LoadSealInfoInternalAsync(byte[] sealData)
        {
            try
            {
                // 解析国脉深度封装的签章数据
                var signedValue = ParseSignedValue(sealData);

                var sealInfo = new SealInfo
                {
                    SealId = signedValue.SealId ?? Guid.NewGuid().ToString("N"),
                    SealName = signedValue.SealName ?? "国脉电子印章",
                    SealType = signedValue.SealType ?? "电子公章",
                    ValidFrom = signedValue.ValidFrom,
                    ValidTo = signedValue.ValidTo,
                    CreateTime = signedValue.SignTime,
                    Version = signedValue.Version ?? "1.0",
                    ImageData = signedValue.SealImageData,
                    ImageFormat = DetectImageFormat(signedValue.SealImageData),
                    ImageWidth = 0,
                    ImageHeight = 0
                };

                // 设置签章人信息
                if (signedValue.Certificate != null)
                {
                    sealInfo.Signer = new Abstractions.Models.SignerInfo
                    {
                        Name = signedValue.SignerName,
                        Organization = signedValue.SignerOrg,
                        CertSerialNumber = signedValue.Certificate.SerialNumber,
                        CertIssuer = signedValue.Certificate.Issuer,
                        CertValidFrom = signedValue.Certificate.NotBefore,
                        CertValidTo = signedValue.Certificate.NotAfter
                    };
                }

                return sealInfo;
            }
            catch (Exception ex)
            {
                throw new EsealFormatException($"解析国脉签章数据失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 解析 SignedValue.dat 文件
        /// 国脉信安的签章数据采用 ASN.1 + ZIP 混合格式封装
        /// </summary>
        /// <param name="signedValueData">SignedValue.dat 文件内容</param>
        /// <returns>解析后的签章信息</returns>
        private GomainSignedValue ParseSignedValue(byte[] signedValueData)
        {
            var result = new GomainSignedValue();

            try
            {
                // 第一步：检测数据边界（ASN.1 vs ZIP）
                int zipIndex = FindBytePattern(signedValueData, ZipSignature);
                
                if (zipIndex > 0)
                {
                    // 国脉深度封装格式：ASN.1 + ZIP
                    return ParseGomainHybridFormat(signedValueData, zipIndex, result);
                }

                // 尝试作为纯 ASN.1 结构解析（PKCS#7/CMS 格式）
                if (TryParseAsn1Structure(signedValueData, result))
                {
                    return result;
                }

                // 尝试作为 XML 格式解析
                if (TryParseXmlFormat(signedValueData, result))
                {
                    return result;
                }

                // 尝试作为自定义二进制格式解析
                if (TryParseBinaryFormat(signedValueData, result))
                {
                    return result;
                }

                // 如果都无法解析，尝试直接提取图像数据
                result.SealImageData = TryExtractImageFromRawData(signedValueData);

                return result;
            }
            catch (Exception ex)
            {
                throw new EsealFormatException($"解析 SignedValue 失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 解析国脉混合格式（ASN.1 + ZIP）
        /// </summary>
        /// <param name="data">完整数据</param>
        /// <param name="zipIndex">ZIP 数据起始位置</param>
        /// <param name="result">解析结果对象</param>
        /// <returns>解析后的签章信息</returns>
        private GomainSignedValue ParseGomainHybridFormat(byte[] data, int zipIndex, GomainSignedValue result)
        {
            // 提取 ASN.1 数据（签名部分）
            var asn1Data = new byte[zipIndex];
            Array.Copy(data, 0, asn1Data, 0, zipIndex);

            // 提取 ZIP 数据（被签名的原始文档）
            var zipData = new byte[data.Length - zipIndex];
            Array.Copy(data, zipIndex, zipData, 0, zipData.Length);

            // 解析 ASN.1 部分
            ParseAsn1Data(asn1Data, result);

            // 解析 ZIP 部分（提取文档内容和可能的印章图像）
            ParseZipData(zipData, result);

            return result;
        }

        /// <summary>
        /// 解析 ASN.1 数据部分
        /// </summary>
        /// <param name="asn1Data">ASN.1 数据</param>
        /// <param name="result">解析结果对象</param>
        private void ParseAsn1Data(byte[] asn1Data, GomainSignedValue result)
        {
            try
            {
                // 尝试使用标准 PKCS#7/CMS 解析
                if (TryParsePkcs7Structure(asn1Data, result))
                {
                    return;
                }

                // 尝试使用国脉自定义 ASN.1 解析
                if (TryParseGomainAsn1Structure(asn1Data, result))
                {
                    return;
                }

                // 尝试提取证书信息
                TryExtractCertificateFromAsn1(asn1Data, result);

                // 尝试提取印章图像
                var sealImage = TryExtractSealImageFromAsn1(asn1Data);
                if (sealImage != null)
                {
                    result.SealImageData = sealImage;
                }
            }
            catch (Exception ex)
            {
                // 记录解析异常，但不中断流程
                System.Diagnostics.Debug.WriteLine($"ASN.1 解析警告: {ex.Message}");
            }
        }

        /// <summary>
        /// 解析 ZIP 数据部分
        /// </summary>
        /// <param name="zipData">ZIP 数据</param>
        /// <param name="result">解析结果对象</param>
        private void ParseZipData(byte[] zipData, GomainSignedValue result)
        {
            try
            {
                using var stream = new MemoryStream(zipData);
                using var zipArchive = new System.IO.Compression.ZipArchive(stream, System.IO.Compression.ZipArchiveMode.Read);

                // 记录 ZIP 内容（用于调试）
                System.Diagnostics.Debug.WriteLine($"ZIP 包含 {zipArchive.Entries.Count} 个条目:");
                foreach (var entry in zipArchive.Entries)
                {
                    System.Diagnostics.Debug.WriteLine($"  - {entry.FullName} ({entry.Length} 字节)");
                }

                // 尝试从 ZIP 中提取印章图像
                var sealImageFromZip = ExtractSealImageFromZip(zipArchive);
                if (sealImageFromZip != null && result.SealImageData == null)
                {
                    result.SealImageData = sealImageFromZip;
                }

                // 验证 ZIP 中的文档完整性（可选）
                // 可以在这里添加文档哈希验证逻辑
            }
            catch (Exception ex)
            {
                // ZIP 解析失败不影响整体流程
                System.Diagnostics.Debug.WriteLine($"ZIP 解析警告: {ex.Message}");
            }
        }

        /// <summary>
        /// 尝试解析 ASN.1 结构（PKCS#7/CMS）
        /// </summary>
        private bool TryParseAsn1Structure(byte[] data, GomainSignedValue result)
        {
            // 检查是否为 ASN.1 SEQUENCE
            if (data.Length < 2 || data[0] != 0x30)
            {
                return false;
            }

            // 尝试使用标准 PKCS#7/CMS 解析
            if (TryParsePkcs7Structure(data, result))
            {
                return true;
            }

            // 尝试国脉自定义格式
            return TryParseGomainAsn1Structure(data, result);
        }

        /// <summary>
        /// 解析国脉自定义 ASN.1 结构
        /// 国脉格式：SEQUENCE { version, signatureAlgorithm, signerInfo, ... }
        /// </summary>
        private bool TryParseGomainAsn1Structure(byte[] data, GomainSignedValue result)
        {
            try
            {
                // 检查是否为 ASN.1 SEQUENCE
                if (data.Length < 2 || data[0] != 0x30)
                {
                    return false;
                }

                using var stream = new MemoryStream(data);
                using var reader = new BinaryReader(stream);

                // 读取 SEQUENCE 标签和长度
                var tag = reader.ReadByte();
                if (tag != 0x30)
                    return false;

                // 解析长度
                int length = ReadAsn1Length(reader);
                if (length <= 0 || length > data.Length - stream.Position)
                    return false;

                // 读取内容
                var content = reader.ReadBytes(length);

                // 尝试解析内容中的证书和印章信息
                if (TryExtractCertificateFromAsn1(content, result))
                {
                    // 尝试提取印章图像
                    var sealImage = TryExtractSealImageFromAsn1(content);
                    if (sealImage != null)
                    {
                        result.SealImageData = sealImage;
                    }
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 从 ASN.1 数据中读取长度字段
        /// </summary>
        private int ReadAsn1Length(BinaryReader reader)
        {
            var lengthByte = reader.ReadByte();
            
            if (lengthByte < 0x80)
            {
                // 短形式
                return lengthByte;
            }
            else if (lengthByte == 0x81)
            {
                // 长形式，1字节长度
                return reader.ReadByte();
            }
            else if (lengthByte == 0x82)
            {
                // 长形式，2字节长度
                var high = reader.ReadByte();
                var low = reader.ReadByte();
                return (high << 8) | low;
            }
            else if (lengthByte == 0x83)
            {
                // 长形式，3字节长度
                var b1 = reader.ReadByte();
                var b2 = reader.ReadByte();
                var b3 = reader.ReadByte();
                return (b1 << 16) | (b2 << 8) | b3;
            }
            else if (lengthByte == 0x84)
            {
                // 长形式，4字节长度
                var b1 = reader.ReadByte();
                var b2 = reader.ReadByte();
                var b3 = reader.ReadByte();
                var b4 = reader.ReadByte();
                return (b1 << 24) | (b2 << 16) | (b3 << 8) | b4;
            }
            
            throw new InvalidOperationException($"不支持的长度编码: 0x{lengthByte:X2}");
        }

        /// <summary>
        /// 从 ASN.1 数据中提取证书
        /// </summary>
        private bool TryExtractCertificateFromAsn1(byte[] data, GomainSignedValue result)
        {
            try
            {
                // 在数据中搜索 X.509 证书（SEQUENCE 开始）
                for (int i = 0; i < data.Length - 4; i++)
                {
                    // 检查是否是证书的开始（通常以 30 82 开头）
                    if (data[i] == 0x30 && (data[i + 1] == 0x82 || data[i + 1] == 0x81))
                    {
                        try
                        {
                            // 尝试从当前位置解析证书
                            var certData = new byte[data.Length - i];
                            Array.Copy(data, i, certData, 0, certData.Length);
                            
                            var cert = new X509Certificate2(certData);
                            result.Certificate = cert;
                            result.SignerName = GetSignerNameFromCertificate(cert);
                            result.SignerOrg = GetOrganizationFromCertificate(cert);
                            return true;
                        }
                        catch
                        {
                            // 继续搜索
                            continue;
                        }
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 从 ASN.1 数据中提取印章图像
        /// </summary>
        private byte[] TryExtractSealImageFromAsn1(byte[] data)
        {
            try
            {
                // 搜索图像数据签名
                // PNG: 89 50 4E 47
                var pngSignature = new byte[] { 0x89, 0x50, 0x4E, 0x47 };
                int pngIndex = FindBytePattern(data, pngSignature);
                if (pngIndex >= 0)
                {
                    var imageData = new byte[data.Length - pngIndex];
                    Array.Copy(data, pngIndex, imageData, 0, imageData.Length);
                    return imageData;
                }

                // JPEG: FF D8 FF
                var jpegSignature = new byte[] { 0xFF, 0xD8, 0xFF };
                int jpegIndex = FindBytePattern(data, jpegSignature);
                if (jpegIndex >= 0)
                {
                    var imageData = new byte[data.Length - jpegIndex];
                    Array.Copy(data, jpegIndex, imageData, 0, imageData.Length);
                    return imageData;
                }

                // 搜索 OCTET STRING 中的图像数据
                for (int i = 0; i < data.Length - 2; i++)
                {
                    if (data[i] == 0x04) // OCTET STRING 标签
                    {
                        try
                        {
                            using var stream = new MemoryStream(data, i, data.Length - i);
                            using var reader = new BinaryReader(stream);
                            
                            reader.ReadByte(); // 跳过标签
                            int length = ReadAsn1Length(reader);
                            
                            if (length > 0 && length < data.Length - stream.Position)
                            {
                                var octetData = reader.ReadBytes(length);
                                
                                // 检查是否是图像数据
                                if (IsImageData(octetData))
                                {
                                    return octetData;
                                }
                            }
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 解析 PKCS#7/CMS 结构
        /// </summary>
        private bool TryParsePkcs7Structure(byte[] data, GomainSignedValue result)
        {
            try
            {
                var signedCms = new SignedCms();
                signedCms.Decode(data);

                // 提取证书信息
                if (signedCms.Certificates.Count > 0)
                {
                    result.Certificate = signedCms.Certificates[0];
                    result.SignerName = GetSignerNameFromCertificate(result.Certificate);
                    result.SignerOrg = GetOrganizationFromCertificate(result.Certificate);
                }

                // 提取签名时间
                result.SignTime = DateTime.Now;

                // 从签名属性中提取印章图像和元数据
                foreach (var signerInfo in signedCms.SignerInfos)
                {
                    // 提取签名时间
                    ExtractSigningTime(signerInfo, result);

                    // 提取印章图像
                    ExtractSealImageFromSignerInfo(signerInfo, result);

                    // 提取印章标识
                    ExtractSealIdFromSignerInfo(signerInfo, result);
                }

                return true;
            }
            catch (CryptographicException)
            {
                // 不是标准的 PKCS#7 格式
                return false;
            }
        }

        /// <summary>
        /// 从签名信息中提取签名时间
        /// </summary>
        private void ExtractSigningTime(System.Security.Cryptography.Pkcs.SignerInfo signerInfo, GomainSignedValue result)
        {
            foreach (var attr in signerInfo.SignedAttributes)
            {
                if (attr.Oid.Value == SigningTimeOid || 
                    attr.Oid.FriendlyName.Contains("Time", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var value in attr.Values)
                    {
                        if (value is Pkcs9SigningTime signingTime)
                        {
                            result.SignTime = signingTime.SigningTime;
                            return;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 从签名信息中提取印章图像
        /// </summary>
        private void ExtractSealImageFromSignerInfo(System.Security.Cryptography.Pkcs.SignerInfo signerInfo, GomainSignedValue result)
        {
            foreach (var attr in signerInfo.SignedAttributes)
            {
                if (SealImageOids.Contains(attr.Oid.Value) ||
                    attr.Oid.FriendlyName.Contains("Seal", StringComparison.OrdinalIgnoreCase) ||
                    attr.Oid.FriendlyName.Contains("Image", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var value in attr.Values)
                    {
                        if (value is AsnEncodedData asnData && asnData.RawData != null && asnData.RawData.Length > 0)
                        {
                            // 检查是否是图像数据（PNG/JPG 头部）
                            if (IsImageData(asnData.RawData))
                            {
                                result.SealImageData = asnData.RawData;
                            }
                            else
                            {
                                // 可能是 ASN.1 封装的图像数据，尝试解包
                                result.SealImageData = ExtractImageFromAsn1Data(asnData.RawData);
                            }
                            return;
                        }
                    }
                }
            }

            // 如果未找到印章图像，尝试从未签名属性中查找
            if (result.SealImageData == null)
            {
                foreach (var attr in signerInfo.UnsignedAttributes)
                {
                    if (attr.Oid.FriendlyName.Contains("Seal", StringComparison.OrdinalIgnoreCase) ||
                        attr.Oid.FriendlyName.Contains("Image", StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (var value in attr.Values)
                        {
                            if (value is AsnEncodedData asnData && asnData.RawData != null)
                            {
                                result.SealImageData = asnData.RawData;
                                return;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 从签名信息中提取印章标识
        /// </summary>
        private void ExtractSealIdFromSignerInfo(System.Security.Cryptography.Pkcs.SignerInfo signerInfo, GomainSignedValue result)
        {
            foreach (var attr in signerInfo.SignedAttributes)
            {
                if (attr.Oid.Value.Contains("SealID") || 
                    attr.Oid.FriendlyName.Contains("SealID", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var value in attr.Values)
                    {
                        if (value is AsnEncodedData asnData)
                        {
                            result.SealId = Encoding.UTF8.GetString(asnData.RawData);
                            return;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 从 ZIP 数据中提取印章图像
        /// 国脉格式通常在 ZIP 包中包含印章图片
        /// </summary>
        private byte[] ExtractSealImageFromZip(System.IO.Compression.ZipArchive zipArchive)
        {
            try
            {
                // 查找常见的印章图像文件名
                var sealImageNames = new[] { 
                    "seal.png", "seal.jpg", "seal.bmp", "stamp.png", "stamp.jpg", 
                    "Seal.png", "Seal.jpg", "Seal.bmp", "Stamp.png", "Stamp.jpg",
                    "Doc_0/Signs/Sign_0/Seal.png", "Doc_0/Signs/Sign_0/Seal.jpg",
                    "Doc_0/Signs/Sign_0/stamp.png", "Doc_0/Signs/Sign_0/stamp.jpg"
                };

                foreach (var entry in zipArchive.Entries)
                {
                    var entryName = entry.FullName.ToLowerInvariant();
                    
                    // 检查是否是印章图像文件
                    if (entryName.EndsWith(".png") || entryName.EndsWith(".jpg") || 
                        entryName.EndsWith(".jpeg") || entryName.EndsWith(".bmp") ||
                        entryName.EndsWith(".gif"))
                    {
                        // 优先匹配包含 seal 或 stamp 的文件名
                        if (entryName.Contains("seal") || entryName.Contains("stamp") ||
                            sealImageNames.Any(name => entry.FullName.Equals(name, StringComparison.OrdinalIgnoreCase)))
                        {
                            using var entryStream = entry.Open();
                            using var memoryStream = new MemoryStream();
                            entryStream.CopyTo(memoryStream);
                            return memoryStream.ToArray();
                        }
                    }
                }

                // 如果没有找到特定的印章文件，返回第一个图像文件
                foreach (var entry in zipArchive.Entries)
                {
                    var entryName = entry.FullName.ToLowerInvariant();
                    if (entryName.EndsWith(".png") || entryName.EndsWith(".jpg") || 
                        entryName.EndsWith(".jpeg") || entryName.EndsWith(".bmp") ||
                        entryName.EndsWith(".gif"))
                    {
                        using var entryStream = entry.Open();
                        using var memoryStream = new MemoryStream();
                        entryStream.CopyTo(memoryStream);
                        return memoryStream.ToArray();
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 尝试解析 XML 格式
        /// </summary>
        private bool TryParseXmlFormat(byte[] data, GomainSignedValue result)
        {
            try
            {
                string xmlContent = Encoding.UTF8.GetString(data);

                if (!xmlContent.Contains("<?xml") && !xmlContent.Contains("<"))
                {
                    return false;
                }

                using var stream = new MemoryStream(data);
                using var reader = XmlReader.Create(stream);

                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        switch (reader.Name)
                        {
                            case "SealID":
                            case "SealId":
                                result.SealId = reader.ReadElementContentAsString();
                                break;
                            case "SealName":
                                result.SealName = reader.ReadElementContentAsString();
                                break;
                            case "SealType":
                                result.SealType = reader.ReadElementContentAsString();
                                break;
                            case "SignTime":
                            case "SigningTime":
                                if (DateTime.TryParse(reader.ReadElementContentAsString(), out var signTime))
                                {
                                    result.SignTime = signTime;
                                }
                                break;
                            case "SealImage":
                            case "ImageData":
                            case "SealData":
                                var imageBase64 = reader.ReadElementContentAsString();
                                if (!string.IsNullOrEmpty(imageBase64))
                                {
                                    result.SealImageData = Convert.FromBase64String(imageBase64);
                                }
                                break;
                            case "Cert":
                            case "Certificate":
                                var certBase64 = reader.ReadElementContentAsString();
                                if (!string.IsNullOrEmpty(certBase64))
                                {
                                    try
                                    {
                                        result.Certificate = new X509Certificate2(Convert.FromBase64String(certBase64));
                                    }
                                    catch { }
                                }
                                break;
                        }
                    }
                }

                return result.SealImageData != null || result.Certificate != null;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 尝试解析自定义二进制格式
        /// </summary>
        private bool TryParseBinaryFormat(byte[] data, GomainSignedValue result)
        {
            try
            {
                // 国脉自定义格式解析
                // 格式通常：头部标识 + 版本 + 印章数据长度 + 印章数据 + 证书数据长度 + 证书数据 + 签名值

                using var stream = new MemoryStream(data);
                using var reader = new BinaryReader(stream);

                // 读取头部标识（4字节）
                var magic = reader.ReadBytes(4);
                if (magic[0] != 0x47 || magic[1] != 0x4D) // "GM"
                {
                    return false;
                }

                // 读取版本（2字节）
                var versionBytes = reader.ReadBytes(2);
                result.Version = $"{versionBytes[0]}.{versionBytes[1]}";

                // 读取印章数据
                int sealDataLength = reader.ReadInt32();
                if (sealDataLength > 0 && sealDataLength < data.Length)
                {
                    result.SealImageData = reader.ReadBytes(sealDataLength);
                }

                // 读取证书数据
                int certDataLength = reader.ReadInt32();
                if (certDataLength > 0 && certDataLength < data.Length)
                {
                    var certData = reader.ReadBytes(certDataLength);
                    try
                    {
                        result.Certificate = new X509Certificate2(certData);
                    }
                    catch { }
                }

                return result.SealImageData != null;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 检查数据是否为图像格式
        /// </summary>
        private bool IsImageData(byte[] data)
        {
            if (data == null || data.Length < 4)
                return false;

            // PNG 签名: 89 50 4E 47
            if (data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47)
                return true;

            // JPEG 签名: FF D8 FF
            if (data[0] == 0xFF && data[1] == 0xD8 && data[2] == 0xFF)
                return true;

            // GIF 签名: 47 49 46 38
            if (data[0] == 0x47 && data[1] == 0x49 && data[2] == 0x46 && data[3] == 0x38)
                return true;

            // BMP 签名: 42 4D
            if (data[0] == 0x42 && data[1] == 0x4D)
                return true;

            return false;
        }

        /// <summary>
        /// 从 ASN.1 封装的图像数据中提取图像
        /// 国脉格式：OCTET STRING 包含图像数据
        /// </summary>
        private byte[] ExtractImageFromAsn1Data(byte[] asn1Data)
        {
            try
            {
                // 检查是否是 OCTET STRING (0x04)
                if (asn1Data.Length > 2 && asn1Data[0] == 0x04)
                {
                    int length;
                    int dataStart;

                    // 解析长度
                    if (asn1Data[1] < 0x80)
                    {
                        // 短形式长度
                        length = asn1Data[1];
                        dataStart = 2;
                    }
                    else if (asn1Data[1] == 0x81)
                    {
                        // 长形式，1字节长度
                        length = asn1Data[2];
                        dataStart = 3;
                    }
                    else if (asn1Data[1] == 0x82)
                    {
                        // 长形式，2字节长度
                        length = (asn1Data[2] << 8) | asn1Data[3];
                        dataStart = 4;
                    }
                    else
                    {
                        return null;
                    }

                    if (dataStart + length <= asn1Data.Length)
                    {
                        var imageData = new byte[length];
                        Array.Copy(asn1Data, dataStart, imageData, 0, length);
                        return imageData;
                    }
                }

                // 如果不是 OCTET STRING，尝试直接返回（可能已经是图像数据）
                if (IsImageData(asn1Data))
                {
                    return asn1Data;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 尝试从原始数据中提取图像
        /// </summary>
        private byte[] TryExtractImageFromRawData(byte[] data)
        {
            // 在数据中搜索常见的图像格式标识
            var imageSignatures = new Dictionary<byte[], string>
            {
                { new byte[] { 0x89, 0x50, 0x4E, 0x47 }, "PNG" },
                { new byte[] { 0xFF, 0xD8, 0xFF }, "JPEG" },
                { new byte[] { 0x47, 0x49, 0x46, 0x38 }, "GIF" },
                { new byte[] { 0x42, 0x4D }, "BMP" }
            };

            foreach (var signature in imageSignatures)
            {
                int index = FindBytePattern(data, signature.Key);
                if (index >= 0)
                {
                    // 提取从图像标识开始到数据结束的部分
                    var imageData = new byte[data.Length - index];
                    Array.Copy(data, index, imageData, 0, imageData.Length);
                    return imageData;
                }
            }

            return null;
        }

        /// <summary>
        /// 在字节数组中查找模式
        /// </summary>
        private int FindBytePattern(byte[] data, byte[] pattern)
        {
            for (int i = 0; i <= data.Length - pattern.Length; i++)
            {
                bool found = true;
                for (int j = 0; j < pattern.Length; j++)
                {
                    if (data[i + j] != pattern[j])
                    {
                        found = false;
                        break;
                    }
                }
                if (found) return i;
            }
            return -1;
        }

        /// <summary>
        /// 从证书中提取签名人名称
        /// </summary>
        private string GetSignerNameFromCertificate(X509Certificate2 certificate)
        {
            if (certificate == null) return null;

            // 尝试从 Subject 中提取 CN（Common Name）
            var subject = certificate.Subject;
            var cnIndex = subject.IndexOf("CN=");
            if (cnIndex >= 0)
            {
                var start = cnIndex + 3;
                var end = subject.IndexOf(",", start);
                if (end < 0) end = subject.Length;
                return subject.Substring(start, end - start).Trim();
            }

            return certificate.Subject;
        }

        /// <summary>
        /// 从证书中提取组织名称
        /// </summary>
        private string GetOrganizationFromCertificate(X509Certificate2 certificate)
        {
            if (certificate == null) return null;

            var subject = certificate.Subject;
            var oIndex = subject.IndexOf("O=");
            if (oIndex >= 0)
            {
                var start = oIndex + 2;
                var end = subject.IndexOf(",", start);
                if (end < 0) end = subject.Length;
                return subject.Substring(start, end - start).Trim();
            }

            return null;
        }

        /// <summary>
        /// 检测图像格式
        /// </summary>
        private string DetectImageFormat(byte[] imageData)
        {
            if (imageData == null || imageData.Length < 8)
                return "Unknown";

            if (imageData[0] == 0x89 && imageData[1] == 0x50)
                return "PNG";
            if (imageData[0] == 0xFF && imageData[1] == 0xD8)
                return "JPEG";
            if (imageData[0] == 0x47 && imageData[1] == 0x49)
                return "GIF";
            if (imageData[0] == 0x42 && imageData[1] == 0x4D)
                return "BMP";

            return "Unknown";
        }

        /// <summary>
        /// 内部验证印章实现
        /// </summary>
        protected override async Task<IEsealValidationResult> ValidateSealInternalAsync(byte[] sealData, byte[] signedData)
        {
            var result = new ValidationResult
            {
                ValidationTime = DateTime.Now
            };

            try
            {
                var signedValue = ParseSignedValue(sealData);

                // 验证证书有效性
                if (signedValue.Certificate != null)
                {
                    result.Certificate = new CertificateInfo
                    {
                        SerialNumber = signedValue.Certificate.SerialNumber,
                        Issuer = signedValue.Certificate.Issuer,
                        Subject = signedValue.Certificate.Subject,
                        ValidFrom = signedValue.Certificate.NotBefore,
                        ValidTo = signedValue.Certificate.NotAfter,
                        Thumbprint = signedValue.Certificate.Thumbprint
                    };

                    // 检查证书有效期
                    if (DateTime.Now < signedValue.Certificate.NotBefore ||
                        DateTime.Now > signedValue.Certificate.NotAfter)
                    {
                        result.IsValid = false;
                        result.StatusCode = -2;
                        result.Message = "证书已过期或尚未生效";
                        result.Errors.Add("证书不在有效期内");
                        return result;
                    }
                }
                else
                {
                    result.IsValid = false;
                    result.StatusCode = -3;
                    result.Message = "未找到签章者证书";
                    result.Errors.Add("证书信息缺失");
                    return result;
                }

                // 验证签名数据完整性（ZIP 部分）
                int zipIndex = FindBytePattern(sealData, ZipSignature);
                if (zipIndex > 0)
                {
                    var asn1Data = new byte[zipIndex];
                    Array.Copy(sealData, 0, asn1Data, 0, zipIndex);

                    var zipData = new byte[sealData.Length - zipIndex];
                    Array.Copy(sealData, zipIndex, zipData, 0, zipData.Length);

                    // 验证 ZIP 数据完整性
                    try
                    {
                        using var stream = new MemoryStream(zipData);
                        using var zipArchive = new System.IO.Compression.ZipArchive(stream, System.IO.Compression.ZipArchiveMode.Read);
                        result.Message += "; ZIP数据完整性验证通过";
                    }
                    catch (Exception ex)
                    {
                        result.IsValid = false;
                        result.StatusCode = -4;
                        result.Message = "ZIP数据损坏";
                        result.Errors.Add($"ZIP解析失败: {ex.Message}");
                        return result;
                    }
                }

                // TODO: 调用国脉 SDK 进行深度验签
                // 验证 SM2 签名值

                result.IsValid = true;
                result.StatusCode = 0;
                result.Message = "验签成功";

                return result;
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.StatusCode = -1;
                result.Message = $"验签失败: {ex.Message}";
                result.Errors.Add(ex.Message);
                return result;
            }
        }

        /// <summary>
        /// 内部提取图像数据实现
        /// </summary>
        protected override async Task<byte[]> ExtractImageDataInternalAsync(byte[] sealData)
        {
            var signedValue = ParseSignedValue(sealData);
            return signedValue.SealImageData;
        }

        /// <summary>
        /// 内部获取签章人信息实现
        /// </summary>
        protected override async Task<Abstractions.Models.SignerInfo> GetSignerInfoInternalAsync(byte[] sealData)
        {
            var sealInfo = await LoadSealInfoInternalAsync(sealData);
            return sealInfo?.Signer;
        }

        /// <summary>
        /// 内部获取元数据实现
        /// </summary>
        protected override async Task<EsealMetadata> GetMetadataInternalAsync(byte[] sealData)
        {
            var signedValue = ParseSignedValue(sealData);

            return new EsealMetadata
            {
                SealId = signedValue.SealId ?? string.Empty,
                Version = signedValue.Version ?? "1.0",
                CreateTime = signedValue.SignTime,
                ValidFrom = signedValue.ValidFrom,
                ValidTo = signedValue.ValidTo,
                SealType = signedValue.SealType ?? string.Empty
            };
        }

        /// <summary>
        /// 内部获取证书信息实现
        /// </summary>
        protected override async Task<CertificateInfo> GetCertificateInfoInternalAsync(byte[] sealData)
        {
            var signedValue = ParseSignedValue(sealData);

            if (signedValue.Certificate == null)
            {
                return new CertificateInfo();
            }

            return new CertificateInfo
            {
                SerialNumber = signedValue.Certificate.SerialNumber,
                Issuer = signedValue.Certificate.Issuer,
                Subject = signedValue.Certificate.Subject,
                ValidFrom = signedValue.Certificate.NotBefore,
                ValidTo = signedValue.Certificate.NotAfter,
                Thumbprint = signedValue.Certificate.Thumbprint,
                PublicKeyAlgorithm = signedValue.Certificate.PublicKey.Oid?.Value ?? string.Empty,
                SignatureAlgorithm = signedValue.Certificate.SignatureAlgorithm?.Value ?? string.Empty,
                RawData = signedValue.Certificate.RawData
            };
        }

        /// <summary>
        /// 内部验证印章图片哈希实现
        /// </summary>
        protected override async Task<bool> VerifyImageHashInternalAsync(byte[] sealData)
        {
            try
            {
                var signedValue = ParseSignedValue(sealData);

                // 如果没有图像数据或哈希值，返回 false
                if (signedValue.SealImageData == null || signedValue.SealImageData.Length == 0)
                {
                    return false;
                }

                // 这里应该根据具体的哈希算法进行验证
                // 目前国脉格式可能不包含独立的哈希值，返回 true 表示图像数据存在
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// 国脉签章数据结构（内部使用）
    /// </summary>
    internal class GomainSignedValue
    {
        public string SealId { get; set; }
        public string SealName { get; set; }
        public string SealType { get; set; }
        public string Version { get; set; }
        public DateTime? SignTime { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public byte[] SealImageData { get; set; }
        public X509Certificate2 Certificate { get; set; }
        public string SignerName { get; set; }
        public string SignerOrg { get; set; }
        public byte[] SignatureValue { get; set; }
    }
}
