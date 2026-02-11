using System;
using System.Collections.Generic;

namespace OfdViewer.ESeal.Abstractions.Models
{
    /// <summary>
    /// 印章元数据
    /// 符合 GM/T 0031-2014 安全电子签章密码技术规范
    /// </summary>
    public class EsealMetadata
    {
        /// <summary>
        /// 印章标识
        /// GM/T 0031-2014: eSealID - 电子印章标识
        /// </summary>
        public string SealId { get; set; } = string.Empty;

        /// <summary>
        /// 印章版本
        /// GM/T 0031-2014: version - 版本号
        /// </summary>
        public string Version { get; set; } = "1.0";

        /// <summary>
        /// 印章创建时间
        /// GM/T 0031-2014: createTime - 创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// 印章生效时间
        /// GM/T 0031-2014: validStart - 有效期起始时间
        /// </summary>
        public DateTime? ValidFrom { get; set; }

        /// <summary>
        /// 印章失效时间
        /// GM/T 0031-2014: validEnd - 有效期结束时间
        /// </summary>
        public DateTime? ValidTo { get; set; }

        /// <summary>
        /// 印章类型
        /// GM/T 0031-2014: eSealType - 电子印章类型
        /// 例如：公章、财务章、合同章、法人章等
        /// </summary>
        public string SealType { get; set; } = string.Empty;

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
        /// 印章名称
        /// GM/T 0031-2014: eSealName - 电子印章名称
        /// </summary>
        public string SealName { get; set; } = string.Empty;

        /// <summary>
        /// 印章所属单位名称
        /// GM/T 0031-2014: certInfoType 中的单位名称
        /// </summary>
        public string Organization { get; set; } = string.Empty;

        /// <summary>
        /// 印章所属单位统一社会信用代码
        /// </summary>
        public string OrganizationCode { get; set; } = string.Empty;

        /// <summary>
        /// 印章图片类型
        /// GM/T 0031-2014: pictureType - 图片类型（PNG/JPG/BMP/GIF/SVG等）
        /// </summary>
        public string ImageType { get; set; } = string.Empty;

        /// <summary>
        /// 印章图片宽度
        /// GM/T 0031-2014: width - 图片宽度
        /// </summary>
        public int ImageWidth { get; set; }

        /// <summary>
        /// 印章图片高度
        /// GM/T 0031-2014: height - 图片高度
        /// </summary>
        public int ImageHeight { get; set; }

        /// <summary>
        /// 印章图片哈希算法
        /// GM/T 0031-2014: hashAlgorithm - 哈希算法标识
        /// </summary>
        public string HashAlgorithm { get; set; } = string.Empty;

        /// <summary>
        /// 印章图片哈希值
        /// GM/T 0031-2014: pictureHash - 印章图片哈希值
        /// </summary>
        public byte[] ImageHash { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// 签名算法标识
        /// GM/T 0031-2014: signatureAlgorithm - 签名算法标识
        /// 例如：SM2withSM3、RSAwithSHA256 等
        /// </summary>
        public string SignatureAlgorithm { get; set; } = string.Empty;

        /// <summary>
        /// 印章状态
        /// </summary>
        public SealStatus Status { get; set; } = SealStatus.Valid;

        /// <summary>
        /// 扩展属性
        /// 用于存储厂商特定的额外信息
        /// </summary>
        public Dictionary<string, string> ExtendedProperties { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// 获取印章有效期剩余天数
        /// </summary>
        /// <returns>剩余天数，如果已过期返回负数</returns>
        public int GetRemainingDays()
        {
            if (!ValidTo.HasValue)
                return int.MaxValue;

            return (ValidTo.Value - DateTime.Now).Days;
        }

        /// <summary>
        /// 检查印章是否在有效期内
        /// </summary>
        /// <returns>是否在有效期内</returns>
        public bool IsValid()
        {
            var now = DateTime.Now;
            return Status == SealStatus.Valid &&
                   (!ValidFrom.HasValue || now >= ValidFrom.Value) &&
                   (!ValidTo.HasValue || now <= ValidTo.Value);
        }
    }
}
