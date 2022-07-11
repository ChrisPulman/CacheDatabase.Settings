using ReactiveMarbles.Locator;

namespace CP.CacheDatabase.SecureSettings.Locator
{
    public static class SecureExtensions
    {
        /// <summary>
        /// Setup the settings store.
        /// </summary>
        /// <typeparam name="T">The Type of settings store.</typeparam>
        /// <param name="this">The dependency resolver.</param>
        /// <param name="password">The password.</param>
        /// <param name="inUnitTest">if set to <c>true</c> [in unit test].</param>
        /// <returns>
        /// The Settings store.
        /// </returns>
        public static T? SetupSettingsStore<T>(this IEditServices @this, string? password = null, bool inUnitTest = false)
            where T : SettingsStorage?, new()
        {
            var viewSettings = AppInfo.SetupSettingsStore<T>((password ?? AppInfo.ExecutingAssemblyName)!, inUnitTest);
            viewSettings?.InitializeAsync().Wait();
            @this.AddLazySingleton(() => viewSettings!, typeof(T).Name);
            return viewSettings;
        }
    }
}