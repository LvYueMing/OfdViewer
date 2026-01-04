using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using OFDViewer.Models.BaseStructure.MainEntry;
using OFDViewer.Utils;

namespace OFDViewer.OFD
{
    /// <summary>
    /// OFD文件写入/创建类,仅负责将实体对象转换为.ofd物理文件
    /// 无任何读取/解析逻辑，实现IDisposable接口确保资源安全释放
    /// </summary>
    public class OFDWriter : IDisposable
    {
        // 私有字段：OFD归档对象（只读，确保初始化后不可修改）
        private readonly OFDArchive _archive;

        // 私有字段：资源释放标记（线程安全的布尔标识）
        private bool _disposed = false;


        #region 构造函数

        /// <summary>
        /// 初始化OFD写入器（文件路径重载，最常用场景：写入本地文件）
        /// </summary>
        /// <param name="filePath">输出OFD文件完整路径</param>
        /// <exception cref="ArgumentNullException">文件路径为空/空白时抛出</exception>
        /// <exception cref="DirectoryNotFoundException">输出目录不存在且无法自动创建时抛出</exception>
        public OFDWriter(string filePath)
        {
            // 严格参数校验，与OFDReader的参数校验风格一致
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath), "OFD输出文件路径不能为空或仅包含空白字符");

