using System;
using EmergencyManagement.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EmergencyManagement.Services.Interfaces;
using Microsoft.AspNetCore.Cors;
using EmergencyManagement.Models.DTOs.Common;


namespace EmergencyManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowAngularApp")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly EHSDbContext _context;
        private readonly IEmailService _emailService;

        public AuthController(IAuthService authService, EHSDbContext context, IEmailService emailService)
        {
            _authService = authService;
            _context = context;
            _emailService = emailService;   
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            var result = await _authService.AuthenticateAsync(request);
            if (result == null)
                return Unauthorized("Invalid credentials");

            return Ok(new { result });
        }
        [HttpGet("me")]

        public async Task<ActionResult<UserInfoDto>> GetCurrentUserInfo([FromQuery] string username)
        {
            //Console.WriteLine("Identity name: " + User.Identity?.Name);
            // var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized("Invalid token or user.");

            var userInfo = await _authService.GetUserInfoAsync(username);

            if (userInfo == null)
                return NotFound("User not found");

            return Ok(userInfo);
        }


        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var user = await _context.Users
     .FirstOrDefaultAsync(u => u.Username == dto.UserName);
            if (user == null)
                return NotFound("User not found");
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.IsFirstLogin = false;
 
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Password reset successful" });
        }
        [HttpPost("forgot-password")]

        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto request)
        {
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Email))
                return BadRequest("Username and Email are required");

            var result = await _emailService.SendMailForgotPasswordAsync(request);

            if (!result)
                return NotFound("User not found or email mismatch");
            return Ok(new { Message = "Password reset link to your email" });
        }

        [HttpGet("getUnits/{empId}")]
        public async Task<IActionResult> GetUnitsByEmployeeId(int empId)
        {
            var Units = await _authService.GetUnitsByEmployeeIdAsync(empId);
            if (Units == null)
                return NotFound();

            return Ok(Units);
        }
    }
}
