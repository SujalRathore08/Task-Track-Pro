using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TaskTrackPro.API.Services;
using TaskTrackPro.Core.Models;
using TaskTrackPro.Core.Repositories.Commands.Interfaces;
using TaskTrackPro.Core.Repositories.Queries.Interfaces;

namespace TaskTrackPro.API.Controllers
{
    [ApiController]
    [Route("api/Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminQuery _adminQuery;
        private readonly IAdminCommand _adminCommand;

        private readonly ElasticsearchServices _elasticssearchServices;
        private readonly ElasticsearchService _elasticssearchService;
        private readonly ITaskInterface _taskInterface;
        private readonly ITaskCount _taskCount;


        public AdminController(IAdminQuery adminQuery, ElasticsearchServices elasticssearchServices, IAdminCommand adminCommand, ITaskInterface taskInterface,ITaskCount taskCount, ElasticsearchService elasticssearchService)  
        {
            _adminQuery = adminQuery;
            _adminCommand = _adminCommand;
            _elasticssearchServices = elasticssearchServices;
            _taskInterface = taskInterface;
            _elasticssearchService = elasticssearchService;

            _taskCount = taskCount;
            
        }

        [HttpGet("{name}")]
        public async Task<IActionResult> SearchTask(string name)
        {
            var task = await _elasticssearchServices.SearchTaskByTitleAsync(name);
            // Remove duplicates based on a unique property, assuming "Id" is unique
            var uniqueTasks = task.DistinctBy(t => t.c_tid).ToList();

            return Ok(uniqueTasks);
        }

        [HttpGet("name")]
        public async Task<IActionResult> SearchUser(string name){
            var user = await _elasticssearchService.SearchUserAsync(name);
            var uniqueUsers = user.DistinctBy(u => u.c_uid).ToList();
            return Ok(uniqueUsers);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTasks()
        {
            string c_uid = "";
            List<t_task> list;
            if (c_uid == "")
            {
                list = await _taskInterface.GetAllTask();
            }
            else
            {
                list = await _taskInterface.GetTaskById(Convert.ToInt32(c_uid));
            }
            return Ok(list);
        }


        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _adminQuery.GetAllUsers();
            return Ok(users);
        }

        [HttpGet("user")]
        public async Task<IActionResult> GetUserById(int userId)
        {
            var user = await _adminQuery.GetUserById(userId);
            return Ok(user);
        }

        [HttpDelete("user/{userId}")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            var isDeleted = await _adminCommand.DeleteUser(userId);

            if (isDeleted)
            {
                return Ok(new { message = "User deleted successfully." });
            }
            else
            {
                return NotFound(new { message = "User not found or deletion failed." });
            }
        }

        [HttpGet("AllMembers")]
        public int GetAllMembers()
        {
            return _taskCount.GetAllMembers();
        }

        [HttpGet("ApproveMembers")]
        public int ApproveMembers()
        {
            return _taskCount.GetApproveMembers();
        }

        [HttpGet("PendingTask")]
        public int PendingTask()
        {
            return _taskCount.GetPendingTask();
        }
    }
}