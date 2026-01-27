namespace OFDViewer.Models.BaseStructure.Resources
{
    /// <summary>
    /// 资源的格式。支 持 BMP、JPEG、PNG、TIFF 及 AVS等 格式,其中TIFF格式不支持多页
    /// </summary>
    public enum MultiMediaFormatType
    {
        BMP,
        JPEG,
        PNG,
        TIFF,
        AVS,
        // 中文点阵字库格式
        GBIG2
    }
}
