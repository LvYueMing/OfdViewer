namespace OFDViewer.Models.BaseStructure.DocumentRoot
{
    /// <summary>
    /// 自动缩放模式, 可取值如下:
    ///   Default———默认缩放
    ///   FitHeight———适合高度
    ///   FitWidth———适合宽度
    ///   FitRect———适合区域
    /// 默认值为 Default
    /// </summary>
    public enum VPreferencesZoomMode
    {
        Default,
        FitHeight,
        FitWidth,
        FitRect
    }
}
