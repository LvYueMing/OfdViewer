using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using OFDViewer.Render;
using OFDViewer.Render.DataModels;

namespace OfdViewer.WinForm.Controls
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
                    OnPropertyChanged(nameof(CurrentPage));
                    OnPropertyChanged(nameof(PageInfo));
                    
                    // 使用智能重绘，避免不必要的重绘
                    SmartInvalidate();
                    
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
        /// 当前缩放比例
        /// </summary>
        private double _zoom = 1.0;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public double Zoom
        {
            get => _zoom;
            set
            {
                if (_zoom != value)
                {
                    _zoom = value;
                    
                    // 使用智能重绘，避免不必要的重绘
                     SmartInvalidate();
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
        /// 用于智能重绘的标记
        /// </summary>
        private int _lastRenderedPage = -1;
        private double _lastZoom = -1;

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

            var zoomInButton = new ToolStripButton("放大");
            zoomInButton.Click += ZoomInButton_Click;
            
            var zoomOutButton = new ToolStripButton("缩小");
            zoomOutButton.Click += ZoomOutButton_Click;
            
            var fitToWindowButton = new ToolStripButton("适应窗口");
            fitToWindowButton.Click += FitToWindowButton_Click;
            
            // 添加到工具栏
            _toolStrip.Items.Add(openButton);
            _toolStrip.Items.Add(prevButton);
            _toolStrip.Items.Add(nextButton);
            _toolStrip.Items.Add(new ToolStripSeparator());
            _toolStrip.Items.Add(pageInfoLabel);
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
            panel.MouseWheel += PictureBoxPanel_MouseWheel;
            panel.Paint += Panel_Paint;
            
            // 保存面板引用
            _pictureBoxPanel = panel;
            
            // 添加到控件
            this.Controls.Add(panel);
            
            // 确保工具栏位于面板上方
            if (_toolStrip != null)
            {
                _toolStrip.BringToFront();
            }
            
            // 触发一次重绘，显示空白文档区域
            panel.Invalidate();
        }

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
                // 计算需要渲染的页面范围
                int firstPage = Math.Max(0, CurrentPage - 1);
                int lastPage = Math.Min(TotalPages - 1, CurrentPage + 1);

                // 清除空白页
                if (_pictureBoxPool.Any(p => p.Tag is string s && s == "BlankDocument"))
                    ClearPanelControls(panel);


                // 渲染并显示页面
                int yOffset = 0;
                bool hasRenderedPages = false;

                for (int pageIndex = firstPage; pageIndex <= lastPage; pageIndex++)
                {

                    // 检查页面是否已经渲染
                    if (!_renderedPages.ContainsKey(pageIndex))
                    {
                        // 异步渲染页面，不阻塞UI
                        _ = RequestRenderPageAsync(pageIndex);
                        continue;
                    }

                    hasRenderedPages = true;

                    if (_pictureBoxPool.Any(p => p.Tag is int index && index == pageIndex))
                        continue;

                    // 从对象池获取PictureBox
                    var pictureBox = GetPictureBoxFromPool();
                    pictureBox.Image = _renderedPages[pageIndex];
                    pictureBox.Location = new Point(0, yOffset);

                    // 缩放图片框
                    int width = (int)(pictureBox.Image.Width * Zoom);
                    int height = (int)(pictureBox.Image.Height * Zoom);
                    pictureBox.Size = new Size(width, height);

                    // 将页码数据记录到PictureBox的Tag属性中
                    pictureBox.Tag = pageIndex;

                    // 添加图片框到面板
                    panel.Controls.Add(pictureBox);

                    // 更新y偏移量
                    yOffset += pictureBox.Height;
                }

                // 设置面板的自动滚动范围
                int totalHeight = 0;
                foreach (Control control in panel.Controls)
                {
                    totalHeight += control.Height;
                }
                panel.AutoScrollMinSize = new Size(0, totalHeight);

                // 如果没有已渲染的页面，确保至少有一个加载提示
                if (!hasRenderedPages && panel.Controls.Count == 0)
                {
                    var loadingLabel = new Label
                    {
                        Text = "文档加载中...",
                        Location = new Point(0, 0),
                        Size = new Size(panel.ClientSize.Width, 100),
                        TextAlign = ContentAlignment.MiddleCenter,
                        BackColor = Color.LightGray,
                        Font = new Font(FontFamily.GenericSansSerif, 14)
                    };
                    panel.Controls.Add(loadingLabel);
                }
                //}
            }
            else
            {
                // 未打开文档时，显示空白的A4大小的文档区域
                // 检查是否已经有空白文档区域
                if (panel.Controls.Count == 0)
                {
                    // 创建空白的A4大小的图片框
                    var blankPictureBox = GetPictureBoxFromPool();

                    // 创建空白的A4大小的位图（白色背景）
                    // 使用实际的A4宽度和高度，不乘以Zoom
                    int a4Width = 595;
                    int a4Height = 842;
                    var blankBitmap = new Bitmap(a4Width, a4Height);

                    using (var g = Graphics.FromImage(blankBitmap))
                    {
                        g.Clear(Color.White); // 白色背景

                        // 绘制边框
                        g.DrawRectangle(Pens.LightGray, 0, 0, a4Width - 1, a4Height - 1);

                        // 显示提示文字
                        string hintText = "请打开OFD文档";
                        var font = new Font(FontFamily.GenericSansSerif, 16, FontStyle.Bold);
                        var textSize = g.MeasureString(hintText, font);
                        var textX = (a4Width - textSize.Width) / 2;
                        var textY = (a4Height - textSize.Height) / 2;
                        g.DrawString(hintText, font, Brushes.Gray, textX, textY);
                    }

                    blankPictureBox.Image = blankBitmap;
                    blankPictureBox.Location = new Point(0, 0);
                    blankPictureBox.Size = new Size(a4Width, a4Height);

                    // 将"BlankDocument"记录到PictureBox的Tag属性中
                    blankPictureBox.Tag = "BlankDocument";

                    // 添加到面板
                    panel.Controls.Add(blankPictureBox);

                    // 设置滚动范围
                    panel.AutoScrollMinSize = new Size(a4Width, a4Height);
                }
            }
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
                
                // 清空已经渲染的页面
                foreach (var bitmap in _renderedPages.Values)
                {
                    bitmap.Dispose();
                }
                _renderedPages.Clear();
                
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
                
                _isRendering = false;
                
                // 创建新的渲染器
                _ofdRenderer = new OfdRenderer(filePath, _renderConfig);
                
                // 更新页面信息
                TotalPages = _ofdRenderer.PageCount;
                CurrentPage = 0;
                
                // 重置智能重绘标记
                _lastRenderedPage = -1;
                _lastZoom = -1;
                
                // 触发面板重绘
                _pictureBoxPanel?.Invalidate();
                
                // 适应窗口
                FitToWindow();
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
            return new PictureBox
            {
                Visible = true,
                SizeMode = PictureBoxSizeMode.AutoSize,
                BorderStyle = BorderStyle.None
            };
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
        /// 清空面板中的所有控件
        /// </summary>
        private void ClearPanelControls(Panel panel)
        {
            // 归还所有PictureBox到对象池
            foreach (Control control in panel.Controls)
            {
                if (control is PictureBox pictureBox)
                {
                    ReturnPictureBoxToPool(pictureBox);
                }
            }            
        }

        /// <summary>
        /// 智能重绘
        /// </summary>
        private void SmartInvalidate()
        {
            // 只有当页面或缩放变化时才重绘
            if (CurrentPage != _lastRenderedPage || Zoom != _lastZoom)
            {
                _pictureBoxPanel?.Invalidate();
                _lastRenderedPage = CurrentPage;
                _lastZoom = Zoom;
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
                    double scaleX = (_pictureBoxPanel.ClientSize.Width - 20) / (double)firstPageBitmap.Width;
                    double scaleY = (_pictureBoxPanel.ClientSize.Height - 20) / (double)firstPageBitmap.Height;
                    Zoom = Math.Min(scaleX, scaleY);
                }
            }
        }

        /// <summary>
        /// 图片框面板鼠标滚轮事件
        /// </summary>
        private void PictureBoxPanel_MouseWheel(object? sender, MouseEventArgs e)
        {
            var panel = sender as Panel;
            if (panel == null) return;

            if (_ofdRenderer != null && TotalPages > 1)
            {
                if (e.Delta > 0) // 向上滚动（滚轮向前）
                {
                    int currentScrollY = panel.AutoScrollPosition.Y;
                    if (currentScrollY <= 50 && CanGoPrevious())
                    {
                        int oldPage = CurrentPage;
                        int excess = currentScrollY;
                        PreviousPage();

                        // 查找旧页面的PictureBox（应该仍在面板中）
                        var oldPagePictureBox = panel.Controls.OfType<PictureBox>()
                            .FirstOrDefault(pb => pb.Tag is int tag && tag == oldPage);

                        if (oldPagePictureBox != null)
                        {
                            panel.AutoScrollPosition = new Point(0, oldPagePictureBox.Height - excess);
                        }
                    }
                }
                else if (e.Delta < 0) // 向下滚动（滚轮向后）
                {
                    int oldPage = CurrentPage;
                    var oldPagePictureBox = panel.Controls.OfType<PictureBox>()
                        .FirstOrDefault(pb => pb.Tag is int tag && tag == oldPage);

                    if (oldPagePictureBox == null) return;

                    int panelHeight = panel.ClientSize.Height;
                    int currentScrollY = panel.AutoScrollPosition.Y;
                    int oldPageBottom = oldPagePictureBox.Bottom;
                    int excess = (currentScrollY + panelHeight) - oldPageBottom;

                    if (excess >= -50 && CanGoNext())
                    {
                        NextPage();
                        panel.AutoScrollPosition = new Point(0, excess);
                    }
                }
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