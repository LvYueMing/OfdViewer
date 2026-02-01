using System.Text.RegularExpressions;
using System.Xml.Serialization;
using OFDViewer.Models.Annotation;
using OFDViewer.Models.BaseStructure.DocumentRoot;
using OFDViewer.Models.BaseStructure.Pages;
using OFDViewer.Models.BaseStructure.Resources;
using OFDViewer.Models.BaseStructure.Resources.ResItems;
using OFDViewer.Models.BaseType;
using OFDViewer.Models.Signature;

namespace OFDViewer.Parse
{
    /// <summary>
    /// OFD 文档核心类，对应 OFD 标准中的 Doc_N 目录
    /// <remarks>
    /// 负责管理单个 OFD 文档的所有内容，包括：
    /// 1. 文档主描述信息（Document.xml）
    /// 2. 公共资源和文档资源
    /// 3. 页面集合
    /// 4. 签章集合
    /// 5. 模板页集合
    /// 6. 页面注释集合（对应PageAnnot_N目录，记录页面上的注释）
    /// 7. 其他资源文件
    /// </remarks>
    /// </summary>
    public class OFDDocument
    {
        /// <summary>
        /// 文档主描述文件（Document.xml），定义页面尺寸、页面总数、文档结构等属性
        /// </summary>
        public Document Document { get; set; }

        /// <summary>
        /// 文档序号（从0开始，只读，构造时赋值）
        /// </summary>
        public int DocIndex { get; set; }


        private string _docDirectoryPath;
        /// <summary>
        /// 当前文档路径（当使用路径构造函数时赋值）
        /// </summary>
        public string DocDirectoryPath
        {
            get => _docDirectoryPath ?? Constants.GetFilePath(Constants.Doc_BaseDirectory, DocIndex);
            set => _docDirectoryPath = value;
        }

        private Res _publicResource;
        /// <summary>
        /// 全文档公共资源描述文件（PublicRes.xml）
        /// </summary>
        public Res PublicResource
        {
            get => _publicResource;
            set => _publicResource = value;
        }

        private Res _documentResource;

        /// <summary>
        /// 当前文档的资源描述文件（DocumentRes.xml）
        /// </summary>
        public Res DocumentResource
        {
            get => _documentResource;
            set => _documentResource = value;
        }


        /// <summary>
        /// 页面对象集合（对应Page_N目录，存储文档所有页面）
        /// </summary>
        public List<PageDocument> PageDocs { get; set; }

        /// <summary>
        /// 签章列表索引对象（对应Signatures.xml，记录所有签章信息）
        /// </summary>
        public Signatures Signatures { get; set; }

        /// <summary>
        /// 签章对象集合（对应Sign_N目录，一个文档可包含多个签章）
        /// </summary>
        public List<SignDocument> SignDocs { get; set; }

        /// <summary>
        /// 注释列表索引对象（对应Annots/Annotations.xml，记录所有注释信息）
        /// </summary>
        public Annotations Annotations { get; set; }

        /// <summary>
        /// 页面注释对象集合（对应Annot_N目录，一个文档可包含多个页面注释）
        /// </summary>
        public List<PageAnnotDocument> PageAnnotDocs { get; set; }

        /// <summary>
        /// 文档级资源文件集合（存储Res目录下的字体、图片等资源）
        /// 延迟加载缓存：首次使用时从归档文件加载，之后缓存在此处
        /// </summary>
        public Dictionary<string, byte[]> ResFiles { get; set; }

        /// <summary>
        /// 模板页对象集合（对应Tpl_N目录，一个文档可包含多个模板页）
        /// </summary>
        public List<TemplateDocument> TemplateDocs { get; set; }

        /// <summary>
        /// 归档文件引用，用于延迟加载资源文件
        /// 解析时保存归档引用，使用时从归档读取资源文件
        /// </summary>
        internal OFDArchive ResourceArchive { get; set; }

        /// <summary>
        /// 缓存锁，用于线程安全的缓存操作
        /// 确保多线程环境下缓存的一致性
        /// </summary>
        private readonly object _cacheLock = new object();

        private string _documentFilePath;
        /// <summary>
        /// 文档主描述文件绝对路径
        /// 默认 Doc_{0}/Document.xml
        /// </summary>
        public string DocumentFilePath
        {
            get
            {
                // 优先返回已设置的路径
                if (!string.IsNullOrEmpty(_documentFilePath))
                    return _documentFilePath;

                // 新建时：使用默认路径
                if (string.IsNullOrEmpty(DocDirectoryPath))
                    return Constants.GetFilePath(Constants.Doc_DocumentFile, DocIndex);

                return Path.Combine(DocDirectoryPath, "Document.xml");
            }
            set => _documentFilePath = value;
        }

