using OfdViewer.WinForm.Controls;

namespace OfdViewer.WinForm.Test;

public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();
        InitializeOfdViewerControl();
    }

    /// <summary>
    /// 初始化 OFD 查看器控件
    /// </summary>
    private void InitializeOfdViewerControl()
    {
        // 创建 OFD 查看器控件
        var ofdViewerControl = new OfdViewerControl();
        ofdViewerControl.Dock = DockStyle.Fill;
        
        // 添加到窗体
        this.Controls.Add(ofdViewerControl);
        
        // 设置窗体标题
        this.Text = "OFD 查看器 (WinForm)";
        
        // 设置窗体大小
        this.Size = new System.Drawing.Size(1024, 768);
    }
}
