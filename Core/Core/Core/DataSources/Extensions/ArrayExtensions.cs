namespace Core.DataSources.Extensions {

    public static class ArrayExtensions {

        public static T[] Add<T>(this T[] target, T item, bool prepend = false) {
            if (target == null) {
                throw new ArgumentNullException();
            }

            T[] result = new T[target.Length + 1];
            if (!prepend) {
                target.CopyTo(result, 0);
                result[target.Length] = item;
            }
            else {
                result[0] = item;
                Array.Copy(target, 0, result, 1, target.Length);
            }

            return result;
        }
    }
}