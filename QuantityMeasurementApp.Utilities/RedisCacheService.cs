using StackExchange.Redis;
using System.Text.Json;

namespace QuantityMeasurementApp.Utilities
{
    public class RedisCacheService
    {
        private readonly IDatabase _database;

        public RedisCacheService(IConnectionMultiplexer redis)
        {
            _database = redis.GetDatabase();
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            try {
                var json = JsonSerializer.Serialize(value);
                
                // Fix: Only pass expiry if it has a value and is greater than zero
                if (expiry.HasValue && expiry.Value > TimeSpan.Zero)
                {
                    await _database.StringSetAsync(key, (RedisValue)json, expiry.Value);
                }
                else
                {
                    // No expiration
                    await _database.StringSetAsync(key, (RedisValue)json);
                }
            } catch (Exception ex) {
                Console.WriteLine($"⚠️ Redis SetAsync failed: {ex.Message}");
            }
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            try {
                RedisValue value = await _database.StringGetAsync(key);
                if (value.IsNullOrEmpty)
                {
                    return default;
                }
                return JsonSerializer.Deserialize<T>((string)value!);
            } catch (Exception ex) {
                Console.WriteLine($"⚠️ Redis GetAsync failed: {ex.Message}");
                return default;
            }
        }

        public async Task RemoveAsync(string key)
        {
            try {
                await _database.KeyDeleteAsync(key);
            } catch (Exception ex) {
                Console.WriteLine($"⚠️ Redis RemoveAsync failed: {ex.Message}");
            }
        }
    }
}
