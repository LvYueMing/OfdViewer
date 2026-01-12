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
        // OFD归档对象（只读，确保初始化后不可修改）
        private readonly OFDArchive _archive;

        // 资源释放标记（线程安全的布尔标识）
        private bool _disposed = false;

        // 归档是否已保存的标记，避免重复保存
        private bool _saved = false;


        #region 构造函数

        /// <summary>
        /// 初始化OFD写入器（文件路径重载，最常用场景：写入本地文件）
        /// </summary>
        /// <param name="filePath">输出OFD文件完整路径</param>
        /// <exception cref="ArgumentNullException">文件路径为空/空白时抛出</exception>
        /// <exception cref="DirectoryNotFoundException">输出目录不存在且无法自动创建时抛出</exception>
        public OFDWriter(string filePath)
        {
            // 严格参数校验,构造阶段的致命错误必须抛出
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath), "OFD输出文件路径不能为空");

            try
            {
                // 确保输出目录存在，增强容错性
                string outputDir = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(outputDir))
                {
                    Directory.CreateDirectory(outputDir);
                }

                // 初始化OFD归档（写入模式）
                _archive = OFDArchive.CreateFromFile(filePath);
            }
            catch (Exception ex) when (ex is not ArgumentNullException)
            {
                throw new DirectoryNotFoundException("无法创建OFD输出目录或初始化归档，请检查路径权限", ex);
            }
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
            // 增强参数校验，构造阶段的致命错误必须抛出
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
        public void WriteOFDDocument(OFDDocument ofdDocument)
        {
            // 资源状态校验 释放对象 ≠ 对象引用为空
            EnsureNotDisposed();

            // 参数有效性校验
            if (ofdDocument == null)
                throw new ArgumentNullException(nameof(ofdDocument), "待写入的OFD文档对象不能为空");
            if (ofdDocument.RootOfd == null)
                throw new ArgumentNullException(nameof(ofdDocument.RootOfd), "OFD文档的全局元数据(RootOFD)不能为空");
            if (ofdDocument.Docs == null || ofdDocument.Docs.Count == 0)
                throw new InvalidOperationException("OFD文档中无可用子文档，无法完成全量写入");

            try
            {
                // 第一步：写入核心元数据（OFD.xml），并自动创建子文档目录框架
                this.WriteRootOFD(ofdDocument.RootOfd);

                // 第二步：遍历写入所有子文档元数据（Doc.xml）
                foreach (var doc in ofdDocument.Docs)
                {
                    // 跳过空的子文档，提升容错性
                    if (doc == null)
                        continue;

                    WriteOFDDoc(doc);
                }
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
        public void WriteRootOFD(RootOFD rootOfd)
        {
            // 资源状态校验
            EnsureNotDisposed();

            // 参数校验，保证输入有效性
            if (rootOfd == null)
                throw new ArgumentNullException(nameof(rootOfd), "OFD全局元数据(RootOFD)对象不能为空");

            try
            {
                // 获取写入流；OFD根文档 OFD.xml
                using var ofdStream = _archive.CreateFileStream("OFD.xml");
                // 二次校验流有效性
                if (ofdStream == null || !ofdStream.CanWrite)
                    throw new InvalidDataException("无法创建OFD核心文件OFD.xml的写入流，归档对象可能损坏");

                // 序列化元数据并写入流
                XmlHelper.SerializeToStream(rootOfd, ofdStream);

            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("写入OFD全局元数据(RootOFD)失败，请检查归档对象完整性", ex);
            }
        }

        /// <summary>
        /// 写入OFD子文档元数据
        /// </summary>
        /// <param name="doc">子文档对象</param>
        /// <exception cref="ArgumentNullException">子文档对象为空时抛出</exception>
        public void WriteOFDDoc(OFDDoc doc)
        {
            // 资源状态校验
            EnsureNotDisposed();

            if (doc == null)
                throw new ArgumentNullException(nameof(doc), "OFD子文档对象不能为空");

            try
            {
                // 文档描述文件
                if (doc.Document != null)
                {
                    // 构建子文档元数据路径（如Doc_0/Document.xml）
                    using var docStream = _archive.CreateFileStream(doc.DocumentFile);

                    // 序列化文档主描述文件（Document.xml）
                    XmlHelper.SerializeToStream(doc.Document, docStream);
                }

                // 公共资源描述文件
                if (doc.PublicResource != null)
                {
                    // 构建公共资源描述文件路径(Doc_{0}/PublicRes.xml)
                    using var publicResStream = _archive.CreateFileStream(doc.PublicResourceFile);

                    // 序列化全文档公共资源描述文件（PublicRes.xml）
                    XmlHelper.SerializeToStream(doc.PublicResource, publicResStream);
                }

                // 文档文档资源描述文件
                if (doc.DocumentResource != null)
                {
                    // 构建全文档文档资源描述文件路径(Doc_{0}/DocumentRes.xml)
                    using var documentResStream = _archive.CreateFileStream(doc.DocumentResourceFile);

                    // 序列化全文档文档资源描述文件（DocumentRes.xml）
                    XmlHelper.SerializeToStream(doc.DocumentResource, documentResStream);
                }

                // 签章列表索引对象
                if (doc.Signatures != null)
                {
                    // 构建签章列表索引文件路径(Doc_{0}/Signs/Signatures.xml)
                    using var signaturesStream = _archive.CreateFileStream(
                        Constants.GetFilePath(Constants.Signs_SignaturesFile, doc.DocIndex));

                    //序列化签章列表索引对象（Signatures.xml）
                    XmlHelper.SerializeToStream(doc.Signatures, signaturesStream);
                }

                // 签章对象集合
                if (doc.SignDocs != null && doc.SignDocs.Count > 0)
                {
                    // 遍历写入每个签章对象
                    for (int i = 0; i < doc.SignDocs.Count; i++)
                    {
                        var signDoc = doc.SignDocs[i];
                        if (signDoc == null)
                            continue;
                        // 写入单个签章对象
                        WriteSignDoc(signDoc);
                    }
                }

                // 页面对象集合
                if (doc.PageDocs != null && doc.PageDocs.Count > 0)
                {
                    // 遍历写入每个页面对象
                    for (int i = 0; i < doc.PageDocs.Count; i++)
                    {
                        var pageDoc = doc.PageDocs[i];
                        if (pageDoc == null)
                            continue;
                        // 写入单个页面对象
                        WritePageDoc(pageDoc);
                    }
                }

                //文档级资源文件集合 Dictionary<string, byte[]> ResFiles
                if (doc.ResFiles != null && doc.ResFiles.Count > 0)
                {
                    // 构建文档级资源目录路径(Doc_{0}/Res/)
                    string resDirectoryPath = Constants.GetFilePath(Constants.Doc_ResDirectory, doc.DocIndex);

                    // 遍历写入每个资源文件
                    foreach (var resFileEntry in doc.ResFiles)
                    {
                        string resFileName = resFileEntry.Key;
                        byte[] resFileContent = resFileEntry.Value;
                        if (string.IsNullOrWhiteSpace(resFileName) || resFileContent == null || resFileContent.Length == 0)
                            continue;
                        // 构建资源文件完整路径(Doc_{0}/Res/{resFileName})
                        string resFilePath = $"{resDirectoryPath}/{resFileName}";
                        using var resFileStream = _archive.CreateFileStream(resFilePath);
                        // 写入资源文件内容
                        resFileStream.Write(resFileContent, 0, resFileContent.Length);
                    }
                }

            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"写入OFD子文档（索引：{doc.DocIndex}）元数据失败", ex);
            }
        }

        /// <summary>
        /// 写入单个签章对象
        /// </summary>
        /// <param name="signDoc">签章对象</param>
        public void WriteSignDoc(SignDoc signDoc)
        {
            // 资源状态校验
            EnsureNotDisposed();

            if (signDoc == null)
                throw new ArgumentNullException(nameof(signDoc), "SignDoc签章对象不能为空");
            try
            {
                if (signDoc.Signature != null)
                {
                    // 构建签章属性描述文件路径(Doc_{0}/Signs/Sign_{1}/Signature.xml)
                    using var signDocStream = _archive.CreateFileStream(
                    Constants.GetFilePath(Constants.Sign_SignatureFile, signDoc.BelongDocIndex, signDoc.SignIndex));

                    // 序列化签章属性描述文件（Signature.xml）
                    XmlHelper.SerializeToStream(signDoc.Signature, signDocStream);
                }

                //电子印章本体文件（Seal.esl） byte[] SignedValue
                if (signDoc.Seal != null && signDoc.Seal.Length > 0)
                {
                    //构建电子印章本体文件路径(Doc_{0}/Signs/Sign_{1}/Seal.esl)
                    using var sealStream = _archive.CreateFileStream(
                        Constants.GetFilePath(Constants.Sign_SealFile, signDoc.BelongDocIndex, signDoc.SignIndex));

                    //写入电子印章本体文件（Seal.esl）
                    sealStream.Write(signDoc.Seal, 0, signDoc.Seal.Length);
                }

                //数字签名密文文件（SignedValue.dat） byte[] SignedValue
                if (signDoc.SignedValue != null && signDoc.SignedValue.Length > 0)
                {
                    //构建数字签名密文文件路径(Doc_{0}/Signs/Sign_{1}/SignedValue.dat)
                    using var signedValueStream = _archive.CreateFileStream(
                        Constants.GetFilePath(Constants.Sign_SignedValueFile, signDoc.BelongDocIndex, signDoc.SignIndex));

                    //写入数字签名密文文件（SignedValue.dat）
                    signedValueStream.Write(signDoc.SignedValue, 0, signDoc.SignedValue.Length);

                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"写入签章对象失败，请检查归档对象完整性", ex);
            }
        }

        /// <summary>
        /// 写入单个页面对象
        /// </summary>
        /// <param name="pageDoc">页面对象</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public void WritePageDoc(PageDoc pageDoc)
        {
            // 资源状态校验
            EnsureNotDisposed();
            if (pageDoc == null)
                throw new ArgumentNullException(nameof(pageDoc), "PageDoc页面对象不能为空");
            try
            {
                //页面内容描述文件（Content.xml）
                if (pageDoc.Content != null)
                {
                    // 构建页面属性描述文件路径(Doc_{0}/Pages/Page_{1}/Content.xml)
                    using var pageDocStream = _archive.CreateFileStream(
                        Constants.GetFilePath(Constants.Page_ContentFile, pageDoc.BelongDocIndex, pageDoc.PageIndex));
                    // 序列化页面内容描述文件（Content.xml）
                    XmlHelper.SerializeToStream(pageDoc.Content, pageDocStream);
                }
                //页面资源映射文件（PageRes.xml）
                if (pageDoc.PageRes != null)
                {
                    // 构建页面资源映射文件路径(Doc_{0}/Pages/Page_{1}/PageRes.xml)
                    using var pageResStream = _archive.CreateFileStream(
                        Constants.GetFilePath(Constants.Page_PageResFile, pageDoc.BelongDocIndex, pageDoc.PageIndex));
                    // 序列化页面资源映射文件（PageRes.xml）
                    XmlHelper.SerializeToStream(pageDoc.PageRes, pageResStream);
                }
                //页面资源文件（Res/Image_{0}.png）
                if (pageDoc.PageResFiles != null && pageDoc.PageResFiles.Count > 0)
                {
                    // 构建页面资源目录路径(Doc_{0}/Pages/Page_{1}/Res/)
                    string resDirectoryPath = Constants.GetFilePath(
                        Constants.Page_ResDirectory, pageDoc.BelongDocIndex, pageDoc.PageIndex);
                    // 遍历写入每个页面资源文件
                    foreach (var resFileEntry in pageDoc.PageResFiles)
                    {
                        string resFileName = resFileEntry.Key;
                        byte[] resFileContent = resFileEntry.Value;
                        if (string.IsNullOrWhiteSpace(resFileName) || resFileContent == null || resFileContent.Length == 0)
                            continue;
                        // 构建页面资源文件完整路径(Doc_{0}/Pages/Page_{1}/Res/{resFileName})
                        string resFilePath = $"{resDirectoryPath}/{resFileName}";
                        using var resFileStream = _archive.CreateFileStream(resFilePath);
                        // 写入页面资源文件内容
                        resFileStream.Write(resFileContent, 0, resFileContent.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"写入页面对象(PageDoc)失败，请检查归档对象完整性", ex);
            }
        }


        /// <summary>
        /// 将OFD归档数据保存为物理文件（核心：完成最终的磁盘写入）
        /// 调用后所有写入的元数据才会真正持久化到.ofd文件中
        /// </summary>
        /// <exception cref="ObjectDisposedException">对象已释放时抛出</exception>
        /// <exception cref="InvalidOperationException">保存失败时抛出</exception>
        public void Save()
        {
            // 校验资源状态
            EnsureNotDisposed();

            // 避免重复保存
            if (_saved)
            {
                //Console.WriteLine("OFD归档已保存，无需重复操作");
                return;
            }

            try
            {
                // 调用OFDArchive的保存方法（关键！将内存数据刷入磁盘）
                _archive.Save();

                // 标记为已保存
                _saved = true;
                //Console.WriteLine("OFD归档保存成功，已生成物理文件");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("保存OFD归档到物理文件失败，请检查文件路径权限或流状态", ex);
            }
        }
        #endregion

        #region 辅助方法（与OFDReader的LoadDocFramework对齐，增强实用性）

        private void EnsureNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(
                    nameof(OFDWriter),
                    $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] OFD写入器（{nameof(OFDWriter)}）已释放，无法执行写入操作");
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
