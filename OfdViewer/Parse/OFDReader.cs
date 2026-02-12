using OFDViewer.Models.Annotation;
using OFDViewer.Models.Attachment;
using OFDViewer.Models.BaseStructure.MainEntry;
using OFDViewer.Models.BaseStructure.Pages;
using OFDViewer.Models.BaseStructure.Resources;
using OFDViewer.Models.CustomTag;
using OFDViewer.Models.Extension;
using OFDViewer.Models.Signature;
using OFDViewer.Utils;

using Page = OFDViewer.Models.BaseStructure.Pages.Page;

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

                // 第二步：从RootOFD的 DocBody 中获取 DocRoot/Signatures 路径
                // （例如：Doc_0/Document.xml、Doc_0/Signs/Signatures.xml）
                if (rootOfd.DocBodies != null && rootOfd.DocBodies.Count > 0)
                {
                    foreach (var docBody in rootOfd.DocBodies)
                    {
                        if (docBody.DocRoot != null)
                        {
                            // Doc_0/Document.xml
                            var documentFilePath = docBody.DocRoot.Path;
                            // Doc_0/Signs/Signatures.xml
                            var signsFilePath = docBody.Signatures.Path;
                            var ofdDoc = ReadOFDDocument(documentFilePath, signsFilePath);
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
        /// 读取 OFD.xml 全局元数据
        /// </summary>
        public RootOFD ReadRootOFD()
        {
            if (!_archive.FileExists("OFD.xml"))
                return null;

            EnsureNotDisposed();

            using var stream = _archive.OpenFileStream("OFD.xml");
            return XmlHelper.DeserializeFromStream<RootOFD>(stream);
        }

        /// <summary>
        /// 读取指定路径的子文档
        /// </summary>
        /// <param name="docFilePath"> document.xml 的路径</param>
        /// <param name="signsFilePath"> Signatures.xml 的路径</param>
        /// <returns>OFDDocument对象</returns>
        public OFDDocument ReadOFDDocument(string docFilePath, string signsFilePath)
        {
            if (string.IsNullOrEmpty(docFilePath))
                return null;

            EnsureNotDisposed();


            var doc = new OFDDocument(docFilePath);

            // 保存归档引用，用于延迟加载资源文件
            // 不立即加载资源文件，等待使用时再从归档读取
            doc.ResourceArchive = _archive;

            // 读取 Document.xml
            if (_archive.FileExists(docFilePath))
            {
                // docFilePath 和 OFD.xml 的同目录路径，所有不用获取绝对路径
                using var stream = _archive.OpenFileStream(docFilePath);
                doc.Document = XmlHelper.DeserializeFromStream<Models.BaseStructure.DocumentRoot.Document>(stream);
            }

            // 读取公共资源(PublicRes.xml)、文档资源(DocumentRes.xml)
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
                        doc.ResDirectoryPath = doc.PublicResource.BaseLoc.GetAbsolutePath(doc.DocDirectoryPath).Path;
                        doc.PublicResourceFilePath = pubResFilePath;

                        // 不立即加载资源文件，等待使用时再从归档读取 doc.ResourceArchive = _archive;
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

                        // 不立即加载资源文件，等待使用时再从归档读取 doc.ResourceArchive = _archive;
                    }
                }

                // 读取模板页面对象
                if (doc.Document.CommonData.TemplatePage != null && doc.Document.CommonData.TemplatePage.Count > 0)
                {
                    foreach (var tpl in doc.Document.CommonData.TemplatePage)
                    {
                        // 获取模板页面对象文件 Doc_0/Templates/Tpl_0/Content.xml 绝对路径 
                        var tplFilePath = tpl.BaseLoc.GetAbsolutePath(doc.DocDirectoryPath).Path;
                        var tplDoc = ReadTemplateDoc(tplFilePath, doc.DocDirectoryPath);
                        //模版页面ID
                        tplDoc.TemplateId = tpl.ID;

                        doc.AddTemplateDoc(tplDoc);
                    }
                }
            }

            // 读取页面对象(Pages/Page_0/Content.xml)
            if (doc.Document != null && doc.Document.Pages != null)
            {
                foreach (var page in doc.Document.Pages)
                {
                    // 获取页面对象文件 Pages/Page_0/Content.xml 绝对路径 
                    var pageFilePath = page.BaseLoc.GetAbsolutePath(doc.DocDirectoryPath).Path;
                    var pageDoc = ReadPageDoc(pageFilePath, doc.DocDirectoryPath);
                    // 页面ID
                    pageDoc.PageId = page.ID;
                    if (pageDoc.PageIndex == -1)
                        pageDoc.PageIndex = doc.PageDocs.Count + 1;

                    doc.AddPageDoc(pageDoc);
                }
            }


            // 读取签章描述文件(Doc_0/Signs/Signatures.xml)
            if (_archive.FileExists(signsFilePath))
            {
                // 获取签章描述文件 Doc_0/Signs/Signatures.xml 
                using var stream = _archive.OpenFileStream(signsFilePath);
                doc.Signatures = XmlHelper.DeserializeFromStream<Signatures>(stream);
                doc.SignsFilePath = signsFilePath;
            }

            // 读取签章对象
            if (doc.Signatures != null && doc.Signatures.SignatureList != null)
            {
                foreach (var sign in doc.Signatures.SignatureList)
                {
                    // 获取签章对象文件 Doc_0/Signs/Sign_0/Signature.xml 绝对路径
                    var signFilePath = sign.BaseLoc.GetAbsolutePath(doc.SignsDirectoryPath).Path;
                    var signDoc = ReadSignDocument(signFilePath);

                    doc.AddSignDoc(signDoc);
                }
            }

            // 读取页面注释（Annots/Annotations.xml）
            var annotsFilePath = doc.Document.Annotations.GetAbsolutePath(doc.DocDirectoryPath).Path;
            if (_archive.FileExists(annotsFilePath))
            {
                // 获取注释列表索引文件 Doc_0/Annots/Annotations.xml
                using var stream = _archive.OpenFileStream(annotsFilePath);
                doc.Annotations = XmlHelper.DeserializeFromStream<Annotations>(stream);
                doc.AnnotationsFilePath = annotsFilePath;
            }

            // 读取页面注释对象
            if (doc.Annotations != null && doc.Annotations.Pages != null)
            {
                foreach (var page in doc.Annotations.Pages)
                {
                    // 获取页面注释文件 Doc_0/Annots/Page_{PageID}/Annotation.xml 绝对路径
                    var pageAnnotFilePath = page.FileLoc.GetAbsolutePath(doc.AnnotsDirectoryPath).Path;
                    var pageAnnotDoc = ReadPageAnnotDocument(pageAnnotFilePath);
                    // 页面ID
                    pageAnnotDoc.PageId = page.PageID;

                    doc.AddPageAnnotDoc(pageAnnotDoc);
                }
            }

            // 读取页面自定义标引（Tags/CustomTags.xml）
            var customTagsFilePath = doc.CustomTagsFilePath;
            if (_archive.FileExists(customTagsFilePath))
            {
                // 获取自定义标引列表索引文件 Doc_0/Tags/CustomTags.xml
                using var customTagsStream = _archive.OpenFileStream(customTagsFilePath);
                doc.CustomTags = XmlHelper.DeserializeFromStream<CustomTags>(customTagsStream);
                doc.CustomTagsFilePath = customTagsFilePath;
            }

            // 读取页面扩展信息（Exts/Extensions.xml）
            var extensionsFilePath = doc.ExtensionsFilePath;
            if (_archive.FileExists(extensionsFilePath))
            {
                // 获取扩展信息列表索引文件 Doc_0/Exts/Extensions.xml
                using var extensionsStream = _archive.OpenFileStream(extensionsFilePath);
                doc.Extensions = XmlHelper.DeserializeFromStream<Extensions>(extensionsStream);
                doc.ExtensionsFilePath = extensionsFilePath;
            }

            // 读取页面附件(Attachs/Attachments.xml)
            var attachmentsFilePath = doc.AttachmentsFilePath;
            if (_archive.FileExists(attachmentsFilePath))
            {
                // 获取附件列表索引文件 Doc_0/Attachs/Attachments.xml
                using var attachmentsStream = _archive.OpenFileStream(attachmentsFilePath);
                doc.Attachments = XmlHelper.DeserializeFromStream<Attachments>(attachmentsStream);
                doc.AttachmentsFilePath = attachmentsFilePath;
            }

            return doc;
        }


        /// <summary>
        /// 读取签章对象（根据路径）
        /// </summary>
        /// <param name="signFilePath">签章属性描述文件</param>
        /// <returns>签章对象</returns>
        private SignDocument ReadSignDocument(string signFilePath)
        {
            if (string.IsNullOrEmpty(signFilePath))
                return null;

            EnsureNotDisposed();

            var signDoc = new SignDocument();

            // Doc_{0}/Signs/Sign_{1}/Signature.xml
            if (_archive.FileExists(signFilePath))
            {
                using var stream = _archive.OpenFileStream(signFilePath);
                signDoc.Signature = XmlHelper.DeserializeFromStream<Signature>(stream);
                signDoc.SignFilePath = signFilePath;
            }

            //签名值 Doc_{0}/Signs/Sign_{1}/SignedValue.dat
            if (signDoc.Signature != null && signDoc.Signature.SignedInfo != null)
            {
                var signValueFile = signDoc.Signature.SignedValue.GetAbsolutePath(signDoc.SignDirectoryPath).Path;
                if (_archive.FileExists(signValueFile))
                {
                    using var stream = _archive.OpenFileStream(signValueFile);
                    using var ms = new MemoryStream();
                    stream.CopyTo(ms);
                    signDoc.SignedValue = ms.ToArray();
                }
            }

            // 签章信息 Doc_{0}/Signs/Sign_{1}/Seal.esl
            if (signDoc.Signature != null&&signDoc.Signature.SignedInfo != null && signDoc.Signature.SignedInfo.Seal != null)
            {
                var signSealFile = signDoc.Signature.SignedInfo.Seal.BaseLoc.GetAbsolutePath(signDoc.SignDirectoryPath).Path;
                if (_archive.FileExists(signSealFile))
                {
                    using var stream = _archive.OpenFileStream(signSealFile);
                    using var ms = new MemoryStream();
                    stream.CopyTo(ms);
                    signDoc.Seal = ms.ToArray();
                }
            }

            return signDoc;
        }

        /// <summary>
        /// 读取页面注释对象（根据路径）
        /// </summary>
        /// <param name="pageAnnotFilePath">页面注释文件路径</param>
        /// <returns>页面注释对象</returns>
        private PageAnnotDocument ReadPageAnnotDocument(string pageAnnotFilePath)
        {
            if (string.IsNullOrEmpty(pageAnnotFilePath))
                return null;

            EnsureNotDisposed();

            var pageAnnotDoc = new PageAnnotDocument();

            // Doc_{0}/Annots/Annot_{PageID}.xml
            if (_archive.FileExists(pageAnnotFilePath))
            {
                using var stream = _archive.OpenFileStream(pageAnnotFilePath);
                pageAnnotDoc.PageAnnot = XmlHelper.DeserializeFromStream<PageAnnot>(stream);
                pageAnnotDoc.PageAnnotFilePath = pageAnnotFilePath;
            }

            return pageAnnotDoc;
        }

        /// <summary>
        /// 读取指定索引的页面对象（根据路径）
        /// </summary>
        /// <param name="pageDocFilePath">文档路径</param>
        /// <returns>页面对象</returns>
        private PageDocument ReadPageDoc(string pageDocFilePath,string docFilePath)
        {
            EnsureNotDisposed();
            var pageDoc = new PageDocument(docFilePath);

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

                        // 不立即加载资源文件，等待使用时再从归档读取 doc.ResourceArchive = _archive;
                    }
                }
            }

            return pageDoc;
        }

        /// <summary>
        /// 读取指定索引的模板页对象（根据路径）
        /// </summary>
        /// <param name="templateDocFilePath">模板页文档路径</param>
        /// <param name="docFilePath">文档路径</param>
        /// <returns>模板页对象</returns>
        private TemplateDocument ReadTemplateDoc(string templateDocFilePath, string docFilePath)
        {
            EnsureNotDisposed();
            var templateDoc = new TemplateDocument(docFilePath);

            // Doc_{0}/Tpls/Tpl_{1}/Content.xml
            if (_archive.FileExists(templateDocFilePath))
            {
                using var stream = _archive.OpenFileStream(templateDocFilePath);
                templateDoc.TemplatePage = XmlHelper.DeserializeFromStream<Page>(stream);
                templateDoc.TemplateFilePath = templateDocFilePath;
            }

            if (templateDoc.TemplatePage != null && templateDoc.TemplatePage.PageRes != null)
            {
                // 读取 TemplateRes.xml 等
                var templateResPaths = templateDoc.TemplatePage.PageRes;

                foreach (var res in templateResPaths)
                {
                    //  获取 TemplateRes.xml 绝对路径
                    var templateResFilePath = res.GetAbsolutePath(templateDoc.TemplateDirectoryPath).Path;
                    if (_archive.FileExists(templateResFilePath))
                    {
                        using var stream = _archive.OpenFileStream(templateResFilePath);
                        templateDoc.TemplateRes = XmlHelper.DeserializeFromStream<Res>(stream);
                        templateDoc.TemplateResFilePath = templateResFilePath;

                        // 读取 TemplateRes.xml 中的资源文件
                        // 不立即加载资源文件，等待使用时再从归档读取 doc.ResourceArchive = _archive;
                    }
                }
            }

            return templateDoc;
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
