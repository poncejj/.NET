using System.Globalization;

namespace Core.DataSources.Extensions {

    public static class StringExtensions {

        public static DateTime? ToDateFromStringIso8601Format(this string dateAsString) {
            return string.IsNullOrWhiteSpace(dateAsString) ? null : ParseToDate(dateAsString, "yyyyMMdd");
        }

        public static DateTime? ToDateFromStringWithHHMM(this string dateAsString) {
            return string.IsNullOrWhiteSpace(dateAsString) ? null : ParseToDate(dateAsString, "yyyyMMddHHmm");
        }

        private static DateTime? ParseToDate(string dateToParse, string format) {
            return DateTime.TryParseExact(dateToParse, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result) ? result : (DateTime?)null;
        }

        public static string RemoveSpecialCharacters(this string text) {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            return text.Replace("\n", string.Empty)
                       .Replace("\r", string.Empty)
                       .Replace("\r\n", string.Empty)
                       .Replace("\t", string.Empty)
                       .Replace("\"", string.Empty)
                       .Replace(",", string.Empty)
                       .Replace("'", string.Empty)
                       .Replace("&", string.Empty)
                       .Replace(Environment.NewLine, string.Empty);
        }
    }
}