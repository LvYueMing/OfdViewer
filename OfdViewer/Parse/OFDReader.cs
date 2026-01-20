using OFDViewer.Models.BaseStructure.MainEntry;
using OFDViewer.Models.BaseStructure.Pages;
using OFDViewer.Models.BaseStructure.Resources;
using OFDViewer.Models.Signature;
using OFDViewer.Utils;

namespace OFDViewer.Parse
{
    /// <summary>
    /// OFD文件读取/解析类，负责将.ofd物理文件解析为OFDDocument对象
    /// 无任何写入逻辑，实现IDisposable接口确保资源安全释放
    /// </summary>
    public class OFDReader : IDisposable
    {
        /// <summary>
        /// OFD归档对象（只读），负责底层文件访问
        /// </summary>
        private readonly OFDArchive _archive;
        
        /// <summary>
        /// 资源释放标记
        /// </summary>
        private bool _disposed = false;

        #region 构造函数

        /// <summary>
        /// 从文件路径初始化OFD读取器
        /// </summary>
        /// <param name="filePath">输入OFD文件完整路径</param>
        /// <exception cref="ArgumentNullException">路径为空</exception>
        /// <exception cref="FileNotFoundException">文件不存在</exception>
        public OFDReader(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath), "OFD输入文件路径不能为空");

            if (!File.Exists(filePath))
                throw new FileNotFoundException("指定的OFD文件不存在", filePath);

