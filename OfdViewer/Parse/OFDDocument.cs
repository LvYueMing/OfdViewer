using OFDViewer.Models.BaseStructure.DocumentRoot;
using OFDViewer.Models.BaseStructure.Resources;
using OFDViewer.Models.BaseStructure.Resources.ResItems;
using OFDViewer.Models.BaseType;
using OFDViewer.Models.Signature;

namespace OFDViewer.Parse
{
    /// <summary>
    /// OFD 文档核心类，对应 OFD 标准中的 Doc_N 目录
    /// </summary>
    /// <remarks>
    /// 负责管理单个 OFD 文档的所有内容，包括：
    /// 1. 文档主描述信息（Document.xml）
    /// 2. 公共资源和文档资源
    /// 3. 页面集合
    /// 4. 签章集合
    /// 5. 文档级资源文件
    /// </remarks>
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
        /// 文档级资源文件集合（存储Res目录下的字体、图片等资源）
        /// </summary>
        public Dictionary<string, byte[]> ResFiles { get; set; }


        private string _documentFilePath;
        /// <summary>
        /// 文档主描述文件绝对路径
        /// 默认 Doc_{0}/Document.xml
        /// </summary>
        public string DocumentFilePath
        {
            get => _documentFilePath ?? 
                (string.IsNullOrEmpty(DocDirectoryPath)
                ? Constants.GetFilePath(Constants.Doc_DocumentFile, DocIndex)
                : Path.Combine(DocDirectoryPath, "DocumentRes.xml"));
            set => _documentFilePath = value;
        }

        private string _publicResourceFilePath;
        /// <summary>
        /// 文档公共资源描述文件绝对路径（Doc_{0}/PublicRes.xml）
        /// </summary>
        public string PublicResourceFilePath
        {
            get => _publicResourceFilePath ?? 
                (string.IsNullOrEmpty(DocDirectoryPath) 
                ? Constants.GetFilePath(Constants.Doc_PublicResFile, DocIndex)
                : Path.Combine(DocDirectoryPath, "PublicRes.xml"));
            set => _publicResourceFilePath = value;
        }

        private string _documentResourceFilePath;
        /// <summary>
        /// 文档私有资源描述文件路径（Doc_{0}/DocumentRes.xml）
        /// </summary>
        public string DocumentResourceFilePath
        {
            get => _documentResourceFilePath ?? 
                (string.IsNullOrEmpty(DocDirectoryPath) 
                ? Constants.GetFilePath(Constants.Doc_DocumentResFile, DocIndex)
                : Path.Combine(DocDirectoryPath, "DocumentRes.xml"));
            set => _documentResourceFilePath = value;
        }


        private string _signsFilePath;
        /// <summary>
        /// 获取签章描述文件 
        /// Doc_0/Signs/Signatures.xml 
        /// </summary>
        public string SignsFilePath
        {
            get => _signsFilePath ?? (string.IsNullOrEmpty(DocDirectoryPath)
                ? Constants.GetFilePath(Constants.Signs_SignaturesFile, DocIndex)
                : Path.Combine(SignsDirectoryPath, "Signatures.xml"));
            set => _signsFilePath = value;
        }

        /// <summary>
        /// 签章对象集合目录(Doc_{0}/Signs)
        /// </summary>
        public string SignsDirectoryPath => string.IsNullOrEmpty(DocDirectoryPath) 
            ? Constants.GetFilePath(Constants.Signs_BaseDirectory, DocIndex) 
            : Path.Combine(DocDirectoryPath, "Signs");

        /// <summary>
        /// 文档级资源目录路径（Doc_{0}/Res）
        /// </summary>
        public string ResDirectoryPath => string.IsNullOrEmpty(DocDirectoryPath) 
            ? Constants.GetFilePath(Constants.Doc_ResDirectory, DocIndex)
            : Path.Combine(DocDirectoryPath, "Res");


        /// <summary>
        /// 文档级资源目录路径（Doc_{0}/Pages）
        /// </summary>
        public string PagesDirectoryPath => string.IsNullOrEmpty(DocDirectoryPath)
            ? Constants.GetFilePath(Constants.Pages_BaseDirectory, DocIndex)
            : Path.Combine(DocDirectoryPath, "Pages");


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
            //todo： 从文档路径 Doc_{0} 获取文档序号 0
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

