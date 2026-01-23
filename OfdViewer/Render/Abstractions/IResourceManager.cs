namespace OFDViewer.Render.Abstractions
{
    /// <summary>
    /// 资源管理器接口
    /// 封装OFD文档资源的获取逻辑，提供统一的资源访问入口
    /// </summary>
    public interface IResourceManager
    {
        /// <summary>
        /// 泛型版本：获取指定类型的资源
        /// </summary>
        /// <typeparam name="T">资源类型（OFDFont、ColorSpace、DrawParam等）</typeparam>
        /// <param name="resourceId">资源ID</param>
        /// <returns>指定类型的资源对象，如果未找到返回default(T)</returns>
        T GetResource<T>(string resourceId);

        /// <summary>
        /// 获取资源文件内容
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>资源文件内容，如果未找到返回null</returns>
        byte[] GetResourceFile(string filePath);
    }
}
