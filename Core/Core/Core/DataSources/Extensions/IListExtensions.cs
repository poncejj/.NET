namespace Core.DataSources.Extensions {

    public static class IListExtensions {

        public static bool IsNullOrEmpty<T>(this IList<T> collection) {
            return collection == null || collection.Count == 0;
        }
    }
}