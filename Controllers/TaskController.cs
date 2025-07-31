using EmergencyManagement.Data;
using EmergencyManagement.Models.DTOs;
using EmergencyManagement.Models.DTOs.Task;
using EmergencyManagement.Models.Entities;
using EmergencyManagement.Services.Implementations;
using EmergencyManagement.Services.Interfaces;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Threading.Tasks;

namespace EmergencyManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowAngularApp")]
    public class TaskController : ControllerBase
    {
        private readonly ITaskService _TaskService;
        private readonly EHSDbContext _context;
        private readonly IEmailService _emailService;
        public TaskController(ITaskService TaskService, EHSDbContext context, IEmailService emailService)
        {
            _TaskService = TaskService;
            _context = context;
            _emailService = emailService;   
        }
        [HttpGet("getTask")]

        public async Task<IActionResult> GetTask([FromQuery] TaskFilterDto filter) 
        {
            var result = await _TaskService.GetAllTaskAsync(filter);  
            return Ok(result);
        }
      
        [HttpGet("getTaskstatus/{taskstatusFor}")]
    
        public async Task<IActionResult> getTaskstatus(string taskstatusFor)
        {
            var result = await _TaskService.GetFilteredTaskStatusAsync(taskstatusFor);
            return Ok(result);
        }
      
        [HttpPut("UpdateTaskSatus")]
        public async Task<IActionResult> UpdateTaskSatus([FromBody] UpdateTaskStatusDto dto)
        {
            try
            {
                var result = await _TaskService.UpdateTaskSatusAsync(dto);

                if (!result)
                {
                    return BadRequest(new { Message = "Update failed. Task not found." });
                }
                else
                {
                    await _emailService.SendMailOnFireDrillTaskStatusUpdationAsync(dto.fireDrillId, dto.taskId);
                    return Ok(new { Message = "Task status updated successfully" });


                }

             

               
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message }); // Will show: "Target date can only be changed twice."
            }
        }

        [HttpPut("UpdateTaskApproval")]
        public async Task<IActionResult> UpdateTaskApproval([FromBody] TaskApprovalDto dto)
        {
            try
            {
                var result = await _TaskService.UpdateTaskApprovalAsync(dto);

                if (!result)
                {
                    return BadRequest(new { Message = "Update failed. Task not found." });
                }
                else
                {
                    await _emailService.SendMailOnFireDrillTaskApprovalAsync(dto.fireDrillId, dto.taskId);
                    return Ok(new { Message = "Task approval submitted successfully" });

                }
                 

               
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message }); 
            }
        }

        [HttpGet("getApprovalstatus")]

        public async Task<IActionResult> getApprovalstatus()
        {
            var result = await _TaskService.GetFilteredApprovalStatusAsync();
            return Ok(result);
        }

        //[HttpGet("firedrill/{fireDrillId}/tasks")]
        //public async Task<IActionResult> GetFireDrillTasks(int fireDrillId)
        //{
        //    var result = await _TaskService.GetAllTaskForMailAsync(fireDrillId);

        //    if (result == null || !result.Any())
        //        return NotFound("No tasks found for the given Fire Drill ID.");

        //    return Ok(result);
        //}


        [HttpGet("getTaskHistory/{TaskId}")]
        public async Task<IActionResult> getTaskHistory(int TaskId)
        {
            var result = await _TaskService.getTaskHistoryAsync(TaskId);
            return Ok(result);
        }
    }
}
