using System;
using System.IO;
using Xunit;
using OFDViewer.Render.Implementation;
using OFDViewer.Render.DataModels;
using SkiaSharp;

namespace OFDViewer.Test.Rendering
{
    /// <summary>
    /// SkiaRenderContext的单元测试
    /// </summary>
    public class SkiaRenderContextTests
    {
        /// <summary>
        /// 测试SkiaRenderContext初始化
        /// </summary>
        [Fact]
        public void Initialize_ShouldSetCorrectWidthAndHeight()
        {
            // Arrange
            using var renderContext = new SkiaRenderContext();
            int width = 800;
            int height = 600;

            // Act
            renderContext.Initialize(width, height);

            // Assert
            Assert.Equal(width, renderContext.Width);
            Assert.Equal(height, renderContext.Height);
        }

        /// <summary>
        /// 测试重置渲染上下文
        /// </summary>
        [Fact]
        public void Reset_ShouldClearCanvas()
        {
            // Arrange
            using var renderContext = new SkiaRenderContext();
            int width = 800;
            int height = 600;
            renderContext.Initialize(width, height);

            // Act
            renderContext.Reset();

            // Assert
            // 重置后应该能够正常获取渲染结果
            var result = renderContext.GetRenderResult();
            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        /// <summary>
        /// 测试绘制直线
        /// </summary>
        [Fact]
        public void DrawLine_ShouldRenderLine()
        {
            // Arrange
            using var renderContext = new SkiaRenderContext();
            int width = 800;
            int height = 600;
            renderContext.Initialize(width, height);

            // Act
            var style = new GraphStyle
            {
                Color = 0xFFFF0000, // 红色
                Alpha = 255,
                Stroke = true,
                StrokeWidth = 2.0f
            };
            renderContext.DrawLine(100, 100, 700, 500, style);

            // Assert
            var result = renderContext.GetRenderResult();
            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        /// <summary>
        /// 测试绘制矩形
        /// </summary>
        [Fact]
        public void DrawRectangle_ShouldRenderRectangle()
        {
            // Arrange
            using var renderContext = new SkiaRenderContext();
            int width = 800;
            int height = 600;
            renderContext.Initialize(width, height);

            // Act
            var style = new GraphStyle
            {
                Color = 0xFF00FF00, // 绿色
                Alpha = 255,
                Fill = true
            };
            renderContext.DrawRectangle(100, 100, 200, 150, style);

            // Assert
            var result = renderContext.GetRenderResult();
            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        /// <summary>
        /// 测试绘制文本
        /// </summary>
        [Fact]
        public void DrawText_ShouldRenderText()
        {
            // Arrange
            using var renderContext = new SkiaRenderContext();
            int width = 800;
            int height = 600;
            renderContext.Initialize(width, height);

            // Act
            var style = new TextStyle
            {
                FontFamily = "SimSun",
                FontSize = 24.0f,
                Color = 0xFF0000FF, // 蓝色
                Alpha = 255
            };
            renderContext.DrawText(100, 200, "测试文本", style);

            // Assert
            var result = renderContext.GetRenderResult();
            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        /// <summary>
        /// 测试测量文本宽度
        /// </summary>
        [Fact]
        public void MeasureTextWidth_ShouldReturnCorrectWidth()
        {
            // Arrange
            using var renderContext = new SkiaRenderContext();
            int width = 800;
            int height = 600;
            renderContext.Initialize(width, height);

            // Act
            var style = new TextStyle
            {
                FontFamily = "SimSun",
                FontSize = 24.0f
            };
            float textWidth = renderContext.MeasureTextWidth("测试文本", style);

            // Assert
            Assert.True(textWidth > 0);
        }

        /// <summary>
        /// 测试保存和恢复渲染状态
        /// </summary>
        [Fact]
        public void SaveStateAndRestoreState_ShouldPreserveState()
        {
            // Arrange
            using var renderContext = new SkiaRenderContext();
            int width = 800;
            int height = 600;
            renderContext.Initialize(width, height);

            // Act
            // 保存初始状态
            renderContext.SaveState();
            
            // 应用变换
            renderContext.Translate(100, 100);
            renderContext.Rotate((float)Math.PI / 4); // 45度
            
            // 绘制变换后的矩形
            var style = new GraphStyle
            {
                Color = 0xFFFF00FF, // 紫色
                Alpha = 255,
                Fill = true
            };
            renderContext.DrawRectangle(0, 0, 100, 100, style);
            
            // 恢复状态
            renderContext.RestoreState();
            
            // 绘制原始位置的矩形
            style.Color = 0xFF00FFFF; // 青色
            renderContext.DrawRectangle(0, 0, 100, 100, style);

            // Assert
            var result = renderContext.GetRenderResult();
            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void DrawImage_WithinConfiguredLimits_ChangesCanvas()
        {
            using var renderContext = new SkiaRenderContext();
            renderContext.Initialize(20, 20);
            byte[] before = renderContext.GetRenderResult();

            renderContext.DrawImage(0, 0, 20, 20, CreatePng(2, 2), new ImageStyle());

            byte[] after = renderContext.GetRenderResult();
            Assert.False(before.SequenceEqual(after));
        }

        [Fact]
        public void DrawImage_ExceedsEncodedByteLimit_DoesNotChangeCanvas()
        {
            using var renderContext = new SkiaRenderContext
            {
                Config = new RenderConfig(96) { MaxEncodedImageBytes = 8 }
            };
            renderContext.Initialize(20, 20);
            byte[] before = renderContext.GetRenderResult();

            renderContext.DrawImage(0, 0, 20, 20, CreatePng(2, 2), new ImageStyle());

            byte[] after = renderContext.GetRenderResult();
            Assert.True(before.SequenceEqual(after));
        }

        [Fact]
        public void DrawImage_ExceedsDecodedPixelLimit_DoesNotChangeCanvas()
        {
            using var renderContext = new SkiaRenderContext
            {
                Config = new RenderConfig(96) { MaxDecodedImagePixels = 1 }
            };
            renderContext.Initialize(20, 20);
            byte[] before = renderContext.GetRenderResult();

            renderContext.DrawImage(0, 0, 20, 20, CreatePng(2, 2), new ImageStyle());

            byte[] after = renderContext.GetRenderResult();
            Assert.True(before.SequenceEqual(after));
        }

        private static byte[] CreatePng(int width, int height)
        {
            using var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
            bitmap.Erase(SKColors.Red);
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }
    }
}