            // 确保输出目录存在，增强容错性
            string outputDir = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(outputDir))
            {
                try
                {
                    Directory.CreateDirectory(outputDir);
                }
                catch (Exception ex)
                {
                    throw new DirectoryNotFoundException("无法创建OFD输出目录，请检查路径权限", ex);
                }
            }

            // 初始化OFD归档（写入模式）
            _archive = OFDArchive.CreateFromFile(filePath);
        }

        /// <summary>
        /// 初始化OFD写入器（流重载，支持内存流/网络流等自定义场景）
        /// </summary>
        /// <param name="stream">输出OFD文件流（需支持写入操作）</param>
        /// <param name="leaveOpen">是否保持流打开状态</param>
        /// <exception cref="ArgumentNullException">输入流为空时抛出</exception>
        /// <exception cref="ArgumentException">输入流不支持写入时抛出</exception>
        public OFDWriter(Stream stream, bool leaveOpen)
        {
            // 增强参数校验，与OFDReader的流校验风格一致
            if (stream == null)
                throw new ArgumentNullException(nameof(stream), "OFD输出流不能为空");

            if (!stream.CanWrite)
                throw new ArgumentException("OFD输出流不支持写入操作，请提供可写的流", nameof(stream));

            // 初始化OFD归档（写入模式）
            _archive = OFDArchive.CreateFromStream(stream, leaveOpen: leaveOpen);
        }
        #endregion



        #region 核心写入方法

        /// <summary>
        /// 一键写入完整的OFD文档（直接传入OFDDocument对象，自动完成所有元数据写入）
        /// </summary>
        /// <param name="ofdDocument">待写入的完整OFD文档对象</param>
        /// <exception cref="ObjectDisposedException">对象已释放时抛出</exception>
        /// <exception cref="ArgumentNullException">OFD文档对象为空时抛出</exception>
        /// <exception cref="InvalidOperationException">写入过程中出现异常时抛出</exception>
        public void WriteEntireDocument(OFDDocument ofdDocument)
        {
            // 1. 资源状态校验
            if (_disposed)
                throw new ObjectDisposedException(nameof(OFDWriter), "OFD写入器已释放，无法执行写入操作");

            // 2. 参数有效性校验
            if (ofdDocument == null)
                throw new ArgumentNullException(nameof(ofdDocument), "待写入的OFD文档对象不能为空");
            if (ofdDocument.OfdMetadata == null)
                throw new ArgumentNullException(nameof(ofdDocument.OfdMetadata), "OFD文档的全局元数据（OfdMetadata）不能为空");
            if (ofdDocument.Docs == null || ofdDocument.Docs.Count == 0)
                throw new InvalidOperationException("OFD文档中无可用子文档，无法完成全量写入");

            try
            {
                // 3. 第一步：写入核心元数据（OFD.xml），并自动创建子文档目录框架
                int docCount = ofdDocument.Docs.Count;
                WriteRootOFD(ofdDocument.OfdMetadata, docCount);

                // 4. 第二步：遍历写入所有子文档元数据（Doc.xml）
                foreach (var doc in ofdDocument.Docs)
                {
                    // 跳过空的子文档，提升容错性
                    if (doc == null)
                        continue;

                    // 复用原有WriteDocMetadata方法，保证逻辑一致性
                    WriteDocMetadata(doc);
                }

                // 可选：如需写入其他附属文件（如缩略图、字体等），可在此扩展
                // WriteAttachments(ofdDocument.Attachments);
            }
            catch (ArgumentNullException ex)
            {
                throw new InvalidOperationException("OFD文档核心参数缺失，无法完成全量写入", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("一键写入完整OFD文档失败，请检查文档对象完整性", ex);
            }
        }

        /// <summary>
        /// 写入OFD核心元数据文件（OFD.xml）
        /// </summary>
        /// <param name="rootOfd">全局元数据对象</param>
        /// <param name="docCount">子文档数量（用于自动创建目录框架，可选）</param>
        /// <exception cref="ObjectDisposedException">对象已释放时抛出</exception>
        /// <exception cref="ArgumentNullException">元数据对象为空时抛出</exception>
        /// <exception cref="InvalidOperationException">写入失败时抛出</exception>
        public void WriteRootOFD(RootOFD rootOfd, int docCount = 0)
        {
            // 资源状态校验，避免操作已释放对象，与OFDReader对齐
            if (_disposed)
                throw new ObjectDisposedException(nameof(OFDWriter), "OFD写入器已释放，无法执行写入操作");

            // 参数校验，保证输入有效性
            if (rootOfd == null)
                throw new ArgumentNullException(nameof(rootOfd), "OFD全局元数据对象不能为空");

            try
            {
                // 获取写入流（对应OFDReader的GetFileStream）
                using var ofdStream = _archive.CreateFileStream("OFD.xml");
                // 二次校验流有效性
                if (ofdStream == null || !ofdStream.CanWrite)
                    throw new InvalidDataException("无法创建OFD核心文件OFD.xml的写入流，归档对象可能损坏");

                // 序列化元数据并写入流（对应OFDReader的反序列化）
                XmlHelper.SerializeToStream(rootOfd, ofdStream);

                if (docCount > 0)
                {
                    CreateDocDirectoryFramework(docCount);
                }
            }
            catch (InvalidDataException ex)
            {
                throw new InvalidOperationException("写入OFD核心文件OFD.xml失败，无法创建有效写入流", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("写入OFD全局元数据失败，请检查归档对象完整性", ex);
            }
        }

        /// <summary>
        /// 写入OFD子文档元数据（Doc.xml，扩展方法，增强实用性）
        /// </summary>
        /// <param name="doc">子文档对象</param>
        /// <exception cref="ObjectDisposedException">对象已释放时抛出</exception>
        /// <exception cref="ArgumentNullException">子文档对象为空时抛出</exception>
        public void WriteDocMetadata(OFDDoc doc)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(OFDWriter), "OFD写入器已释放，无法执行写入操作");

            if (doc == null)
                throw new ArgumentNullException(nameof(doc), "OFD子文档对象不能为空");

            try
            {
                // 构建子文档元数据路径（如Doc_0/Doc.xml）
                string docXmlPath = $"{doc.DocDirectoryPath}/Doc.xml";
                // 创建子文档目录（确保目录存在）
                _archive.CreateDirectory(doc.DocDirectoryPath);
                // 创建并写入子文档元数据文件
                using var docStream = _archive.CreateFileStream(docXmlPath);
                if (docStream == null || !docStream.CanWrite)
                    throw new InvalidDataException($"无法创建子文档文件{docXmlPath}的写入流");

                // 序列化子文档元数据（此处可替换为自定义DocMetadata实体）
                // 示例：临时序列化OFDDoc对象，实际可扩展专属DocMetadata类
                XmlHelper.SerializeToStream(doc, docStream);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"写入OFD子文档（索引：{doc.DocIndex}）元数据失败", ex);
            }
        }
        #endregion

        #region 辅助方法（与OFDReader的LoadDocFramework对齐，增强实用性）
        /// <summary>
        /// 根据文档数量创建子文档目录框架
        /// 【改造点】：不再操作OfdDocument.Docs集合，仅负责创建归档目录
        /// </summary>
        /// <param name="docCount">子文档数量</param>
        private void CreateDocDirectoryFramework(int docCount)
        {
            if (docCount <= 0) return;

            // 【删除】原有操作OfdDocument的逻辑：OfdDocument.Docs.Clear();
            // 仅保留目录创建逻辑，不维护文档对象集合（由调用方自行管理）
            for (int i = 1; i <= docCount; i++)
            {
                // 按原有规则构建目录路径（如Doc_0、Doc_1）
                string docDirectoryPath = $"Doc_{i - 1}";
                _archive.CreateDirectory(docDirectoryPath);
            }
        }
        #endregion

        #region IDisposable实现（与OFDReader完全一致，规范资源释放）
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
        /// 析构函数：兜底释放资源，防止遗漏手动Dispose
        /// </summary>
        ~OFDWriter()
        {
            Dispose(false);
        }
        #endregion
    }
}
