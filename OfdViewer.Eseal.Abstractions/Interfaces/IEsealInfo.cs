using System;
using System.IO;
using OfdViewer.Eseal.Abstractions.Models;

namespace OfdViewer.Eseal.Abstractions.Interfaces
{
    /// <summary>
    /// 印章信息接口
    /// 封装印章的核心信息
    /// </summary>
    public interface IEsealInfo
    {
        /// <summary>
        /// 印章标识
        /// </summary>
        string SealId { get; }

        /// <summary>
        /// 印章名称
        /// </summary>
        string SealName { get; }

        /// <summary>
        /// 印章类型（公章、私章等）
        /// </summary>
        string SealType { get; }

        /// <summary>
        /// 印章有效期开始时间
        /// </summary>
        DateTime? ValidFrom { get; }

        /// <summary>
        /// 印章有效期结束时间
        /// </summary>
        DateTime? ValidTo { get; }

        /// <summary>
        /// 签章人信息
        /// </summary>
        SignerInfo Signer { get; }

        /// <summary>
        /// 印章图像数据
        /// </summary>
        byte[] ImageData { get; }

        /// <summary>
        /// 图像格式（PNG/JPG等）
        /// </summary>
        string ImageFormat { get; }

        /// <summary>
        /// 印章图像宽度（像素）
        /// </summary>
        int ImageWidth { get; }

        /// <summary>
        /// 印章图像高度（像素）
        /// </summary>
        int ImageHeight { get; }

        /// <summary>
        /// 创建时间
        /// </summary>
        DateTime? CreateTime { get; }

        /// <summary>
        /// 印章版本
        /// </summary>
        string Version { get; }

        /// <summary>
        /// 获取印章图像流
        /// </summary>
        /// <returns>图像流</returns>
        Stream GetImageStream();
    }
}
