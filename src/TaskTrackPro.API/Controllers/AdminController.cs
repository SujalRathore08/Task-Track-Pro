using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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

        public AdminController(IAdminQuery adminQuery, IAdminCommand adminCommand)
        {
            _adminQuery = adminQuery;
            _adminCommand = adminCommand;
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
    }
}