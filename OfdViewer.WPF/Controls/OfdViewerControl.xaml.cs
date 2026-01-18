using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using OFDViewer.Render;
using OFDViewer.Render.DataModels;

namespace OfdViewer.WPF.Controls
{
    /// <summary>
    /// OFD文档查看器控件
    /// 用于显示OFD文档，支持页面导航、缩放等功能
    /// </summary>
    public partial class OfdViewerControl : UserControl, INotifyPropertyChanged
    {
        #region 属性

        /// <summary>
        /// 当前页码
        /// </summary>
        private int _currentPage = 0;
        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                if (_currentPage != value)
                {
                    _currentPage = value;
                    OnPropertyChanged(nameof(CurrentPage));
                    OnPropertyChanged(nameof(PageInfo));
                    RenderCurrentPage();
                }
            }
        }

        /// <summary>
        /// 总页数
        /// </summary>
        private int _totalPages = 0;
        public int TotalPages
        {
            get => _totalPages;
            set
            {
                if (_totalPages != value)
                {
                    _totalPages = value;
                    OnPropertyChanged(nameof(TotalPages));
                    OnPropertyChanged(nameof(PageInfo));
                }
            }
        }

        /// <summary>
        /// 页面信息文本
        /// </summary>
        public string PageInfo => $"第 {CurrentPage + 1} 页 / 共 {TotalPages} 页";

        /// <summary>
        /// 当前缩放比例
        /// </summary>
        private double _zoom = 1.0;
        public double Zoom
        {
            get => _zoom;
            set
            {
                if (_zoom != value)
                {
                    _zoom = value;
                    OfdImage.LayoutTransform = new System.Windows.Media.ScaleTransform(_zoom, _zoom);
                    OnPropertyChanged(nameof(Zoom));
                }
            }
        }

        /// <summary>
        /// OFD渲染器
        /// </summary>
        private OfdRenderer _ofdRenderer;

        /// <summary>
        /// 渲染配置
        /// </summary>
        private readonly RenderConfig _renderConfig = new RenderConfig();

        #endregion

        #region 命令

        /// <summary>
        /// 打开OFD文档命令
        /// </summary>
        public ICommand OpenCommand { get; private set; }

        /// <summary>
        /// 上一页命令
        /// </summary>
        public ICommand PreviousPageCommand { get; private set; }

        /// <summary>
        /// 下一页命令
        /// </summary>
        public ICommand NextPageCommand { get; private set; }

        /// <summary>
        /// 放大命令
        /// </summary>
        public ICommand ZoomInCommand { get; private set; }

        /// <summary>
        /// 缩小命令
        /// </summary>
        public ICommand ZoomOutCommand { get; private set; }

        /// <summary>
        /// 适应窗口命令
        /// </summary>
        public ICommand FitToWindowCommand { get; private set; }

        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        public OfdViewerControl()
        {
            InitializeComponent();
            DataContext = this;
            InitializeCommands();
        }

        #endregion

        #region 初始化方法

        /// <summary>
        /// 初始化命令
        /// </summary>
        private void InitializeCommands()
        {
            OpenCommand = new RelayCommand(OpenDocument);
            PreviousPageCommand = new RelayCommand(PreviousPage, CanGoPrevious);
            NextPageCommand = new RelayCommand(NextPage, CanGoNext);
            ZoomInCommand = new RelayCommand(ZoomIn);
            ZoomOutCommand = new RelayCommand(ZoomOut);
            FitToWindowCommand = new RelayCommand(FitToWindow);
        }

        #endregion

        #region 文档操作方法

        /// <summary>
        /// 打开OFD文档
        /// </summary>
        public void OpenDocument(string filePath)
        {
            try
            {
                // 释放之前的渲染器
                _ofdRenderer?.Dispose();
                
                // 创建新的渲染器
                _ofdRenderer = new OfdRenderer(filePath, _renderConfig);
                
                // 更新页面信息
                TotalPages = _ofdRenderer.PageCount;
                CurrentPage = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开OFD文档失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 打开OFD文档（通过文件对话框）
        /// </summary>
        private void OpenDocument()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "OFD文档|*.ofd",
                Title = "打开OFD文档"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                OpenDocument(openFileDialog.FileName);
            }
        }

        /// <summary>
        /// 渲染当前页面
        /// </summary>
        private void RenderCurrentPage()
        {
            if (_ofdRenderer == null || TotalPages == 0)
                return;

            try
            {
                // 渲染当前页面
                byte[] imageData = _ofdRenderer.RenderPageToBitmap(CurrentPage);
                
                // 将字节数组转换为BitmapImage
                using (var stream = new MemoryStream(imageData))
                {
                    var bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.StreamSource = stream;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();
                    
                    // 更新图像
                    OfdImage.Source = bitmapImage;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"渲染页面失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region 页面导航方法

        /// <summary>
        /// 上一页
        /// </summary>
        private void PreviousPage()
        {
            if (CanGoPrevious())
            {
                CurrentPage--;
            }
        }

        /// <summary>
        /// 是否可以上一页
        /// </summary>
        /// <returns>是否可以上一页</returns>
        private bool CanGoPrevious()
        {
            return CurrentPage > 0;
        }

        /// <summary>
        /// 下一页
        /// </summary>
        private void NextPage()
        {
            if (CanGoNext())
            {
                CurrentPage++;
            }
        }

        /// <summary>
        /// 是否可以下一页
        /// </summary>
        /// <returns>是否可以下一页</returns>
        private bool CanGoNext()
        {
            return CurrentPage < TotalPages - 1;
        }

        #endregion

        #region 缩放方法

        /// <summary>
        /// 放大
        /// </summary>
        private void ZoomIn()
        {
            Zoom += 0.1;
        }

        /// <summary>
        /// 缩小
        /// </summary>
        private void ZoomOut()
        {
            if (Zoom > 0.1)
            {
                Zoom -= 0.1;
            }
        }

        /// <summary>
        /// 适应窗口
        /// </summary>
        private void FitToWindow()
        {
            Zoom = 1.0;
        }

        #endregion

        #region INotifyPropertyChanged实现

        /// <summary>
        /// 属性变更事件
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 触发属性变更事件
        /// </summary>
        /// <param name="propertyName">属性名称</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    #region 命令辅助类

    /// <summary>
    /// 简单的命令实现
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="execute">执行方法</param>
        public RelayCommand(Action execute) : this(execute, null)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="execute">执行方法</param>
        /// <param name="canExecute">是否可执行判断方法</param>
        public RelayCommand(Action execute, Func<bool> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// 是否可执行变更事件
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// 判断是否可执行
        /// </summary>
        /// <param name="parameter">命令参数</param>
        /// <returns>是否可执行</returns>
        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute();
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="parameter">命令参数</param>
        public void Execute(object parameter)
        {
            _execute();
        }
    }

    #endregion
}
