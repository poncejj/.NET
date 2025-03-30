namespace Core.DataSources.Extensions {

    public static class IntegerExtensions {

        public static int? ParseOrDefault(string s, int? defaultValue) {
            try {
                return Int32.Parse(s);
            }
            catch {
                return defaultValue ?? default(int?);
            }
        }
    }
}