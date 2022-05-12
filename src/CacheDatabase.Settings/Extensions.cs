using ReactiveMarbles.CacheDatabase.Core;
using System.Reactive.Linq;

namespace CP.CacheDatabase.Settings
{
    public static class Extensions
    {
        public static IObservable<T?> GetOrInsertObject<T>(this IBlobCache blobCache, string key, Func<T> fetchFunc)
        {
            if (blobCache is null)
            {
                throw new ArgumentNullException(nameof(blobCache));
            }

            return blobCache.GetObject<T>(key).Catch<T?, Exception>(ex =>
            {
                var value = fetchFunc();
                blobCache.InsertObject(key, value).Wait();
                return Observable.Return(value);
            });
        }
    }
}