        private string _publicResourceFilePath;
        /// <summary>
        /// 文档公共资源描述文件绝对路径（Doc_{0}/PublicRes.xml）
        /// </summary>
        public string PublicResourceFilePath
        {
            get
            {
                // 优先返回已设置的路径
                if (!string.IsNullOrEmpty(_publicResourceFilePath))
                    return _publicResourceFilePath;

                // 解析时：从 Document.CommonData.PublicRes 读取（取第一个）
                if (Document?.CommonData?.PublicRes != null && Document.CommonData.PublicRes.Count > 0)
                    return Document.CommonData.PublicRes[0].GetAbsolutePath(DocDirectoryPath).Path;

                // 新建时：使用默认路径
                if (string.IsNullOrEmpty(DocDirectoryPath))
                    return Constants.GetFilePath(Constants.Doc_PublicResFile, DocIndex);

                return Path.Combine(DocDirectoryPath, "PublicRes.xml");
            }
            set => _publicResourceFilePath = value;
        }

        private string _documentResourceFilePath;
        /// <summary>
        /// 文档私有资源描述文件路径（Doc_{0}/DocumentRes.xml）
        /// </summary>
        public string DocumentResourceFilePath
        {
            get
            {
                // 优先返回已设置的路径
                if (!string.IsNullOrEmpty(_documentResourceFilePath))
                    return _documentResourceFilePath;

                // 解析时：从 Document.CommonData.DocumentRes 读取（取第一个）
                if (Document?.CommonData?.DocumentRes != null && Document.CommonData.DocumentRes.Count > 0)
                    return Document.CommonData.DocumentRes[0].GetAbsolutePath(DocDirectoryPath).Path;

                // 新建时：使用默认路径
                if (string.IsNullOrEmpty(DocDirectoryPath))
                    return Constants.GetFilePath(Constants.Doc_DocumentResFile, DocIndex);

                return Path.Combine(DocDirectoryPath, "DocumentRes.xml");
            }
            set => _documentResourceFilePath = value;
        }

        private string _signsFilePath;
        /// <summary>
        /// 获取签章描述文件 
        /// Doc_0/Signs/Signatures.xml 
        /// </summary>
        public string SignsFilePath
        {
            get
            {
                // 优先返回已设置的路径（解析时由OFDReader设置）
                if (!string.IsNullOrEmpty(_signsFilePath))
                    return _signsFilePath;

                // 新建时：使用默认路径
                if (string.IsNullOrEmpty(DocDirectoryPath))
                    return Constants.GetFilePath(Constants.Signs_SignaturesFile, DocIndex);

                return Path.Combine(SignsDirectoryPath, "Signatures.xml");
            }
            set => _signsFilePath = value;
        }

        private string _signsDirectoryPath;
        /// <summary>
        /// 签章对象集合目录(Doc_{0}/Signs)
        /// </summary>
        public string SignsDirectoryPath
        {
            get
            {
                // 优先返回已设置的目录路径
                if (!string.IsNullOrEmpty(_signsDirectoryPath))
                    return _signsDirectoryPath;

                // 解析时：从 SignsFilePath 推断目录
                if (!string.IsNullOrEmpty(SignsFilePath))
                    return Path.GetDirectoryName(SignsFilePath) ?? string.Empty;

                // 新建时：使用默认路径
                if (string.IsNullOrEmpty(DocDirectoryPath))
                    return Constants.GetFilePath(Constants.Signs_BaseDirectory, DocIndex);

                return Path.Combine(DocDirectoryPath, "Signs");
            }
            set => _signsDirectoryPath = value;
        }

        private string _annotationsFilePath;
        /// <summary>
        /// 获取注释列表索引文件路径
        /// Doc_0/Annots/Annotations.xml 
        /// </summary>
        public string AnnotationsFilePath
        {
            get
            {
                // 优先返回已设置的路径
                if (!string.IsNullOrEmpty(_annotationsFilePath))
                    return _annotationsFilePath;

                // 解析时：从 Document.Annotations 读取
                if (Document?.Annotations != null)
                    return Document.Annotations.GetAbsolutePath(DocDirectoryPath).Path;

                // 新建时：使用默认路径
                if (string.IsNullOrEmpty(DocDirectoryPath))
                    return Constants.GetFilePath(Constants.Annots_AnnotationsFile, DocIndex);

                return Path.Combine(AnnotsDirectoryPath, "Annotations.xml");
            }
            set => _annotationsFilePath = value;
        }

        private string _annotsDirectoryPath;
        /// <summary>
        /// 注释对象集合目录(Doc_{0}/Annots)
        /// </summary>
        public string AnnotsDirectoryPath
        {
            get
            {
                // 优先返回已设置的目录路径
                if (!string.IsNullOrEmpty(_annotsDirectoryPath))
                    return _annotsDirectoryPath;

                // 解析时：从 AnnotationsFilePath 推断目录
                if (!string.IsNullOrEmpty(AnnotationsFilePath))
                    return Path.GetDirectoryName(AnnotationsFilePath) ?? string.Empty;

                // 新建时：使用默认路径
                if (string.IsNullOrEmpty(DocDirectoryPath))
                    return Constants.GetFilePath(Constants.Annots_BaseDirectory, DocIndex);

                return Path.Combine(DocDirectoryPath, "Annots");
            }
            set => _annotsDirectoryPath = value;
        }

