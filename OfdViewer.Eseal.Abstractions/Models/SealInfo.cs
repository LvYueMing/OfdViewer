using System;
using System.IO;
using OfdViewer.Eseal.Abstractions.Interfaces;

namespace OfdViewer.Eseal.Abstractions.Models
{
    /// <summary>
    /// 印章信息数据模型
    /// 实现 IEsealInfo 接口
    /// </summary>
    public class SealInfo : IEsealInfo
    {
        /// <summary>
        /// 印章标识
        /// </summary>
        public string SealId { get; set; }

        /// <summary>
        /// 印章名称
        /// </summary>
        public string SealName { get; set; }

        /// <summary>
        /// 印章类型（公章、私章等）
        /// </summary>
        public string SealType { get; set; }

        /// <summary>
        /// 印章有效期开始时间
        /// </summary>
        public DateTime? ValidFrom { get; set; }

        /// <summary>
        /// 印章有效期结束时间
        /// </summary>
        public DateTime? ValidTo { get; set; }

        /// <summary>
        /// 签章人信息
        /// </summary>
        public SignerInfo Signer { get; set; }

        /// <summary>
        /// 印章图像数据
        /// </summary>
        public byte[] ImageData { get; set; }

        /// <summary>
        /// 图像格式（PNG/JPG等）
        /// </summary>
        public string ImageFormat { get; set; }

        /// <summary>
        /// 印章图像宽度（像素）
        /// </summary>
        public int ImageWidth { get; set; }

        /// <summary>
        /// 印章图像高度（像素）
        /// </summary>
        public int ImageHeight { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// 印章版本
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// 获取印章图像流
        /// </summary>
        /// <returns>图像流</returns>
        public Stream GetImageStream()
        {
            return ImageData != null ? new MemoryStream(ImageData) : null;
        }
    }
}
