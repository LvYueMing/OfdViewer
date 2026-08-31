using OFDViewer.Models.BaseStructure.DocumentRoot;
using OFDViewer.Models.BaseStructure.MainEntry;
using OFDViewer.Utils;
using Xunit;

namespace OFDViewer.Tests
{
    /// <summary>
    /// DateParser 健壮日期解析测试
    /// 覆盖：PDF 日期（D:YYYYMMDDHHmmSS+HH'mm'）、ISO 8601、紧凑格式、无效值容错
    /// </summary>
    public class DateParserTests
    {
        #region PDF 日期格式（D: 前缀）

        [Fact]
        public void TryParse_PdfDateWithTimeZone_ReturnsUtc()
        {
            // D:20180701152117+08'00' = 2018-07-01 15:21:17 (+08:00) = UTC 2018-07-01 07:21:17
            bool ok = DateParser.TryParse("D:20180701152117+08'00'", out var result);

            Assert.True(ok);
            Assert.Equal(new DateTime(2018, 7, 1, 7, 21, 17, DateTimeKind.Utc), result);
        }

        [Fact]
        public void TryParse_PdfDateUtcSuffix_ReturnsUtc()
        {
            bool ok = DateParser.TryParse("D:20180701152117Z", out var result);

            Assert.True(ok);
            Assert.Equal(new DateTime(2018, 7, 1, 15, 21, 17, DateTimeKind.Utc), result);
        }

        [Fact]
        public void TryParse_PdfDateNegativeOffset_ReturnsUtc()
        {
            // D:20180701152117-05'00' = 2018-07-01 15:21:17 (-05:00) = UTC 2018-07-01 20:21:17
            bool ok = DateParser.TryParse("D:20180701152117-05'00'", out var result);

            Assert.True(ok);
            Assert.Equal(new DateTime(2018, 7, 1, 20, 21, 17, DateTimeKind.Utc), result);
        }

        [Fact]
        public void TryParse_PdfDatePartialFields_CompletesWithDefaults()
        {
            // PDF 规范允许省略部分字段：D:2018 → 2018-01-01 00:00:00
            bool ok = DateParser.TryParse("D:2018", out var result);

            Assert.True(ok);
            Assert.Equal(new DateTime(2018, 1, 1, 0, 0, 0, DateTimeKind.Utc), result);
        }

        #endregion

        #region 标准 OFD / ISO 8601 格式

        [Fact]
        public void TryParse_IsoDate_ReturnsDate()
        {
            bool ok = DateParser.TryParse("2018-07-01", out var result);

            Assert.True(ok);
            Assert.Equal(new DateTime(2018, 7, 1), result);
        }

        [Fact]
        public void TryParse_IsoDateTimeWithTimeZone_ReturnsUtc()
        {
            bool ok = DateParser.TryParse("2018-07-01T15:21:17+08:00", out var result);

            Assert.True(ok);
            Assert.Equal(new DateTime(2018, 7, 1, 7, 21, 17, DateTimeKind.Utc), result);
        }

        [Fact]
        public void TryParse_CompactDate_ReturnsDate()
        {
            bool ok = DateParser.TryParse("20180701", out var result);

            Assert.True(ok);
            Assert.Equal(new DateTime(2018, 7, 1), result);
        }

        #endregion

        #region 无效值容错

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("not-a-date")]
        [InlineData("D:")]
        [InlineData("D:20")]
        public void TryParse_InvalidInput_ReturnsFalse(string? value)
        {
            bool ok = DateParser.TryParse(value, out _);

            Assert.False(ok);
        }

        #endregion

        #region CT_DocInfo 集成（回归：修复 DateTime.Parse 崩溃）

        [Fact]
        public void CT_DocInfo_ModDateString_PdfDate_DoesNotThrow()
        {
            // 回归测试：D: 前缀的 PDF 日期不应导致解析异常
            var docInfo = new CT_DocInfo();

            docInfo.ModDateString = "D:20180701152117+08'00'";

            Assert.Equal(new DateTime(2018, 7, 1, 7, 21, 17, DateTimeKind.Utc), docInfo.ModDate);
        }

        [Fact]
        public void CT_DocInfo_CreationDateString_PdfDate_DoesNotThrow()
        {
            var docInfo = new CT_DocInfo();

            docInfo.CreationDateString = "D:20180701152117Z";

            Assert.Equal(new DateTime(2018, 7, 1, 15, 21, 17, DateTimeKind.Utc), docInfo.CreationDate);
        }

        [Fact]
        public void CT_DocInfo_ModDateString_InvalidValue_KeepsDefault()
        {
            // 容错：无效日期不抛异常，保留默认值（DateTime.MinValue）
            var docInfo = new CT_DocInfo();

            docInfo.ModDateString = "not-a-date";

            Assert.Equal(DateTime.MinValue, docInfo.ModDate);
        }

        #endregion

        #region PermissionValidPeriod 集成（回归：修复 DateTime.Parse 崩溃）

        [Fact]
        public void PermissionValidPeriod_PdfDate_DoesNotThrow()
        {
            var period = new PermissionValidPeriod();

            period.StartDateString = "D:20180701152117+08'00'";
            period.EndDateString = "D:20181231235959Z";

            Assert.Equal(new DateTime(2018, 7, 1, 7, 21, 17, DateTimeKind.Utc), period.StartDate);
            Assert.Equal(new DateTime(2018, 12, 31, 23, 59, 59, DateTimeKind.Utc), period.EndDate);
        }

        [Fact]
        public void PermissionValidPeriod_EmptyValue_SetsNull()
        {
            var period = new PermissionValidPeriod();

            period.StartDateString = "";
            period.EndDateString = null;

            Assert.Null(period.StartDate);
            Assert.Null(period.EndDate);
        }

        [Fact]
        public void PermissionValidPeriod_InvalidValue_SetsNull()
        {
            // 容错：无效日期不抛异常，置 null
            var period = new PermissionValidPeriod();

            period.StartDateString = "not-a-date";

            Assert.Null(period.StartDate);
        }

        #endregion
    }
}
