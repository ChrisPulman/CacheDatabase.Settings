using ReactiveMarbles.CacheDatabase.Core;
#if IS_SECURE
using CP.CacheDatabase.SecureSettings;
#endif

namespace CP.CacheDatabase.Settings.Core
{

    /// <summary>
    /// SettingsBase.
    /// </summary>
    /// <seealso cref="CP.CacheDatabase.SettingsStorage" />
    public abstract class SettingsBase : SettingsStorage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsBase"/> class.
        /// </summary>
        /// <param name="className">Name of the class.</param>
        /// <param name="blobCache">The BLOB cache.</param>
        protected SettingsBase(string className, IBlobCache? blobCache = null)
            : base($"__{className}__", blobCache ?? AppInfo.SettingsCache)
        {
        }
    }
}
