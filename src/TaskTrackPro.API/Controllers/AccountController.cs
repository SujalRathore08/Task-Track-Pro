using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TaskTrackPro.API.Services;
using TaskTrackPro.Core.Models;
using TaskTrackPro.Core.Repositories.Commands.Interfaces;

namespace TaskTrackPro.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountCommand _accountCommand;
        private readonly RedisService _redisService;

        public AccountController(IAccountCommand accountCommand, RedisService redisService)
        {
            _accountCommand = accountCommand;
            _redisService = redisService;
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

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromForm] t_User user)
        {

            if (user.c_profile != null && user.c_profile.Length > 0)
            {
                var fileName = user.c_email + Path.GetExtension(
                    user.c_profile.FileName
                );
                var filePath = Path.Combine("../TaskTrackPro.MVC/wwwroot/profile_images", fileName);
                user.c_profilepicture = fileName;
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await user.c_profile.CopyToAsync(fileStream);
                }
            }
            Console.WriteLine(user.c_profilepicture);
            try
            {
                var result = await _accountCommand.Register(user);
                if (result == 1)
                {
                    return Ok(new { message = "User registered successfully." });
                }
                else
                {
                    return BadRequest(new { message = "User registration failed." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while registering user.", error = ex.Message });
            }
        }


        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromForm] vm_Login user)
        {
            string redisKey = $"user:{user.c_email}";
            var cachedUser = await _redisService.GetUserDataAsync(redisKey);

            if (cachedUser != null)
            {
                return Ok(new
                {
                    success = true,
                    message = "Login Successful (Cached)",
                    user = cachedUser
                });
            }

            var userData = await _accountCommand.Login(user);
            if (userData != null)
            {
                var userObject = new t_User
                {
                    c_uid = userData.c_uid,
                    c_uname = userData.c_uname,
                    c_email = userData.c_email,
                    c_gender = userData.c_gender
                };

                // ✅ Store in Redis
                bool isStored = await _redisService.SetUserDataAsync(redisKey, userObject, 1800); // 30 min expiry
                Console.WriteLine($"[Login API] Redis Set Status: {isStored}");

                return Ok(new
                {
                    success = true,
                    message = "Login Successful",
                    user = userObject
                });
            }
            else
            {
                return Ok(new { success = false, message = "Invalid Email or Password" });
            }
        }
    }
}