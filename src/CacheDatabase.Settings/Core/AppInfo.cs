using ReactiveMarbles.CacheDatabase.Core;
using System.Diagnostics;
using System.Reflection;

namespace CP.CacheDatabase.Settings.Core
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
    }
}
