using ReactiveMarbles.CacheDatabase.Core;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace CP.CacheDatabase.Settings.Core
{
    public class InMemoryBlobCache : IBlobCache
    {
        private bool _disposed;
        private Dictionary<string, CacheEntry> _cache = new Dictionary<string, CacheEntry>();
        private const string ClassName = nameof(InMemoryBlobCache);
        private readonly IObservable<Unit> _initialized;

        public InMemoryBlobCache(IScheduler? scheduler = null)
        {
            Scheduler = scheduler ?? CoreRegistrations.TaskpoolScheduler;
            _initialized = Initialize();
        }

        /// <summary>
        /// Initializes the database.
        /// </summary>
        /// <returns>An observable.</returns>
        private IObservable<Unit> Initialize()
        {
            var obs = Observable.Create<Unit>(async (obs, _) =>
            {
                try
                {
                    _cache.Clear();
                    await Task.FromResult(Unit.Default);
                    obs.OnNext(Unit.Default);
                    obs.OnCompleted();
                }
                catch (Exception ex)
                {
                    obs.OnError(ex);
                }
            });

            var connected = obs.PublishLast();
            connected.Connect();

            return connected.SubscribeOn(Scheduler);
        }

        public IScheduler Scheduler { get; }

        /// <inheritdoc/>
        public ValueTask DisposeAsync()
        {
            Dispose(false);
#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
            GC.SuppressFinalize(this);
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
            return default;
        }

        /// <inheritdoc/>
        public IObservable<Unit> Flush()
        {
            if (_disposed)
            {
                return IBlobCache.ExceptionHelpers.ObservableThrowObjectDisposedException<Unit>(ClassName);
            }

            // no-op on sql.
            return Observable.Return(Unit.Default);
        }

        /// <inheritdoc/>
        public IObservable<Unit> Flush(Type type)
        {
            if (_disposed)
            {
                return IBlobCache.ExceptionHelpers.ObservableThrowObjectDisposedException<Unit>(ClassName);
            }

            // no-op on sql.
            return Observable.Return(Unit.Default);
        }

        /// <inheritdoc/>
        public IObservable<byte[]?> Get(string key)
        {
            if (_disposed)
            {
                return IBlobCache.ExceptionHelpers.ObservableThrowObjectDisposedException<byte[]>(ClassName);
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                return Observable.Throw<byte[]>(new ArgumentNullException(nameof(key)));
            }

            var time = DateTimeOffset.UtcNow;
            return _initialized
                .Select(_ =>
                    {
                        try
                        {
                            var value = _cache[key];

                            if (value is null)
                            {
                                throw new KeyNotFoundException($"Key {key} could not be found.");
                            }

                            if (value.TypeName is not null)
                            {
                                throw new KeyNotFoundException($"Key {key} is associated with an object.");
                            }

                            if (value.Id is null)
                            {
                                throw new KeyNotFoundException($"Key {key} is id is null.");
                            }

                            if (value.ExpiresAt <= time)
                            {
                                throw new KeyNotFoundException($"Key {key} has expired.");
                            }

                            return value;
                        }
                        catch (Exception)
                        {
                            throw new KeyNotFoundException($"Key {key} could not be found.");
                        }
                    })
                .Select(x => x.Value)
                .Where(x => x is not null)
                .Select(x => x!);
        }

        /// <inheritdoc/>
        public IObservable<KeyValuePair<string, byte[]>> Get(IEnumerable<string> keys)
        {
            if (keys is null)
            {
                return Observable.Throw<KeyValuePair<string, byte[]>>(new ArgumentNullException(nameof(keys)));
            }

            if (_disposed)
            {
                return IBlobCache.ExceptionHelpers.ObservableThrowObjectDisposedException<KeyValuePair<string, byte[]>>(ClassName);
            }

            var time = DateTimeOffset.UtcNow;

            return _initialized.Select((_, _) => _cache.Where(x => x.Value.Id != null && x.Value.ExpiresAt > time && keys.Contains(x.Value.Id)).ToList())
                .SelectMany(x => x)
                .Where(x => x.Value?.Value is not null && x.Value?.Id is not null)
                .Select(x => new KeyValuePair<string, byte[]>(x.Value.Id!, x.Value.Value!));
        }

        /// <inheritdoc/>
        public IObservable<byte[]?> Get(string key, Type type)
        {
            if (key is null)
            {
                return Observable.Throw<byte[]>(new ArgumentNullException(nameof(key)));
            }

            if (type is null)
            {
                return Observable.Throw<byte[]>(new ArgumentNullException(nameof(type)));
            }

            if (_disposed)
            {
                return IBlobCache.ExceptionHelpers.ObservableThrowObjectDisposedException<byte[]>(ClassName);
            }

            var time = DateTimeOffset.UtcNow;
            return _initialized.Select((_, __) => _cache.FirstOrDefault(x => x.Value.Id != null && x.Value.ExpiresAt > time && x.Value.Id == key && x.Value.TypeName == type.FullName).Value)
                .Catch<CacheEntry, InvalidOperationException>(ex => Observable.Throw<CacheEntry>(new KeyNotFoundException(ex.Message)))
                .Where(x => x?.Value is not null)
                .Select(x => x.Value!);
        }

        /// <inheritdoc/>
        public IObservable<KeyValuePair<string, byte[]>> Get(IEnumerable<string> keys, Type type)
        {
            if (keys is null)
            {
                return Observable.Throw<KeyValuePair<string, byte[]>>(new ArgumentNullException(nameof(keys)));
            }

            if (type is null)
            {
                return Observable.Throw<KeyValuePair<string, byte[]>>(new ArgumentNullException(nameof(type)));
            }

            if (_disposed)
            {
                return IBlobCache.ExceptionHelpers.ObservableThrowObjectDisposedException<KeyValuePair<string, byte[]>>(ClassName);
            }

            var time = DateTimeOffset.UtcNow;
            return _initialized.Select((_, _) => _cache.Where(x => x.Value.Id != null && x.Value.ExpiresAt > time && keys.Contains(x.Value.Id) && x.Value.TypeName == type.FullName).ToList())
                .SelectMany(x => x)
                .Where(x => x.Value?.Value is not null && x.Value?.Id is not null)
                .Select(x => new KeyValuePair<string, byte[]>(x.Value.Id!, x.Value.Value!));
        }

        /// <inheritdoc/>
        public IObservable<KeyValuePair<string, byte[]>> GetAll(Type type)
        {
            if (type is null)
            {
                return Observable.Throw<KeyValuePair<string, byte[]>>(new ArgumentNullException(nameof(type)));
            }

            if (_disposed)
            {
                return IBlobCache.ExceptionHelpers.ObservableThrowObjectDisposedException<KeyValuePair<string, byte[]>>(ClassName);
            }

            var time = DateTimeOffset.UtcNow;
            return _initialized.Select((_, _) => _cache.Where(x => x.Value.Id != null && x.Value.ExpiresAt > time && x.Value.TypeName == type.FullName).ToList())
                .SelectMany(x => x)
                .Where(x => x.Value?.Value is not null && x.Value?.Id is not null)
                .Select(x => new KeyValuePair<string, byte[]>(x.Value.Id!, x.Value.Value!));
        }

        /// <inheritdoc/>
        public IObservable<string> GetAllKeys()
        {
            if (_disposed)
            {
                return IBlobCache.ExceptionHelpers.ObservableThrowObjectDisposedException<string>(ClassName);
            }

            var time = DateTimeOffset.UtcNow;
            return _initialized.Select((_, _) => _cache.Where(x => x.Value.Id != null && x.Value.ExpiresAt > time).ToList())
                .SelectMany(x => x)
                .Where(x => x.Value?.Id is not null)
                .Select(x => x.Value.Id!);
        }

        /// <inheritdoc/>
        public IObservable<string> GetAllKeys(Type type)
        {
            if (type is null)
            {
                return Observable.Throw<string>(new ArgumentNullException(nameof(type)));
            }

            if (_disposed)
            {
                return IBlobCache.ExceptionHelpers.ObservableThrowObjectDisposedException<string>(ClassName);
            }

            var time = DateTimeOffset.UtcNow;
            return _initialized.Select((_, _) => _cache.Where(x => x.Value.Id != null && x.Value.ExpiresAt > time && x.Value.TypeName == type.Name).ToList())
                .SelectMany(x => x)
                .Where(x => x.Value?.Id is not null)
                .Select(x => x.Value.Id!);
        }

        /// <inheritdoc/>
        public IObservable<(string Key, DateTimeOffset? Time)> GetCreatedAt(IEnumerable<string> keys)
        {
            if (keys is null)
            {
                return Observable.Throw<(string Key, DateTimeOffset? Time)>(new ArgumentNullException(nameof(keys)));
            }

            if (_disposed)
            {
                return IBlobCache.ExceptionHelpers.ObservableThrowObjectDisposedException<(string Key, DateTimeOffset? Time)>(ClassName);
            }

            var time = DateTimeOffset.UtcNow;
            return _initialized.Select((_, _) => _cache.Where(x => x.Value.Id != null && x.Value.ExpiresAt > time && keys.Contains(x.Value.Id)).ToList())
                .SelectMany(x => x)
                .Where(x => x.Value?.Id is not null)
                .Select(x => (Key: x.Value.Id!, Time: x.Value?.CreatedAt));
        }

        /// <inheritdoc/>
        public IObservable<DateTimeOffset?> GetCreatedAt(string key)
        {
            if (key is null)
            {
                return Observable.Throw<DateTimeOffset?>(new ArgumentNullException(nameof(key)));
            }

            if (_disposed)
            {
                return IBlobCache.ExceptionHelpers.ObservableThrowObjectDisposedException<DateTimeOffset?>(ClassName);
            }

            var time = DateTimeOffset.UtcNow;
            return _initialized.Select((_, _) => _cache.Where(x => x.Value.Id != null && x.Value.ExpiresAt > time && x.Value.Id == key).ToList())
                .SelectMany(x => x)
                .Where(x => x.Value?.Id is not null)
                .Select(x => x.Value?.CreatedAt);
        }

        /// <inheritdoc/>
        public IObservable<(string Key, DateTimeOffset? Time)> GetCreatedAt(IEnumerable<string> keys, Type type)
        {
            if (type is null)
            {
                return Observable.Throw<(string Key, DateTimeOffset? Time)>(new ArgumentNullException(nameof(type)));
            }

            if (keys is null)
            {
                return Observable.Throw<(string Key, DateTimeOffset? Time)>(new ArgumentNullException(nameof(keys)));
            }

            if (_disposed)
            {
                return IBlobCache.ExceptionHelpers.ObservableThrowObjectDisposedException<(string Key, DateTimeOffset? Time)>(ClassName);
            }

            var time = DateTimeOffset.UtcNow;
            return _initialized.Select((_, _) => _cache.Where(x => x.Value.Id != null && x.Value.ExpiresAt > time && keys.Contains(x.Value.Id) && x.Value.TypeName == type.FullName).ToList())
                .SelectMany(x => x)
                .Where(x => x.Value?.Id is not null)
                .Select(x => (Key: x.Value.Id!, Time: x.Value?.CreatedAt));
        }

        /// <inheritdoc/>
        public IObservable<DateTimeOffset?> GetCreatedAt(string key, Type type)
        {
            if (type is null)
            {
                return Observable.Throw<DateTimeOffset?>(new ArgumentNullException(nameof(type)));
            }

            if (key is null)
            {
                return Observable.Throw<DateTimeOffset?>(new ArgumentNullException(nameof(key)));
            }

            if (_disposed)
            {
                return IBlobCache.ExceptionHelpers.ObservableThrowObjectDisposedException<DateTimeOffset?>(ClassName);
            }

            var time = DateTimeOffset.UtcNow;
            return _initialized.Select((_, _) => _cache.Where(x => x.Value.Id != null && x.Value.ExpiresAt > time && x.Value.Id == key && x.Value.TypeName == type.FullName).ToList())
                .SelectMany(x => x)
                .Where(x => x.Value?.Id is not null)
                .Select(x => x.Value?.CreatedAt);
        }

        /// <inheritdoc/>
        public IObservable<Unit> Insert(IEnumerable<KeyValuePair<string, byte[]>> keyValuePairs, DateTimeOffset? absoluteExpiration = null)
        {
            if (keyValuePairs is null)
            {
                return Observable.Throw<Unit>(new ArgumentNullException(nameof(keyValuePairs)));
            }

            if (_disposed)
            {
                return IBlobCache.ExceptionHelpers.ObservableThrowObjectDisposedException<Unit>(ClassName);
            }

            var expiry = (absoluteExpiration ?? DateTimeOffset.MaxValue).UtcDateTime;

            return _initialized.Select((_, _) =>
                {
                    var entries = keyValuePairs.Select(x => new CacheEntry { CreatedAt = DateTime.Now, Id = x.Key, Value = x.Value, ExpiresAt = expiry });

                    foreach (var entry in entries)
                    {
                        if (_cache.ContainsKey(entry.Id!))
                        {
                            _cache[entry.Id!] = entry;
                        }
                        else
                        {
                            _cache.Add(entry.Id!, entry);
                        }
                    }

                    return Unit.Default;
                })
                .Select(_ => Unit.Default);
        }

        /// <inheritdoc/>
        public IObservable<Unit> Insert(string key, byte[] data, DateTimeOffset? absoluteExpiration = null) =>
            Insert(new[] { new KeyValuePair<string, byte[]>(key, data) }, absoluteExpiration);

        /// <inheritdoc/>
        public IObservable<Unit> Insert(IEnumerable<KeyValuePair<string, byte[]>> keyValuePairs, Type type, DateTimeOffset? absoluteExpiration = null)
        {
            if (type is null)
            {
                return Observable.Throw<Unit>(new ArgumentNullException(nameof(type)));
            }

            if (keyValuePairs is null)
            {
                return Observable.Throw<Unit>(new ArgumentNullException(nameof(keyValuePairs)));
            }

            if (_disposed)
            {
                return IBlobCache.ExceptionHelpers.ObservableThrowObjectDisposedException<Unit>(ClassName);
            }

            var expiry = (absoluteExpiration ?? DateTimeOffset.MaxValue).UtcDateTime;

            return _initialized.Select((_, _) =>
                {
                    var entries = keyValuePairs.Select(x => new CacheEntry { CreatedAt = DateTime.Now, Id = x.Key, Value = x.Value, ExpiresAt = expiry, TypeName = type.FullName });

                    foreach (var entry in entries)
                    {
                        if (_cache.ContainsKey(entry.Id!))
                        {
                            _cache[entry.Id!] = entry;
                        }
                        else
                        {
                            _cache.Add(entry.Id!, entry);
                        }
                    }

                    return Unit.Default;
                })
                .Select(_ => Unit.Default);
        }

        /// <inheritdoc/>
        public IObservable<Unit> Insert(string key, byte[] data, Type type, DateTimeOffset? absoluteExpiration = null)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return Observable.Throw<Unit>(new ArgumentException($"'{nameof(key)}' cannot be null or whitespace.", nameof(key)));
            }

            if (data is null)
            {
                return Observable.Throw<Unit>(new ArgumentNullException(nameof(data)));
            }

            if (type is null)
            {
                return Observable.Throw<Unit>(new ArgumentNullException(nameof(type)));
            }

            return Insert(new[] { new KeyValuePair<string, byte[]>(key, data) }, type, absoluteExpiration);
        }

        /// <inheritdoc/>
        public IObservable<Unit> Invalidate(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return Observable.Throw<Unit>(new ArgumentException($"'{nameof(key)}' cannot be null or whitespace.", nameof(key)));
            }

            return Invalidate(new[] { key });
        }

        /// <inheritdoc/>
        public IObservable<Unit> Invalidate(string key, Type type)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return Observable.Throw<Unit>(new ArgumentException($"'{nameof(key)}' cannot be null or whitespace.", nameof(key)));
            }

            if (type is null)
            {
                return Observable.Throw<Unit>(new ArgumentNullException(nameof(type)));
            }

            return Invalidate(new[] { key }, type);
        }

        /// <inheritdoc/>
        public IObservable<Unit> Invalidate(IEnumerable<string> keys)
        {
            if (keys is null)
            {
                return Observable.Throw<Unit>(new ArgumentNullException(nameof(keys)));
            }

            if (_disposed)
            {
                return IBlobCache.ExceptionHelpers.ObservableThrowObjectDisposedException<Unit>(ClassName);
            }

            return _initialized.Select(
                (_) =>
                {
                    foreach (var key in keys)
                    {
                        _cache.Remove(key);
                    }

                    return Unit.Default;
                });
        }

        /// <inheritdoc/>
        public IObservable<Unit> Invalidate(IEnumerable<string> keys, Type type)
        {
            if (type is null)
            {
                return Observable.Throw<Unit>(new ArgumentNullException(nameof(type)));
            }

            if (keys is null)
            {
                return Observable.Throw<Unit>(new ArgumentNullException(nameof(keys)));
            }

            if (_disposed)
            {
                return IBlobCache.ExceptionHelpers.ObservableThrowObjectDisposedException<Unit>(ClassName);
            }

            return _initialized.Select(
                (_) =>
                {
                    var entries = _cache.Where(x => keys.Contains(x.Value.Id) && x.Value.TypeName == type.FullName).ToList();
                    foreach (var key in entries)
                    {
                        _cache.Remove(key.Value.Id!);
                    }

                    return Unit.Default;
                });
        }

        /// <inheritdoc/>
        public IObservable<Unit> InvalidateAll(Type type)
        {
            if (_disposed)
            {
                return IBlobCache.ExceptionHelpers.ObservableThrowObjectDisposedException<Unit>(ClassName);
            }

            return _initialized.Select(
                (_) =>
                {
                    var entries = _cache.Where(x => x.Value.TypeName == type.FullName).ToList();
                    foreach (var key in entries)
                    {
                        _cache.Remove(key.Value.Id!);
                    }

                    return Unit.Default;
                });
        }

        /// <inheritdoc/>
        public IObservable<Unit> InvalidateAll()
        {
            if (_disposed)
            {
                return IBlobCache.ExceptionHelpers.ObservableThrowObjectDisposedException<Unit>(ClassName);
            }

            return _initialized.Select(
                (_) =>
                {
                    var entries = _cache.Where(x => x.Value.TypeName == null).ToList();
                    foreach (var key in entries)
                    {
                        _cache.Remove(key.Value.Id!);
                    }

                    return Unit.Default;
                });
        }

        /// <inheritdoc/>
        public IObservable<Unit> Vacuum()
        {
            if (_disposed)
            {
                return IBlobCache.ExceptionHelpers.ObservableThrowObjectDisposedException<Unit>(ClassName);
            }

            return _initialized.Select((_, _) =>
            {
                return Unit.Default;
            });
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}