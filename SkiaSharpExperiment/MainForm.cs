using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using JBig2Decoder.NETStandard;
using SkiaSharp;

namespace SkiaSharpExperiment;

public partial class MainForm : Form
{
    private SKCanvas? _canvas;
    private SKBitmap? _bitmap;
    private SKSurface? _surface;

    public MainForm()
    {
        InitializeSkiaCanvas();
        
        this.ClientSize = new Size(800, 600);
        this.Text = "SkiaSharp 图形绘制实验";
    }

    /// <summary>
    /// 初始化Skia画布
    /// </summary>
    private void InitializeSkiaCanvas()
    {
        var info = new SKImageInfo(800, 600);
        
        _bitmap = new SKBitmap(info);
        _surface = SKSurface.Create(info);
        _canvas = _surface.Canvas;
    }

    /// <summary>
    /// 重绘窗体
    /// </summary>
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        
        if (_canvas == null || _bitmap == null || _surface == null)
            return;
        
        _canvas.Clear(SKColors.White);

        //DrawBasicShapes();
        //DrawText();
        //DrawPaths();
        //DrawImages();

        //DrawPaths1();
        //DrawBmpImage();
        ConvertJB2ToPNG();

        _surface.Canvas.Flush();
        
        using var image = _surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = new System.IO.MemoryStream(data.ToArray());
        using var bitmap = new Bitmap(stream);
        
