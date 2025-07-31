using System;
using EmergencyManagement.Data;
using EmergencyManagement.Token;
using Microsoft.AspNetCore.Mvc;
using EmergencyManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using EmergencyManagement.Models.DTOs.Common;
using Microsoft.AspNetCore.Cors;

namespace EmergencyManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowAngularApp")]
    public class UserController : ControllerBase
    {
        private readonly ISubmitReportService _submitReportService;
        private readonly EHSDbContext _context;
        public UserController(ISubmitReportService submitReportService, EHSDbContext context)
        {
            _submitReportService = submitReportService;
            _context = context;
        }
       // [Authorize]
        [HttpGet("me")]

        public async Task<ActionResult<UserInfoDto>> GetCurrentUserInfo([FromQuery] string username)
        {
            //Console.WriteLine("Identity name: " + User.Identity?.Name);
           // var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized("Invalid token or user.");

            var userInfo = await _submitReportService.GetUserInfoAsync(username);

            if (userInfo == null)
                return NotFound("User not found");

            return Ok(userInfo);
        }
    }
}
