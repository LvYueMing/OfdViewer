using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using OfdViewer.Eseal.Abstractions.Models;

namespace OfdViewer.Eseal.Abstractions.Interfaces
{
    /// <summary>
    /// 电子印章解析器通用接口
    /// 所有厂商实现必须遵循此接口规范
    /// </summary>
    public interface IEsealParser : IDisposable
    {
        /// <summary>
        /// 解析器名称（厂商标识）
        /// </summary>
        string ParserName { get; }

        /// <summary>
        /// 支持的文件格式
        /// </summary>
        IEnumerable<string> SupportedFormats { get; }

        /// <summary>
        /// 加载印章文件
        /// </summary>
        /// <param name="sealData">印章二进制数据</param>
        /// <returns>印章信息对象</returns>
        Task<IEsealInfo> LoadAsync(byte[] sealData);

        /// <summary>
        /// 验证印章有效性
        /// </summary>
        /// <param name="sealData">印章二进制数据</param>
        /// <param name="signedData">签名数据（可选）</param>
        /// <returns>验签结果</returns>
        Task<IEsealValidationResult> ValidateAsync(byte[] sealData, byte[] signedData = null);

        /// <summary>
        /// 提取印章图像
        /// </summary>
        /// <param name="sealData">印章二进制数据</param>
        /// <returns>印章图像流（PNG格式，可直接被SkiaSharp加载）</returns>
        Task<Stream> ExtractImageAsync(byte[] sealData);

        /// <summary>
        /// 提取印章图像（带指定尺寸）
        /// </summary>
        /// <param name="sealData">印章二进制数据</param>
        /// <param name="width">目标宽度（像素）</param>
        /// <param name="height">目标高度（像素）</param>
        /// <returns>印章图像流</returns>
        Task<Stream> ExtractImageAsync(byte[] sealData, int width, int height);

        /// <summary>
        /// 获取签章人信息
        /// </summary>
        /// <param name="sealData">印章二进制数据</param>
        /// <returns>签章人信息</returns>
        Task<SignerInfo> GetSignerInfoAsync(byte[] sealData);

        /// <summary>
        /// 获取印章元数据
        /// </summary>
        /// <param name="sealData">印章二进制数据</param>
        /// <returns>印章元数据</returns>
        Task<EsealMetadata> GetMetadataAsync(byte[] sealData);

        /// <summary>
        /// 检查是否支持该格式的印章文件
        /// </summary>
        /// <param name="sealData">印章二进制数据</param>
        /// <returns>是否支持</returns>
        bool CanParse(byte[] sealData);
    }
}
