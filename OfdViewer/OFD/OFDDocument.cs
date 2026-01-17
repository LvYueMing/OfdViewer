using OFDViewer.Models.BaseStructure.DocumentRoot;
using OFDViewer.Models.BaseStructure.Resources;
using OFDViewer.Models.BaseType;
using OFDViewer.Models.Signature;

namespace OFDViewer.OFD
{
    /// <summary>
    /// OFD子文档对象，对应 Doc_N 目录
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

        private Res _publicResource;
        /// <summary>
        /// 全文档公共资源描述文件（PublicRes.xml）
        /// </summary>
        public Res PublicResource
        {
            get => _publicResource;
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
                publicResource.BaseLocString = ST_Loc.GetRelativePath(ResDirectory, DocBaseDirectory).Path;
            }
            _publicResource = publicResource;

            // 更新Document对象中的公共资源路径
            if (Document != null && Document.CommonData != null && publicResource != null)
            {
                var publicResFileName = ST_Loc.GetRelativePath(PublicResourceFile, DocBaseDirectory);
                
                // 确保PublicRes集合已初始化
                if (Document.CommonData.PublicRes == null)
                {
                    Document.CommonData.PublicRes = new List<ST_Loc>();
                }
                
                // 移除旧的路径
                Document.CommonData.PublicRes = Document.CommonData.PublicRes
                    ?.Where(path => !path.ToString().EndsWith(publicResFileName.Path))
                    ?.ToList() ?? new List<ST_Loc>();
                
                // 添加新的路径（使用相对当前文档目录的路径）
                Document.CommonData.PublicRes.Add(publicResFileName);
            }
        }

        private Res _documentResource;

        /// <summary>
        /// 当前文档的资源描述文件（DocumentRes.xml）
        /// </summary>
        public Res DocumentResource
        {
            get => _documentResource;
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
                documentResource.BaseLocString = ST_Loc.GetRelativePath(ResDirectory, DocBaseDirectory).Path;
            }
            _documentResource = documentResource;
            // 更新Document对象中的文档资源路径
            if (Document != null && Document.CommonData != null && documentResource != null)
            {
                var documentResFileName = ST_Loc.GetRelativePath(DocumentResourceFile, DocBaseDirectory);
                
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

        /// <summary>
        /// 签章列表索引对象（对应Signatures.xml，记录所有签章信息）
        /// </summary>
        public Signatures Signatures { get; set; }

        /// <summary>
        /// 签章对象集合（对应Sign_N目录，一个文档可包含多个签章）
        /// </summary>
        public List<SignDocument> SignDocs { get; set; }

        /// <summary>
        /// 页面对象集合（对应Page_N目录，存储文档所有页面）
        /// </summary>
        public List<PageDocument> PageDocs { get; set; }

        /// <summary>
        /// 文档级资源文件集合（存储Res目录下的字体、图片等资源）
        /// </summary>
        public Dictionary<string, byte[]> ResFiles { get; set; }


        /// <summary>
        /// 当前文档路径 (Doc_{0})
        /// </summary>
        public string DocBaseDirectory => Constants.GetFilePath(Constants.Doc_BaseDirectory, DocIndex);

        /// <summary>
        /// 文档主描述文件路径（Doc_{0}/Document.xml）
        /// </summary>
        public string DocumentFile => Constants.GetFilePath(Constants.Doc_DocumentFile, DocIndex);

        /// <summary>
        /// 文档公共资源描述文件路径（Doc_{0}/PublicRes.xml）
        /// </summary>
        public string PublicResourceFile => Constants.GetFilePath(Constants.Doc_PublicResFile, DocIndex);

        /// <summary>
        /// 文档私有资源描述文件路径（Doc_{0}/DocumentRes.xml）
        /// </summary>
        public string DocumentResourceFile => Constants.GetFilePath(Constants.Doc_DocumentResFile, DocIndex);

        /// <summary>
        /// 文档级资源目录路径（Doc_{0}/Res）
        /// </summary>
        public string ResDirectory => Constants.GetFilePath(Constants.Doc_ResDirectory, DocIndex);

        /// <summary>
        /// 页面对象集合目录（Doc_{0}/Pages）
        /// </summary>
        public string PagesDirectory => Constants.GetFilePath(Constants.Pages_BaseDirectory, DocIndex);

        /// <summary>
        /// 签章对象集合目录(Doc_{0}/Signs)
        /// </summary>
        public string SignsDirectory => Constants.GetFilePath(Constants.Signs_BaseDirectory, DocIndex);

        //无参构造函数
        public OFDDocument() : this(0)
        {

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
        /// 添加页面对象
        /// </summary>
        public void AddPageDoc()
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
        /// 添加页面对象
        /// </summary>
        /// <param name="pageDoc"></param>
        public void AddPageDoc(PageDocument pageDoc)
        {
            // 计算新的页面序号（当前页面数量，从0开始）
            int newPageIndex = PageDocs.Count;

            // 设置页面序号
            pageDoc.BelongDocIndex = DocIndex;
            pageDoc.PageIndex = newPageIndex;


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


    }
}
