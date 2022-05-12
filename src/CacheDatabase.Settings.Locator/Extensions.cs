﻿using CP.CacheDatabase.Settings.Core;
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
        public static T? SetupSettingsStore<T>(this IEditServices @this, bool inUnitTest = false)
            where T : SettingsStorage?, new()
        {
            var viewSettings = AppInfo.SetupSettingsStore<T>(inUnitTest);
            viewSettings?.InitializeAsync().Wait();
            @this.AddLazySingleton(() => viewSettings!, typeof(T).Name);
            return viewSettings;
        }
    }
}