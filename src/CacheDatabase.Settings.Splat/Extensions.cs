using ReactiveMarbles.CacheDatabase.Settings;
using ReactiveMarbles.CacheDatabase.Settings.Core;
using Splat;

namespace CP.CacheDatabase.Settings.Splat
{
    public static class Extensions
    {
        /// <summary>
        /// Setup the settings store.
        /// </summary>
        /// <typeparam name="T">The Type of settings store.</typeparam>
        /// <param name="this">The dependency resolver.</param>
        /// <returns>The Settings store.</returns>
        public static async Task<T?> SetupSettingsStore<T>(this IMutableDependencyResolver @this, bool inUnitTest = false)
            where T : ISettingsStorage?, new()
        {
            var viewSettings = await AppInfo.SetupSettingsStore<T>(inUnitTest);
            @this.RegisterLazySingleton(() => viewSettings!, typeof(T).Name);
            return viewSettings;
        }
    }
}