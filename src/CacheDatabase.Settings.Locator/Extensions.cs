using ReactiveMarbles.CacheDatabase.Settings;
using ReactiveMarbles.CacheDatabase.Settings.Core;
using ReactiveMarbles.Locator;

namespace CP.CacheDatabase.Settings.Locator
{
    public static class Extensions
    {
        /// <summary>
        /// Setup the settings store.
        /// </summary>
        /// <typeparam name="T">The Type of settings store.</typeparam>
        /// <param name="this">The dependency resolver.</param>
        /// <returns>The Settings store.</returns>
        public static async Task<T?> SetupSettingsStore<T>(this IEditServices @this, bool inUnitTest = false)
            where T : ISettingsStorage?, new()
        {
            var viewSettings = await AppInfo.SetupSettingsStore<T>(inUnitTest);
            @this.AddLazySingleton(() => viewSettings!, typeof(T).Name);
            return viewSettings;
        }
    }
}