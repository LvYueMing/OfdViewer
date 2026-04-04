using System.ComponentModel;

namespace OFDViewer.Models.Signature
{
    /// <summary>
    /// 摘要方法
    /// </summary>
    public enum CheckMethodEnum
    {
        MD5,
        SHA1,
        [Description("1.2.156.10197.1.401")]
        SM3  
    }
}