        private string _resDirectoryPath;
        /// <summary>
        /// 文档级资源目录路径（Doc_{0}/Res）
        /// </summary>
        public string ResDirectoryPath
        {
            get
            {
                // 优先返回已设置的目录路径
                if (!string.IsNullOrEmpty(_resDirectoryPath))
                    return _resDirectoryPath;

                // 新建时：使用默认路径
                if (string.IsNullOrEmpty(DocDirectoryPath))
                    return Constants.GetFilePath(Constants.Doc_ResDirectory, DocIndex);

                return Path.Combine(DocDirectoryPath, "Res");
            }
            set => _resDirectoryPath = value;
        }


        private string _pagesDirectoryPath;
        /// <summary>
        /// 文档级资源目录路径（Doc_{0}/Pages）
        /// </summary>
        public string PagesDirectoryPath
        {
            get
            {
                // 优先返回已设置的目录路径
                if (!string.IsNullOrEmpty(_pagesDirectoryPath))
                    return _pagesDirectoryPath;

                // 新建时：使用默认路径
                if (string.IsNullOrEmpty(DocDirectoryPath))
                    return Constants.GetFilePath(Constants.Pages_BaseDirectory, DocIndex);

                return Path.Combine(DocDirectoryPath, "Pages");
            }
            set => _pagesDirectoryPath = value;
        }

        private string _templatesDirectoryPath;
        /// <summary>
        /// 模板页总目录路径（Doc_{0}/Tpls）
        /// </summary>
        public string TemplatesDirectoryPath
        {
            get
            {
                // 优先返回已设置的目录路径
                if (!string.IsNullOrEmpty(_templatesDirectoryPath))
                    return _templatesDirectoryPath;

                // 新建时：使用默认路径
                if (string.IsNullOrEmpty(DocDirectoryPath))
                    return Constants.GetFilePath(Constants.Templates_BaseDirectory, DocIndex);

                return Path.Combine(DocDirectoryPath, "Tpls");
            }
            set => _templatesDirectoryPath = value;
        }


        //无参构造函数
        public OFDDocument()
        {
        }

        /// <summary>
        /// 构造函数，初始化文档路径及默认对象
        /// </summary>
        /// <param name="docFilePath">文档路径</param>
        public OFDDocument(string docFilePath)
        {
            if (string.IsNullOrEmpty(docFilePath))
            {
                throw new ArgumentNullException(nameof(docFilePath), "文档路径不能为空");
            }
            DocDirectoryPath = Path.GetDirectoryName(docFilePath);
            DocumentFilePath = docFilePath;
            // 从文档路径 Doc_{0} 获取文档序号 0
            DocIndex = int.Parse(Regex.Match(DocDirectoryPath, @"Doc_(\d+)").Groups[1].Value);
            Document = new Document();
            PageDocs = new List<PageDocument>();
            ResFiles = new Dictionary<string, byte[]>();
        }

        /// <summary>
        /// 构造函数，初始化文档序号及默认对象（DocIndex从0开始）
        /// </summary>
        /// <param name="docIndex">文档序号（从0开始）</param>
        public OFDDocument(int docIndex)
        {
            // 校验文档序号合法性：从0开始，不允许负数
            if (docIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(docIndex), "文档序号必须从0开始，不允许为负数");
            }
            DocIndex = docIndex;
            Document = new Document();
            PageDocs = new List<PageDocument>();
            ResFiles = new Dictionary<string, byte[]>();
        }

        /// <summary>
        /// 添加空白页面对象
        /// </summary>
        /// <remarks>
        /// 自动创建新的PageDocument对象并添加到页面集合中
        /// 同时更新Document.Pages集合，建立页面与文档的关联
        /// </remarks>
        public void NewPageDoc()
        {
            PageDocs = PageDocs ?? new List<PageDocument>();

            // 计算新的页面序号（当前页面数量，从0开始）
            int newPageIndex = PageDocs.Count;

            var pageDoc = new PageDocument();

            // 设置页面序号
            pageDoc.BelongDocIndex = DocIndex;


            // 创建并添加对应的DocumentPage对象到Document.Pages集合中
            if (Document != null && Document.Pages != null)
            {
                // 创建DocumentPage对象
                var documentPage = new DocumentPage();

                // 设置BaseLoc为页面对象描述文件的路径（使用相对当前文档目录的路径）
                documentPage.BaseLoc = new ST_Loc($"Pages/Page_{newPageIndex}/Content.xml");

                // 添加到Document.Pages集合
                Document.Pages.Add(documentPage);
            }

            // 添加页面对象
            PageDocs.Add(pageDoc);
        }

