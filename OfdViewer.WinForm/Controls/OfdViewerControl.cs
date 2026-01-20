using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
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
                    RenderCurrentPage();
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
                    OnPropertyChanged(nameof(Zoom));
                    UpdatePictureBoxSize();
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
        /// 图片框
        /// </summary>
        private PictureBox? _pictureBox;

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
            this.Size = new Size(800, 600);
            
            // 初始化工具栏
            InitializeToolStrip();
            
            // 初始化图像显示区域
            InitializePictureBox();
            
            // 设置默认A4大小的空白文档
            SetDefaultA4Document();
            
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
            panel.Resize += Panel_Resize;
            
            // 创建图片框
            _pictureBox = new PictureBox();
            _pictureBox.Name = "picOfd";
            _pictureBox.SizeMode = PictureBoxSizeMode.AutoSize;
            _pictureBox.BackColor = Color.White;
            _pictureBox.BorderStyle = BorderStyle.FixedSingle;
            
            // 添加到面板
            panel.Controls.Add(_pictureBox);
            
            // 保存面板引用
            _pictureBoxPanel = panel;
            
            // 添加到控件（确保在工具栏之后添加，以避免被覆盖）
            this.Controls.Add(panel);
            
            // 确保面板位于工具栏下方
            if (_toolStrip != null)
            {
                panel.BringToFront();
            }
        }

        /// <summary>
        /// 面板大小改变事件
        /// </summary>
        private void Panel_Resize(object? sender, EventArgs e)
        {
            CenterPictureBox();
        }



        /// <summary>
        /// 设置默认A4大小的空白文档
        /// </summary>
        private void SetDefaultA4Document()
        {
            // A4纸张大小（毫米）
            float a4WidthMm = 210f;
            float a4HeightMm = 297f;
            
            // 获取当前DPI
            float dpi = 96f;
            if (_pictureBox != null && _pictureBox.Parent != null)
            {
                using (var graphics = _pictureBox.Parent.CreateGraphics())
                {
                    dpi = graphics.DpiX;
                }
            }
            
            // 转换为像素
            int a4Width = (int)(a4WidthMm * dpi / 25.4f);
            int a4Height = (int)(a4HeightMm * dpi / 25.4f);
            
            // 创建空白位图
            var bitmap = new Bitmap(a4Width, a4Height);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                // 设置背景色为白色
                graphics.Clear(Color.White);
                
                // 绘制边框
                using (var pen = new Pen(Color.LightGray, 1))
                {
                    graphics.DrawRectangle(pen, 0, 0, a4Width - 1, a4Height - 1);
                }
            }
            
            // 设置图片框图像
            if (_pictureBox != null)
            {
                _pictureBox.Image = bitmap;
                UpdatePictureBoxSize();
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
        /// 图片框面板鼠标滚轮事件
        /// </summary>
        private void PictureBoxPanel_MouseWheel(object? sender, MouseEventArgs e)
        {
            // 获取当前滚动位置
            var panel = sender as Panel;
            if (panel == null) return;
            
            // 检查是否需要翻页
            if (_ofdRenderer != null && TotalPages > 1)
            {
                // 向上滚动（滚轮向前）
                if (e.Delta > 0 && panel.AutoScrollPosition.Y >= 0 && CanGoPrevious())
                {
                    PreviousPage();
                    // 滚动到页面顶部
                    panel.AutoScrollPosition = new Point(0, 0);
                }
                // 向下滚动（滚轮向后）
                else if (e.Delta < 0)
                {
                    // 计算图片框的底部位置
                    int pictureBoxBottom = _pictureBox?.Bottom ?? 0;
                    int panelHeight = panel.ClientSize.Height;
                    
                    // 如果已经滚动到图片底部，并且可以下一页
                    if (panel.AutoScrollPosition.Y + panelHeight >= pictureBoxBottom - 50 && CanGoNext())
                    {
                        NextPage();
                        // 滚动到页面顶部
                        panel.AutoScrollPosition = new Point(0, 0);
                    }
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
                
                // 创建新的渲染器
                _ofdRenderer = new OfdRenderer(filePath, _renderConfig);
                
                // 更新页面信息
                TotalPages = _ofdRenderer.PageCount;
                CurrentPage = 0;
                
                // 更新页面信息显示
                UpdatePageInfo();
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
                
                // 将字节数组转换为Bitmap
                using (var stream = new MemoryStream(imageData))
                {
                    var bitmap = new Bitmap(stream);
                    
                    // 更新图像
                    if (_pictureBox != null)
                    {
                        _pictureBox.Image = bitmap;

                        UpdatePictureBoxSize();
                    }
                }
                
                // 更新页面信息
                UpdatePageInfo();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"渲染页面失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 居中显示图片框
        /// </summary>
        private void CenterPictureBox()
        {
            if (_pictureBox != null && _pictureBoxPanel != null)
            {
                // 计算居中位置
                int x = (_pictureBoxPanel.ClientSize.Width - _pictureBox.Width) / 2;
                int y = (_pictureBoxPanel.ClientSize.Height - _pictureBox.Height) / 2;
                
                // 确保位置不小于0，并且顶部有一定的边距
                x = Math.Max(0, x);
                y = Math.Max(20, y); // 顶部边距20像素
                
                // 设置图片框位置
                _pictureBox.Location = new Point(x, y);
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

        /// <summary>
        /// 更新图片框大小
        /// </summary>
        private void UpdatePictureBoxSize()
        {
            if (_pictureBox != null && _pictureBox.Image != null)
            {
                // 计算缩放后的大小
                int width = (int)(_pictureBox.Image.Width * Zoom);
                int height = (int)(_pictureBox.Image.Height * Zoom);
                
                // 更新图片框大小
                _pictureBox.Size = new Size(width, height);
                
                // 居中显示
                CenterPictureBox();
            }
        }

        /// <summary>
        /// 适应窗口
        /// </summary>
        private void FitToWindow()
        {
            if (_pictureBox != null && _pictureBox.Image != null)
            {
                // 获取容器大小
                if (_pictureBoxPanel != null)
                {
                    // 计算适应窗口的缩放比例
                    double scaleX = (_pictureBoxPanel.ClientSize.Width - 20) / (double)_pictureBox.Image.Width;
                    double scaleY = (_pictureBoxPanel.ClientSize.Height - 20) / (double)_pictureBox.Image.Height;
                    Zoom = Math.Min(scaleX, scaleY);
                }
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

        /// <summary>
        /// 更新页面信息显示
        /// </summary>
        private void UpdatePageInfo()
        {
            //if (_toolStrip != null)
            //{
            //    var lblPageInfo = _toolStrip.Items.Find("lblPageInfo", false)[0] as ToolStripLabel;
            //    if (lblPageInfo != null)
            //    {
            //        lblPageInfo.Text = PageInfo;
            //    }
            //}
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