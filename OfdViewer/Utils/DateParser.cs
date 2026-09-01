using System.Globalization;

namespace OFDViewer.Utils
{
    /// <summary>
    /// 健壮日期解析辅助类。
    /// 除 OFD 标准（xs:date / xs:dateTime，ISO 8601）外，兼容常见的非标准格式：
    /// - PDF 日期格式：D:YYYYMMDDHHmmSSOHH'mm'（如 D:20180701152117+08'00'，O 为 +/-/Z）
    /// - 紧凑数字格式：yyyyMMdd / yyyyMMddHHmmss
    /// 解析失败返回 false，不抛异常，供元数据字段做容错处理。
    /// </summary>
    public static class DateParser
    {
        private static readonly string[] ExactFormats =
        {
            "yyyy-MM-dd'T'HH:mm:sszzz",
            "yyyy-MM-dd'T'HH:mm:ss",
            "yyyy-MM-dd HH:mm:ss",
            "yyyy-MM-dd",
            "yyyyMMddHHmmsszzz",
            "yyyyMMddHHmmss",
            "yyyyMMdd"
        };

        /// <summary>
        /// 尝试解析日期字符串。
        /// 支持：PDF 日期（D: 前缀）、ISO 8601、常用日期时间格式、紧凑数字格式。
        /// </summary>
        /// <param name="value">日期字符串</param>
        /// <param name="result">解析结果；带时区的输入统一转换为 UTC</param>
        /// <returns>解析成功返回 true</returns>
        public static bool TryParse(string value, out DateTime result)
        {
            result = default;
            if (string.IsNullOrWhiteSpace(value))
                return false;

            string text = value.Trim();

            // PDF 日期格式：D:YYYYMMDDHHmmSSOHH'mm'
            if (text.StartsWith("D:", StringComparison.OrdinalIgnoreCase))
            {
                return TryParsePdfDate(text.Substring(2), out result);
            }

            // ISO 8601 / 常用格式
            if (HasTimeZoneIndicator(text))
            {
                // 带时区 → 统一转换为 UTC（与 PDF 日期路径行为一致）
                if (DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture,
                        DateTimeStyles.AllowWhiteSpaces, out var dto))
                {
                    result = dto.UtcDateTime;
                    return true;
                }
            }
            else
            {
                // 无时区 → 按原值解析（不转换）
                if (DateTime.TryParse(text, CultureInfo.InvariantCulture,
                        DateTimeStyles.AllowWhiteSpaces, out result))
                {
                    return true;
                }
            }

            // 紧凑数字格式兜底
            return DateTime.TryParseExact(text, ExactFormats,
                CultureInfo.InvariantCulture, DateTimeStyles.None, out result);
        }

        /// <summary>
        /// 检测字符串是否带时区指示（Z 后缀，或日期时间部分之后的 +/- 偏移）。
        /// </summary>
        private static bool HasTimeZoneIndicator(string s)
        {
            if (s.EndsWith("Z", StringComparison.OrdinalIgnoreCase))
                return true;
            if (s.IndexOf('+') > 4)
                return true;   // '+' 只会出现在时区偏移（年份数字中不含 +）

            // '-' 偏移出现在时间部分之后；日期分隔符 '-' 位于索引 4/7，需跳过
            int lastDash = s.LastIndexOf('-');
            return lastDash >= 12;
        }

        /// <summary>
        /// 解析 PDF 日期：YYYY[MM[DD[HH[mm[SS]]]]] 后跟可选时区（+HH'mm' / -HH'mm' / Z）。
        /// PDF 规范（ISO 32000-1 7.9.4）允许省略部分字段，省略项取最小值。
        /// </summary>
        private static bool TryParsePdfDate(string s, out DateTime result)
        {
            result = default;
            s = s.Trim();
            if (s.Length < 4)
                return false;

            // 定位时区符号（+/-/Z），从年份之后（索引 4）开始找
            int zoneStart = -1;
            for (int i = 4; i < s.Length; i++)
            {
                char c = s[i];
                if (c is '+' or '-' or 'Z' or 'z')
                {
                    zoneStart = i;
                    break;
                }
            }

            string body = zoneStart >= 0 ? s.Substring(0, zoneStart) : s;
            int offsetHours = 0, offsetMinutes = 0;

            if (zoneStart >= 0)
            {
                string zone = s.Substring(zoneStart);
                if (zone[0] is not ('Z' or 'z'))
                {
                    // 去掉撇号：+08'00' → +0800
                    string digits = zone.Replace("'", "");
                    if (digits.Length >= 3 && int.TryParse(digits.AsSpan(1, 2), out int hh))
                    {
                        offsetHours = hh;
                        if (digits.Length >= 5 && int.TryParse(digits.AsSpan(3, 2), out int mm))
                            offsetMinutes = mm;
                    }
                    if (zone[0] == '-')
                    {
                        offsetHours = -offsetHours;
                        offsetMinutes = -offsetMinutes;
                    }
                }
            }

            // 逐段补齐：YYYY[MM[DD[HH[mm[SS]]]]]
            if (!TryGetDigits(body, 0, 4, out int year))
                return false;
            int month = TryGetDigits(body, 4, 2, out var mo) ? mo : 1;
            int day   = TryGetDigits(body, 6, 2, out var d)  ? d  : 1;
            int hour  = TryGetDigits(body, 8, 2, out var h)  ? h  : 0;
            int min   = TryGetDigits(body, 10, 2, out var mi) ? mi : 0;
            int sec   = TryGetDigits(body, 12, 2, out var se) ? se : 0;

            try
            {
                // DateTimeOffset 构造自动校验日期范围；.UtcDateTime 将带偏移时间转为 UTC
                result = new DateTimeOffset(year, month, day, hour, min, sec,
                    new TimeSpan(offsetHours, offsetMinutes, 0)).UtcDateTime;
                return true;
            }
            catch (ArgumentOutOfRangeException)
            {
                return false;   // 无效日期（如月份 13、20180732）
            }
        }

        private static bool TryGetDigits(string s, int start, int length, out int value)
        {
            value = 0;
            return start + length <= s.Length
                && int.TryParse(s.AsSpan(start, length), out value);
        }
    }
}
