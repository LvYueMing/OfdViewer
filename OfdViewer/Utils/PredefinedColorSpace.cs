using OFDViewer.Models.BaseType;

namespace OFDViewer.Utils
{
    /// <summary>
    /// 预定义色彩空间（GB/T 33190-2016 9.2 色彩空间）。
    /// 标准规定 CT_Color 的 ColorSpace 属性类型为 ST_RefID（引用资源中的颜色空间定义），
    /// 但部分 OFD 生成工具直接使用预定义色彩空间名称（GRAY / RGB / CMYK）作为取值。
    /// 该类提供名称到标准约定标识的映射（GRAY=1、RGB=2、CMYK=3），供解析器做兼容处理。
    /// </summary>
    public static class PredefinedColorSpace
    {
        public const uint GrayId = 1;
        public const uint RgbId = 2;
        public const uint CmykId = 3;

        /// <summary>
        /// 尝试将预定义色彩空间名称解析为引用标识。
        /// </summary>
        /// <param name="value">色彩空间名称（GRAY/GREY/RGB/CMYK，大小写不敏感）</param>
        /// <param name="refId">解析出的引用标识</param>
        /// <returns>是否为可识别的预定义色彩空间名称</returns>
        public static bool TryParse(string value, out ST_RefID refId)
        {
            refId = ST_RefID.Invalid;
            if (string.IsNullOrWhiteSpace(value))
                return false;

            uint id = value.Trim().ToUpperInvariant() switch
            {
                "GRAY" => GrayId,
                "GREY" => GrayId,
                "RGB" => RgbId,
                "CMYK" => CmykId,
                _ => 0
            };

            if (id == 0)
                return false;

            refId = new ST_RefID(new ST_ID(id));
            return true;
        }
    }
}
