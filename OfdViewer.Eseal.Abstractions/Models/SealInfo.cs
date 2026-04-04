using System;
using System.IO;
using OfdViewer.ESeal.Abstractions.Interfaces;

namespace OfdViewer.ESeal.Abstractions.Models
{
    /// <summary>
    /// 印章信息数据模型
    /// 实现 IEsealInfo 接口
    /// 符合 GM/T 0031-2014 安全电子签章密码技术规范
    /// </summary>
    public class SealInfo : IEsealInfo
    {
        /// <summary>
        /// 印章标识（唯一标识符）
        /// GM/T 0031-2014: eSealID - 电子印章标识
        /// </summary>
        public string SealId { get; set; } = string.Empty;

        /// <summary>
        /// 印章名称
        /// GM/T 0031-2014: eSealName - 电子印章名称
        /// </summary>
        public string SealName { get; set; } = string.Empty;

        /// <summary>
        /// 印章类型（公章、财务章、合同章、法人章等）
        /// GM/T 0031-2014: eSealType - 电子印章类型
        /// </summary>
        public string SealType { get; set; } = string.Empty;

        /// <summary>
        /// 印章有效期开始时间
        /// GM/T 0031-2014: validStart - 有效期起始时间
        /// </summary>
        public DateTime? ValidFrom { get; set; }

        /// <summary>
        /// 印章有效期结束时间
        /// GM/T 0031-2014: validEnd - 有效期结束时间
        /// </summary>
        public DateTime? ValidTo { get; set; }

        /// <summary>
        /// 签章人信息
        /// GM/T 0031-2014: signerInfo - 签章人信息
        /// </summary>
        public SignerInfo Signer { get; set; } = new SignerInfo();

        /// <summary>
        /// 印章图像数据（符合 GM/T 0031-2014 的印章图片格式）
        /// GM/T 0031-2014: pictureData - 印章图片数据
        /// </summary>
        public byte[] ImageData { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// 图像格式（PNG/JPG/BMP/GIF/SVG等）
        /// GM/T 0031-2014: pictureType - 图片类型
        /// </summary>
        public string ImageFormat { get; set; } = string.Empty;

        /// <summary>
        /// 印章图像宽度（像素）
        /// GM/T 0031-2014: width - 图片宽度
        /// </summary>
        public int ImageWidth { get; set; }

        /// <summary>
        /// 印章图像高度（像素）
        /// GM/T 0031-2014: height - 图片高度
        /// </summary>
        public int ImageHeight { get; set; }

        /// <summary>
        /// 创建时间
        /// GM/T 0031-2014: createTime - 创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// 印章版本
        /// GM/T 0031-2014: version - 版本号
        /// </summary>
        public string Version { get; set; } = "1.0";

        /// <summary>
        /// 印章制作单位
        /// GM/T 0031-2014: maker - 印章制作单位
        /// </summary>
        public string Maker { get; set; } = string.Empty;

        /// <summary>
        /// 印章制作时间
        /// GM/T 0031-2014: makeTime - 印章制作时间
        /// </summary>
        public DateTime? MakeTime { get; set; }

        /// <summary>
        /// 印章图片哈希值（用于完整性验证）
        /// GM/T 0031-2014: pictureHash - 印章图片哈希值
        /// </summary>
        public byte[] ImageHash { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// 印章图片哈希算法
        /// GM/T 0031-2014: hashAlgorithm - 哈希算法标识
        /// </summary>
        public string HashAlgorithm { get; set; } = string.Empty;

        /// <summary>
        /// 印章所属单位名称
        /// GM/T 0031-2014: certInfoType - 证书信息类型中的单位名称
        /// </summary>
        public string Organization { get; set; } = string.Empty;

        /// <summary>
        /// 印章所属单位统一社会信用代码
        /// </summary>
        public string OrganizationCode { get; set; } = string.Empty;

        /// <summary>
        /// 印章状态（有效、过期、吊销等）
        /// </summary>
        public SealStatus Status { get; set; } = SealStatus.Valid;

        /// <summary>
        /// 获取印章图像流
        /// </summary>
        /// <returns>图像流</returns>
        public Stream GetImageStream()
        {
            return ImageData != null && ImageData.Length > 0 
                ? new MemoryStream(ImageData) 
                : Stream.Null;
        }

        /// <summary>
        /// 验证印章是否在有效期内
        /// </summary>
        /// <returns>是否在有效期内</returns>
        public bool IsValidPeriod()
        {
            var now = DateTime.Now;
            return Status == SealStatus.Valid &&
                   (!ValidFrom.HasValue || now >= ValidFrom.Value) &&
                   (!ValidTo.HasValue || now <= ValidTo.Value);
        }
    }

    /// <summary>
    /// 印章状态枚举
    /// </summary>
    public enum SealStatus
    {
        /// <summary>
        /// 有效
        /// </summary>
        Valid = 0,

        /// <summary>
        /// 已过期
        /// </summary>
        Expired = 1,

        /// <summary>
        /// 已吊销
        /// </summary>
        Revoked = 2,

        /// <summary>
        /// 已冻结
        /// </summary>
        Frozen = 3,

        /// <summary>
        /// 未知状态
        /// </summary>
        Unknown = 99
    }
}
