using CP.CacheDatabase.Settings.Core;
using ReactiveMarbles.CacheDatabase.Core;
#if IS_SECURE
using ReactiveMarbles.CacheDatabase.EncryptedSqlite3;
#else
using ReactiveMarbles.CacheDatabase.Sqlite3;
#endif
using System.Diagnostics;
using System.Reflection;

#if IS_SECURE
namespace CP.CacheDatabase.SecureSettings
#else
namespace CP.CacheDatabase.Settings
#endif
{
    /// <summary>
    /// App Info.
    /// </summary>
    public static class AppInfo
    {
        private static readonly Lazy<IBlobCache> _settingsCache;

        /// <summary>
        /// The application root path.
        /// </summary>
        public static readonly string? ApplicationRootPath;

        /// <summary>
        /// The BLOB cache path.
        /// </summary>
        public static readonly string? SettingsCachePath;

        /// <summary>
        /// The executing assembly.
        /// </summary>
        public static readonly Assembly ExecutingAssembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();

        /// <summary>
        /// The executing assembly.
        /// </summary>
        public static readonly string? ExecutingAssemblyName;

        /// <summary>
        /// The version.
        /// </summary>
        public static readonly Version? Version;

        public static IBlobCache SettingsCache { get; set; }

        static AppInfo()
        {
            _settingsCache = new Lazy<IBlobCache>(() => new InMemoryBlobCache());
            SettingsCache = _settingsCache.Value;
            ExecutingAssemblyName = ExecutingAssembly.FullName!.Split(',')[0];

            ApplicationRootPath = Path.Combine(Path.GetDirectoryName(ExecutingAssembly.Location)!, "..");
            SettingsCachePath = Path.Combine(ApplicationRootPath, "SettingsCache");
            Version = ExecutingAssembly.GetName().Version;
        }

#if IS_SECURE
        /// <summary>
        /// Setup the secure settings store.
        /// </summary>
        /// <typeparam name="T">The Type of settings store.</typeparam>
        /// <param name="password">Secure password.</param>
        /// <param name="inUnitTest">In a Unit Test.</param>
        /// <returns>The Settings store.</returns>
        public static async Task<T?> SetupSettingsStore<T>(string password, bool inUnitTest = false)
            where T : SettingsStorage?, new()
        {
            if (!inUnitTest)
            {
                Directory.CreateDirectory(SettingsCachePath!);
                SettingsCache = new EncryptedSqliteBlobCache(Path.Combine(SettingsCachePath!, $"{typeof(T).Name}.db"), password);
            }

            var viewSettings = inUnitTest ? (T?)Activator.CreateInstance(typeof(T), new InMemoryBlobCache()) : new();
            await viewSettings!.InitializeAsync().ConfigureAwait(false);
            return viewSettings;
        }
#else

        /// <summary>
        /// Setup the settings store.
        /// </summary>
        /// <typeparam name="T">The Type of settings store.</typeparam>
        /// <param name="inUnitTest">In a Unit Test.</param>
        /// <returns>The Settings store.</returns>
        public static async Task<T?> SetupSettingsStore<T>(bool inUnitTest = false)
            where T : SettingsStorage?, new()
        {
            if (!inUnitTest)
            {
                Directory.CreateDirectory(SettingsCachePath!);
                SettingsCache = new SqliteBlobCache(Path.Combine(SettingsCachePath!, $"{typeof(T).Name}.db"));
            }

            var viewSettings = inUnitTest ? (T?)Activator.CreateInstance(typeof(T), new InMemoryBlobCache()) : new();
            await viewSettings!.InitializeAsync().ConfigureAwait(false);
            return viewSettings;
        }
#endif
    }
}