        /// <summary>
        /// 添加指定的页面对象
        /// </summary>
        /// <param name="pageDoc">要添加的页面对象</param>
        /// <remarks>
        /// 将指定的PageDocument对象添加到页面集合中
        /// 同时更新Document.Pages集合，建立页面与文档的关联
        /// 自动设置页面对象的所属文档序号和页面序号
        /// </remarks>
        public void AddPageDoc(PageDocument pageDoc)
        {
            PageDocs = PageDocs ?? new List<PageDocument>();            
            // 添加页面对象
            PageDocs.Add(pageDoc);
        }


        /// <summary>
        /// 添加指定的签章对象
        /// </summary>
        /// <param name="signDoc"></param>
        public void AddSignDoc(SignDocument signDoc)
        {
            SignDocs = SignDocs ?? new List<SignDocument>();
            // 添加签章对象
            SignDocs.Add(signDoc);
        }

        /// <summary>
        /// 添加页面注释对象
        /// </summary>
        /// <param name="pageAnnotDoc">页面注释对象</param>
        /// <remarks>
        /// 将PageAnnotDocument对象添加到页面注释集合中
        /// </remarks>
        public void AddPageAnnotDoc(PageAnnotDocument pageAnnotDoc)
        {
            PageAnnotDocs = PageAnnotDocs ?? new List<PageAnnotDocument>();
            // 添加页面注释对象
            PageAnnotDocs.Add(pageAnnotDoc);
        }

        /// <summary>
        /// 添加空白模板页对象
        /// </summary>
        /// <remarks>
        /// 自动创建新的TemplateDocument对象并添加到模板页集合中
        /// 同时更新Document.CommonData.Templates集合，建立模板页与文档的关联
        /// </remarks>
        public void NewTemplateDoc()
        {
            TemplateDocs = TemplateDocs ?? new List<TemplateDocument>();

            // 计算新的模板页序号（当前模板页数量，从0开始）
            int newTemplateIndex = TemplateDocs.Count;

            var templateDoc = new TemplateDocument();

            // 设置模板页序号
            templateDoc.BelongDocIndex = DocIndex;

            // 添加模板页对象
            TemplateDocs.Add(templateDoc);
        }

        /// <summary>
        /// 添加指定的模板页对象
        /// </summary>
        /// <param name="templateDoc">要添加的模板页对象</param>
        /// <remarks>
        /// 将指定的TemplateDocument对象添加到模板页集合中
        /// 自动设置模板页对象的所属文档序号和模板页序号
        /// </remarks>
        public void AddTemplateDoc(TemplateDocument templateDoc)
        {
            TemplateDocs = TemplateDocs ?? new List<TemplateDocument>();
            
            // 添加模板页对象
            TemplateDocs.Add(templateDoc);
        }


        /// <summary>
        /// 设置全文档公共资源描述文件（PublicRes.xml）
        /// </summary>
        /// <param name="publicResource">公共资源对象</param>
        public void SetPublicResource(Res publicResource)
        {
            if (publicResource != null)
            {
                // 设置相对路径，资源文件位于Doc_0目录下，资源目录是Doc_0/Res，所以相对路径是Res
                publicResource.BaseLoc = string.IsNullOrEmpty(publicResource.BaseLocString) 
                    ? ST_Loc.GetRelativePath(ResDirectoryPath, DocDirectoryPath) 
                    : publicResource.BaseLoc;
            }
            _publicResource = publicResource;

            // 更新Document对象中的公共资源路径
            if (Document != null && Document.CommonData != null && publicResource != null)
            {
                var publicResFileName = ST_Loc.GetRelativePath(PublicResourceFilePath, DocDirectoryPath);

                // 确保PublicRes集合已初始化
                if (Document.CommonData.PublicRes == null)
                {
                    Document.CommonData.PublicRes = new List<ST_Loc>();
                }

                // 移除旧的路径
                Document.CommonData.PublicRes = Document.CommonData.PublicRes
                    ?.Where(path => !path.ToString().EndsWith(publicResFileName.Path))?.ToList() 
                    ?? new List<ST_Loc>();

                // 添加新的路径（使用相对当前文档目录的路径）
                Document.CommonData.PublicRes.Add(publicResFileName);
            }
        }

        /// <summary>
        /// 设置当前文档的资源描述文件（DocumentRes.xml）
        /// </summary>
        /// <param name="documentResource">文档资源对象</param>
        public void SetDocumentResource(Res documentResource)
        {
            if (documentResource != null)
            {
                // 设置相对路径，资源文件位于Doc_0目录下，资源目录是Doc_0/Res，所以相对路径是Res
                documentResource.BaseLoc = string.IsNullOrEmpty(documentResource.BaseLocString)
                    ? ST_Loc.GetRelativePath(ResDirectoryPath, DocDirectoryPath)
                    : documentResource.BaseLoc;
            }
            _documentResource = documentResource;
            // 更新Document对象中的文档资源路径
            if (Document != null && Document.CommonData != null && documentResource != null)
            {
                var documentResFileName = ST_Loc.GetRelativePath(DocumentResourceFilePath, DocDirectoryPath);

                // 确保DocumentRes集合已初始化
                if (Document.CommonData.DocumentRes == null)
                {
                    Document.CommonData.DocumentRes = new List<ST_Loc>();
                }

                // 移除旧的路径
                Document.CommonData.DocumentRes = Document.CommonData.DocumentRes
                    ?.Where(path => !path.ToString().EndsWith(documentResFileName.Path))
                    ?.ToList() ?? new List<ST_Loc>();

                // 添加新的路径（使用相对当前文档目录的路径）
                Document.CommonData.DocumentRes.Add(documentResFileName);
            }
        }


