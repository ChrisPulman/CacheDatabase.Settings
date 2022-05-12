using System.Globalization;
using System.Reactive.Linq;

namespace CP.CacheDatabase.Settings
{
    /// <summary>
    /// Exception Helper.
    /// </summary>
    internal static class ExceptionHelper
    {
        public static IObservable<T> ObservableThrowKeyNotFoundException<T>(string key, Exception? innerException = null) => 
            Observable.Throw<T>(
            new KeyNotFoundException(
                string.Format(
                CultureInfo.InvariantCulture,
                "The given key '{0}' was not present in the cache.",
                key),
                innerException));

        public static IObservable<T> ObservableThrowObjectDisposedException<T>(string obj, Exception? innerException = null) => 
            Observable.Throw<T>(
            new ObjectDisposedException(
                string.Format(
                CultureInfo.InvariantCulture,
                "The cache '{0}' was disposed.",
                obj),
                innerException));
    }
}