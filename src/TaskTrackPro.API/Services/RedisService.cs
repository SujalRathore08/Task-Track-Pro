using System;
using System.Threading.Tasks;
using StackExchange.Redis;
using TaskTrackPro.Core.Models;
using System.Text.Json;

namespace TaskTrackPro.API.Services
{
    public class RedisService
    {
        private readonly IDatabase _redisDB;

        public RedisService(IDatabase redisDB)
        {
            _redisDB = redisDB;
        }

        public async Task<string> GetAsync(string key)
        {
            return await _redisDB.StringGetAsync(key);
        }

        public async Task<bool> SetAsync(string key, string value)
{
    var result = await _redisDB.StringSetAsync(key, value);
    Console.WriteLine("SetAsync");
    return result;
}

        public async Task<bool> SetAsync(string key, string value, int timeInSeconds)
        {
            return await _redisDB.StringSetAsync(key, value, TimeSpan.FromSeconds(timeInSeconds));
        }

        public async Task<string> PopAsync(string key)
        {
            var value = await _redisDB.StringGetAsync(key);
            if (!value.IsNullOrEmpty)
            {
                await _redisDB.KeyDeleteAsync(key); // Remove key after retrieving value
                return value;
            }
            return null;
        }

       public async Task<t_User> GetUserDataAsync(string key)
{
    var value = await _redisDB.StringGetAsync(key);
    if (!value.IsNullOrEmpty)
    {
        Console.WriteLine($"[RedisService] Retrieved Data for Key: {key}, Value: {value}");
        await _redisDB.KeyDeleteAsync(key);
        return JsonSerializer.Deserialize<t_User>(value);
    }

    Console.WriteLine($"[RedisService] No Data Found for Key: {key}");
    return null;
}


        public async Task<bool> SetUserDataAsync(string key, t_User user, int timeInSeconds = 0)
{
    var value = JsonSerializer.Serialize(user);
    Console.WriteLine($"[RedisService] Setting Data for Key: {key}, Value: {value}"); // Debugging Log

    bool isSet;
    if (timeInSeconds > 0)
    {
        isSet = await _redisDB.StringSetAsync(key, value, TimeSpan.FromSeconds(timeInSeconds));
    }
    else
    {
        isSet = await _redisDB.StringSetAsync(key, value);
    }

    if (isSet)
    {
        Console.WriteLine($"[RedisService] Successfully set key: {key} in Redis.");
    }
    else
    {
        Console.WriteLine($"[RedisService] Failed to set key: {key} in Redis.");
    }

    return isSet;
}

    }
}
