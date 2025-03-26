using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using TaskTrackPro.API.Services;
using TaskTrackPro.Core.Repositories.Commands.Interfaces;
using TaskTrackPro.Core.Repositories.Queries.Interfaces;
using MailKit.Net.Smtp;
using MimeKit;

namespace TaskTrackPro.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountCommand _accountCommand;
        private readonly RedisService _redisService;
        private readonly ITaskCount _taskCount;
        private readonly IConfiguration _configuration;

        public AccountController(IAccountCommand accountCommand, RedisService redisService, ITaskCount taskCount, IConfiguration configuration)
        {
            _accountCommand = accountCommand;
            _redisService = redisService;
            _taskCount = taskCount;
            _configuration = configuration;
        }

        [HttpPost("SendOtp")]
        public async Task<IActionResult> SendOtp([FromBody] OtpRequest request)
        {
            if (string.IsNullOrEmpty(request.Email))
            {
                return BadRequest(new { success = false, message = "Email is required." });
            }

            string otp = new Random().Next(100000, 999999).ToString();
            bool isSet = await _redisService.SetAsync(request.Email, otp, 300); // Store OTP for 5 minutes

            if (!isSet)
            {
                return BadRequest(new { success = false, message = "Failed to store OTP." });
            }

            bool emailSent = await SendOtpEmailAsync(request.Email, otp);
            if (!emailSent)
            {
                return StatusCode(500, new { success = false, message = "Failed to send OTP email." });
            }

            return Ok(new { success = true, message = "OTP sent to your email." });
        }

        private async Task<bool> SendOtpEmailAsync(string email, string otp)
        {
            try
            {
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]);
                var senderEmail = _configuration["EmailSettings:SenderEmail"];
                var senderPassword = _configuration["EmailSettings:SenderPassword"];

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("TaskTrackPro", senderEmail));
                message.To.Add(new MailboxAddress("", email));
                message.Subject = "Your OTP Code";

                message.Body = new TextPart("plain")
                {
                    Text = $"Your OTP code is: {otp}. It is valid for 5 minutes."
                };

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(smtpServer, smtpPort, false);
                    await client.AuthenticateAsync(senderEmail, senderPassword);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
                return false;
            }
        }

        [HttpPost("VerifyOtp")]
        public async Task<IActionResult> VerifyOtp([FromBody] OtpVerificationRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Otp))
            {
                return BadRequest(new { success = false, message = "Email and OTP are required." });
            }

            string storedOtp = await _redisService.PopAsync(request.Email);
            if (storedOtp == request.Otp)
            {
                return Ok(new { success = true, message = "OTP verified." });
            }
            return BadRequest(new { success = false, message = "Invalid or expired OTP." });
        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            Console.WriteLine($"Received Email: {request.Email}, NewPassword: {request.NewPassword}");

            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.NewPassword))
            {
                return BadRequest(new { success = false, message = "Email and new password are required." });
            }

            bool updated = await _accountCommand.ChangePassword(request.Email, request.NewPassword);
            if (updated)
            {
                return Ok(new { success = true, message = "Password reset successful." });
            }
            return StatusCode(500, new { success = false, message = "Failed to reset password." });
        }

        [HttpGet("pendingTask")]
        public int GetPendingTaskCount(int userId)
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

    public class OtpRequest
    {
        public string Email { get; set; }
    }

    public class OtpVerificationRequest
    {
        public string Email { get; set; }
        public string Otp { get; set; }
    }

    public class ResetPasswordRequest
    {
        public string Email { get; set; }
        public string NewPassword { get; set; }
    }
}
