using ReactiveMarbles.CacheDatabase.EncryptedSettings;
using ReactiveMarbles.CacheDatabase.Settings.Core;
using Splat;

namespace CP.CacheDatabase.SecureSettings.Splat
{
    public static class SecureExtensions
    {
        /// <summary>
        /// Setup the settings store.
        /// </summary>
        /// <typeparam name="T">The Type of settings store.</typeparam>
        /// <param name="this">The dependency resolver.</param>
        /// <returns>The Settings store.</returns>
        public static async Task<T?> SetupSettingsStoreAsync<T>(this IMutableDependencyResolver @this, string? password = null, bool inUnitTest = false)
            where T : ISettingsStorage?, new()
        {
            var viewSettings = await AppInfo.SetupSettingsStore<T>((password ?? AppInfo.ExecutingAssemblyName)!, inUnitTest);
            @this.RegisterLazySingleton(() => viewSettings!, typeof(T).Name);
            return viewSettings;
        }
    }
}