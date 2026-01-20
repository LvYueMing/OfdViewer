using System;
using OFDViewer.Render.DataModels;

namespace OFDViewer.Render.Abstractions
{
    /// <summary>
    /// 渲染上下文接口，作为渲染操作的统一入口
    /// 管理渲染生命周期、画布资源、渲染配置
    /// </summary>
    public interface IRenderContext : IDisposable
    {
        /// <summary>
        /// 初始化渲染上下文，绑定渲染目标
        /// </summary>
        /// <param name="width">渲染宽度（像素）</param>
        /// <param name="height">渲染高度（像素）</param>
        void Initialize(int width, int height);

        /// <summary>
        /// 重置渲染上下文，清空画布并恢复初始状态
        /// </summary>
        void Reset();

        /// <summary>
        /// 保存当前渲染状态
        /// </summary>
        void SaveState();

        /// <summary>
        /// 恢复之前保存的渲染状态
        /// </summary>
        void RestoreState();

        /// <summary>
        /// 设置背景色
        /// </summary>
        /// <param name="color">背景色（ARGB格式）</param>
        void SetBackgroundColor(uint color);

        /// <summary>
        /// 平移画布
        /// </summary>
        /// <param name="dx">X轴平移量</param>
        /// <param name="dy">Y轴平移量</param>
        void Translate(float dx, float dy);

        /// <summary>
        /// 旋转画布
        /// </summary>
        /// <param name="angle">旋转角度（弧度）</param>
        void Rotate(float angle);

        /// <summary>
        /// 缩放画布
        /// </summary>
        /// <param name="sx">X轴缩放因子</param>
        /// <param name="sy">Y轴缩放因子</param>
        void Scale(float sx, float sy);

        /// <summary>
        /// 设置渲染质量
        /// </summary>
        /// <param name="quality">渲染质量</param>
        void SetRenderQuality(RenderQuality quality);

        /// <summary>
        /// 获取渲染结果（位图数据）
        /// </summary>
        /// <returns>位图数据</returns>
        byte[] GetRenderResult();

        /// <summary>
        /// 获取渲染结果宽度
        /// </summary>
        int Width { get; }

        /// <summary>
        /// 获取渲染结果高度
        /// </summary>
        int Height { get; }

        /// <summary>
        /// 渲染配置
        /// </summary>
        RenderConfig Config { get; set; }

        /// <summary>
        /// 设置矩形裁剪区
        /// </summary>
        /// <param name="x">左上角X坐标</param>
        /// <param name="y">左上角Y坐标</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        void SetClipRect(float x, float y, float width, float height);

        /// <summary>
        /// 重置裁剪区
        /// </summary>
        void ResetClip();

        /// <summary>
        /// 将毫米转换为像素
        /// </summary>
        /// <param name="millimeters">毫米值</param>
        /// <returns>像素值</returns>
        float MillimetersToPixels(float millimeters);

        /// <summary>
        /// 将像素转换为毫米
        /// </summary>
        /// <param name="pixels">像素值</param>
        /// <returns>毫米值</returns>
        float PixelsToMillimeters(float pixels);
    }

    /// <summary>
    /// 渲染质量枚举
    /// </summary>
    public enum RenderQuality
    {
        /// <summary>
        /// 性能优先模式
        /// </summary>
        Performance,
        /// <summary>
        /// 高清质量模式
        /// </summary>
        HighQuality
    }
}