            _archive = OFDArchive.OpenFromFile(filePath);
        }

        /// <summary>
        /// 从流初始化OFD读取器
        /// </summary>
        /// <param name="stream">输入OFD文件流（需支持读取）</param>
        /// <param name="leaveOpen">是否保持流打开</param>
        /// <exception cref="ArgumentNullException">流为空</exception>
        /// <exception cref="ArgumentException">流不可读</exception>
        public OFDReader(Stream stream, bool leaveOpen = false)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream), "OFD输入流不能为空");
            if (!stream.CanRead)
                throw new ArgumentException("OFD输入流不支持读取操作", nameof(stream));

            _archive = OFDArchive.OpenFromStream(stream, leaveOpen: leaveOpen);
        }

        #endregion

        #region 核心读取方法

        /// <summary>
        /// 解析完整的OFD文档为OFDDocument对象
        /// </summary>
        /// <returns>解析后的OFDDocument</returns>
        /// <exception cref="ObjectDisposedException">对象已释放</exception>
        /// <exception cref="InvalidOperationException">解析失败</exception>
        public OFDRootDocument ParseOFDDocument()
        {
            EnsureNotDisposed();

            try
            {
                var ofdRootDocument = new OFDRootDocument();

                // 第一步：读取OFD.xml → RootOFD
                var rootOfd = ReadRootOFD();
                if (rootOfd == null)
                    throw new InvalidOperationException("无法读取OFD核心元数据（OFD.xml）");

                var ofdDocs = new List<OFDDocument>();

                // 第二步：从RootOFD的DocBody中获取DocRoot路径 （即 Doc_0, Doc_1...目录）
                if (rootOfd.DocBodies != null && rootOfd.DocBodies.Count > 0)
                {
                    foreach (var docBody in rootOfd.DocBodies)
                    {
                        if (docBody.DocRoot != null)
                        {
                            // Document.xml 的路径
                            var documentFilePath = docBody.DocRoot.Path;
                            var ofdDoc = ReadOFDDocument(documentFilePath);
                            if (ofdDoc != null)
                                ofdDocs.Add(ofdDoc);
                        }
                    }
                }

                ofdRootDocument.RootOfd = rootOfd;
                ofdRootDocument.Docs = ofdDocs;

                return ofdRootDocument;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("读取完整OFD文档失败", ex);
            }
        }

        /// <summary>
        /// 读取OFD.xml 全局元数据
        /// </summary>
        public RootOFD ReadRootOFD()
        {
            EnsureNotDisposed();

            if (!_archive.FileExists("OFD.xml"))
                return null;

            using var stream = _archive.OpenFileStream("OFD.xml");
            return XmlHelper.DeserializeFromStream<RootOFD>(stream);
        }

        /// <summary>
        /// 读取指定路径的子文档
        /// </summary>
        /// <param name="docFilePath"> document.xml 的路径</param>
        /// <returns>OFDDocument对象</returns>
        public OFDDocument ReadOFDDocument(string docFilePath)
        {
            EnsureNotDisposed();

            if (string.IsNullOrEmpty(docFilePath))
                return null;

            var doc = new OFDDocument(docFilePath);

            // 读取 Document.xml
            if (_archive.FileExists(docFilePath))
            {
                // docFilePath 和 OFD.xml 的同目录路径，所有不能获取绝对路径
                using var stream = _archive.OpenFileStream(docFilePath);
                doc.Document = XmlHelper.DeserializeFromStream<Models.BaseStructure.DocumentRoot.Document>(stream);
            }

            if (doc.Document != null && doc.Document.CommonData != null)
            {
                // 读取 PublicRes.xml 等
                var pubResPaths = doc.Document.CommonData.PublicRes;

                foreach (var res in pubResPaths)
                {
                    //  获取PublicRes.xml绝对路径
                    var pubResFilePath = res.GetAbsolutePath(doc.DocDirectoryPath).Path;
                    if (_archive.FileExists(pubResFilePath))
                    {
                        using var stream = _archive.OpenFileStream(pubResFilePath);
                        doc.PublicResource = XmlHelper.DeserializeFromStream<Res>(stream);
                        doc.PublicResourceFilePath = pubResFilePath;

                        //todo: 读取PublicRes.xml 中的资源文件
                    }
                }

                // 读取 DocumentRes.xml 等
                var docResPaths = doc.Document.CommonData.DocumentRes;

                foreach (var res in docResPaths)
                {
                    // 获取 DocumentRes.xml 绝对路径
                    var docResFilePath = res.GetAbsolutePath(doc.DocDirectoryPath).Path;
                    if (_archive.FileExists(docResFilePath))
                    {
                        using var stream = _archive.OpenFileStream(docResFilePath);
                        doc.DocumentResource = XmlHelper.DeserializeFromStream<Res>(stream);
                        doc.DocumentResourceFilePath = docResFilePath;

                        //todo: 读取PublicRes.xml 中的资源文件
                    }
                }
            }

            // 读取页面对象
            if (doc.Document != null && doc.Document.Pages != null)
            {
                foreach (var page in doc.Document.Pages)
                {
                    // 获取页面对象文件 Pages/Page_0/Content.xml 绝对路径 
                    var pageFilePath = page.BaseLoc.GetAbsolutePath(doc.DocDirectoryPath).Path;
                    var pageDoc = ReadPageDoc(pageFilePath);

                    doc.AddPageDoc(pageDoc);
                }
            }


            //// 读取Signatures.xml
            //string sigIndexPath = Path.Combine(Path.GetDirectoryName(docFilePath), "Signs", "Signatures.xml");
            //if (_archive.FileExists(sigIndexPath))
            //{
            //    try
            //    {
            //        using var stream = _archive.OpenFileStream(sigIndexPath);
            //        doc.Signatures = XmlHelper.DeserializeFromStream<Signatures>(stream);
            //    }
            //    catch (Exception ex)
            //    {
            //        // 忽略Signatures.xml读取错误，继续执行
            //    }
            //}

            //// 读取签章对象
            //try
            //{
            //    doc.SignDocs = ReadSignDocs(Path.GetDirectoryName(docFilePath));
            //}
            //catch (Exception ex)
            //{
            //    // 忽略签章读取错误，继续执行
            //    doc.SignDocs = new List<SignDocument>();
            //}


            //// 读取文档级资源
            //string resDirectoryPath = Path.Combine(Path.GetDirectoryName(docFilePath), "Res");
            //if (_archive.DirectoryExists(resDirectoryPath))
            //{
            //    try
            //    {
            //        doc.ResFiles = ReadFileResInDirectory(resDirectoryPath);
            //    }
            //    catch (Exception ex)
            //    {
            //        // 忽略资源文件读取错误，继续执行
            //        doc.ResFiles = new Dictionary<string, byte[]>();
            //    }
            //}

            return doc;
        }

        /// <summary>
        /// 读取指定索引的页面对象（根据路径）
        /// </summary>
        /// <param name="pageDocFilePath">文档路径</param>
        /// <returns>页面对象</returns>
        private PageDocument ReadPageDoc(string pageDocFilePath)
        {
            EnsureNotDisposed();
            var pageDoc = new PageDocument();

            // Doc_{0}/Pages/Page_{1}/Content.xml
            if (_archive.FileExists(pageDocFilePath))
            {
                using var stream = _archive.OpenFileStream(pageDocFilePath);
                pageDoc.Page = XmlHelper.DeserializeFromStream<Page>(stream);
                pageDoc.PageFilePath = pageDocFilePath;
            }

            if (pageDoc.Page != null && pageDoc.Page.PageRes != null)
            {
                // 读取 PageRes.xml 等
                var pageResPaths = pageDoc.Page.PageRes;

                foreach (var res in pageResPaths)
                {
                    //  获取 PageRes.xml 绝对路径
                    var pubResFilePath = res.GetAbsolutePath(pageDoc.PageDirectoryPath).Path;
                    if (_archive.FileExists(pubResFilePath))
                    {
                        using var stream = _archive.OpenFileStream(pubResFilePath);
                        pageDoc.PageRes = XmlHelper.DeserializeFromStream<Res>(stream);
                        pageDoc.PageResFilePath = pubResFilePath;

                        //todo: 读取 PageRes.xml 中的资源文件
                    }
                }
            }

            return pageDoc;
        }





        /// <summary>
        /// 读取指定索引的签章对象（根据路径）
        /// </summary>
        /// <param name="signDocIndex">签章索引</param>
        /// <param name="docPath">文档路径</param>
        /// <returns>签章对象</returns>
        private SignDocument ReadSignDoc(int signDocIndex, string docPath)
        {
            EnsureNotDisposed();

            var signDoc = new SignDocument(signDocIndex, docPath);

            // Doc_{0}/Signs/Sign_{1}/Signature.xml
            string sigFile = System.IO.Path.Combine(docPath, "Signs", $"Sign_{signDocIndex}", "Signature.xml");
            if (_archive.FileExists(sigFile))
            {
                using var stream = _archive.OpenFileStream(sigFile);
                signDoc.Signature = XmlHelper.DeserializeFromStream<Signature>(stream);
            }

            // Doc_{0}/Signs/Sign_{1}/Seal.esl
            string sealFile = System.IO.Path.Combine(docPath, "Signs", $"Sign_{signDocIndex}", "Seal.esl");
            if (_archive.FileExists(sealFile))
            {
                using var stream = _archive.OpenFileStream(sealFile);
                using var ms = new MemoryStream();
                stream.CopyTo(ms);
                signDoc.Seal = ms.ToArray();
            }

            // Doc_{0}/Signs/Sign_{1}/SignedValue.dat
            string svFile = System.IO.Path.Combine(docPath, "Signs", $"Sign_{signDocIndex}", "SignedValue.dat");
            if (_archive.FileExists(svFile))
            {
                using var stream = _archive.OpenFileStream(svFile);
                using var ms = new MemoryStream();
                stream.CopyTo(ms);
                signDoc.SignedValue = ms.ToArray();
            }

            return signDoc;
        }


        /// <summary>
        /// 读取指定索引的子文档
        /// </summary>
        /// <param name="docIndex">子文档索引</param>
        /// <returns>OFDDocument对象</returns>
        public OFDDocument ReadOFDDocument(int docIndex)
        {
            EnsureNotDisposed();

            var doc = new OFDDocument(docIndex);

            // Doc_{0}/Document.xml
            string docFilePath = Constants.GetFilePath(Constants.Doc_DocumentFile, docIndex);
            if (_archive.FileExists(docFilePath))
            {
                try
                {
                    using var stream = _archive.OpenFileStream(docFilePath);
                    doc.Document = XmlHelper.DeserializeFromStream<Models.BaseStructure.DocumentRoot.Document>(stream);
                }
                catch (Exception ex)
                {
                    // 忽略Document.xml读取错误，继续执行
                }
            }

            // Doc_{0}/PublicRes.xml
            string pubResPath = Constants.GetFilePath(Constants.Doc_PublicResFile, docIndex);
            if (_archive.FileExists(pubResPath))
            {
                try
                {
                    using var stream = _archive.OpenFileStream(pubResPath);
                    doc.SetPublicResource(XmlHelper.DeserializeFromStream<Res>(stream));
                }
                catch (Exception ex)
                {
                    // 忽略PublicRes.xml读取错误，继续执行
                }
            }

            // Doc_{0}/DocumentRes.xml
            string docResPath = Constants.GetFilePath(Constants.Doc_DocumentResFile, docIndex);
            if (_archive.FileExists(docResPath))
            {
                try
                {
                    using var stream = _archive.OpenFileStream(docResPath);
                    doc.SetDocumentResource(XmlHelper.DeserializeFromStream<Res>(stream));
                }
                catch (Exception ex)
                {
                    // 忽略DocumentRes.xml读取错误，继续执行
                }
            }

            // Doc_{0}/Signs/Signatures.xml
            string sigIndexPath = Constants.GetFilePath(Constants.Signs_SignaturesFile, docIndex);
            if (_archive.FileExists(sigIndexPath))
            {
                try
                {
                    using var stream = _archive.OpenFileStream(sigIndexPath);
                    doc.Signatures = XmlHelper.DeserializeFromStream<Signatures>(stream);
                }
                catch (Exception ex)
                {
                    // 忽略Signatures.xml读取错误，继续执行
                }
            }

            // 读取签章对象（Doc_{0}/Signs）
            try
            {
                doc.SignDocs = ReadSignDocs(docIndex);
            }
            catch (Exception ex)
            {
                // 忽略签章读取错误，继续执行
                doc.SignDocs = new List<SignDocument>();
            }

            // 读取页面对象 (Doc_{0}/Pages/Page_{1})
            try
            {
                doc.PageDocs = ReadPageDocs(docIndex);
            }
            catch (Exception ex)
            {
                // 忽略页面读取错误，继续执行
                doc.PageDocs = new List<PageDocument>();
            }

            // 读取文档级资源 (Doc_{0}/Res)
            string resDirectoryPath = Constants.GetFilePath(Constants.Doc_ResDirectory, docIndex);
            if (_archive.DirectoryExists(resDirectoryPath))
            {
                try
                {
                    doc.ResFiles = ReadFileResInDirectory(resDirectoryPath);
                }
                catch (Exception ex)
                {
                    // 忽略资源文件读取错误，继续执行
                    doc.ResFiles = new Dictionary<string, byte[]>();
                }
            }

            return doc;
        }


        /// <summary>
        /// 读取签章对象 (Doc_{0}/Signs/Sign_{1}/)
        /// </summary>
        private SignDocument ReadSignDoc(int signDocIndex, int docIndex)
        {
            EnsureNotDisposed();

            var signDoc = new SignDocument(signDocIndex, docIndex);

            // Doc_{0}/Signs/Sign_{1}/Signature.xml
            string sigFile = Constants.GetFilePath(Constants.Sign_SignatureFile, docIndex, signDocIndex);
            if (_archive.FileExists(sigFile))
            {
                using var stream = _archive.OpenFileStream(sigFile);
                signDoc.Signature = XmlHelper.DeserializeFromStream<Signature>(stream);
            }

            // Doc_{0}/Signs/Sign_{1}/Seal.esl
            string sealFile = Constants.GetFilePath(Constants.Sign_SealFile, docIndex, signDocIndex);
            if (_archive.FileExists(sealFile))
            {
                using var stream = _archive.OpenFileStream(sealFile);
                using var ms = new MemoryStream();
                stream.CopyTo(ms);
                signDoc.Seal = ms.ToArray();
            }

            // Doc_{0}/Signs/Sign_{1}/SignedValue.dat
            string svFile = Constants.GetFilePath(Constants.Sign_SignedValueFile, docIndex, signDocIndex);
            if (_archive.FileExists(svFile))
            {
                using var stream = _archive.OpenFileStream(svFile);
                using var ms = new MemoryStream();
                stream.CopyTo(ms);
                signDoc.SignedValue = ms.ToArray();
            }

            return signDoc;
        }

        /// <summary>
        /// 获取所有存在的签章对象索引
        /// Doc_{0}/Signs/Sign_{1} 如 [0, 1, 2] 对应 Sign_0, Sign_1, Sign_2
        /// </summary>
        private List<int> GetSignDocIndices(int docIndex)
        {
            var indices = new List<int>();
            // Doc_{0}/Signs
            var signBaseDir = Constants.GetFilePath(Constants.Signs_BaseDirectory, docIndex);
            var docEntrie = _archive.GetDirectEntryNamesInDirectory(signBaseDir);
            foreach (var entry in docEntrie)
            {
                if (entry.StartsWith("Sign_"))
                {
                    var index = int.Parse(entry.Substring(5));
                    indices.Add(index);
                }
            }
            // 从小到大排序
            return indices.OrderBy(x => x).ToList();
        }

        /// <summary>
        /// 读取所有签章对象列表 (Doc_{0}/Signs/)
        /// </summary>
        /// <param name="docIndex">文档索引</param>
        /// <returns>签章对象列表，如果没有签章文档则返回空列表</returns>
        private List<SignDocument> ReadSignDocs(int docIndex)
        {
            var signDocIndices = GetSignDocIndices(docIndex);
            var signDocs = new List<SignDocument>();

            foreach (int index in signDocIndices)
            {
                var signDoc = ReadSignDoc(index, docIndex);
                if (signDoc != null)
                    signDocs.Add(signDoc);
            }
            return signDocs;
        }

        /// <summary>
        /// 读取所有签章对象列表（根据路径）
        /// </summary>
        /// <param name="docPath">文档路径</param>
        /// <returns>签章对象列表，如果没有签章文档则返回空列表</returns>
        private List<SignDocument> ReadSignDocs(string docPath)
        {
            var signDocIndices = GetSignDocIndices(docPath);
            var signDocs = new List<SignDocument>();

            foreach (int index in signDocIndices)
            {
                var signDoc = ReadSignDoc(index, docPath);
                if (signDoc != null)
                    signDocs.Add(signDoc);
            }
            return signDocs;
        }

        /// <summary>
        /// 获取所有存在的签章对象索引（根据路径）
        /// </summary>
        /// <param name="docPath">文档路径</param>
        /// <returns>签章对象索引列表</returns>
        private List<int> GetSignDocIndices(string docPath)
        {
            var indices = new List<int>();
            var signBaseDir = System.IO.Path.Combine(docPath, "Signs");
            var docEntrie = _archive.GetDirectEntryNamesInDirectory(signBaseDir);
            foreach (var entry in docEntrie)
            {
                if (entry.StartsWith("Sign_"))
                {
                    var index = int.Parse(entry.Substring(5));
                    indices.Add(index);
                }
            }
            // 从小到大排序
            return indices.OrderBy(x => x).ToList();
        }

        /// <summary>
        /// 读取指定索引的页面对象 (Doc_{0}/Pages/Page_{1}/)
        /// </summary>
        /// <param name="pageDocIndex">页面索引</param>
        /// <param name="docIndex">所属文档索引</param>
        /// <returns>页面对象</returns>
        private PageDocument ReadPageDoc(int pageDocIndex, int docIndex)
        {
            EnsureNotDisposed();
            var pageDoc = new PageDocument();

            // Doc_{0}/Pages/Page_{1}/Content.xml
            string contentFile = Constants.GetFilePath(Constants.Page_ContentFile, docIndex, pageDocIndex);
            if (_archive.FileExists(contentFile))
            {
                using var stream = _archive.OpenFileStream(contentFile);
                pageDoc.Page = XmlHelper.DeserializeFromStream<Page>(stream);
            }

            // Doc_{0}/Pages/Page_{1}/PageRes.xml
            string pageResFile = Constants.GetFilePath(Constants.Page_PageResFile, docIndex, pageDocIndex);
            if (_archive.FileExists(pageResFile))
            {
                using var stream = _archive.OpenFileStream(pageResFile);
                pageDoc.PageRes = XmlHelper.DeserializeFromStream<Res>(stream);
            }

            // 页面资源 Doc_{0}/Pages/Page_{1}/Res
            string pageResDirectory = Constants.GetFilePath(Constants.Page_ResDirectory, docIndex, pageDocIndex);
            if (_archive.DirectoryExists(pageResDirectory))
            {
                pageDoc.PageResFileContents = ReadFileResInDirectory(pageResDirectory);
            }

            return pageDoc;
        }


        /// <summary>
        /// 获取所有存在的页面索引（Doc_{0}/Pages/Page_{1} 如 [0, 1, 2] 对应 Page_0, Page_1, Page_2）
        /// </summary>
        private List<int> GetPageDocIndices(int docIndex)
        {
            var indices = new List<int>();
            // Doc_{0}/Signs
            var pageBaseDir = Constants.GetFilePath(Constants.Pages_BaseDirectory, docIndex);
            var docEntrie = _archive.GetDirectEntryNamesInDirectory(pageBaseDir);
            foreach (var entry in docEntrie)
            {
                if (entry.StartsWith("Page_"))
                {
                    var index = int.Parse(entry.Substring(5));
                    indices.Add(index);
                }
            }
            // 从小到大排序
            return indices.OrderBy(x => x).ToList();
        }



        /// <summary>
        /// 读取所有页面对象列表 (Doc_{0}/Pages/)
        /// </summary>
        /// <param name="docIndex">文档索引</param>
        /// <returns>页面对象列表，如果没有页面则返回空列表</returns>
        private List<PageDocument> ReadPageDocs(int docIndex)
        {
            var pageDocIndices = GetPageDocIndices(docIndex);
            var pageDocs = new List<PageDocument>();

            foreach (int index in pageDocIndices)
            {
                var pageDoc = ReadPageDoc(index, docIndex);
                if (pageDoc != null)
                    pageDocs.Add(pageDoc);
            }
            return pageDocs;
        }


        /// <summary>
        /// 获取所有存在的页面索引（根据路径）
        /// </summary>
        /// <param name="docPath">文档路径</param>
        /// <returns>页面索引列表</returns>
        private List<int> GetPageDocIndices(string docPath)
        {
            var indices = new List<int>();
            var pageBaseDir = System.IO.Path.Combine(docPath, "Pages");
            var docEntrie = _archive.GetDirectEntryNamesInDirectory(pageBaseDir);
            foreach (var entry in docEntrie)
            {
                if (entry.StartsWith("Page_"))
                {
                    var index = int.Parse(entry.Substring(5));
                    indices.Add(index);
                }
            }
            // 从小到大排序
            return indices.OrderBy(x => x).ToList();
        }

        /// <summary>
        /// 读取指定目录下的文件资源
        /// </summary>
        /// <param name="path">目录路径</param>
        /// <returns>文件资源字典，键为文件名，值为文件内容</returns>
        private Dictionary<string, byte[]> ReadFileResInDirectory(string path)
        {
            var res = new Dictionary<string, byte[]>();
            var files = _archive.GetDirectFilePathsInDirectory(path);
            foreach (var filePath in files)
            {
                using var stream = _archive.OpenFileStream(filePath);
                using var ms = new MemoryStream();
                stream.CopyTo(ms);
                var fileName = Path.GetFileName(filePath);
                res[fileName] = ms.ToArray();
            }
            return res;
        }


        #endregion

        #region 辅助方法

        private void EnsureNotDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(
                    nameof(OFDReader),
                    $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] OFD读取器（{nameof(OFDReader)}）已释放，无法执行读取操作");
        }

        #endregion

        #region IDisposable 实现

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _archive?.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~OFDReader()
        {
            Dispose(false);
        }

        #endregion
    }
}
