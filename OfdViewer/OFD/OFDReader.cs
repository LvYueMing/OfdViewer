using OFDViewer.Models.BaseStructure.MainEntry;
using OFDViewer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace OFDViewer.OFD
{
    /// <summary>
    /// OFD文件读取/解析类,仅负责读取.ofd文件并解析为实体对象
    /// 无任何写入/创建逻辑，实现IDisposable接口确保资源安全释放
    /// </summary>
    public class OFDReader : IDisposable
    {
        // 私有字段：OFD归档对象（只读，确保初始化后不可修改）
        private readonly OFDArchive _archive;

        // 私有字段：是否已释放资源（线程安全的布尔标识）
        private bool _disposed = false;

        /// <summary>
        /// OFD 文档信息（延迟初始化，避免构造时提前解析造成性能损耗）
        /// </summary>
        public OFDDocument OfdDocument { get; private set; }

        /// <summary>
        /// 初始化 OFD 读取器（文件路径重载，最常用）
        /// </summary>
        /// <param name="filePath">OFD 文件路径</param>
        /// <exception cref="ArgumentNullException">文件路径为空/空白时抛出</exception>
        /// <exception cref="FileNotFoundException">文件不存在时抛出</exception>
        public OFDReader(string filePath)
        {
            // 严格参数校验，明确异常信息
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath), "OFD文件路径不能为空或仅包含空白字符");

            if (!File.Exists(filePath))
                throw new FileNotFoundException("指定的OFD文件不存在，请检查路径是否正确", filePath);

            // 初始化OFD归档
            _archive = OFDArchive.OpenFromFile(filePath);
            // 可选：初始化空的OFDDocument，避免后续使用时出现NullReferenceException
            OfdDocument = new OFDDocument();
        }

        /// <summary>
        /// 初始化 OFD 读取器（流重载，支持灵活的流场景）
        /// </summary>
        /// <param name="stream">OFD 文件流（支持FileStream/MemoryStream等）</param>
        /// <param name="leaveOpen">是否保持流打开状态</param>
        /// <exception cref="ArgumentNullException">输入流为空时抛出</exception>
        /// <exception cref="ArgumentException">输入流不可读时抛出</exception>
        public OFDReader(Stream stream, bool leaveOpen)
        {
            // 增强参数校验，提升容错性
            if (stream == null)
                throw new ArgumentNullException(nameof(stream), "OFD输入流不能为空");

            if (!stream.CanRead)
                throw new ArgumentException("OFD输入流不支持读取操作，请提供可读的流", nameof(stream));

            // 初始化OFD归档
            _archive = OFDArchive.OpenFromStream(stream, leaveOpen: leaveOpen);
            // 可选：初始化空的OFDDocument，避免后续使用时出现NullReferenceException
            OfdDocument = new OFDDocument();
        }

        /// <summary>
        /// 解析 OFD 主入口文件（OFD.xml）
        /// </summary>
        /// <returns>解析后的RootOFD全局元数据对象</returns>
        /// <exception cref="InvalidOperationException">解析失败时抛出（包含内部异常信息）</exception>
        /// <exception cref="ObjectDisposedException">对象已释放时抛出</exception>
        public RootOFD ParseRootOFD()
        {
            // 先校验对象是否已释放，避免操作已释放的资源
            if (_disposed)
                throw new ObjectDisposedException(nameof(OFDReader), "OFD读取器已释放，无法执行解析操作");

            try
            {
                // 读取 OFD.xml 入口文件（核心配置文件）
                using var ofdStream = _archive.GetFileStream("OFD.xml");
                // 二次校验流有效性
                if (ofdStream == null || !ofdStream.CanRead)
                    throw new InvalidDataException("无法读取OFD核心文件OFD.xml，文件可能损坏或格式无效");

                // 验证文件签名（可选，保留扩展入口）
                // ValidateOFDSignature(ofdStream);

                // 反序列化获取RootOFD对象
                var rootOfd = XmlHelper.DeserializeFromStream<RootOFD>(ofdStream);
                // 同步解析结果到OfdDocument属性，保持数据一致性
                if (rootOfd != null)
                {
                    OfdDocument.OfdMetadata = rootOfd;
                    // 可选：自动加载文档数量对应的子文档框架
                    LoadDocFramework(rootOfd.DocCount);
                }

                // 加载相关资源（可选，保留扩展入口）
                // LoadRelatedResources();

                return rootOfd;
            }
            catch (InvalidDataException ex)
            {
                throw new InvalidOperationException("解析OFD核心文件OFD.xml失败，文件格式无效或已损坏", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("解析OFD文档失败，请检查文件完整性", ex);
            }
        }

        /// <summary>
        /// 辅助方法：根据文档数量加载子文档框架（提升实用性）
        /// </summary>
        /// <param name="docCount">OFD文档数量</param>
        private void LoadDocFramework(int docCount)
        {
            if (docCount <= 0) return;

            // 清空原有文档集合，避免重复加载
            OfdDocument.Docs.Clear();

            // 根据文档数量创建子文档框架
            for (int i = 1; i <= docCount; i++)
            {
                OfdDocument.AddNewDoc();
            }
        }

        #region IDisposable 实现（线程安全，规范释放资源）
        /// <summary>
        /// 受保护的释放方法，区分托管资源和非托管资源
        /// </summary>
        /// <param name="disposing">是否释放托管资源</param>
        protected virtual void Dispose(bool disposing)
        {
            // 双重校验，确保资源仅释放一次（线程安全）
            if (!_disposed)
            {
                if (disposing)
                {
                    // 释放托管资源：OFDArchive（实现IDisposable的托管对象）
                    _archive?.Dispose();
                    // 清空OfdDocument引用，帮助GC回收
                    OfdDocument = null;
                }

                // 标记资源已释放
                _disposed = true;
            }
        }

        /// <summary>
        /// 手动释放资源（公共接口）
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 析构函数：防止手动未调用Dispose时资源泄漏
        /// 仅释放非托管资源（此处无自定义非托管资源，仅做兜底）
        /// </summary>
        ~OFDReader()
        {
            Dispose(false);
        }
        #endregion
    }
}
