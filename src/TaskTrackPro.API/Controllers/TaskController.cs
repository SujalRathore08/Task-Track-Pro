using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TaskTrackPro.API.Consumers;
using TaskTrackPro.API.Services;
using TaskTrackPro.Core.Models;
using TaskTrackPro.Core.Repositories.Commands.Interfaces;

namespace TaskTrackPro.API.Controllers
{
    [ApiController]
    // [Route("api/[controller]")]
    [Route("api/task")]

    public class TaskController : ControllerBase
    {
        private readonly ITaskInterface _taskRepository;
        private readonly RedisServices _redisService;
        private readonly RabbitMqConsumer _rabbitMqConsumer;

        public TaskController(ITaskInterface taskRepository, RedisServices redisService, RabbitMqConsumer rabbitMqConsumer)
        {
            _taskRepository = taskRepository;
            _redisService = redisService;
            _rabbitMqConsumer = rabbitMqConsumer;
        }

        // POST: api/task/add
        [HttpPost("add")]
        public async Task<IActionResult> AddTask([FromBody] t_task task)
        {
            if (task == null)
            {
                return BadRequest("Task data is required.");
            }

            int result = await _taskRepository.Add(task);

            if (result > 0)
            {
                return Ok(new { message = "Task added successfully!" });
            }
            else
            {
                return StatusCode(500, "Failed to add task.");
            }
        }

        [HttpGet("notifications/{userId}")]
        public async Task<IActionResult> GetNotifications(string userId)
        {
            using var scope = HttpContext.RequestServices.CreateScope();
            var redisService = scope.ServiceProvider.GetRequiredService<RedisService>();

            var notifications = await _redisService.GetNotificationsAsync(userId);

            if (notifications == null || notifications.Count == 0)
            {
                return Ok(new List<object>()); // ✅ Return empty array instead of NoContent()
            }

            return Ok(notifications);
        }

        [HttpDelete("notifications/clear/{userId}")]
        public async Task<IActionResult> ClearNotifications(string userId)
        {
            await _redisService.ClearNotificationsAsync(userId);
            return Ok("Notifications cleared successfully.");
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _taskRepository.GetAllUsers();
            return Ok(users);
        }
        [HttpGet("tasks")]
        public async Task<IActionResult> GetAllTasks()
        {
            var users = await _taskRepository.GetAllTask();
            return Ok(users);
        }
        [HttpGet("approved-users")]
        public async Task<IActionResult> GetApprovedUsernames()
        {
            var users = await _taskRepository.GetApprovedUsernames();
            return Ok(users);
        }

        [HttpGet("task")]
        public async Task<IActionResult> GetTasksByid(int userId)
        {
            var users = await _taskRepository.GetTaskById(userId);
            return Ok(users);
        }


        [HttpPut("approve/{userId}")]
        public async Task<IActionResult> ApproveUser(int userId)
        {
            bool isUpdated = await _taskRepository.ApproveUser(userId);

            if (isUpdated)
            {
                return Ok(new { message = "User approved successfully!" });
            }
            else
            {
                return NotFound("User not found or update failed.");
            }
        }


        [HttpPut("update")]
        public async Task<IActionResult> UpdateTask([FromBody] t_task task)
        {
            Console.WriteLine(task);
            if (task == null || task.c_tid == 0)
            {
                return BadRequest("Invalid task data.");
            }

            int result = await _taskRepository.Update(task);

            if (result > 0)
            {
                return Ok(new { message = "Task updated successfully!" });
            }
            else
            {
                return NotFound("Task not found or update failed.");
            }
        }
        [HttpDelete("delete/{taskId}")]
        public async Task<IActionResult> DeleteTask(int taskId)
        {
            if (taskId == 0)
            {
                return BadRequest("Invalid task ID.");
            }

            int result = await _taskRepository.Delete(taskId);

            if (result > 0)
            {
                return Ok(new { message = "Task deleted successfully!" });
            }
            else
            {
                return NotFound("Task not found or delete failed.");
            }
        }

        [HttpGet("notifications/count/{userId}")]
        public async Task<IActionResult> GetNotificationCount(string userId)
        {
            var count = await _redisService.CountNotificationsAsync(userId);
            return Ok(new { notificationCount = count });
        }

    }
}
