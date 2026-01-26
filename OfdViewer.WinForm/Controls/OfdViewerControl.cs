using Microsoft.VisualBasic.ApplicationServices;
using OFDViewer.Render;
using OFDViewer.Render.DataModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OfdViewer.WinForm.Controls
{
    /// <summary>
    /// OFD文档查看器控件
    /// 用于显示OFD文档，支持页面导航、缩放等功能
    /// </summary>
    public partial class OfdViewerControl : UserControl, INotifyPropertyChanged
    {
        #region 常量定义

        /// <summary>
        /// A4纸张宽度（毫米）
        /// </summary>
        private const float A4_WIDTH_MM = 210;

        /// <summary>
        /// A4纸张高度（毫米）
        /// </summary>
        private const float A4_HEIGHT_MM = 297;

        /// <summary>
        /// 英寸到毫米的转换因子
        /// </summary>
        private const float INCH_TO_MM = 25.4f;

        /// <summary>
        /// 基础页面间距（像素，100%缩放时）
        /// </summary>
        private const int BasePageSpacing = 10;

        #endregion

        #region 计算属性

        /// <summary>
        /// 当前缩放比例下的页面间距（像素）
        /// </summary>
        private int PageSpacing
        {
            get
            {
                // 根据缩放比例计算页面间距，确保至少为1像素
                return Math.Max(1, (int)(BasePageSpacing * Zoom));
            }
        }

        #endregion

        #region 缓存字段

        /// <summary>
        /// 缓存的A4宽度（像素）
        /// </summary>
        private int _cachedA4Width = 0;

        /// <summary>
        /// 缓存的A4高度（像素）
        /// </summary>
        private int _cachedA4Height = 0;

        /// <summary>
        /// 缓存的DPI值，用于检测DPI变化
        /// </summary>
        private float _cachedDpi = 0;

        #endregion

        #region 辅助方法

        /// <summary>
        /// 获取A4尺寸的像素值
        /// </summary>
        /// <param name="width">输出参数，A4宽度（像素）</param>
        /// <param name="height">输出参数，A4高度（像素）</param>
        private void GetA4PixelSize(out int width, out int height)
        {
            // 检查缓存是否有效（DPI是否发生变化）
            if (_cachedDpi != _renderConfig.Dpi)
            {
                // 计算毫米到像素的转换因子
                float mmToPixel = _renderConfig.Dpi / INCH_TO_MM;
                
                // 计算A4尺寸的像素值
                _cachedA4Width = (int)(A4_WIDTH_MM * mmToPixel);
                _cachedA4Height = (int)(A4_HEIGHT_MM * mmToPixel);
                
                // 更新缓存的DPI值
                _cachedDpi = _renderConfig.Dpi;
            }
            
            // 返回缓存的像素值
            width = _cachedA4Width;
            height = _cachedA4Height;
        }

        #endregion

        #region 属性

        /// <summary>
        /// 当前页码
        /// </summary>
        private int _currentPage = -1;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                if (_currentPage != value)
                {
                    _currentPage = value;
                    //OnPropertyChanged(nameof(CurrentPage));
                    OnPropertyChanged(nameof(PageInfo));

                    // 确保页面偏移量已更新
                    UpdatePageOffsets();

                    // 滚动到当前页面
                    if (_pictureBoxPanel != null && _pageOffsets.TryGetValue(_currentPage, out int offsetHeight))
                    {
                        // 设置滚动位置
                        var scrollY = offsetHeight - PageSpacing; // 减去上方间距
                        _pictureBoxPanel.AutoScrollPosition = new Point(0, scrollY);
                    }

                    UpdateNavigationButtons();
                }
            }
        }

        /// <summary>
        /// 总页数
        /// </summary>
        private int _totalPages = 0;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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
                    UpdateNavigationButtons();
                }
            }
        }

        /// <summary>
        /// 页面信息文本
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string PageInfo => $"第 {CurrentPage + 1} 页 / 共 {TotalPages} 页";        

        /// <summary>
        /// 缩放比例文本
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ZoomInfo => $"缩放: {Math.Round(Zoom * 100)}%";

        /// <summary>
        /// 当前缩放比例
        /// </summary>
        private double _zoom = 1.0;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public double Zoom
        {
            get => _zoom;
            set
            {
                // 限制最小缩放比例为25%
                value = Math.Max(value, 0.25);
                
                if (_zoom != value)
                {
                    _zoom = value;
                    
                    // 触发属性变更事件
                    OnPropertyChanged(nameof(Zoom));
                    OnPropertyChanged(nameof(ZoomInfo));
                    
                    // 触发重绘，更新所有页面的缩放
                    _pictureBoxPanel?.Invalidate();
                }
            }
        }

        /// <summary>
        /// OFD渲染器
        /// </summary>
        private OfdRenderer? _ofdRenderer;

        /// <summary>
        /// 渲染配置
        /// </summary>
        private readonly RenderConfig _renderConfig = new RenderConfig();

        /// <summary>
        /// 已渲染的页面缓存
        /// </summary>
        private readonly Dictionary<int, Bitmap> _renderedPages = new Dictionary<int, Bitmap>();

        /// <summary>
        /// PictureBox 对象池
        /// </summary>
        private readonly Queue<PictureBox> _pictureBoxPool = new Queue<PictureBox>();
        private readonly object _poolLock = new object();
        private const int MaxPoolSize = 10;

        /// <summary>
        /// 页面偏移量（用于滚动）
        /// </summary>
        private readonly Dictionary<int, int> _pageOffsets = new Dictionary<int, int>();

        /// <summary>
        /// 累计高度（用于滚动）
        /// </summary>
        private int _accumulatedHeight = 0;

        /// <summary>
        /// 用于优化Panel_Paint的标记
        /// </summary>
        private int _lastScrollPosition = -1;
        private int _lastFirstPage = -1;
        private int _lastLastPage = -1;
        private double _lastZoom = 1.0;


        /// <summary>
        /// 待渲染的页面队列
        /// </summary>
        private readonly Queue<int> _pendingRenderQueue = new Queue<int>();
        private bool _isRendering = false;
        private readonly object _renderLock = new object();

        /// <summary>
        /// 工具栏
        /// </summary>
        private ToolStrip? _toolStrip;

        /// <summary>
        /// 工具栏是否可见
        /// </summary>
        private bool _toolStripVisible = true;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ToolStripVisible
        {
            get => _toolStripVisible;
            set
            {
                if (_toolStripVisible != value)
                {
                    _toolStripVisible = value;
                    OnPropertyChanged(nameof(ToolStripVisible));
                    UpdateToolStripVisibility();
                }
            }
        }

        /// <summary>
        /// 图片框的容器面板
        /// </summary>
        private Panel? _pictureBoxPanel;


        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        public OfdViewerControl()
        {
            InitializeComponent();
            InitializeUI();
            this.DoubleBuffered = true; // 启用双缓冲，提高显示质量
        }

        #endregion

        #region 对象池和优化方法

        /// <summary>
        /// 从对象池获取PictureBox
        /// </summary>
        private PictureBox GetPictureBoxFromPool()
        {
            lock (_poolLock)
            {
                if (_pictureBoxPool.Count > 0)
                {
                    return _pictureBoxPool.Dequeue();
                }
            }

            // 池为空，创建新的PictureBox
            var pictureBox = new PictureBox
            {
                Visible = true,
                SizeMode = PictureBoxSizeMode.Zoom,  // 使用Zoom模式，保持图片比例并支持缩放
                BorderStyle = BorderStyle.FixedSingle
            };
            
            // 设置图片插值模式为高质量
            pictureBox.Paint += (s, e) =>
            {
                e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                e.Graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            };
            
            return pictureBox;
        }

        /// <summary>
        /// 将PictureBox归还到对象池
        /// </summary>
        private void ReturnPictureBoxToPool(PictureBox pictureBox)
        {
            if (pictureBox == null) return;

            // 清理PictureBox
            pictureBox.Image = null;
            pictureBox.Location = Point.Empty;
            pictureBox.Size = Size.Empty;
            pictureBox.Visible = true;

            lock (_poolLock)
            {
                if (_pictureBoxPool.Count < MaxPoolSize)
                {
                    _pictureBoxPool.Enqueue(pictureBox);
                }
                else
                {
                    // 池已满，清除最早进入队列的PictureBox，然后添加新的
                    var oldestPictureBox = _pictureBoxPool.Dequeue();
                    oldestPictureBox.Dispose();
                    _pictureBoxPool.Enqueue(pictureBox);
                }
            }
        }

        /// <summary>
        /// 更新页面偏移量和累计高度
        /// </summary>
        private void UpdatePageOffsets()
        {
            _pageOffsets.Clear();
            _accumulatedHeight = PageSpacing;
            for (int pageIndex = 0; pageIndex < TotalPages; pageIndex++)
            {
                _pageOffsets[pageIndex] = _accumulatedHeight;
                if (_renderedPages.ContainsKey(pageIndex))
                {
                    _accumulatedHeight += (int)(_renderedPages[pageIndex].Height * Zoom);
                }
                else
                {
                    // 使用缓存的A4尺寸像素值
                    GetA4PixelSize(out _, out int a4Height);
                    _accumulatedHeight += (int)(a4Height * Zoom);
                }

                // 添加页面间距（最后一页也添加，用于底边间距）
                _accumulatedHeight += PageSpacing;
            }
        }

        /// <summary>
        /// 请求渲染页面（异步）
        /// </summary>
        private async Task RequestRenderPageAsync(int pageIndex)
        {
            if (_renderedPages.ContainsKey(pageIndex))
            {
                return; // 已经渲染过
            }

            // 添加到队列
            lock (_renderLock)
            {
                if (!_pendingRenderQueue.Contains(pageIndex))
                {
                    _pendingRenderQueue.Enqueue(pageIndex);
                }
            }

            // 如果正在渲染，等待直到空闲
            while (_isRendering)
            {
                await Task.Delay(10);
            }

            _isRendering = true;

            try
            {
                // 渲染队列中的所有页面
                while (true)
                {
                    int pendingPageIndex;

                    lock (_renderLock)
                    {
                        if (_pendingRenderQueue.Count == 0)
                        {
                            break;
                        }
                        pendingPageIndex = _pendingRenderQueue.Dequeue();
                    }

                    // 检查是否已经渲染（可能在等待时已渲染）
                    if (_renderedPages.ContainsKey(pendingPageIndex))
                    {
                        continue;
                    }

                    // 渲染页面（异步，不阻塞UI）
                    await Task.Run(() => RenderPage(pendingPageIndex));

                    // 触发重绘
                    _pictureBoxPanel?.Invalidate();

                    // 给UI线程喘息机会
                    await Task.Delay(16); // ~60fps
                }
            }
            finally
            {
                _isRendering = false;
            }
        }

        /// <summary>
        /// 渲染页面
        /// </summary>
        private void RenderPage(int pageIndex)
        {
            if (_ofdRenderer == null) return;

            byte[] imageData = _ofdRenderer.RenderPageToBitmap(pageIndex);

            using (var stream = new MemoryStream(imageData))
            {
                var bitmap = new Bitmap(stream);

                // 检查是否已存在旧的Bitmap，如果有则释放
                if (_renderedPages.ContainsKey(pageIndex))
                {
                    _renderedPages[pageIndex]?.Dispose();
                }

                _renderedPages[pageIndex] = bitmap;
            }
        }

        #endregion

        #region 初始化方法

        /// <summary>
        /// 初始化UI
        /// </summary>
        private void InitializeUI()
        {
            // 设置控件大小
            this.Size = new Size(600, 800);

            // 初始化工具栏
            InitializeToolStrip();

            // 初始化图像显示区域
            InitializePictureBox();

            // 更新UI状态
            UpdateNavigationButtons();

        }

        /// <summary>
        /// 初始化工具栏
        /// </summary>
        private void InitializeToolStrip()
        {
            // 创建工具栏
            _toolStrip = new ToolStrip();
            _toolStrip.Dock = DockStyle.Top;
            _toolStrip.AutoSize = true;
            
            // 添加按钮
            var openButton = new ToolStripButton("打开");
            openButton.Click += OpenButton_Click;
            
            var prevButton = new ToolStripButton("上一页");
            prevButton.Click += PrevButton_Click;
            prevButton.Name = "btnPrev";
            
            var nextButton = new ToolStripButton("下一页");
            nextButton.Click += NextButton_Click;
            nextButton.Name = "btnNext";
            
            var pageInfoLabel = new ToolStripLabel("页面信息");
            pageInfoLabel.Name = "lblPageInfo";
            // pageInfoLabel.Text = PageInfo;
            // 绑定模式：双向绑定（控件值变化 → 模型属性变化；模型属性变化 → 控件值变化）
            pageInfoLabel.DataBindings.Add("Text",this, nameof(PageInfo),false,DataSourceUpdateMode.OnPropertyChanged);
            
            // 添加页码输入框
            var pageNumberTextBox = new ToolStripTextBox();
            pageNumberTextBox.Name = "txtPageNumber";
            pageNumberTextBox.Size = new Size(50, 20);
            pageNumberTextBox.KeyPress += PageNumberTextBox_KeyPress;

            var zoomInButton = new ToolStripButton("放大");
            zoomInButton.Click += ZoomInButton_Click;
            
            var zoomOutButton = new ToolStripButton("缩小");
            zoomOutButton.Click += ZoomOutButton_Click;
            
            var fitToWindowButton = new ToolStripButton("适应窗口");
            fitToWindowButton.Click += FitToWindowButton_Click;
            
            // 添加缩放比例显示
            var zoomInfoLabel = new ToolStripLabel("缩放信息");
            zoomInfoLabel.Name = "lblZoomInfo";
            zoomInfoLabel.DataBindings.Add("Text", this, nameof(ZoomInfo), false, DataSourceUpdateMode.OnPropertyChanged);
            
            // 添加常用缩放比例快捷按钮
            var zoom25Button = new ToolStripButton("25%");
            zoom25Button.Click += (sender, e) => Zoom = 0.25;
            
            var zoom50Button = new ToolStripButton("50%");
            zoom50Button.Click += (sender, e) => Zoom = 0.5;
            
            var zoom75Button = new ToolStripButton("75%");
            zoom75Button.Click += (sender, e) => Zoom = 0.75;
            
            var zoom100Button = new ToolStripButton("100%");
            zoom100Button.Click += (sender, e) => Zoom = 1.0;
            
            var zoom150Button = new ToolStripButton("150%");
            zoom150Button.Click += (sender, e) => Zoom = 1.5;
            
            var zoom200Button = new ToolStripButton("200%");
            zoom200Button.Click += (sender, e) => Zoom = 2.0;
            
            // 添加到工具栏
            _toolStrip.Items.Add(openButton);
            _toolStrip.Items.Add(prevButton);
            _toolStrip.Items.Add(nextButton);
            _toolStrip.Items.Add(new ToolStripSeparator());
            _toolStrip.Items.Add(pageInfoLabel);
            _toolStrip.Items.Add(pageNumberTextBox);
            _toolStrip.Items.Add(new ToolStripSeparator());
            _toolStrip.Items.Add(zoomInfoLabel);
            _toolStrip.Items.Add(new ToolStripSeparator());
            _toolStrip.Items.Add(zoom25Button);
            _toolStrip.Items.Add(zoom50Button);
            _toolStrip.Items.Add(zoom75Button);
            _toolStrip.Items.Add(zoom100Button);
            _toolStrip.Items.Add(zoom150Button);
            _toolStrip.Items.Add(zoom200Button);
            _toolStrip.Items.Add(new ToolStripSeparator());
            _toolStrip.Items.Add(zoomInButton);
            _toolStrip.Items.Add(zoomOutButton);
            _toolStrip.Items.Add(fitToWindowButton);
            
            // 添加到控件
            this.Controls.Add(_toolStrip);
        }

        /// <summary>
        /// 初始化图像显示区域
        /// </summary>
        private void InitializePictureBox()
        {
            // 创建面板作为容器
            var panel = new Panel();
            panel.Dock = DockStyle.Fill;
            panel.AutoScroll = true;
            panel.BackColor = Color.FromArgb(229, 229, 229);
            panel.Paint += Panel_Paint;
            panel.MouseWheel += Panel_MouseWheel;
            panel.KeyDown += Panel_KeyDown;
            panel.Focus();
            panel.TabStop = true;
            
            // 保存面板引用
            _pictureBoxPanel = panel;
            
            // 添加到控件
            this.Controls.Add(panel);
            
            // 确保工具栏位于面板上方
            if (_toolStrip != null)
            {
                panel.BringToFront();
            }
            
            // 触发一次重绘，显示空白文档区域
            panel.Invalidate();
        }
        
        /// <summary>
        /// 面板键盘按下事件
        /// </summary>
        private void Panel_KeyDown(object? sender, KeyEventArgs e)
        {
            // 检查是否按住Ctrl键
            if (e.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.Add: // Ctrl++ 放大
                    case Keys.Oemplus: // Ctrl+小键盘+
                        ZoomIn();
                        e.Handled = true;
                        break;
                    case Keys.Subtract: // Ctrl+- 缩小
                    case Keys.OemMinus: // Ctrl+小键盘-
                        ZoomOut();
                        e.Handled = true;
                        break;
                    case Keys.D0: // Ctrl+0 重置缩放
                    case Keys.NumPad0:
                        Zoom = 1.0;
                        e.Handled = true;
                        break;
                    case Keys.F: // Ctrl+F 适应窗口
                        FitToWindow();
                        e.Handled = true;
                        break;
                }
            }
        }

        #endregion

        #region 文档操作方法

        /// <summary>
        /// 打开OFD文档
        /// </summary>
        public async void OpenDocument(string filePath)
        {
            try
            {
                // 等待当前渲染任务完成
                while (_isRendering)
                {
                    await Task.Delay(10);
                }

                // 释放之前的渲染器
                _ofdRenderer?.Dispose();


                // 清空对象池
                lock (_poolLock)
                {
                    while (_pictureBoxPool.Count > 0)
                    {
                        _pictureBoxPool.Dequeue()?.Dispose();
                    }
                }

                // 清空待渲染队列
                lock (_renderLock)
                {
                    _pendingRenderQueue.Clear();
                }

                // 清空面板（在UI线程上执行，确保安全）
                if (_pictureBoxPanel != null && !_pictureBoxPanel.IsDisposed)
                {
                    if (_pictureBoxPanel.InvokeRequired)
                    {
                        _pictureBoxPanel.Invoke(new Action(() =>
                        {
                            if (_pictureBoxPanel != null && !_pictureBoxPanel.IsDisposed)
                            {
                                _pictureBoxPanel.Controls.Clear();
                            }
                        }));
                    }
                    else
                    {
                        _pictureBoxPanel.Controls.Clear();
                    }
                }

                // 清空已经渲染的页面
                foreach (var bitmap in _renderedPages.Values)
                {
                    bitmap.Dispose();
                }
                _renderedPages.Clear();

                // 页面偏移量（用于滚动）
                _pageOffsets.Clear();

                // 累计高度（用于滚动）
                _accumulatedHeight = 0;


                // 用于优化Panel_Paint的标记
                _lastScrollPosition = -1;
                _lastFirstPage = -1;
                _lastLastPage = -1;
                _lastZoom = 1.0;

                _isRendering = false;

                // 创建新的渲染器
                _ofdRenderer = new OfdRenderer(filePath, _renderConfig);

                // 更新页面信息
                TotalPages = _ofdRenderer.PageCount;

                // 先设置CurrentPage为-1，避免触发不必要的重绘
                _currentPage = -1;

                // 最后设置CurrentPage为0，触发一次重绘
                CurrentPage = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开OFD文档失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 打开OFD文档（通过文件对话框）
        /// </summary>
        private void OpenDocument()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "OFD文档|*.ofd",
                Title = "打开OFD文档"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                OpenDocument(openFileDialog.FileName);
            }
        }


        /// <summary>
        /// 更新工具栏可见性
        /// </summary>
        private void UpdateToolStripVisibility()
        {
            if (_toolStrip != null)
            {
                _toolStrip.Visible = _toolStripVisible;
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

        #endregion

        #region UI更新方法

        /// <summary>
        /// 更新导航按钮状态
        /// </summary>
        private void UpdateNavigationButtons()
        {
            if (_toolStrip != null)
            {
                var btnPrev = _toolStrip.Items.Find("btnPrev", false)[0] as ToolStripButton;
                var btnNext = _toolStrip.Items.Find("btnNext", false)[0] as ToolStripButton;
                if (btnPrev != null)
                {
                    btnPrev.Enabled = CanGoPrevious();
                }
                if (btnNext != null)
                {
                    btnNext.Enabled = CanGoNext();
                }
            }
        }

        #endregion

        #region 事件处理方法

        /// <summary>
        /// 面板绘制事件（优化版本）
        /// </summary>
        private void Panel_Paint(object? sender, PaintEventArgs e)
        {
            var panel = sender as Panel;
            if (panel == null) return;

            // 检查是否已打开文档
            if (_ofdRenderer != null && TotalPages > 0)
            {
                // 获取当前滚动位置（取绝对值）
                int scrollPositionY = Math.Abs(panel.AutoScrollPosition.Y);

                // 当滚动位置变化、Zoom值变化或页面偏移量为空时重新计算页面偏移量和当前页码
                if (scrollPositionY != _lastScrollPosition || _zoom != _lastZoom || _pageOffsets.Count == 0)
                {
                    // 计算所有页面的累计高度（用于定位）
                    UpdatePageOffsets();

                    // 计算当前滚动位置所在的页面
                    int currentPageIndex = 0;
                    for (int pageIndex = 0; pageIndex < TotalPages; pageIndex++)
                    {
                        int nextPageOffset = _pageOffsets.ContainsKey(pageIndex + 1) ? _pageOffsets[pageIndex + 1] : _accumulatedHeight;
                        if (scrollPositionY < nextPageOffset - PageSpacing)
                        {
                            currentPageIndex = pageIndex;
                            break;
                        }
                    }

                    // 计算需要渲染的页面范围（当前页及其前后各一页）
                    int tempFirstPage = Math.Max(0, currentPageIndex - 1);
                    int tempLastPage = Math.Min(TotalPages - 1, currentPageIndex + 1);

                    // 更新CurrentPage（如果发生变化）
                    if (currentPageIndex != CurrentPage)
                    {
                        // 直接更新字段，不触发SmartInvalidate，避免无限循环
                        _currentPage = currentPageIndex;
                        OnPropertyChanged(nameof(CurrentPage));
                        OnPropertyChanged(nameof(PageInfo));
                        UpdateNavigationButtons();
                    }

                    // 更新最后滚动位置和Zoom值
                    _lastScrollPosition = scrollPositionY;
                    _lastZoom = _zoom;
                    _lastFirstPage = tempFirstPage;
                    _lastLastPage = tempLastPage;
                }

                // 使用缓存的页面范围
                int firstPage = _lastFirstPage;
                int lastPage = _lastLastPage;
                
                // 收集需要保留的页面索引
                var pagesToKeep = new HashSet<int>();
                for (int pageIndex = firstPage; pageIndex <= lastPage; pageIndex++)
                {
                    pagesToKeep.Add(pageIndex);
                }

                // 移除面板中不需要的PictureBox并返回到对象池
                var pictureBoxesToRemove = new List<PictureBox>();
                foreach (Control control in panel.Controls)
                {
                    if (control is PictureBox pictureBox && pictureBox.Tag is int pageIndex)
                    {
                        if (!pagesToKeep.Contains(pageIndex))
                        {
                            pictureBoxesToRemove.Add(pictureBox);
                        }
                    }
                }

                foreach (var pictureBox in pictureBoxesToRemove)
                {
                    panel.Controls.Remove(pictureBox);
                    ReturnPictureBoxToPool(pictureBox);
                }

                // 渲染并显示页面
                for (int pageIndex = firstPage; pageIndex <= lastPage; pageIndex++)
                {
                    // 检查页面是否已经渲染
                    if (!_renderedPages.ContainsKey(pageIndex))
                    {
                        // 异步渲染页面，不阻塞UI
                        _ = RequestRenderPageAsync(pageIndex);
                        continue;
                    }

                    // 计算页面的实际位置                   
                    int pageY = _pageOffsets[pageIndex] - scrollPositionY;

                    // 检查页面是否在视图范围内
                    int pageHeight = (int)(_renderedPages[pageIndex].Height * Zoom);
                    if (pageY + pageHeight < 0 || pageY > panel.ClientSize.Height)
                    {
                        // 页面不在视图范围内，跳过
                        continue;
                    }

                    // 检查是否存在对应的PictureBox
                    var existingPictureBox = panel.Controls.OfType<PictureBox>()
                        .FirstOrDefault(p => p.Tag is int index && index == pageIndex);

                    // 计算页面水平居中位置
                    int pageWidth = (int)(_renderedPages[pageIndex].Width * Zoom);
                    int pageX = (panel.ClientSize.Width - pageWidth) / 2;
                    pageX = Math.Max(0, pageX); // 确保不小于0

                    if (existingPictureBox != null)
                    {
                        // 更新已存在的PictureBox
                        existingPictureBox.Image = _renderedPages[pageIndex];
                        existingPictureBox.Location = new Point(pageX, pageY);

                        // 缩放图片框
                        existingPictureBox.Size = new Size(pageWidth, pageHeight);
                    }
                    else
                    {
                        // 添加缺失的页面
                        // 从对象池获取PictureBox
                        var pictureBox = GetPictureBoxFromPool();
                        pictureBox.Image = _renderedPages[pageIndex];
                        pictureBox.Location = new Point(pageX, pageY);

                        // 缩放图片框
                        pictureBox.Size = new Size(pageWidth, pageHeight);

                        // 将页码数据记录到PictureBox的Tag属性中
                        pictureBox.Tag = pageIndex;

                        // 添加图片框到面板
                        panel.Controls.Add(pictureBox);
                    }
                }

                // 计算所有页面的总高度（用于设置滚动范围）
                int totalHeight = _accumulatedHeight; // 使用之前计算的累计高度
                
                // 确保总高度不小于面板高度
                totalHeight = Math.Max(totalHeight, panel.ClientSize.Height);
                
                // 只有当总高度发生变化时才更新滚动范围
                if (panel.AutoScrollMinSize.Height != totalHeight)
                {
                    panel.AutoScrollMinSize = new Size(0, totalHeight);
                }             
            }
            else
            {
                // 未打开文档时，显示空白的A4大小的文档区域
                RenderBlankDocument(panel);
            }
        }

        /// <summary>
        /// 渲染空白文档区域（未打开文档时显示）
        /// </summary>
        /// <param name="panel">要渲染到的面板</param>
        private void RenderBlankDocument(Panel panel)
        {
            // 检查是否已经有空白文档区域
            if (panel.Controls.Count == 0 || panel.Controls.Count == 1)
            {
                var existingPictureBox = panel.Controls.OfType<PictureBox>()
                                        .FirstOrDefault(p => p.Tag is string s && s == "BlankDocument");
                if (existingPictureBox != null && existingPictureBox.Image != null)
                {
                    // 获取当前滚动位置（取绝对值）
                    int scrollPositionY = Math.Abs(panel.AutoScrollPosition.Y);
                    // 计算水平居中位置
                    int pageX = (panel.ClientSize.Width - existingPictureBox.Image.Width) / 2;
                    pageX = Math.Max(0, pageX); // 确保不小于0

                    // 计算页面的实际位置                    
                    int pageY = PageSpacing - scrollPositionY;

                    // 设置位置，确保上下边有间距
                    existingPictureBox.Location = new Point(pageX, pageY);

                    // 设置滚动范围（包含上下间距）
                    panel.AutoScrollMinSize = new Size(existingPictureBox.Image.Width, existingPictureBox.Image.Height + PageSpacing * 2);
                }
                else
                {
                    // 创建空白的A4大小的图片框
                    var blankPictureBox = GetPictureBoxFromPool();

                    // 使用缓存的A4尺寸像素值
                    GetA4PixelSize(out int a4Width, out int a4Height);

                    var blankBitmap = new Bitmap(a4Width, a4Height);

                    using (var g = Graphics.FromImage(blankBitmap))
                    {
                        g.Clear(Color.White); // 白色背景

                        // 绘制边框（使用更粗的线条和更深的颜色，模拟其他阅读器的边框）
                        //g.DrawRectangle(Pens.LightGray, 0, 0, a4Width - 1, a4Height - 1);
                        //g.DrawRectangle(Pens.DarkGray, 1, 1, a4Width - 3, a4Height - 3);

                        // 显示提示文字
                        string hintText = "请打开OFD文档";
                        var font = new Font(FontFamily.GenericSansSerif, 16, FontStyle.Bold);
                        var textSize = g.MeasureString(hintText, font);
                        var textX = (a4Width - textSize.Width) / 2;
                        var textY = (a4Height - textSize.Height) / 2;
                        g.DrawString(hintText, font, Brushes.Gray, textX, textY);
                    }

                    blankPictureBox.Image = blankBitmap;

                    // 计算水平居中位置
                    int pageX = (panel.ClientSize.Width - a4Width) / 2;
                    pageX = Math.Max(0, pageX); // 确保不小于0

                    // 设置位置，确保上下边有间距
                    blankPictureBox.Location = new Point(pageX, PageSpacing);
                    blankPictureBox.Size = new Size(a4Width, a4Height);

                    // 将"BlankDocument"记录到PictureBox的Tag属性中
                    blankPictureBox.Tag = "BlankDocument";

                    // 添加到面板
                    panel.Controls.Add(blankPictureBox);

                    // 设置滚动范围（包含上下间距）
                    panel.AutoScrollMinSize = new Size(a4Width, a4Height + PageSpacing * 2);
                }
            }
        }


        /// <summary>
        /// 打开按钮点击事件
        /// </summary>
        private void OpenButton_Click(object? sender, EventArgs e)
        {
            OpenDocument();
        }

        /// <summary>
        /// 上一页按钮点击事件
        /// </summary>
        private void PrevButton_Click(object? sender, EventArgs e)
        {
            PreviousPage();
        }

        /// <summary>
        /// 下一页按钮点击事件
        /// </summary>
        private void NextButton_Click(object? sender, EventArgs e)
        {
            NextPage();
        }

        /// <summary>
        /// 放大按钮点击事件
        /// </summary>
        private void ZoomInButton_Click(object? sender, EventArgs e)
        {
            ZoomIn();
        }

        /// <summary>
        /// 缩小按钮点击事件
        /// </summary>
        private void ZoomOutButton_Click(object? sender, EventArgs e)
        {
            ZoomOut();
        }

        /// <summary>
        /// 适应窗口按钮点击事件
        /// </summary>
        private void FitToWindowButton_Click(object? sender, EventArgs e)
        {
            FitToWindow();
        }

        /// <summary>
        /// 页码输入框按键事件
        /// </summary>
        private void PageNumberTextBox_KeyPress(object? sender, KeyPressEventArgs e)
        {
            // 检查是否按下回车键
            if (e.KeyChar == (char)Keys.Enter)
            {
                // 获取输入框
                var textBox = sender as ToolStripTextBox;
                if (textBox != null && int.TryParse(textBox.Text, out int pageNumber))
                {
                    // 转换为从0开始的索引
                    int pageIndex = pageNumber - 1;
                    
                    // 检查页码是否有效
                    if (pageIndex >= 0 && pageIndex < TotalPages)
                    {
                        // 跳转到指定页码
                        CurrentPage = pageIndex;
                        
                        // 清空输入框
                        textBox.Text = string.Empty;
                    }
                    else
                    {
                        // 显示错误提示
                        MessageBox.Show("页码超出范围", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    // 显示错误提示
                    MessageBox.Show("请输入有效的页码", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                
                // 取消按键事件，避免输入框失去焦点
                e.Handled = true;
            }
        }



        /// <summary>
        /// 适应窗口
        /// </summary>
        private void FitToWindow()
        {
            if (_pictureBoxPanel != null && _renderedPages.Count > 0)
            {
                // 获取第一页的图片大小
                var firstPageBitmap = _renderedPages[0];
                if (firstPageBitmap != null)
                {
                    // 计算适应窗口的缩放比例
                    double scaleX = (_pictureBoxPanel.ClientSize.Width) / (double)firstPageBitmap.Width;
                    double scaleY = (_pictureBoxPanel.ClientSize.Height - PageSpacing) / (double)firstPageBitmap.Height;
                    Zoom = Math.Min(scaleX, scaleY);
                }
            }
        }
        
        /// <summary>
        /// 面板鼠标滚轮事件
        /// </summary>
        private void Panel_MouseWheel(object? sender, MouseEventArgs e)
        {
            var panel = sender as Panel;
            if (panel == null) return;

            // 检查是否按住Ctrl键，如果是则执行缩放操作
            if (Control.ModifierKeys == Keys.Control)
            {
                if (e.Delta > 0) // 向上滚动（滚轮向前）
                {
                    ZoomIn();
                }
                else if (e.Delta < 0) // 向下滚动（滚轮向后）
                {
                    ZoomOut();
                }
                return;
            }
        }

        #endregion

        #region INotifyPropertyChanged实现

        /// <summary>
        /// 属性变更事件
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

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
}