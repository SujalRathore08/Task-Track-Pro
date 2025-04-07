using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;
using TaskTrackPro.Core.Models;

namespace TaskTrackPro.Core.Services.Email
{
    public class UserRegistrationConsumer
    {
        private readonly IConnection _rabbitConnection;
        private readonly IDatabase _redis;
        private readonly IConfiguration _config;
        private readonly IModel _channel;

        public UserRegistrationConsumer(IConnection rabbitConnection, IConnectionMultiplexer redis, IConfiguration config)
        {
            _rabbitConnection = rabbitConnection;
            _redis = redis.GetDatabase();
            _config = config;
            _channel = _rabbitConnection.CreateModel(); // Persistent channel
        }

        public void StartListening()
        {
            _channel.QueueDeclare(queue: "UserRegistrationQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($"Message received from RabbitMQ: {message}");

                var user = JsonSerializer.Deserialize<t_User>(message);
                if (user != null)
                {
                    Console.WriteLine($"Processing registration for {user.c_email}");
                    await SendEmailToAdmin(user.c_email, user.c_uname);
                    Console.WriteLine("Email sending process completed.");
                }
            };

            _channel.BasicConsume(queue: "UserRegistrationQueue", autoAck: true, consumer: consumer);
            Console.WriteLine("Listening for new user registrations...");
        }

        private async Task SendEmailToAdmin(string userEmail, string userName)
        {
            Console.WriteLine("Email: " + userEmail + " Name: " + userName);
            var emailSettings = _config.GetSection("EmailSettings");

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("TaskTrackPro", emailSettings["SenderEmail"]));
            email.To.Add(new MailboxAddress("Admin", emailSettings["AdminEmail"]));
            email.Subject = "📩 New User Registration - TaskTrackPro";

            // HTML Email Body
            string emailBody = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    body {{
                        font-family: Arial, sans-serif;
                        background-color: #f4f4f4;
                        padding: 20px;
                    }}
                    .container {{
                        max-width: 600px;
                        background: white;
                        padding: 20px;
                        border-radius: 10px;
                        box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1);
                        text-align: center;
                    }}
                    .header {{
                        background-color: #007bff;
                        color: white;
                        padding: 10px;
                        font-size: 20px;
                        border-radius: 10px 10px 0 0;
                    }}
                    .content {{
                        padding: 20px;
                        font-size: 16px;
                        color: #333;
                    }}
                    .footer {{
                        background-color: #f4f4f4;
                        padding: 10px;
                        font-size: 14px;
                        color: #777;
                        border-radius: 0 0 10px 10px;
                    }}
                    .button {{
                        display: inline-block;
                        padding: 10px 20px;
                        color: white;
                        background-color: #28a745;
                        text-decoration: none;
                        border-radius: 5px;
                        margin-top: 10px;
                    }}
                    @media (max-width: 600px) {{
                        .container {{
                            width: 100%;
                        }}
                    }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>🚀 New User Registration</div>
                    <div class='content'>
                        <p><strong>UserName:</strong> {userName}</p>
                        <p><strong>Email:</strong> {userEmail}</p>
                        <p style='color:white;'>A new user has registered on <b>TaskTrackPro</b>. Click the button below to view details.</p>
                        <a href='https://yourwebsite.com/admin-dashboard' class='button'>View User</a>
                    </div>
                    <div class='footer'>© 2025 TaskTrackPro. All Rights Reserved.</div>
                </div>
            </body>
            </html>";

            email.Body = new TextPart("html") { Text = emailBody };

            using var smtp = new MailKit.Net.Smtp.SmtpClient();
            await smtp.ConnectAsync(emailSettings["SmtpServer"], int.Parse(emailSettings["SmtpPort"]), SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(emailSettings["SenderEmail"], emailSettings["SenderPassword"]); // ✅ Updated this
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);

            Console.WriteLine($"Admin notified via email: {emailSettings["AdminEmail"]}");
        }
    }
}