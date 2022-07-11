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
        public static T? SetupSettingsStore<T>(this IMutableDependencyResolver @this, string? password = null, bool inUnitTest = false)
            where T : SettingsStorage?, new()
        {
            var viewSettings = AppInfo.SetupSettingsStore<T>((password ?? AppInfo.ExecutingAssemblyName)!, inUnitTest);
            viewSettings?.InitializeAsync().Wait();
            @this.RegisterLazySingleton(() => viewSettings!, typeof(T).Name);
            return viewSettings;
        }
    }
}