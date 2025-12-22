using OFDViewer.BasicStructure.MainEntry;
using OFDViewer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace OFDViewer.OFDReader
{
    public class OfdReader : IDisposable
    {
        private readonly OfdArchive _archive;
        private bool _disposed = false;

        /// <summary>
        /// OFD 文档信息
        /// </summary>
        public OFD OfdDocument { get; private set; }

        /// <summary>
        /// 初始化 OFD 读取器
        /// </summary>
        /// <param name="filePath">OFD 文件路径</param>
        /// <exception cref="FileNotFoundException">文件不存在时抛出</exception>
        /// <exception cref="ArgumentNullException">文件路径为空时抛出</exception>
        public OfdReader(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath), "文件路径不能为空");

            if (!File.Exists(filePath))
                throw new FileNotFoundException("OFD 文件不存在", filePath);

            _archive = OfdArchive.OpenFromFile(filePath);
        }

        /// <summary>
        /// 初始化 OFD 读取器
        /// </summary>
        /// <param name="stream">OFD 文件流</param>
        /// <param name="leaveOpen">是否保持流打开状态</param>
        public OfdReader(Stream stream, bool leaveOpen)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream), "输入流不能为空");

            _archive = OfdArchive.OpenFromStream(stream, leaveOpen: leaveOpen);
        }

        /// <summary>
        /// 解析 OFD 主入口文件
        /// </summary>
        public OFD ParseOfdDocument()
        {
            if (OfdDocument != null)
                return OfdDocument;

            try
            {
                // 读取 OFD.xml 入口文件
                using var ofdStream = _archive.GetFileStream("OFD.xml");

                // 验证文件签名（可选）
                //ValidateOFDSignature(ofdStream);

                // 解析主文档
                OfdDocument = XmlHelper.DeserializeFromStream<OFD>(ofdStream);

                // 加载相关资源
                //LoadRelatedResources();

                return OfdDocument;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("解析 OFD 文档失败", ex);
            }
        }



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
        #endregion
    }
}
