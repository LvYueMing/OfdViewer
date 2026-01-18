using System;
using System.IO;
using OFDViewer.Parse;
using OFDViewer.Render.Implementation;
using OFDViewer.Render.DataModels;
using OFDViewer.Render;

namespace OFDViewer.Tests.Rendering
{
    public class RenderTest
    {
        public static void RenderOfdToImage()
        {
            // OFD文件路径
            string ofdFilePath = "d:\\MySoft\\GitHub\\OfdViewer\\OFD-File\\ofd标准测试文件\\6.2.001 正常文件结构.ofd";
            // 输出图片路径
            string outputImagePath = "d:\\MySoft\\GitHub\\OfdViewer\\output.png";

            try
            {
                Console.WriteLine($"正在读取OFD文件：{ofdFilePath}");

                // 使用新创建的OfdRenderer类
                using (var renderer = new OfdRenderer(ofdFilePath))
                {
                    Console.WriteLine($"OfdRenderer初始化成功");
                    Console.WriteLine($"文档总页数：{renderer.PageCount}");

                    // 渲染第一页到文件
                    renderer.RenderPageToFile(outputImagePath, 0);

                    Console.WriteLine($"渲染成功，图片已保存到：{outputImagePath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"渲染失败：{ex.Message}");
                Console.WriteLine($"异常类型：{ex.GetType().FullName}");
                Console.WriteLine($"堆栈跟踪：{ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"内部异常：{ex.InnerException.Message}");
                    Console.WriteLine($"内部异常类型：{ex.InnerException.GetType().FullName}");
                    Console.WriteLine($"内部异常堆栈：{ex.InnerException.StackTrace}");
                }
            }
        }

        /// <summary>
        /// 使用OfdRenderer渲染多个页面到目录
        /// </summary>
        public static void RenderOfdToMultipleImages()
        {
            // OFD文件路径
            string ofdFilePath = "d:\\MySoft\\GitHub\\OfdViewer\\OFD-File\\ofd标准测试文件\\6.2.001 正常文件结构.ofd";
            // 输出目录
            string outputDirectory = "d:\\MySoft\\GitHub\\OfdViewer\\output_pages";

            try
            {
                Console.WriteLine($"正在读取OFD文件：{ofdFilePath}");

                // 使用OfdRenderer类
                using (var renderer = new OfdRenderer(ofdFilePath))
                {
                    Console.WriteLine($"OfdRenderer初始化成功");
                    Console.WriteLine($"文档总页数：{renderer.PageCount}");

                    // 渲染所有页面到目录
                    renderer.RenderAllPagesToFile(outputDirectory);

                    Console.WriteLine($"渲染成功，所有页面已保存到：{outputDirectory}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"渲染失败：{ex.Message}");
                Console.WriteLine($"异常类型：{ex.GetType().FullName}");
                Console.WriteLine($"堆栈跟踪：{ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"内部异常：{ex.InnerException.Message}");
                    Console.WriteLine($"内部异常类型：{ex.InnerException.GetType().FullName}");
                    Console.WriteLine($"内部异常堆栈：{ex.InnerException.StackTrace}");
                }
            }
        }

        /// <summary>
        /// 使用自定义渲染配置渲染OFD文档
        /// </summary>
        public static void RenderOfdWithCustomConfig()
        {
            // OFD文件路径
            string ofdFilePath = "d:\\MySoft\\GitHub\\OfdViewer\\OFD-File\\ofd标准测试文件\\6.2.001 正常文件结构.ofd";
            // 输出图片路径
            string outputImagePath = "d:\\MySoft\\GitHub\\OfdViewer\\output_custom_config.png";

            try
            {
                Console.WriteLine($"正在读取OFD文件：{ofdFilePath}");

                // 创建自定义渲染配置
                var renderConfig = new RenderConfig
                {
                    Dpi = 150, // 提高DPI到150
                    AntiAlias = true // 开启抗锯齿
                };

                // 使用OfdRenderer类并传入自定义配置
                using (var renderer = new OfdRenderer(ofdFilePath, renderConfig))
                {
                    Console.WriteLine($"OfdRenderer初始化成功");
                    Console.WriteLine($"文档总页数：{renderer.PageCount}");

                    // 渲染第一页到文件
                    renderer.RenderPageToFile(outputImagePath, 0);

                    Console.WriteLine($"渲染成功，图片已保存到：{outputImagePath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"渲染失败：{ex.Message}");
                Console.WriteLine($"异常类型：{ex.GetType().FullName}");
                Console.WriteLine($"堆栈跟踪：{ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"内部异常：{ex.InnerException.Message}");
                    Console.WriteLine($"内部异常类型：{ex.InnerException.GetType().FullName}");
                    Console.WriteLine($"内部异常堆栈：{ex.InnerException.StackTrace}");
                }
            }
        }
    }
}