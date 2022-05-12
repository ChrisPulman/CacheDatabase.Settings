using CP.CacheDatabase.Settings.Core;
using ReactiveMarbles.CacheDatabase.Core;
using ReactiveMarbles.CacheDatabase.Sqlite3;
using ReactiveMarbles.Locator;
using System.Reactive.Linq;

namespace CP.CacheDatabase.Settings
{
    public static class Extensions
    {
        /// <summary>
        /// Setup the settings store.
        /// </summary>
        /// <typeparam name="T">The Type of settings store.</typeparam>
        /// <param name="this">The dependency resolver.</param>
        /// <returns>The Settings store.</returns>
        public static T? SetupSettingsStore<T>(this IEditServices @this, bool inUnitTest = false)
            where T : SettingsStorage?, new()
        {
            if (!inUnitTest)
            {
                Directory.CreateDirectory(AppInfo.SettingsCachePath!);
                AppInfo.SettingsCache = new SqliteBlobCache(Path.Combine(AppInfo.SettingsCachePath!, $"{typeof(T).Name}.db"));

                var newBlobCache = AppInfo.SettingsCache;
            }

            var viewSettings = inUnitTest ? (T?)Activator.CreateInstance(typeof(T), new InMemoryBlobCache()) : new();
            @this.AddLazySingleton(() => viewSettings!, typeof(T).Name);
            viewSettings?.InitializeAsync().Wait();
            return viewSettings;
        }

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
