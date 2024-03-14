using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Collections.Immutable;
using System.Text.Json;

namespace OrderAgregator.API.Cache.RedisCache
{
    public interface IRedisDatabase : IDisposable
    {
        /// <summary>
        ///     Get data under setkey:key
        /// </summary>
        Task<T?> GetData<T>(string key, string setKey);

        /// <summary>
        ///     Remove data and key from the set for setkey:key
        /// </summary>
        Task RemoveData(string key, string setKey);

        /// <summary>
        ///     Insert data for setkey:key
        /// </summary>
        Task SetData<T>(string key, string setKey, T value, TimeSpan expirationTime);

        /// <summary>
        ///     Get all keys under setkey
        /// </summary>
        Task<ImmutableArray<string>> GetSetKeys(string setKey);
    }

    public class RedisDatabase : IRedisDatabase
    {
        /// <summary> Flag if unit and underlaying connection is disposed </summary>
        private bool disposed = false;

        /// <summary> Underlaying connection </summary>
        private readonly ConnectionMultiplexer? _redis;

        /// <summary> Redis database </summary>
        private readonly IDatabase _cacheDb;

        public RedisDatabase(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Redis") ?? throw new ArgumentNullException("Redis connection string not set");

            _redis = ConnectionMultiplexer.Connect(connectionString);
            _cacheDb = _redis.GetDatabase();
        }

        /// <inheritdoc/>
        public async Task<T?> GetData<T>(string key, string setKey)
        {
            var value = await _cacheDb.StringGetAsync($"{setKey}:{key}");

            if (!string.IsNullOrWhiteSpace(value))
                return JsonSerializer.Deserialize<T>(value!);

            return default;
        }

        /// <inheritdoc/>
        public async Task RemoveData(string key, string setKey)
        {
            var exists = await _cacheDb.KeyExistsAsync($"{setKey}:{key}");

            if (exists)
                await _cacheDb.KeyDeleteAsync($"{setKey}:{key}");

            await _cacheDb.SetRemoveAsync(setKey, key);
        }

        /// <inheritdoc/>
        public async Task SetData<T>(string key, string setKey, T value, TimeSpan expirationTime)
        {
            await _cacheDb.StringSetAsync($"{setKey}:{key}", JsonSerializer.Serialize(value), expirationTime);
            await _cacheDb.SetAddAsync(setKey, key);
        }

        /// <inheritdoc/>
        public async Task<ImmutableArray<string>> GetSetKeys(string setKey)
        {
            var setMembers = await _cacheDb.SetMembersAsync(setKey);

            var itemKeys = setMembers.Select(key => key.ToString()).ToImmutableArray();
            
            return itemKeys;
        }

        public void Dispose() => Dispose(disposed != true);
        protected virtual void Dispose(bool disposing)
        {
            _redis?.Dispose();

            disposed = true;
        }
    }
}
