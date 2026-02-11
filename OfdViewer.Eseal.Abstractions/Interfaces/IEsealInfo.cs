using System;
using System.IO;
using OfdViewer.ESeal.Abstractions.Models;

namespace OfdViewer.ESeal.Abstractions.Interfaces
{
    /// <summary>
    /// 印章信息接口
    /// 封装印章的核心信息
    /// 符合 GM/T 0031-2014 安全电子签章密码技术规范
    /// </summary>
    public interface IEsealInfo
    {
        /// <summary>
        /// 印章标识（唯一标识符）
        /// GM/T 0031-2014: eSealID - 电子印章标识
        /// </summary>
        string SealId { get; }

        /// <summary>
        /// 印章名称
        /// GM/T 0031-2014: eSealName - 电子印章名称
        /// </summary>
        string SealName { get; }

        /// <summary>
        /// 印章类型（公章、私章等）
        /// GM/T 0031-2014: eSealType - 电子印章类型
        /// </summary>
        string SealType { get; }

        /// <summary>
        /// 印章有效期开始时间
        /// GM/T 0031-2014: validStart - 有效期起始时间
        /// </summary>
        DateTime? ValidFrom { get; }

        /// <summary>
        /// 印章有效期结束时间
        /// GM/T 0031-2014: validEnd - 有效期结束时间
        /// </summary>
        DateTime? ValidTo { get; }

        /// <summary>
        /// 签章人信息
        /// GM/T 0031-2014: signerInfo - 签章人信息
        /// </summary>
        SignerInfo Signer { get; }

        /// <summary>
        /// 印章图像数据
        /// GM/T 0031-2014: pictureData - 印章图片数据
        /// </summary>
        byte[] ImageData { get; }

        /// <summary>
        /// 图像格式（PNG/JPG等）
        /// GM/T 0031-2014: pictureType - 图片类型
        /// </summary>
        string ImageFormat { get; }

        /// <summary>
        /// 印章图像宽度（像素）
        /// GM/T 0031-2014: width - 图片宽度
        /// </summary>
        int ImageWidth { get; }

        /// <summary>
        /// 印章图像高度（像素）
        /// GM/T 0031-2014: height - 图片高度
        /// </summary>
        int ImageHeight { get; }

        /// <summary>
        /// 创建时间
        /// GM/T 0031-2014: createTime - 创建时间
        /// </summary>
        DateTime? CreateTime { get; }

        /// <summary>
        /// 印章版本
        /// GM/T 0031-2014: version - 版本号
        /// </summary>
        string Version { get; }

        /// <summary>
        /// 印章制作单位
        /// GM/T 0031-2014: maker - 印章制作单位
        /// </summary>
        string Maker { get; }

        /// <summary>
        /// 印章制作时间
        /// GM/T 0031-2014: makeTime - 印章制作时间
        /// </summary>
        DateTime? MakeTime { get; }

        /// <summary>
        /// 印章图片哈希值（用于完整性验证）
        /// GM/T 0031-2014: pictureHash - 印章图片哈希值
        /// </summary>
        byte[] ImageHash { get; }

        /// <summary>
        /// 印章图片哈希算法
        /// GM/T 0031-2014: hashAlgorithm - 哈希算法标识
        /// </summary>
        string HashAlgorithm { get; }

        /// <summary>
        /// 印章所属单位名称
        /// GM/T 0031-2014: certInfoType 中的单位名称
        /// </summary>
        string Organization { get; }

        /// <summary>
        /// 印章所属单位统一社会信用代码
        /// </summary>
        string OrganizationCode { get; }

        /// <summary>
        /// 印章状态（有效、过期、吊销等）
        /// </summary>
        SealStatus Status { get; }

        /// <summary>
        /// 获取印章图像流
        /// </summary>
        /// <returns>图像流</returns>
        Stream GetImageStream();

        /// <summary>
        /// 验证印章是否在有效期内
        /// </summary>
        /// <returns>是否在有效期内</returns>
        bool IsValidPeriod();
    }
}