        #region 获取资源对象

        /// 从模版页获取指定类型的资源（非泛型版本）
        /// </summary>
        /// <param name="templateIndex">模版页索引</param>
        /// <param name="resourceId">资源ID</param>
        /// <param name="resourceType">资源类型</param>
        /// <param name="location">资源位置</param>
        /// <returns>指定类型的资源对象，如果未找到返回null</returns>
        public object GetTemplateResource(int templateIndex, string resourceId, ResourceType resourceType, ResourceLocation location = ResourceLocation.Auto)
        {
            // 获取模版页面对象
            TemplateDocument templateDoc = TemplateDocs?.FirstOrDefault(t => t.TemplateId == templateIndex);
            if (templateDoc == null) return null;

            // 按照指定位置或自动搜索顺序查找资源
            switch (location)
            {
                case ResourceLocation.Template:
                    return GetResourceFromLocation(templateDoc.TemplateRes, resourceId, resourceType);

                case ResourceLocation.Document:
                    return GetResourceFromLocation(DocumentResource, resourceId, resourceType);

                case ResourceLocation.Public:
                    return GetResourceFromLocation(PublicResource, resourceId, resourceType);

                case ResourceLocation.Auto:
                default:
                    // 自动搜索顺序：Template -> Document -> Public
                    // 注意：模版页资源获取不包含Page资源
                    object resource = GetResourceFromLocation(templateDoc.TemplateRes, resourceId, resourceType);
                    if (resource != null) return resource;

                    resource = GetResourceFromLocation(DocumentResource, resourceId, resourceType);
                    if (resource != null) return resource;

                    resource = GetResourceFromLocation(PublicResource, resourceId, resourceType);
                    return resource;
            }
        }

        /// <summary>
        /// 从模版页获取指定类型的资源（泛型版本）
        /// </summary>
        /// <typeparam name="T">资源类型（OFDFont、ColorSpace、DrawParam等）</typeparam>
        /// <param name="templateIndex">模版页索引</param>
        /// <param name="resourceId">资源ID</param>
        /// <param name="location">资源位置</param>
        /// <returns>指定类型的资源对象，如果未找到返回default(T)</returns>
        public T GetTemplateResource<T>(int templateIndex, string resourceId, ResourceLocation location = ResourceLocation.Auto)
        {
            // 获取模版页面对象
            TemplateDocument templateDoc = TemplateDocs?.FirstOrDefault(t => t.TemplateId == templateIndex);
            if (templateDoc == null) return default(T);

            // 按照指定位置或自动搜索顺序查找资源
            switch (location)
            {
                case ResourceLocation.Template:
                    return GetResourceFromLocation<T>(templateDoc.TemplateRes, resourceId);

                case ResourceLocation.Document:
                    return GetResourceFromLocation<T>(DocumentResource, resourceId);

                case ResourceLocation.Public:
                    return GetResourceFromLocation<T>(PublicResource, resourceId);

                case ResourceLocation.Auto:
                default:
                    // 自动搜索顺序：Template -> Document -> Public
                    // 注意：模版页资源获取不包含Page资源
                    T resource = GetResourceFromLocation<T>(templateDoc.TemplateRes, resourceId);
                    if (resource != null) return resource;

                    resource = GetResourceFromLocation<T>(DocumentResource, resourceId);
                    if (resource != null) return resource;

                    resource = GetResourceFromLocation<T>(PublicResource, resourceId);
                    return resource;
            }
        }

        /// <summary>
        /// 泛型版本：从指定位置获取指定类型的资源
        /// </summary>
        /// <typeparam name="T">资源类型（OFDFont、ColorSpace、DrawParam等）</typeparam>
        /// <param name="pageIndex">页面索引</param>
        /// <param name="resourceId">资源ID</param>
        /// <param name="location">资源位置</param>
        /// <returns>指定类型的资源对象，如果未找到返回default(T)</returns>
        public T GetResource<T>(int pageIndex, string resourceId, ResourceLocation location = ResourceLocation.Auto)
        {
            // 获取页面对象
            PageDocument pageDoc = PageDocs?.FirstOrDefault(p => p.PageIndex == pageIndex);
            if (pageDoc == null) return default(T);

            // 按照指定位置或自动搜索顺序查找资源
            switch (location)
            {
                case ResourceLocation.Page:
                    return GetResourceFromLocation<T>(pageDoc.PageRes, resourceId);

                case ResourceLocation.Document:
                    return GetResourceFromLocation<T>(DocumentResource, resourceId);

                case ResourceLocation.Public:
                    return GetResourceFromLocation<T>(PublicResource, resourceId);

                case ResourceLocation.Auto:
                default:
                    // 自动搜索顺序：Page -> Document -> Public
                    // 注意：自动搜索时不包含模版页资源
                    T resource = GetResourceFromLocation<T>(pageDoc.PageRes, resourceId);
                    if (resource != null) return resource;

                    resource = GetResourceFromLocation<T>(DocumentResource, resourceId);
                    if (resource != null) return resource;

                    resource = GetResourceFromLocation<T>(PublicResource, resourceId);
                    return resource;
            }
        }

