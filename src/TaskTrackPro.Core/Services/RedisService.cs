using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace TaskTrackPro.API.Services
{
    public class RedisServices
    {
        private static Lazy<ConnectionMultiplexer> _lazyConnection;
        private readonly IDatabase _database;

        public RedisServices(IConfiguration configuration)
        {
            var redisHost = configuration["Redis:Host"] ?? "localhost";

            // Ensure only one Redis connection is created (Singleton)
            if (_lazyConnection == null || !_lazyConnection.Value.IsConnected)
            {
                _lazyConnection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(redisHost));
            }

            _database = _lazyConnection.Value.GetDatabase();
        }

        /// <summary>
        /// Stores a notification in Redis for a specific user.
        /// </summary>
        public async Task StoreNotificationAsync(string userId, string notification)
        {
            try
            {
                await _database.ListRightPushAsync($"notifications:{userId}", notification);
                Console.WriteLine($"[✔] Stored notification in Redis for User {userId}: {notification}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌] Redis error storing notification: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves all notifications for a specific user and clears them safely.
        /// </summary>
        public async Task<List<string>> GetNotificationsAsync(string userId)
        {
            var notifications = new List<string>();
            var key = $"notifications:{userId}";

            try
            {
                var redisNotifications = await _database.ListRangeAsync(key);

                foreach (var notification in redisNotifications)
                {
                    notifications.Add(notification);
                }

                Console.WriteLine($"[✔] Retrieved {notifications.Count} notifications for User {userId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌] Redis error retrieving notifications: {ex.Message}");
            }

            return notifications;
        }



        /// <summary>
        /// Clears all notifications for a user.
        /// </summary>
        public async Task ClearNotificationsAsync(string userId)
        {
            try
            {
                await _database.KeyDeleteAsync($"notifications:{userId}");
                Console.WriteLine($"[✔] Cleared all notifications for User {userId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌] Redis error clearing notifications: {ex.Message}");
            }
        }

        public async Task<long> CountNotificationsAsync(string userId)
        {
            try
            {
                var count = await _database.ListLengthAsync($"notifications:{userId}");
                Console.WriteLine($"[✔] User {userId} has {count} notifications.");
                return count;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌] Redis error counting notifications: {ex.Message}");
                return 0;
            }
        }
    }
}