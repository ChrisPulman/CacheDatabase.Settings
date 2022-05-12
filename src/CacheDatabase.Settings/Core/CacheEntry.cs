namespace CP.CacheDatabase.Settings.Core
{
    /// <summary>
    /// A entry in a memory cache.
    /// </summary>
    internal class CacheEntry
    {
        /// <summary>
        /// Gets or sets the ID.
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the entry was created.
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the entry will expire.
        /// </summary>
        public DateTimeOffset? ExpiresAt { get; set; }

        /// <summary>
        /// Gets or sets the type name of the entry.
        /// </summary>
        public string? TypeName { get; set; }

        /// <summary>
        /// Gets or sets the value of the entry.
        /// </summary>
        public byte[]? Value { get; set; } = Array.Empty<byte>();
    }
}