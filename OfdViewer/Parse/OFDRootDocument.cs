using OFDViewer.Models.BaseStructure.MainEntry;
using OFDViewer.Models.BaseType;

namespace OFDViewer.Parse
{
    /// <summary>
    /// OFD根文档对象，对应整个OFD压缩包（ZIP容器）
    /// </summary>
    public class OFDRootDocument
    {
        /// <summary>
        /// OFD.xml 对应的全局元数据对象（文档根节点、版本、文档数量等核心信息）
        /// </summary>
        public RootOFD RootOfd { get; set; }

        /// <summary>
        /// 全局元数据文件路径（相对根目录，固定路径）
        /// </summary>
        public string RootOfdFile => Constants.Root_OfdFile;

        /// <summary>
        /// 当前OFD根目录路径
        /// "." 表示当前路径, ".." 表示父路径
        /// 约定:
        /// 1. "/"代表根节点;
        /// 2. 未显式指定时代表当前路径;
        /// 3. 路径区分大小写
        /// </summary>
        public string OFDRootDirectory => "/";

        /// <summary>
        /// 文档集合（对应 Doc_0、Doc_1... 子文档目录，有序存储）
        /// </summary>
        public List<OFDDocument> Docs { get; set; } = new List<OFDDocument>();

        //文档数量属性
        public int DocCount => Docs.Count;

        /// <summary>
        /// 单文档快捷访问属性（默认获取第一个文档 DocIndex=0，适配绝大多数单文档场景）
        /// 简化操作：无需通过 Docs[0] 遍历，直接访问 DefaultDoc
        /// </summary>
        public OFDDocument DefaultOFDDocument
        {
            get
            {
                // 若文档集合为空，自动创建第一个文档（DocIndex=0，容错处理）
                if (Docs == null || Docs.Count == 0)
                {
                    NewDoc();
                }
                // 返回第一个文档（DocIndex=0，默认单文档）
                return Docs[0];
            }
        }

        /// <summary>
        /// 无参构造函数，初始化全局元数据对象，并自动创建第一个默认文档（DocIndex=0）
        /// </summary>
        public OFDRootDocument()
        {
            RootOfd = new RootOFD();
            // 自动初始化第一个文档（DocIndex=0），适配单文档默认场景
            NewDoc();
        }

        /// <summary>
        /// 添加新文档到OFD根文档，并自动同步文档序号
        /// </summary>
        /// <returns>新增的子文档对象</returns>
        public OFDDocument NewDoc()
        {
            // 自动生成文档序号（从0开始，基于现有文档数量自增）
            int newDocIndex = this.Docs.Count + 1 - 1;
            var newDoc = new OFDDocument(newDocIndex);
            this.Docs.Add(newDoc);

            // 在OFD.xml中记录 文档根节点、版本、文档数量等核心信息
            var docBody = new DocBody();

            docBody.DocRoot = new ST_Loc(Constants.GetFilePath(Constants.Doc_DocumentFile, newDocIndex));
            this.RootOfd.DocBodies.Add(docBody);

            return newDoc;
        }
    }
}
