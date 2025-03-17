using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TaskTrackPro.Core.Models;
using TaskTrackPro.Core.Repositories.Commands.Interfaces;

namespace TaskTrackPro.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountCommand _accountCommand;

        public AccountController(IAccountCommand accountCommand)
        {
            _accountCommand = accountCommand;
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromForm] t_User user)
        {

            var status = await _accountCommand.Register(user);
            if (status == 1)
            {
                return Ok(new { success = true, message = "User Registered" });
            }
            else if (status == 0)
            {
                return Ok(new { success = false, message = "User Already Exist" });
            }
            else
            {
                return BadRequest(new
                {
                    success = false,
                    message = "There was some error while Registration"
                });
            }
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromForm] vm_Login user)
        {
            var userData = await _accountCommand.Login(user);

            if (userData != null)
            {
                return Ok(new
                {
                    success = true,
                    message = "Login Successful",
                    user = new
                    {
                        id = userData.c_uid,
                        name = userData.c_uname,
                        email = userData.c_email,
                        gender = userData.c_gender
                    }
                });
            }
            else
            {
                return Ok(new { success = false, message = "Invalid Email or Password" });
            }
        }

    }
}