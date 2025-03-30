using System.Text.RegularExpressions;

namespace Core.DataSources.Extensions {

    public static class DateExtensions {

        public static string ToDateFormat(this DateTime? date) {
            return date?.ToString("yyyyMMdd") ?? string.Empty;
        }

        public static string ToDateFormatWithTime(this DateTime? date) {
            return date?.ToString("yyyyMMddHHmmss") ?? string.Empty;
        }

        public static string ToDateFormatWithTime(this DateTime date) {
            return date.ToString("yyyyMMddHHmmss") ?? string.Empty;
        }

        public static string ToDateFormatWithTimeISO8601(this DateTime date) {
            return date.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'") ?? string.Empty;
        }

        public static DateTime? ParsePart(string s, string regex, string format, IFormatProvider provider, DateTime? defaultValue) {
            try {
                var match = Regex.Match(s, regex);
                if (match.Success) {
                    return DateExtensions.ParseExactOrDefault(match.Groups[0]?.ToString(), format, provider, defaultValue);
                }
                else {
                    return defaultValue ?? default(DateTime?);
                }
            }
            catch {
                return defaultValue ?? default(DateTime?);
            }
        }

        public static DateTime? ParseExactOrDefault(string s, string format, IFormatProvider provider, DateTime? defaultValue) {
            try {
                return DateTime.ParseExact(s, format, provider);
            }
            catch {
                return defaultValue ?? default(DateTime?);
            }
        }
    }
}