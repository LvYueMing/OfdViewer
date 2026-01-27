using System.Text.RegularExpressions;
using OFDViewer.Models.BaseStructure.Pages;
using OFDViewer.Models.BaseStructure.Resources;

namespace OFDViewer.Parse
{
    /// <summary>
    /// OFD模板页对象，对应 Tpl_N 目录（N可从0开始）
    /// </summary>
    public class TemplateDocument
    {
        /// <summary>
        /// 模板页序号（从0开始，自动计算）
        /// </summary>
        public int TemplateIndex { get; internal set; }

        /// <summary>
        /// 所属文档序号（从0开始）
        /// </summary>
        public int BelongDocIndex { get; internal set; }

        /// <summary>
        /// 所属文档路径（当使用路径构造函数时赋值）
        /// </summary>
        public string BelongDocPath { get; internal set; }

        private Page _templatePage;
        /// <summary>
        /// 模板页内容描述文件（Doc_{0}/Templates/Tpl_{1}/Content.xml）
        /// 记录模板页中的文字、图形、图片的坐标、样式、层级等
        /// 注:在模板页的内容描述中 Template 属性无效
        /// </summary>
        public Page TemplatePage
        {
            get => _templatePage;
            set
            {
                _templatePage = value;
                // 确保模板页中的Template属性无效
                if (_templatePage != null)
                {
                    _templatePage.Template = null;
                }
            }
        }

        /// <summary>
        /// 模板页资源映射文件（Doc_{0}/Tpls/Tpl_{1}/TemplateRes.xml）
        /// 定义当前模板页专属资源的引用关系
        /// </summary>
        public Res TemplateRes { get; set; }

        /// <summary>
        /// 模板页私有资源路径集合（Doc_{0}/Tpls/Tpl_{1}/Res）
        /// 如局部插图、水印等
        /// </summary>
        public Dictionary<string, byte[]> TemplateResFiles { get; set; }

        private string _templateFilePath;
        /// <summary>
        /// 模板页文件绝对路径
        /// </summary>
        public string TemplateFilePath
        {
            get
            {
                if (string.IsNullOrEmpty(_templateFilePath))
                {
                    _templateFilePath = Constants.GetFilePath(Constants.Template_ContentFile, BelongDocIndex, TemplateIndex);
                }
                return _templateFilePath;
            }
            set
            {
                _templateFilePath = value;
                TemplateIndex = int.Parse(Regex.Match(value, @"Tpl_(\d+)").Groups[1].Value);
            }
        }

        /// <summary>
        /// 模板页目录路径（相对根目录，格式：Doc_{BelongDocIndex}/Templates/Tpl_{TemplateIndex}）
        /// </summary>
        public string TemplateDirectoryPath => string.IsNullOrEmpty(TemplateFilePath)
            ? $"Doc_{BelongDocIndex}/Templates/Tpl_{TemplateIndex}"
            : Path.GetDirectoryName(TemplateFilePath);

        private string _templateResFilePath;
        /// <summary>
        /// 模板页资源映射文件绝对路径
        /// </summary>
        public string TemplateResFilePath
        {
            get
            {
                if (string.IsNullOrEmpty(_templateResFilePath))
                {
                    _templateResFilePath = Constants.GetFilePath(Constants.Template_TemplateResFile, BelongDocIndex, TemplateIndex);
                }
                return _templateResFilePath;
            }
            set => _templateResFilePath = value;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public TemplateDocument()
        {
            TemplateIndex = 0;
            BelongDocIndex = 0;
            TemplatePage = new Page();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="belongDocPath">所属文档路径</param>
        public TemplateDocument(string belongDocPath)
        {
            if (string.IsNullOrEmpty(belongDocPath))
            {
                throw new ArgumentNullException(nameof(belongDocPath), "所属文档路径不能为空");
            }
            BelongDocPath = belongDocPath;
            BelongDocIndex = int.Parse(Regex.Match(belongDocPath, @"Doc_(\d+)").Groups[1].Value);
            TemplatePage = new Page();
        }
    }
}
