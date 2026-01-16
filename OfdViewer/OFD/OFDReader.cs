using OFDViewer.Models.BaseStructure.MainEntry;
using OFDViewer.Models.BaseStructure.Pages;
using OFDViewer.Models.BaseStructure.Resources;
using OFDViewer.Models.Signature;
using OFDViewer.Utils;

namespace OFDViewer.OFD
{
    /// <summary>
    /// OFD文件读取/解析类，负责将.ofd物理文件解析为OFDDocument对象
    /// 无任何写入逻辑，实现IDisposable接口确保资源安全释放
    /// </summary>
    public class OFDReader : IDisposable
    {
        // OFD归档对象（只读）
        private readonly OFDArchive _archive;
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
        /// 一键读取完整的OFD文档为OFDDocument对象
        /// </summary>
        /// <returns>解析后的OFDDocument</returns>
        /// <exception cref="ObjectDisposedException">对象已释放</exception>
        /// <exception cref="InvalidOperationException">解析失败</exception>
        public OFDRootDocument ReadOFDDocument()
        {
            EnsureNotDisposed();

            try
            {
                // 第一步：读取OFD.xml → RootOFD
                var rootOfd = ReadRootOFD();
                if (rootOfd == null)
                    throw new InvalidOperationException("无法读取OFD核心元数据（OFD.xml）");

                // 第二步：推断子文档数量（通过Doc_0, Doc_1...目录）
                var docIndices = GetOFDDocIndices();
                if (docIndices.Count == 0)
                    throw new InvalidOperationException("未发现任何子文档（Doc_x 目录）");

                var ofdDocs = new List<OFDDocument>();
                foreach (int index in docIndices)
                {
                    var ofdDoc = ReadOFDDoc(index);
                    if (ofdDoc != null)
                        ofdDocs.Add(ofdDoc);
                }

                return new OFDRootDocument
                {
                    RootOfd = rootOfd,
                    Docs = ofdDocs
                };
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
        /// 获取所有存在的子文档索引（如 [0, 1, 2] 对应 Doc_0, Doc_1, Doc_2）
        /// </summary>
        private List<int> GetOFDDocIndices()
        {
            var indices = new List<int>();
            var docEntrie = _archive.GetDirectEntryNamesInDirectory("/");
            foreach (var entry in docEntrie)
            {
                if (entry.StartsWith("Doc_"))
                {
                    var index = int.Parse(entry.Substring(4));
                    indices.Add(index);
                }
            }
            // 从小到大排序
            return indices.OrderBy(x => x).ToList();
        }

        /// <summary>
        /// 读取指定索引的子文档
        /// </summary>
        /// <param name="docIndex">子文档索引</param>
        /// <returns>OFDDoc对象</returns>
        public OFDDocument ReadOFDDoc(int docIndex)
        {
            EnsureNotDisposed();

            var doc = new OFDDocument(docIndex);

            // Doc_{0}/Document.xml
            string docFilePath = Constants.GetFilePath(Constants.Doc_DocumentFile, docIndex);
            if (_archive.FileExists(docFilePath))
            {
                using var stream = _archive.OpenFileStream(docFilePath);
                doc.Document = XmlHelper.DeserializeFromStream<Models.BaseStructure.DocumentRoot.Document>(stream);
            }

            // Doc_{0}/PublicRes.xml
            string pubResPath = Constants.GetFilePath(Constants.Doc_PublicResFile, docIndex);
            if (_archive.FileExists(pubResPath))
            {
                using var stream = _archive.OpenFileStream(pubResPath);
                doc.PublicResource = XmlHelper.DeserializeFromStream<Res>(stream);
            }

            // Doc_{0}/DocumentRes.xml
            string docResPath = Constants.GetFilePath(Constants.Doc_DocumentResFile, docIndex);
            if (_archive.FileExists(docResPath))
            {
                using var stream = _archive.OpenFileStream(docResPath);
                doc.DocumentResource = XmlHelper.DeserializeFromStream<Res>(stream);
            }

            // Doc_{0}/Signs/Signatures.xml
            string sigIndexPath = Constants.GetFilePath(Constants.Signs_SignaturesFile, docIndex);
            if (_archive.FileExists(sigIndexPath))
            {
                using var stream = _archive.OpenFileStream(sigIndexPath);
                doc.Signatures = XmlHelper.DeserializeFromStream<Signatures>(stream);
            }

            // 读取签章对象（Doc_{0}/Signs）
            doc.SignDocs = ReadSignDocs(docIndex);


            // 读取页面对象 (Doc_{0}/Pages/Page_{1})
            doc.PageDocs = ReadPageDocs(docIndex);

            // 读取文档级资源 (Doc_{0}/Res)
            string resDirectoryPath = Constants.GetFilePath(Constants.Doc_ResDirectory, docIndex);
            if (_archive.DirectoryExists(resDirectoryPath))
            {
                doc.ResFiles = ReadFileResInDirectory(resDirectoryPath);
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
        /// <param name="docIndex"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private List<SignDocument> ReadSignDocs(int docIndex)
        {
            var signDocIndices = GetSignDocIndices(docIndex);
            if (signDocIndices.Count == 0)
                throw new InvalidOperationException("未发现任何子文档（Sign_x 目录）");

            var signDocs = new List<SignDocument>();

            foreach (int index in signDocIndices)
            {
                var signDoc = ReadSignDoc(index, docIndex);
                if (signDoc != null)
                    signDocs.Add(signDoc);
            }
            return signDocs;
        }


        private PageDocument ReadPageDoc(int pageDocIndex, int docIndex)
        {
            EnsureNotDisposed();
            var pageDoc = new PageDocument();

            // Doc_{0}/Pages/Page_{1}/Content.xml
            string contentFile = Constants.GetFilePath(Constants.Page_ContentFile, docIndex, pageDocIndex);
            if (_archive.FileExists(contentFile))
            {
                using var stream = _archive.OpenFileStream(contentFile);
                pageDoc.Content = XmlHelper.DeserializeFromStream<Page>(stream);
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
                pageDoc.PageResFiles = ReadFileResInDirectory(pageResDirectory);
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



        private List<PageDocument> ReadPageDocs(int docIndex)
        {
            var pageDocIndices = GetPageDocIndices(docIndex);
            if (pageDocIndices.Count == 0)
                throw new InvalidOperationException("未发现任何子文档（Sign_x 目录）");

            var pageDocs = new List<PageDocument>();

            foreach (int index in pageDocIndices)
            {
                var pageDoc = ReadPageDoc(index, docIndex);
                if (pageDoc != null)
                    pageDocs.Add(pageDoc);
            }
            return pageDocs;
        }

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