        /// <summary>
        /// 泛型版本：从指定资源集合中获取指定类型的资源
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="res">资源集合</param>
        /// <param name="resourceId">资源ID</param>
        /// <returns>指定类型的资源对象，如果未找到返回default(T)</returns>
        private T GetResourceFromLocation<T>(Res res, string resourceId)
        {
            if (res == null || res.ResItems == null)
                return default(T);

            // 根据泛型类型查找对应的资源
            if (typeof(T) == typeof(OFDFont))
            {
                var fontsCollection = res.ResItems.OfType<OFDFonts>().FirstOrDefault();
                var font = fontsCollection?.ofdFonts.FirstOrDefault(f => f.ID.ToString() == resourceId);
                return (T)(object)font;
            }
            else if (typeof(T) == typeof(ColorSpace))
            {
                var colorSpacesCollection = res.ResItems.OfType<ColorSpaces>().FirstOrDefault();
                var colorSpace = colorSpacesCollection?.colorSpaces?.FirstOrDefault(c => c.ID.ToString() == resourceId);
                return (T)(object)colorSpace;
            }
            else if (typeof(T) == typeof(DrawParam))
            {
                var drawParamsCollection = res.ResItems.OfType<DrawParams>().FirstOrDefault();
                var drawParam = drawParamsCollection?.drawParams?.FirstOrDefault(d => d.ID.ToString() == resourceId);
                return (T)(object)drawParam;
            }
            else if (typeof(T) == typeof(CompositeGraphicUnit))
            {
                var compositeGraphicsCollection = res.ResItems.OfType<CompositeGraphicUnits>().FirstOrDefault();
                var vectorGraphic = compositeGraphicsCollection?.compositeGraphicUnits?.FirstOrDefault(v => v.ID.ToString() == resourceId);
                return (T)(object)vectorGraphic;
            }
            else if (typeof(T) == typeof(MultiMedia))
            {
                var multiMediasCollection = res.ResItems.OfType<MultiMedias>().FirstOrDefault();
                var multimedia = multiMediasCollection?.multiMedias?.FirstOrDefault(m => m.ID.ToString() == resourceId);
                return (T)(object)multimedia;
            }

            // 如果不是已知的资源类型，返回default(T)
            return default(T);
        }

        /// 从指定位置获取指定类型的资源
        /// </summary>
        /// <param name="pageIndex">页面索引</param>
        /// <param name="resourceId">资源ID</param>
        /// <param name="resourceType">资源类型</param>
        /// <param name="location">资源位置</param>
        /// <returns>资源对象，如果未找到返回null</returns>
        public object GetResource(int pageIndex, string resourceId, ResourceType resourceType = ResourceType.All, ResourceLocation location = ResourceLocation.Auto)
        {
            // 获取页面对象
            PageDocument pageDoc = PageDocs?.FirstOrDefault(p => p.PageIndex == pageIndex);
            if (pageDoc == null) return null;

            // 按照指定位置或自动搜索顺序查找资源
            switch (location)
            {
                case ResourceLocation.Page:
                    return GetResourceFromLocation(pageDoc.PageRes, resourceId, resourceType);

                case ResourceLocation.Document:
                    return GetResourceFromLocation(DocumentResource, resourceId, resourceType);

                case ResourceLocation.Public:
                    return GetResourceFromLocation(PublicResource, resourceId, resourceType);

                case ResourceLocation.Auto:
                default:
                    // 自动搜索顺序：Page -> Document -> Public
                    // 注意：自动搜索时不包含模版页资源
                    object resource = GetResourceFromLocation(pageDoc.PageRes, resourceId, resourceType);
                    if (resource != null) return resource;

                    resource = GetResourceFromLocation(DocumentResource, resourceId, resourceType);
                    if (resource != null) return resource;

                    resource = GetResourceFromLocation(PublicResource, resourceId, resourceType);
                    return resource;
            }
        }

        /// <summary>
        /// 从指定资源集合中获取资源
        /// </summary>
        /// <param name="res">资源集合</param>
        /// <param name="resourceId">资源ID</param>
        /// <param name="resourceType">资源类型</param>
        /// <returns>资源对象，如果未找到返回null</returns>
        private object GetResourceFromLocation(Res res, string resourceId, ResourceType resourceType)
        {
            if (res == null || res.ResItems == null)
                return null;

