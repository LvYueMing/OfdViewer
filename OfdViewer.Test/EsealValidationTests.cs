using OfdViewer.ESeal.Abstractions.Models;
using OfdViewer.ESeal.Abstractions.Exceptions;
using OfdViewer.ESeal.Implementations.Common;
using OfdViewer.ESeal.Implementations.Gomain;
using Xunit;

namespace OFDViewer.Tests
{
    /// <summary>
    /// 电子签章验签结果语义测试。
    /// </summary>
    public class EsealValidationTests
    {
        /// <summary>
        /// 国脉深度验签未实现时必须返回不支持，禁止把结构检查当作验签成功。
        /// </summary>
        [Fact]
        public async Task GomainValidateAsync_CryptographicVerificationUnavailable_ReturnsUnsupported()
        {
            using var parser = new GomainEsealParser();
            byte[] gomainLikeData = { 0x30, 0x81, 0x01, 0x00, 0x47, 0x4D, 0x00, 0x00 };

            var result = await parser.ValidateAsync(gomainLikeData);

            Assert.False(result.IsValid);
            Assert.Equal(ValidationStatusCodes.UnsupportedAlgorithm, result.StatusCode);
            Assert.Contains("尚未实现", result.Message);
            Assert.Contains(result.Errors, error => error.Contains("未执行签名值验证"));
        }

        /// <summary>
        /// 默认解析器识别出合法图片时仍不能返回数字签名验证成功。
        /// </summary>
        [Fact]
        public async Task DefaultValidateAsync_ValidImageWithoutSignature_ReturnsUnsupported()
        {
            using var parser = new DefaultEsealParser();
            byte[] pngData = Convert.FromBase64String(
                "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=");

            var result = await parser.ValidateAsync(pngData);

            Assert.False(result.IsValid);
            Assert.Equal(ValidationStatusCodes.UnsupportedAlgorithm, result.StatusCode);
            Assert.Contains("不能执行数字签名验证", result.Message);
            Assert.Contains(result.Errors, error => error.Contains("未执行数字签名验证"));
        }

        /// <summary>
        /// 默认解析器收到损坏图片时应返回通用失败，而不是算法不支持。
        /// </summary>
        [Fact]
        public async Task DefaultValidateAsync_InvalidImage_ReturnsGeneralFailure()
        {
            using var parser = new DefaultEsealParser();
            byte[] invalidImageData = { 0x47, 0x49, 0x46, 0x38, 0x00, 0x00, 0x00, 0x00 };

            var result = await parser.ValidateAsync(invalidImageData);

            Assert.False(result.IsValid);
            Assert.Equal(ValidationStatusCodes.GeneralFailure, result.StatusCode);
            Assert.Contains("格式无效", result.Message);
        }

        /// <summary>
        /// 国脉解析器会容错加载未知数据，但不得因此把数据判定为验签成功。
        /// </summary>
        [Fact]
        public async Task GomainLoadAsync_ToleratedUnknownData_StillDoesNotValidate()
        {
            using var parser = new GomainEsealParser();
            byte[] invalidData = { 0x30, 0x81, 0x01, 0x00, 0x47, 0x4D, 0x00, 0x00 };

            var sealInfo = await parser.LoadAsync(invalidData);
            var validation = await parser.ValidateAsync(invalidData);

            Assert.NotNull(sealInfo);
            Assert.False(validation.IsValid);
            Assert.Equal(ValidationStatusCodes.UnsupportedAlgorithm, validation.StatusCode);
        }

        /// <summary>
        /// 默认解析器收到损坏图片时，加载操作应返回明确的格式异常。
        /// </summary>
        [Fact]
        public async Task DefaultLoadAsync_InvalidImage_ThrowsFormatException()
        {
            using var parser = new DefaultEsealParser();
            byte[] invalidImageData = { 0x47, 0x49, 0x46, 0x38, 0x00, 0x00, 0x00, 0x00 };

            await Assert.ThrowsAsync<EsealFormatException>(() => parser.LoadAsync(invalidImageData));
        }

        /// <summary>
        /// 解析器释放后不得继续执行加载或验签操作。
        /// </summary>
        [Fact]
        public async Task ParserAfterDispose_PublicOperationsThrowObjectDisposedException()
        {
            var parser = new DefaultEsealParser();
            parser.Dispose();
            byte[] imageData = Convert.FromBase64String(
                "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=");

            await Assert.ThrowsAsync<ObjectDisposedException>(() => parser.LoadAsync(imageData));
            await Assert.ThrowsAsync<ObjectDisposedException>(() => parser.ValidateAsync(imageData));
        }
    }
}
