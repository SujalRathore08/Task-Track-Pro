using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TaskTrackPro.API.Services;

namespace TaskTrackPro.API.Consumers
{
    public class RabbitMqConsumer
    {
        private readonly string _queueName = "taskQueue";
        private readonly ConnectionFactory _factory;
        private readonly IServiceProvider _serviceProvider;

        public RabbitMqConsumer(IServiceProvider serviceProvider)
        {
            _factory = new ConnectionFactory() { HostName = "localhost", DispatchConsumersAsync = true };
            _serviceProvider = serviceProvider;
            Task.Run(() => StartListening());
        }

        private async void StartListening()
        {
            using var connection = _factory.CreateConnection();
            using var channel = connection.CreateModel();
            channel.QueueDeclare(_queueName, false, false, false, null);
            channel.BasicQos(0, 1, false);

            Console.WriteLine("[✔] RabbitMQ Consumer Started - Listening for notifications...");

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                var notification = JsonSerializer.Deserialize<NotificationMessage>(message);
                Console.WriteLine($"[✔] Received Notification: {notification.NotificationTitle} for User {notification.UserId}");

                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var redisService = scope.ServiceProvider.GetRequiredService<RedisServices>();
                    var existingNotifications = await redisService.GetNotificationsAsync(notification.UserId);

                    // Only store if it's not already in Redis
                    if (!existingNotifications.Contains(notification.NotificationTitle))
                    {
                        await redisService.StoreNotificationAsync(notification.UserId, notification.NotificationTitle);
                        Console.WriteLine($"[✔] Stored in Redis for User {notification.UserId}");
                    }
                    else
                    {
                        Console.WriteLine($"[⚠] Notification already exists in Redis: {notification.NotificationTitle}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[❌] Error processing message: {ex.Message}");
                }
                finally
                {
                    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                }
            };

            channel.BasicConsume(_queueName, false, consumer);
            await Task.Delay(-1);
        }

        public async Task<List<string>> GetNotificationsByUser(string userId)
        {
            using var scope = _serviceProvider.CreateScope();
            var redisService = scope.ServiceProvider.GetRequiredService<RedisServices>();
            var redisNotifications = await redisService.GetNotificationsAsync(userId);
            return redisNotifications ?? new List<string>();
        }
    }

    public class NotificationMessage
    {
        public string UserId { get; set; }
        public string NotificationTitle { get; set; }
    }
}