            // 根据资源类型查找
            switch (resourceType)
            {
                case ResourceType.Font:
                    // 查找Fonts资源集合
                    var fontsCollection = res.ResItems.OfType<OFDFonts>().FirstOrDefault();
                    return fontsCollection?.ofdFonts.FirstOrDefault(f => f.ID.ToString() == resourceId);
                case ResourceType.ColorSpace:
                    // 查找ColorSpaces资源集合
                    var colorSpacesCollection = res.ResItems.OfType<ColorSpaces>().FirstOrDefault();
                    return colorSpacesCollection?.colorSpaces?.FirstOrDefault(c => c.ID.ToString() == resourceId);
                case ResourceType.DrawParam:
                    // 查找DrawParams资源集合
                    var drawParamsCollection = res.ResItems.OfType<DrawParams>().FirstOrDefault();
                    return drawParamsCollection?.drawParams?.FirstOrDefault(d => d.ID.ToString() == resourceId);
                case ResourceType.VectorGraphic:
                    // 查找CompositeGraphicUnits资源集合（包含矢量图像）
                    var compositeGraphicsCollection = res.ResItems.OfType<CompositeGraphicUnits>().FirstOrDefault();
                    return compositeGraphicsCollection?.compositeGraphicUnits?.FirstOrDefault(v => v.ID.ToString() == resourceId);
                case ResourceType.Multimedia:
                    // 查找MultiMedias资源集合
                    var multiMediasCollection = res.ResItems.OfType<MultiMedias>().FirstOrDefault();
                    return multiMediasCollection?.multiMedias?.FirstOrDefault(m => m.ID.ToString() == resourceId);
                case ResourceType.All:
                default:
                    // 搜索所有类型
                    // 1. 查找Fonts
                    var allFontsCollection = res.ResItems.OfType<OFDFonts>().FirstOrDefault();
                    var font = allFontsCollection?.ofdFonts.FirstOrDefault(f => f.ID.ToString() == resourceId);
                    if (font != null) return font;

                    // 2. 查找ColorSpaces
                    var allColorSpacesCollection = res.ResItems.OfType<ColorSpaces>().FirstOrDefault();
                    var colorSpace = allColorSpacesCollection?.colorSpaces?.FirstOrDefault(c => c.ID.ToString() == resourceId);
                    if (colorSpace != null) return colorSpace;

                    // 3. 查找DrawParams
                    var allDrawParamsCollection = res.ResItems.OfType<DrawParams>().FirstOrDefault();
                    var drawParam = allDrawParamsCollection?.drawParams?.FirstOrDefault(d => d.ID.ToString() == resourceId);
                    if (drawParam != null) return drawParam;

                    // 4. 查找CompositeGraphicUnits（矢量图像）
                    var allCompositeGraphicsCollection = res.ResItems.OfType<CompositeGraphicUnits>().FirstOrDefault();
                    var vectorGraphic = allCompositeGraphicsCollection?.compositeGraphicUnits?.FirstOrDefault(v => v.ID.ToString() == resourceId);
                    if (vectorGraphic != null) return vectorGraphic;

                    // 5. 查找MultiMedias
                    var allMultiMediasCollection = res.ResItems.OfType<MultiMedias>().FirstOrDefault();
                    var multimedia = allMultiMediasCollection?.multiMedias?.FirstOrDefault(m => m.ID.ToString() == resourceId);
                    return multimedia;
            }
        }

        /// <summary>
        /// 获取资源文件内容
        /// </summary>
        /// <param name="pageIndex">页面索引</param>
        /// <param name="filePath">文件路径</param>
        /// <param name="location">获取位置</param>
        /// <returns>资源文件内容，如果未找到返回null</returns>
        public byte[] GetResourceFile(int pageIndex, string filePath, ResourceLocation location = ResourceLocation.Auto)
        {
            // 获取页面对象
            PageDocument pageDoc = PageDocs?.FirstOrDefault(p => p.PageIndex == pageIndex);
            if (pageDoc == null) return null;

            // 构建完整的文件路径
            string fullResFilePath = pageDoc.PageRes?.BaseLoc == null
                ? filePath
                : ST_Loc.GetAbsolutePath(filePath,pageDoc.PageRes.BaseLocString).Path;

            // 按照指定位置或自动搜索顺序查找资源文件
            switch (location)
            {
                case ResourceLocation.Page:
                    // 构建完整的文件路径
                    if (pageDoc.PageResFiles != null && pageDoc.PageResFiles.TryGetValue(fullResFilePath, out byte[] pageContent))
                    {
                        return pageContent;
                    }
                    return null;
                case ResourceLocation.Document:
                case ResourceLocation.Public:
                case ResourceLocation.Auto:
                default:
                    // 自动搜索顺序：Page -> Document
                    if (pageDoc.PageResFiles != null && pageDoc.PageResFiles.TryGetValue(fullResFilePath, out byte[] pageFileContent))
                    {
                        return pageFileContent;
                    }
                    // 构建完整的文件路径
                    fullResFilePath =  ST_Loc.GetAbsolutePath(filePath, ResDirectoryPath).Path;

                    // 延迟加载：先检查缓存
                    if (ResFiles != null && ResFiles.TryGetValue(fullResFilePath, out byte[] cachedContent))
                    {
                        return cachedContent;
                    }
                    
                    // 缓存未命中，从归档文件加载
                    byte[] fileContent = LoadResourceFileFromArchive(fullResFilePath);
                    
                    // 缓存结果
                    if (fileContent != null)
                    {
                        lock (_cacheLock)
                        {
                            if (ResFiles == null)
                            {
                                ResFiles = new Dictionary<string, byte[]>();
                            }
                            
                            if (!ResFiles.ContainsKey(filePath))
                            {
                                ResFiles[filePath] = fileContent;
                            }
                        }
                    }
                    
                    return fileContent;
            }
        }
        
