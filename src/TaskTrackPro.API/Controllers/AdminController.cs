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
    public class AdminController : Controller
    {
        private readonly IAdminQuery _adminQuery;

        public AdminController(IAdminQuery adminQuery)
        {
            _adminQuery = adminQuery;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _adminQuery.GetAllUsers();
            return Ok(users);
        }
    }
}