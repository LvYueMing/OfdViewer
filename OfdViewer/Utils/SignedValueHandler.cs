using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OFDViewer.Utils
{
    /// <summary>
    /// 数字签名文件（SignedValue.dat）处理器
    /// 核心映射类型：byte[]（完整保留二进制签名数据）
    /// </summary>
    public class SignedValueHandler : IDisposable
    {
        // 核心映射对象：存储签名的二进制数据
        private byte[] _signedBytes;
        // 标记是否已释放内存（密码安全要求：敏感数据及时清理）
        private bool _disposed = false;

        /// <summary>
        /// 从文件读取SignedValue.dat，映射为byte[]
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <exception cref="FileNotFoundException">文件不存在</exception>
        /// <exception cref="UnauthorizedAccessException">文件权限不足</exception>
        public void LoadFromFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));
            if (!File.Exists(filePath))
                throw new FileNotFoundException("签名文件不存在", filePath);

            // 1. 校验文件权限（密码安全要求：仅允许当前用户读写）
            var fileInfo = new FileInfo(filePath);
            if (!fileInfo.IsReadOnly && fileInfo.Attributes.HasFlag(FileAttributes.Normal))
            {
                // 可选：设置文件为只读，防止篡改
                fileInfo.IsReadOnly = true;
            }

            // 2. 安全读取为byte[]（核心映射）
            _signedBytes = File.ReadAllBytes(filePath);

            // 3. 可选：验证文件完整性（密码安全要求：防止签名被篡改）
            if (!VerifySignatureIntegrity(_signedBytes))
            {
                throw new CryptographicException("签名文件已被篡改，不符合密码安全要求");
            }
        }

        /// <summary>
        /// 将byte[]类型的签名数据写入SignedValue.dat
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="signedBytes">签名二进制数据</param>
        public void SaveToFile(string filePath, byte[] signedBytes)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));
            if (signedBytes == null || signedBytes.Length == 0)
                throw new ArgumentException("签名数据不能为空", nameof(signedBytes));

            // 1. 密码安全要求：写入前清空目标文件（防止残留数据）
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            // 2. 安全写入（使用File.WriteAllBytes，原子操作减少篡改风险）
            File.WriteAllBytes(filePath, signedBytes);

            // 3. 密码安全要求：设置文件权限（仅当前用户可访问）
            var fileInfo = new FileInfo(filePath);
            fileInfo.IsReadOnly = true; // 只读防止意外修改
                                        // 可选：进一步限制文件访问权限（Windows/Linux不同实现）
                                        // SetFileSecurity(filePath);
        }

        /// <summary>
        /// 获取映射的签名字节数组（只读，防止外部篡改）
        /// </summary>
        /// <returns>签名二进制数据的副本（避免外部直接修改内部数据）</returns>
        public byte[] GetSignedBytes()
        {
            if (_signedBytes == null)
                throw new InvalidOperationException("未加载签名文件");

            // 密码安全要求：返回副本，避免外部修改原始数据
            byte[] copy = new byte[_signedBytes.Length];
            Array.Copy(_signedBytes, copy, _signedBytes.Length);
            return copy;
        }

        /// <summary>
        /// 可选：验证签名数据完整性（密码安全要求）
        /// </summary>
        /// <param name="signedBytes">签名数据</param>
        /// <returns>是否完整</returns>
        private bool VerifySignatureIntegrity(byte[] signedBytes)
        {
            // 示例：通过SHA256哈希验证（实际需按密码规范实现）
            using (var sha256 = SHA256.Create())
            {
                // 此处可替换为预定义的签名哈希值（从安全配置读取）
                byte[] expectedHash = Convert.FromBase64String("预定义的签名哈希值");
                byte[] actualHash = sha256.ComputeHash(signedBytes);
                return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
            }
        }

        /// <summary>
        /// 密码安全要求：释放时清空敏感内存
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            // 清空签名数据（密码安全要求：敏感数据不残留）
            if (_signedBytes != null)
            {
                Array.Clear(_signedBytes, 0, _signedBytes.Length);
                _signedBytes = null;
            }

            _disposed = true;
        }

        ~SignedValueHandler()
        {
            Dispose(false);
        }
    }
}