        e.Graphics.DrawImage(bitmap, 0, 0);
    }

    /// <summary>
    /// 绘制基本图形
    /// </summary>
    private void DrawBasicShapes()
    {
        if (_canvas == null) return;
        
        using var paint = new SKPaint
        {
            Color = SKColors.Blue,
            Style = SKPaintStyle.Fill
        };
        _canvas.DrawRect(new SKRect(50, 50, 150, 150), paint);
        
        paint.Color = SKColors.Red;
        paint.Style = SKPaintStyle.Stroke;
        paint.StrokeWidth = 3;
        _canvas.DrawRect(new SKRect(200, 50, 300, 150), paint);
        
        paint.Color = SKColors.Green;
        paint.Style = SKPaintStyle.Fill;
        _canvas.DrawCircle(100, 250, 40, paint);
        
        paint.Color = SKColors.Purple;
        paint.Style = SKPaintStyle.Stroke;
        paint.StrokeWidth = 3;
        _canvas.DrawCircle(250, 250, 40, paint);
        
        paint.Color = SKColors.Orange;
        paint.Style = SKPaintStyle.Fill;
        _canvas.DrawOval(new SKRect(350, 200, 450, 300), paint);
        
        paint.Color = SKColors.Cyan;
        paint.Style = SKPaintStyle.Fill;
        _canvas.DrawRoundRect(new SKRect(50, 350, 150, 450), 20, 20, paint);
    }

    /// <summary>
    /// 绘制文本
    /// </summary>
    private void DrawText()
    {
        if (_canvas == null) return;
        
        using var font = new SKFont(SKTypeface.Default, 24);
        using var paint = new SKPaint
        {
            Color = SKColors.Black,
            IsAntialias = true
        };
        _canvas.DrawText("Hello SkiaSharp!", 500, 100, SKTextAlign.Left, font, paint);
        
        font.Size = 32;
        paint.Color = SKColors.Yellow;
        _canvas.DrawText("描边文本", 500, 150, SKTextAlign.Left, font, paint);
        
        paint.Style = SKPaintStyle.Stroke;
        paint.StrokeWidth = 2;
        paint.Color = SKColors.Black;
        _canvas.DrawText("描边文本", 500, 150, SKTextAlign.Left, font, paint);
        
        paint.Style = SKPaintStyle.Fill;
        paint.Color = SKColors.DarkBlue;
        font.Size = 28;
        
        _canvas.Save();
        _canvas.Translate(600, 250);
        _canvas.RotateDegrees(45);
        _canvas.DrawText("旋转文本", 0, 0, SKTextAlign.Left, font, paint);
        _canvas.Restore();
    }

    /// <summary>
    /// 绘制路径
    /// </summary>
    private void DrawPaths()
    {
        if (_canvas == null) return;
        
        using var paint = new SKPaint
        {
            Color = SKColors.DarkGreen,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2
        };
        _canvas.DrawLine(new SKPoint(500, 300), new SKPoint(750, 300), paint);
        _canvas.DrawLine(new SKPoint(500, 300), new SKPoint(500, 400), paint);
        
        paint.Color = SKColors.Magenta;
        paint.StrokeWidth = 3;
        using var path = new SKPath();
        path.MoveTo(500, 350);
        path.QuadTo(new SKPoint(600, 300), new SKPoint(700, 350));
        _canvas.DrawPath(path, paint);
        
        paint.Color = SKColors.Brown;
        path.Reset();
        path.MoveTo(500, 400);
        path.CubicTo(new SKPoint(550, 350), new SKPoint(650, 450), new SKPoint(750, 400));
        _canvas.DrawPath(path, paint);
        
        paint.Color = SKColors.LightBlue;
        paint.Style = SKPaintStyle.Fill;
        path.Reset();
        path.MoveTo(550, 450);
        path.LineTo(650, 450);
        path.LineTo(700, 500);
        path.LineTo(600, 550);
        path.LineTo(550, 500);
        path.Close();
        _canvas.DrawPath(path, paint);
    }


    /// <summary>
    /// 绘制路径
    /// </summary>
    private void DrawPaths1()
    {
        if (_canvas == null) return;

        using var paint = new SKPaint();

        paint.Color = SKColors.Red;
        paint.Style = SKPaintStyle.Stroke;
        paint.StrokeWidth = 3;

        var x = 96 / 25.4f;

        _canvas.DrawRect(new SKRect(31.75f, 38.480999f, 71.331665f, 62.568668f), paint);

        _canvas.Save();

        _canvas.Translate(31.75f * x, 38.480999f * x);

        // 71.331665 0 0 62.568668 0 0
        var matrix = new SKMatrix
        {
            ScaleX = 71.331665f,
            SkewX = 0,
            SkewY = 0,
            ScaleY = 62.568668f,
            TransX = 0,
            TransY = 0,
            Persp0 = 0,
            Persp1 = 0,
            Persp2 = 1
        };
        _canvas.Concat(in matrix);

        // 计算缩放因子以保持描边宽度不变
        var scaleX = matrix.ScaleX;
        var scaleY = matrix.ScaleY;
        var avgScale = (Math.Abs(scaleX) + Math.Abs(scaleY)) / 2;
        var originalStrokeWidth = 3f;
        var adjustedStrokeWidth = (float)(originalStrokeWidth / avgScale);

        paint.StrokeWidth = adjustedStrokeWidth;
        paint.StrokeCap = SKStrokeCap.Butt;
        paint.StrokeJoin = SKStrokeJoin.Miter;

        //_canvas.DrawRect(new SKRect(31.75f*x, 38.480999f * x, 71.331665f * x, 62.568668f * x), paint);
        _canvas.DrawRect(new SKRect(0, 0, 1, 1), paint);
        _canvas.Restore();

    }

    /// <summary>
    /// 绘制图像
    /// </summary>
    private void DrawImages()
    {
        if (_canvas == null) return;
        
        var info = new SKImageInfo(100, 100);
        using var bitmap = new SKBitmap(info);
        using var surface = SKSurface.Create(info);
        var canvas = surface.Canvas;
        
        using var shader = SKShader.CreateLinearGradient(
            new SKPoint(0, 0),
            new SKPoint(100, 100),
            new[] { SKColors.Red, SKColors.Blue },
            null,
            SKShaderTileMode.Clamp);
        using var paint = new SKPaint
        {
            Shader = shader
        };
        canvas.DrawRect(new SKRect(0, 0, 100, 100), paint);
        
        paint.Shader = null;
        paint.Color = SKColors.White;
        paint.Style = SKPaintStyle.Fill;
        canvas.DrawCircle(50, 50, 30, paint);
        
        surface.Canvas.Flush();
        using var image = surface.Snapshot();
        _canvas.DrawImage(image, 500, 450);
    }

    /// <summary>
    /// 绘制BMP图片（应用平移和变换矩阵）
    /// </summary>
    private void DrawBmpImage()
    {
        if (_canvas == null) return;

        var MmToPixel = 96 / 25.4f;
        var PixelToMm = 25.4f / 96;
        var imagePath = @"d:\MySoft\GitHub\OfdViewer\OFD-File\Res\image_28.bmp";

        if (!System.IO.File.Exists(imagePath))
        {
            using var font = new SKFont(SKTypeface.Default, 16);
            using var paint = new SKPaint
            {
                Color = SKColors.Red,
                IsAntialias = true
            };
            _canvas.DrawText("图片文件不存在: " + imagePath, 10, 10, SKTextAlign.Left, font, paint);
            return;
        }

        using var bitmap = SKBitmap.Decode(imagePath);
        if (bitmap == null)
        {
            using var font = new SKFont(SKTypeface.Default, 16);
            using var paint = new SKPaint
            {
                Color = SKColors.Red,
                IsAntialias = true
            };
            _canvas.DrawText("无法加载图片: " + imagePath, 10, 10, SKTextAlign.Left, font, paint);
            return;
        }

        // 将图片像素转换为单位长度（mm）

        var Width = bitmap.Width * PixelToMm;
        var Height = bitmap.Height * PixelToMm;

        //bitmap.Width = Width;
        //bitmap.Height = Height;

        _canvas.Save();

        _canvas.Translate(31.75f * MmToPixel, 38.480999f * MmToPixel);

        var matrix = new SKMatrix
        {
            ScaleX = 71.331665f * MmToPixel,
            SkewX = 0,
            SkewY = 0,
            ScaleY = 62.568668f * MmToPixel,
            TransX = 0,
            TransY = 0,
            Persp0 = 0,
            Persp1 = 0,
            Persp2 = 1
        };
        _canvas.Concat(in matrix);


        // MessageBox.Show($"图片尺寸: {bitmap.Width} x {bitmap.Height}");
        // MessageBox.Show($"图片缩放尺寸: {scaledWidth} x {scaledHeight}");

        // 绘制图片边框（在变换后的坐标系中）
        using var borderPaint = new SKPaint
        {
            Color = SKColors.Green,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 0.01f
        };
        _canvas.DrawRect(new SKRect(0, 0, 1, 1), borderPaint);

        // 绘制图片（在变换后的坐标系中，缩放到1x1单位）
        // _canvas.DrawBitmap(bitmap, new SKRect(0, 0, 1, 1));
        _canvas.DrawBitmap(bitmap, new SKRect(0, 0, 1, 1));

        _canvas.Restore();
    }


    private void ConvertJB2ToPNG()
    {
        byte[] imageData = File.ReadAllBytes(@"d:\MySoft\GitHub\OfdViewer\OFD-File\Res\image_80.jb2");

        try
        {
            // 使用JBig2Decoder.NETStandard库解析JBIG2数据
            var jbig = new JBIG2StreamDecoder();
            int width = 0;
            int height = 0;
            // the resulting 'byte[] rgbBuffer' is a RGB array
            byte[] rgbBuffer = jbig.DecodeJBIG2(imageData, out width, out height);
            int i = jbig.GetNumberOfPages();

            MessageBox.Show($"rgbBuffer.Length({rgbBuffer.Length})  = width * height * 3 ({width * height * 3}):({rgbBuffer.Length == width * height * 3})" );
           
           //再此处添加转换代码
           // 使用System.Drawing.Bitmap创建位图
           using var systemBitmap = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
           
           // 锁定位图数据
           var bmpData = systemBitmap.LockBits(
               new System.Drawing.Rectangle(0, 0, width, height),
               System.Drawing.Imaging.ImageLockMode.WriteOnly,
               System.Drawing.Imaging.PixelFormat.Format24bppRgb);
           
           try
           {
               // 将RGB数据拷贝到位图
               System.Runtime.InteropServices.Marshal.Copy(rgbBuffer, 0, bmpData.Scan0, rgbBuffer.Length);
           }
           finally
           {
               // 解锁位图数据
               systemBitmap.UnlockBits(bmpData);
           }
           
           // 保存为PNG格式
           systemBitmap.Save(@"d:\MySoft\GitHub\OfdViewer\OFD-File\Res\image_80.png", System.Drawing.Imaging.ImageFormat.Png);
           
           // 在SkiaSharp画布上绘制转换后的图片
           if (_canvas != null)
           {
               // 将System.Drawing.Bitmap转换为SkiaSharp位图
               using var skiaBitmap = SKBitmap.Decode(@"d:\MySoft\GitHub\OfdViewer\OFD-File\Res\image_80.png");
               if (skiaBitmap != null)
               {
                   _canvas.DrawBitmap(skiaBitmap, 10, 10);
               }
           }
           

        }
        catch (Exception ex)
        {
            // 记录错误并返回原始数据，避免整个渲染过程失败
            System.Diagnostics.Debug.WriteLine($"JB2转换失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _surface?.Dispose();
            _bitmap?.Dispose();
            _canvas?.Dispose();
        }
        base.Dispose(disposing);
    }
}
