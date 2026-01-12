using System.Xml.Serialization;
using OFDViewer.Models.BaseStructure.Resources.ResItems;
using OFDViewer.Models.BaseType;
using OFDViewer.Utils;

namespace OFDViewer.Models.BaseStructure.Resources
{
    /// <summary>
    /// 资源是绘制图元时所需数据(如绘制参数、颜色空间、字型、图像、音视频等)的集合
    /// </summary>
    public class Res
    {
        private List<BaseRes> _resItems;
        
        /// <summary>
        /// 混合路径节点集合：匹配 xs:choice minOccurs="0" maxOccurs="unbounded"
        /// </summary>
        [XmlElement("ColorSpaces", typeof(ColorSpaces))]
        [XmlElement("DrawParams", typeof(DrawParams))]
        [XmlElement("Fonts", typeof(OFDFonts))]
        [XmlElement("MultiMedias", typeof(MultiMedias))]
        [XmlElement("CompositeGraphicUnits", typeof(CompositeGraphicUnits))]
        public List<BaseRes> ResItems
        {
            get { return _resItems; }
            set { _resItems = value; }
        }

        /// <summary>
        /// 定义此资源文件的通用数据存储路径,BaseLoc属性的意义在于明确资源文件存储的位置,比如 R1.xml中可以指定 BaseLoc
        /// 为“./Res”, 表明该资源文件中所有数据文件的默认存储位置在当前路径的 Res 目录下
        /// 必选
        /// </summary>
        [XmlAttribute("BaseLoc")]
        [XmlRequired(ErrorMsg = "BaseLoc 属性为必选项，且不能为空")]
        public string BaseLocString
        {
            get { return BaseLoc.ToString(); }
            set { BaseLoc = value; }
        }
        [XmlIgnore]
        public ST_Loc BaseLoc { get; set; }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public Res()
        {
            _resItems = new List<BaseRes>();
        }
        
        /// <summary>
        /// 添加资源对象到ResItems集合
        /// </summary>
        /// <param name="resource">要添加的资源对象</param>
        public void AddResource(BaseRes resource)
        {
            if (resource == null)
                throw new ArgumentNullException(nameof(resource));
            
            // 根据资源类型设置资源文件路径
            SetResourceFilePaths(resource);
            
            // 添加到集合
            _resItems.Add(resource);
        }
        
        /// <summary>
        /// 批量添加资源对象到ResItems集合
        /// </summary>
        /// <param name="resources">要添加的资源对象集合</param>
        public void AddResources(IEnumerable<BaseRes> resources)
        {
            if (resources == null)
                throw new ArgumentNullException(nameof(resources));
            
            foreach (var resource in resources)
            {
                AddResource(resource);
            }
        }
        
        /// <summary>
        /// 根据资源类型设置资源文件路径
        /// </summary>
        /// <param name="resource">要设置路径的资源对象</param>
        private void SetResourceFilePaths(BaseRes resource)
        {
            // 检查资源类型并设置相应的资源文件路径
            if (resource is OFDFonts fonts)
            {
                SetFontFilePaths(fonts);
            }
            else if (resource is MultiMedias medias)
            {
                SetMediaFilePaths(medias);
            }
            // 可以添加更多资源类型的处理
        }
        
        /// <summary>
        /// 设置字体资源的文件路径
        /// </summary>
        /// <param name="fonts">字体资源</param>
        private void SetFontFilePaths(OFDFonts fonts)
        {
            if (fonts?.ofdFonts == null || BaseLoc == null) return;
            
            foreach (var font in fonts.ofdFonts)
            {
                // 设置字体文件路径
                if (font.FontFile != null)
                {
                    // 将font.FontFile与BaseLoc拼接
                    font.FontFile = ST_Loc.Resolve(font.FontFile, BaseLoc);
                }
            }
        }
        
        /// <summary>
        /// 设置多媒体资源的文件路径
        /// </summary>
        /// <param name="medias">多媒体资源</param>
        private void SetMediaFilePaths(MultiMedias medias)
        {
            if (medias?.multiMedias == null || BaseLoc == null) return;
            
            foreach (var media in medias.multiMedias)
            {
                // 设置多媒体文件路径
                if (media.MediaFile != null)
                {
                    // 将media.MediaFile与BaseLoc拼接
                    media.MediaFile = ST_Loc.Resolve(media.MediaFile, BaseLoc);
                }
            }
        }
    }
}