        /// <summary>
        /// 获取指定资源
        /// </summary>
        /// <param name="pageIndex">页面索引</param>
        /// <param name="resourceId">资源ID</param>
        /// <param name="resourceType">资源类型</param>
        /// <param name="location">获取位置</param>
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
                    return fontsCollection?.ofdFonts.FirstOrDefault(f => f.ID == resourceId);
                case ResourceType.ColorSpace:
                    // 查找ColorSpaces资源集合
                    var colorSpacesCollection = res.ResItems.OfType<ColorSpaces>().FirstOrDefault();
                    return colorSpacesCollection?.colorSpaces?.FirstOrDefault(c => c.ID == resourceId);
                case ResourceType.DrawParam:
                    // 查找DrawParams资源集合
                    var drawParamsCollection = res.ResItems.OfType<DrawParams>().FirstOrDefault();
                    return drawParamsCollection?.drawParams?.FirstOrDefault(d => d.ID == resourceId);
                case ResourceType.VectorGraphic:
                    // 查找CompositeGraphicUnits资源集合（包含矢量图像）
                    var compositeGraphicsCollection = res.ResItems.OfType<CompositeGraphicUnits>().FirstOrDefault();
                    return compositeGraphicsCollection?.compositeGraphicUnits?.FirstOrDefault(v => v.ID == resourceId);
                case ResourceType.Multimedia:
                    // 查找MultiMedias资源集合
                    var multiMediasCollection = res.ResItems.OfType<MultiMedias>().FirstOrDefault();
                    return multiMediasCollection?.multiMedias?.FirstOrDefault(m => m.ID == resourceId);
                case ResourceType.All:
                default:
                    // 搜索所有类型
                    // 1. 查找Fonts
                    var allFontsCollection = res.ResItems.OfType<OFDFonts>().FirstOrDefault();
                    var font = allFontsCollection?.ofdFonts.FirstOrDefault(f => f.ID == resourceId);
                    if (font != null) return font;

                    // 2. 查找ColorSpaces
                    var allColorSpacesCollection = res.ResItems.OfType<ColorSpaces>().FirstOrDefault();
                    var colorSpace = allColorSpacesCollection?.colorSpaces?.FirstOrDefault(c => c.ID == resourceId);
                    if (colorSpace != null) return colorSpace;

                    // 3. 查找DrawParams
                    var allDrawParamsCollection = res.ResItems.OfType<DrawParams>().FirstOrDefault();
                    var drawParam = allDrawParamsCollection?.drawParams?.FirstOrDefault(d => d.ID == resourceId);
                    if (drawParam != null) return drawParam;

                    // 4. 查找CompositeGraphicUnits（矢量图像）
                    var allCompositeGraphicsCollection = res.ResItems.OfType<CompositeGraphicUnits>().FirstOrDefault();
                    var vectorGraphic = allCompositeGraphicsCollection?.compositeGraphicUnits?.FirstOrDefault(v => v.ID == resourceId);
                    if (vectorGraphic != null) return vectorGraphic;

                    // 5. 查找MultiMedias
                    var allMultiMediasCollection = res.ResItems.OfType<MultiMedias>().FirstOrDefault();
                    var multimedia = allMultiMediasCollection?.multiMedias?.FirstOrDefault(m => m.ID == resourceId);
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

            // 按照指定位置或自动搜索顺序查找资源文件
            switch (location)
            {
                case ResourceLocation.Page:
                    if (pageDoc.PageResFileContents != null && pageDoc.PageResFileContents.TryGetValue(filePath, out byte[] pageContent))
                    {
                        return pageContent;
                    }
                    return null;
                case ResourceLocation.Document:
                case ResourceLocation.Public:
                case ResourceLocation.Auto:
                default:
                    // 自动搜索顺序：Page -> Document
                    if (pageDoc.PageResFileContents != null && pageDoc.PageResFileContents.TryGetValue(filePath, out byte[] pageFileContent))
                    {
                        return pageFileContent;
                    }
                    if (ResFiles != null && ResFiles.TryGetValue(filePath, out byte[] docContent))
                    {
                        return docContent;
                    }
                    return null;
            }
        }
        #endregion

    }
}

