using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TaskTrackPro.API.Services;
using TaskTrackPro.Core.Models;
using TaskTrackPro.Core.Repositories.Commands.Interfaces;
using TaskTrackPro.Core.Repositories.Queries.Interfaces;

namespace TaskTrackPro.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountCommand _accountCommand;
        private readonly RedisService _redisService;
        private readonly ITaskCount _taskCount;


        public AccountController(IAccountCommand accountCommand, RedisService redisService, ITaskCount taskCount)
        {
            _accountCommand = accountCommand;
            _redisService = redisService;
            _taskCount = taskCount;
        }

        [HttpPost("set")]
        public async Task<IActionResult> SetData(string key, string value)
        {
            bool isSet = await _redisService.SetAsync(key, value, 600); // Store data for 10 minutes
            return Ok(new { success = isSet, message = isSet ? "Data stored in Redis" : "Failed to store data" });
        }

        [HttpGet("pop")]
        public async Task<IActionResult> PopData(string key)
        {
            string value = await _redisService.PopAsync(key);
            if (value != null)
            {
                return Ok(new { success = true, message = "Data retrieved and deleted", data = value });
            }
            else
            {
                return Ok(new { success = false, message = "No data found" });
            }
        }



        [HttpGet("pendingTask")]
        public int GetPendigTaskCount(int userId)
        {
            return _taskCount.GetPendingCount(userId);
        }
        [HttpGet("completeTask")]
        public int GetCompleteTaskCount(int userId)
        {
            return _taskCount.GetCompletedCount(userId);
        }
        [HttpGet("progressTask")]
        public int GetInProgressTaskCount(int userId)
        {
            return _taskCount.GetInProgressCount(userId);
        }
    }
}