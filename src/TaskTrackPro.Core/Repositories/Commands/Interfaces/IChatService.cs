// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using RabbitMQ.Client.Events;
// using TaskTrackPro.Core.Models;

// namespace TaskTrackPro.Core.Repositories.Commands.Interfaces
// {
//     public interface IChatService
//     {
//         Task<List<Message>> GetChatHistory(string userId, string otherUserId);
//         Task<int> SaveMessage(Message message);
//         void SubscribeToMessages(string userId, EventHandler<BasicDeliverEventArgs> handler);
//     }
// }