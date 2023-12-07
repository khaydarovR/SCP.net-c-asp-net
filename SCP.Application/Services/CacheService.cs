using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace SCP.Application.Services
{
    public class CacheService
    {
        private readonly IDistributedCache _cache;

        public CacheService(IDistributedCache distributedCache)
        {
            _cache = distributedCache;
        }

        public async Task Save<T>(string key, T value, int cacheMinutes)
        {
            var serializedValue = JsonConvert.SerializeObject(value);
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(cacheMinutes)
            };
            await _cache.SetStringAsync(key, serializedValue, cacheOptions);
        }

        public T? Get<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key cannot be null or empty.", nameof(key));
            }

            var serializedValue = _cache.GetString(key);
            return string.IsNullOrEmpty(serializedValue) ? default : JsonConvert.DeserializeObject<T>(serializedValue);
        }

        public bool Exists(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key cannot be null or empty.", nameof(key));
            }

            return _cache.GetString(key) != null;
        }

        public void Delete(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key cannot be null or empty.", nameof(key));
            }

            _cache.Remove(key);
        }

    }
}
