using OFDViewer.Parse;
using OFDViewer.Render.Abstractions;

namespace OFDViewer.Render.Implementation
{
    /// <summary>
    /// 资源管理器实现类
    /// 封装OFD文档资源的获取逻辑
    /// </summary>
    public class ResourceManager : IResourceManager
    {
        private readonly OFDDocument _document;
        private readonly int _pageIndex;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="document">OFD文档对象</param>
        /// <param name="pageIndex">页面索引</param>
        public ResourceManager(OFDDocument document, int pageIndex)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _pageIndex = pageIndex;
        }

        /// <summary>
        /// 泛型版本：获取指定类型的资源
        /// </summary>
        /// <typeparam name="T">资源类型（OFDFont、ColorSpace、DrawParam等）</typeparam>
        /// <param name="resourceId">资源ID</param>
        /// <returns>指定类型的资源对象，如果未找到返回default(T)</returns>
        public T GetResource<T>(string resourceId)
        {
            return _document.GetResource<T>(_pageIndex, resourceId);
        }

        /// <summary>
        /// 泛型版本：获取指定类型的模版资源
        /// </summary>
        /// <typeparam name="T">资源类型（OFDFont、ColorSpace、DrawParam等）</typeparam>
        /// <param name="resourceId">资源ID</param>
        /// <returns>指定类型的资源对象，如果未找到返回default(T)</returns>
        public T GetTemplateResource<T>(string resourceId)
        {
            // 由于模版页可能有多个，这里需要遍历所有模版页
            // 但目前我们没有模版页索引，所以暂时返回null
            // 后续可以根据需要修改为遍历所有模版页
            return default(T);
        }

        /// <summary>
        /// 获取资源文件内容
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>资源文件内容，如果未找到返回null</returns>
        public byte[] GetResourceFile(string filePath)
        {
            return _document.GetResourceFile(_pageIndex, filePath);
        }

        /// <summary>
        /// 获取指定索引的模版页对象
        /// </summary>
        /// <param name="templateIndex">模版页索引</param>
        /// <returns>模版页对象，如果未找到返回null</returns>
        public object GetTemplatePage(uint templateId)
        {
            return _document.GetTemplatePage(templateId);
        }
    }
}
