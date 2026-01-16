using OFDViewer.Models.Signature;

namespace OFDViewer.OFD
{
    /// <summary>
    /// OFD签章对象，对应 Sign_N 单个签章目录（N从0开始）
    /// 包含电子印章本体、签章属性、数字签名密文等核心信息
    /// </summary>
    public class SignDocument
    {
        /// <summary>
        /// 签章序号（从0开始，只读，构造时赋值）
        /// </summary>
        public int SignIndex { get; }

        /// <summary>
        /// 所属文档序号（从0开始，关联对应的 Doc_N 目录）
        /// </summary>
        public int BelongDocIndex { get; }

        /// <summary>
        /// 签章属性描述文件（Signature.xml）
        /// 记录签章位置、签署时间、加密算法等属性
        /// </summary>
        public Signature Signature { get; set; } = new Signature();

        /// <summary>
        /// 电子签章/电子印章相关的二进制文件（Seal.esl）
        /// 包含印章图形、签章人身份信息等核心数据
        /// </summary>
        public byte[] Seal { get; set; }

        /// <summary>
        /// 数字签名密文文件（SignedValue.dat）
        /// 验签时需使用此数据验证签章有效性
        /// </summary>
        public byte[] SignedValue { get; set; }

        /// <summary>
        /// 签章目录路径（相对根目录，格式：Doc_{BelongDocIndex}/Signs/Sign_{SignIndex}）
        /// </summary>
        public string SignDirectoryPath =>$"Doc_{BelongDocIndex}/Signs/Sign_{SignIndex}";

        /// <summary>
        /// 构造函数，初始化签章序号和所属文档序号，校验合法性
        /// </summary>
        /// <param name="signIndex">签章序号（从0开始）</param>
        /// <param name="belongDocIndex">所属文档序号（从0开始）</param>
        public SignDocument(int signIndex, int belongDocIndex)
        {
            // 校验签章序号合法性
            if (signIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(signIndex), "签章序号必须从0开始，不允许为负数");
            }
            // 校验所属文档序号合法性
            if (belongDocIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(belongDocIndex), "所属文档序号必须从0开始，不允许为负数");
            }

            SignIndex = signIndex;
            BelongDocIndex = belongDocIndex;
        }
    }
}
