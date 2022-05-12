using ReactiveMarbles.CacheDatabase.Core;

namespace CP.CacheDatabase.Settings.Core
{
    /// <summary>
    /// Empty Base.
    /// </summary>
    /// <seealso cref="AICS.SettingsStorage" />
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
