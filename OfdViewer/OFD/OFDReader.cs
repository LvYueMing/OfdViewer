using OFDViewer.Models.BaseStructure.MainEntry;
using OFDViewer.Models.BaseStructure.Resources.ResItems;
using OFDViewer.Models.Signature;
using OFDViewer.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

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
        public OFDDocument ReadOFDDocument()
        {
            EnsureNotDisposed();

            try
            {
                // 第一步：读取OFD.xml → RootOFD
                var rootOfd = ReadRootOFD();
                if (rootOfd == null)
                    throw new InvalidOperationException("无法读取OFD核心元数据（OFD.xml）");

                // 第二步：推断子文档数量（通过Doc_0, Doc_1...目录）
                var docIndices = GetDocumentIndices();
                if (docIndices.Count == 0)
                    throw new InvalidOperationException("未发现任何子文档（Doc_x 目录）");

                var docs = new List<OFDDoc>();
                foreach (int index in docIndices)
                {
                    var doc = ReadOFDDoc(index);
                    if (doc != null)
                        docs.Add(doc);
                }

                return new OFDDocument
                {
                    RootOfd = rootOfd,
                    Docs = docs
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
        private List<int> GetDocumentIndices()
        {
            //_archive 中可以

            var indices = new List<int>();
            for (int i = 0; i < 1000; i++) // 安全上限
            {
                string docPath = $"Doc_{i}";
                if (_archive.DirectoryExists(docPath))
                    indices.Add(i);
                else
                    break; // 假设连续编号
            }
            return indices;
        }

        /// <summary>
        /// 读取指定索引的子文档
        /// </summary>
        /// <param name="docIndex">子文档索引</param>
        /// <returns>OFDDoc对象</returns>
        public OFDDoc ReadOFDDoc(int docIndex)
        {
            EnsureNotDisposed();

            var doc = new OFDDoc { DocIndex = docIndex };

            // Document.xml
            string docFilePath = Constants.GetFilePath(Constants.Doc_DocumentFile, docIndex);
            if (_archive.FileExists(docFilePath))
            {
                using var stream = _archive.OpenFileStream(docFilePath);
                doc.Document = XmlHelper.DeserializeFromStream<Document>(stream);
            }

            // PublicRes.xml
            string pubResPath = Constants.GetFilePath(Constants.Doc_PublicResFile, docIndex);
            if (_archive.FileExists(pubResPath))
            {
                using var stream = _archive.OpenFileStream(pubResPath);
                doc.PublicResource = XmlHelper.DeserializeFromStream<PublicResource>(stream);
            }

            // DocumentRes.xml
            string docResPath = Constants.GetFilePath(Constants.Doc_DocumentResFile, docIndex);
            if (_archive.FileExists(docResPath))
            {
                using var stream = _archive.OpenFileStream(docResPath);
                doc.DocumentResource = XmlHelper.DeserializeFromStream<DocumentResource>(stream);
            }

            // Signatures.xml
            string sigIndexPath = Constants.GetFilePath(Constants.Signs_SignaturesFile, docIndex);
            if (_archive.FileExists(sigIndexPath))
            {
                using var stream = _archive.OpenFileStream(sigIndexPath);
                doc.Signatures = XmlHelper.DeserializeFromStream<Signatures>(stream);
            }

            // 读取签章对象（Sign_0, Sign_1...）
            doc.SignDocs = ReadSignDocs(docIndex);

            // 读取页面对象（Page_0, Page_1...）
            doc.PageDocs = ReadPageDocs(docIndex);

            // 读取文档级资源 Res/
            doc.ResFiles = ReadDocumentResources(docIndex);

            return doc;
        }

        private List<SignDoc> ReadSignDocs(int docIndex)
        {
            var signs = new List<SignDoc>();
            for (int i = 0; i < 100; i++)
            {
                string sigDir = Constants.GetFilePath("Signs/Sign_{0}", docIndex, i);
                if (!_archive.DirectoryExists(sigDir)) break;

                var signDoc = new SignDoc
                {
                    BelongDocIndex = docIndex,
                    SignIndex = i
                };

                // Signature.xml
                string sigFile = Constants.GetFilePath(Constants.Sign_SignatureFile, docIndex, i);
                if (_archive.FileExists(sigFile))
                {
                    using var stream = _archive.OpenFileStream(sigFile);
                    signDoc.Signature = XmlHelper.DeserializeFromStream<Signature>(stream);
                }

                // Seal.esl
                string sealFile = Constants.GetFilePath(Constants.Sign_SealFile, docIndex, i);
                if (_archive.FileExists(sealFile))
                {
                    using var stream = _archive.OpenFileStream(sealFile);
                    using var ms = new MemoryStream();
                    stream.CopyTo(ms);
                    signDoc.Seal = ms.ToArray();
                }

                // SignedValue.dat
                string svFile = Constants.GetFilePath(Constants.Sign_SignedValueFile, docIndex, i);
                if (_archive.FileExists(svFile))
                {
                    using var stream = _archive.OpenFileStream(svFile);
                    using var ms = new MemoryStream();
                    stream.CopyTo(ms);
                    signDoc.SignedValue = ms.ToArray();
                }

                signs.Add(signDoc);
            }
            return signs;
        }

        private List<PageDoc> ReadPageDocs(int docIndex)
        {
            var pages = new List<PageDoc>();
            for (int i = 0; i < 1000; i++)
            {
                string pageDir = Constants.GetFilePath("Pages/Page_{0}", docIndex, i);
                if (!_archive.DirectoryExists(pageDir)) break;

                var pageDoc = new PageDoc
                {
                    BelongDocIndex = docIndex,
                    PageIndex = i
                };

                // Content.xml
                string contentFile = Constants.GetFilePath(Constants.Page_ContentFile, docIndex, i);
                if (_archive.FileExists(contentFile))
                {
                    using var stream = _archive.OpenFileStream(contentFile);
                    pageDoc.Content = XmlHelper.DeserializeFromStream<Content>(stream);
                }

                // PageRes.xml
                string pageResFile = Constants.GetFilePath(Constants.Page_PageResFile, docIndex, i);
                if (_archive.FileExists(pageResFile))
                {
                    using var stream = _archive.OpenFileStream(pageResFile);
                    pageDoc.PageRes = XmlHelper.DeserializeFromStream<PageRes>(stream);
                }

                // 页面资源 Res/
                pageDoc.PageResFiles = ReadPageResources(docIndex, i);

                pages.Add(pageDoc);
            }
            return pages;
        }

        private Dictionary<string, byte[]> ReadDocumentResources(int docIndex)
        {
            var res = new Dictionary<string, byte[]>();
            string resDir = Constants.GetFilePath(Constants.Doc_ResDirectory, docIndex);
            if (!_archive.DirectoryExists(resDir)) return res;

            var files = _archive.GetFilesInDirectory(resDir);
            foreach (var file in files)
            {
                string fileName = Path.GetFileName(file);
                using var stream = _archive.OpenFileStream(file);
                using var ms = new MemoryStream();
                stream.CopyTo(ms);
                res[fileName] = ms.ToArray();
            }
            return res;
        }

        private Dictionary<string, byte[]> ReadPageResources(int docIndex, int pageIndex)
        {
            var res = new Dictionary<string, byte[]>();
            string resDir = Constants.GetFilePath(Constants.Page_ResDirectory, docIndex, pageIndex);
            if (!_archive.DirectoryExists(resDir)) return res;

            var files = _archive.GetFilesInDirectory(resDir);
            foreach (var file in files)
            {
                string fileName = Path.GetFileName(file);
                using var stream = _archive.OpenFileStream(file);
                using var ms = new MemoryStream();
                stream.CopyTo(ms);
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

        ～OFDReader()
        {
            Dispose(false);
        }

        #endregion
    }
}
