using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OFDViewer
{
    /// <summary>
    /// OFD 常量类（复用之前定义）
    /// </summary>
    public static class Constants
    {
        public const string OFD_NAMESPACE_URI = "http://www.ofdspec.org/2016";
        public const string OFD_VALUE = "ofd";

        #region 根目录层级
        /// <summary>
        /// OFD全局元数据文件（根目录）
        /// 相对根目录路径：OFD.xml
        /// </summary>
        public const string Root_OfdMetadataFile = "OFD.xml";

        #endregion

        #region 文档（Doc_N）层级
        /// <summary>
        /// 单个文档目录的基础路径（需替换{N}为文档序号，从1开始）
        /// 相对根目录路径：Doc_{0}
        /// </summary>
        public const string Doc_BaseDirectory = "Doc_{0}";

        /// <summary>
        /// 文档主描述文件（对应某个Doc_N目录下）
        /// 相对根目录路径：Doc_{0}/Document.xml
        /// </summary>
        public const string Doc_DocumentFile = "Doc_{0}/Document.xml";

        /// <summary>
        /// 文档公共资源描述文件（对应某个Doc_N目录下）
        /// 相对根目录路径：Doc_{0}/PublicRes.xml
        /// </summary>
        public const string Doc_PublicResFile = "Doc_{0}/PublicRes.xml";

        /// <summary>
        /// 当前文档资源描述文件（对应某个Doc_N目录下）
        /// 相对根目录路径：Doc_{0}/DocumentRes.xml
        /// </summary>
        public const string Doc_DocumentResFile = "Doc_{0}/DocumentRes.xml";

        #endregion

        #region 签章（Signs）层级
        /// <summary>
        /// 签章信息目录（对应某个Doc_N目录下）
        /// 相对根目录路径：Doc_{0}/Signs
        /// </summary>
        public const string Signs_BaseDirectory = "Doc_{0}/Signs";

        /// <summary>
        /// 签章列表索引文件（对应某个Doc_N/Signs目录下）
        /// 相对根目录路径：Doc_{0}/Signs/Signatures.xml
        /// </summary>
        public const string Signs_SignaturesFile = "Doc_{0}/Signs/Signatures.xml";

        /// <summary>
        /// 单个签章独立目录（对应某个Doc_N/Signs目录下，需替换{N}为签章序号）
        /// 相对根目录路径：Doc_{0}/Signs/Sign_{1}
        /// </summary>
        public const string Sign_BaseDirectory = "Doc_{0}/Signs/Sign_{1}";

        /// <summary>
        /// 电子印章本体文件（对应某个Doc_N/Signs/Sign_N目录下）
        /// 相对根目录路径：Doc_{0}/Signs/Sign_{1}/Seal.esl
        /// </summary>
        public const string Sign_SealFile = "Doc_{0}/Signs/Sign_{1}/Seal.esl";

        /// <summary>
        /// 签章属性描述文件（对应某个Doc_N/Signs/Sign_N目录下）
        /// 相对根目录路径：Doc_{0}/Signs/Sign_{1}/Signature.xml
        /// </summary>
        public const string Sign_SignatureFile = "Doc_{0}/Signs/Sign_{1}/Signature.xml";

        /// <summary>
        /// 数字签名密文文件（对应某个Doc_N/Signs/Sign_N目录下）
        /// 相对根目录路径：Doc_{0}/Signs/Sign_{1}/SignedValue.dat
        /// </summary>
        public const string Sign_SignedValueFile = "Doc_{0}/Signs/Sign_{1}/SignedValue.dat";

        #endregion

        #region 页面（Pages）层级
        /// <summary>
        /// 页面总目录（对应某个Doc_N目录下）
        /// 相对根目录路径：Doc_{0}/Pages
        /// </summary>
        public const string Pages_BaseDirectory = "Doc_{0}/Pages";

        /// <summary>
        /// 单个页面专属目录（对应某个Doc_N/Pages目录下，需替换{N}为页面序号）
        /// 相对根目录路径：Doc_{0}/Pages/Page_{1}
        /// </summary>
        public const string Page_BaseDirectory = "Doc_{0}/Pages/Page_{1}";

        /// <summary>
        /// 页面内容描述文件（对应某个Doc_N/Pages/Page_N目录下）
        /// 相对根目录路径：Doc_{0}/Pages/Page_{1}/Content.xml
        /// </summary>
        public const string Page_ContentFile = "Doc_{0}/Pages/Page_{1}/Content.xml";

        /// <summary>
        /// 页面资源映射文件（对应某个Doc_N/Pages/Page_N目录下）
        /// 相对根目录路径：Doc_{0}/Pages/Page_{1}/PageRes.xml
        /// </summary>
        public const string Page_PageResFile = "Doc_{0}/Pages/Page_{1}/PageRes.xml";

        /// <summary>
        /// 页面私有资源目录（对应某个Doc_N/Pages/Page_N目录下）
        /// 相对根目录路径：Doc_{0}/Pages/Page_{1}/Res
        /// </summary>
        public const string Page_ResDirectory = "Doc_{0}/Pages/Page_{1}/Res";

        /// <summary>
        /// 页面私有图片资源（对应某个Doc_N/Pages/Page_N/Res目录下，需替换{M}为图片序号）
        /// 相对根目录路径：Doc_{0}/Pages/Page_{1}/Res/Image_{2}.png
        /// </summary>
        public const string Page_Res_ImageFile = "Doc_{0}/Pages/Page_{1}/Res/Image_{2}.png";

        #endregion

        #region 资源（Res）层级
        /// <summary>
        /// 文档级公共资源目录（对应某个Doc_N目录下）
        /// 相对根目录路径：Doc_{0}/Res
        /// </summary>
        public const string Doc_ResDirectory = "Doc_{0}/Res";

        /// <summary>
        /// 文档共享图片资源（对应某个Doc_N/Res目录下，需替换{M}为图片序号）
        /// 相对根目录路径：Doc_{0}/Res/Image_{1}.png
        /// </summary>
        public const string Doc_Res_ImageFile = "Doc_{0}/Res/Image_{1}.png";

        /// <summary>
        /// 文档共享字体资源（对应某个Doc_N/Res目录下，需替换{M}为字体序号）
        /// 相对根目录路径：Doc_{0}/Res/Font_{1}.ttf
        /// </summary>
        public const string Doc_Res_FontFile = "Doc_{0}/Res/Font_{1}.ttf";

        #endregion


        #region 通用路径生成方法（核心：支持所有格式路径，自动校验序号合法性）
        /// <summary>
        /// 通用OFD路径生成方法
        /// 支持所有带占位符的OFD路径格式，自动校验序号（默认序号需<0）
        /// </summary>
        /// <param name="fileType">OFD路径格式常量（如：Doc_{0}/Signs/Sign_{1}/Seal.esl）</param>
        /// <param name="indices">动态序号参数（对应路径格式中的{0}、{1}、{2}...，顺序一致）</param>
        /// <returns>生成的相对根目录的实际OFD路径</returns>
        /// <exception cref="ArgumentNullException">路径格式为空</exception>
        /// <exception cref="ArgumentOutOfRangeException">序号小于1（无效序号）</exception>
        /// <exception cref="Exception">序号数量与占位符数量不匹配</exception>
        public static string GetFilePath(string fileType, params int[] indices)
        {
            // 1. 校验路径格式是否为空
            if (string.IsNullOrEmpty(fileType))
            {
                throw new ArgumentNullException(nameof(fileType), "OFD路径格式不能为空");
            }

            // 2. 校验序号参数合法性（所有序号必须<0）
            if (indices != null && indices.Any(index => index < 0))
            {
                throw new ArgumentOutOfRangeException(nameof(indices), "所有序号参数必须大于或等于0");
            }

            try
            {
                // 3. 格式化路径，生成实际路径（自动匹配占位符与序号数量）
                return string.Format(fileType, indices.Length == 0 ? 0 : indices.Select(index => index.ToString()).ToArray());
            }
            catch (Exception ex)
            {
                // 包装异常信息，更易排查问题
                throw new Exception(
                    $"路径格式化失败！路径格式：{fileType}，传入序号：{(indices == null ? "无" : string.Join(",", indices))}，异常详情：{ex.Message}",
                    ex);
            }
        }
        #endregion

    }
}