        /// <summary>
        /// 从模版页获取资源文件内容
        /// </summary>
        /// <param name="templateIndex">模版页索引</param>
        /// <param name="filePath">文件路径</param>
        /// <param name="location">资源位置</param>
        /// <returns>资源文件内容，如果未找到返回null</returns>
        public byte[] GetTemplateResourceFile(int templateIndex, string filePath, ResourceLocation location = ResourceLocation.Auto)
        {
            // 获取模版页面对象
            TemplateDocument templateDoc = TemplateDocs?.FirstOrDefault(t => t.TemplateId == templateIndex);
            if (templateDoc == null) return null;

            // 构建完整的文件路径
            string fullResFilePath = templateDoc.TemplateRes?.BaseLoc == null
                ? filePath
                : ST_Loc.GetAbsolutePath(filePath, templateDoc.TemplateRes.BaseLocString).Path;

            // 按照指定位置或自动搜索顺序查找资源文件
            switch (location)
            {
                case ResourceLocation.Template:
                    // 构建完整的文件路径
                    if (templateDoc.TemplateResFiles != null && templateDoc.TemplateResFiles.TryGetValue(fullResFilePath, out byte[] templateContent))
                    {
                        return templateContent;
                    }
                    return null;

                case ResourceLocation.Document:
                case ResourceLocation.Public:
                case ResourceLocation.Auto:
                default:
                    // 自动搜索顺序：Template -> Document
                    if (templateDoc.TemplateResFiles != null && templateDoc.TemplateResFiles.TryGetValue(fullResFilePath, out byte[] templateFileContent))
                    {
                        return templateFileContent;
                    }
                    // 构建完整的文件路径
                    fullResFilePath = ST_Loc.GetAbsolutePath(filePath, ResDirectoryPath).Path;

                    // 延迟加载：先检查缓存
                    if (ResFiles != null && ResFiles.TryGetValue(fullResFilePath, out byte[] cachedContent))
                    {
                        return cachedContent;
                    }

                    // 缓存未命中，从归档文件加载
                    byte[] fileContent = LoadResourceFileFromArchive(fullResFilePath);

                    // 缓存结果
                    if (fileContent != null)
                    {
                        lock (_cacheLock)
                        {
                            if (ResFiles == null)
                            {
                                ResFiles = new Dictionary<string, byte[]>();
                            }

                            if (!ResFiles.ContainsKey(filePath))
                            {
                                ResFiles[filePath] = fileContent;
                            }
                        }
                    }

                    return fileContent;
            }
        }

        /// <summary>
        /// 获取指定索引的模版页对象
        /// </summary>
        /// <param name="templateIndex">模版页索引</param>
        /// <returns>模版页对象，如果未找到返回null</returns>
        public Models.BaseStructure.Pages.Page GetTemplatePage(uint templateId)
        {
            return TemplateDocs?.FirstOrDefault(t => t.TemplateId == templateId)?.TemplatePage;
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 从归档文件加载资源文件内容
        /// 延迟加载的核心方法：使用时从归档读取，不使用时不加载
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>资源文件内容，如果未找到返回null</returns>
        private byte[] LoadResourceFileFromArchive(string fullFilePath)
        {
            // 参数验证
            if (string.IsNullOrEmpty(fullFilePath))
                return null;
            
            // 检查归档引用是否存在
            if (ResourceArchive == null)
                return null;
            
            
            // 检查文件是否存在于归档中
            if (!ResourceArchive.FileExists(fullFilePath))
            {
                // 尝试直接使用相对路径
                if (!ResourceArchive.FileExists(fullFilePath))
                {
                    return null;
                }
                fullFilePath = fullFilePath;
            }
            
            // 从归档文件读取内容
            try
            {
                using var stream = ResourceArchive.OpenFileStream(fullFilePath);
                using var memoryStream = new MemoryStream();
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
            catch (Exception ex)
            {
                // 记录错误日志（可以添加日志框架）
                System.Diagnostics.Debug.WriteLine($"加载资源文件失败: {fullFilePath}, 错误: {ex.Message}");
                return null;
            }
        }
        
        #endregion
    }
}

