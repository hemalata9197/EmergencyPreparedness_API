using EmergencyManagement.Data;
using EmergencyManagement.Models.DTOs.Master;
using EmergencyManagement.Services.Implementations;
using EmergencyManagement.Services.Interfaces;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace EmergencyManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowAngularApp")]

    public class MasterController : ControllerBase
    {
        private readonly IMastersService _MastersService;
        private readonly EHSDbContext _context;
        //private readonly IHttpContextAccessor _httpContextAccessor;
        public MasterController(IMastersService MastersService, EHSDbContext context)
        {
            _MastersService = MastersService;
            _context = context;

        }

        private int? GetCurrentEmployeeId()
        {
            var empClaim = User.FindFirst("employeeId");

            if (empClaim != null && int.TryParse(empClaim.Value, out var empId))
                return empId;

            return null;
        }
        [HttpGet("GetDropdownOptions/{source}/{unitId}")]

        public async Task<IActionResult> GetDropdownOptions(string source, int unitId)
        {
            var result = await _MastersService.GetDropdownOptionsAsync(source, unitId);
            return Ok(result);
        }

        //[HttpGet("GetAllMasterFields")]
        //public async Task<IActionResult> GetAllMasterFields([FromQuery] string source, [FromQuery] int unitId)
        //{
        //    switch (source)
        //    {
        //        case "FacilityMaster-Area":
        //            var areas = await _MastersService.GetFacilityAreasAsync(unitId);
        //            return Ok(areas);

        //        case "FacilityMaster-Section":
        //            var sections = await _MastersService.GetFacilitySectionsAsync(unitId);
        //            return Ok(sections);

        //        case "ScenarioMaster":
        //            var scenarios = await _MastersService.GetScenariosAsync(unitId);
        //            return Ok(scenarios);

        //        case "Roles":
        //            var roles = await _MastersService.GetRolesAsync(unitId);
        //            return Ok(roles);

        //        case "Designation":
        //            var designation = await _MastersService.GetDesignationsAsync(unitId);
        //            return Ok(designation);

        //        case "Employees":
        //            var employees = await _MastersService.GetEmployeesAsync(unitId);
        //            return Ok(employees);

        //        default:
        //            return BadRequest("Invalid source");
        //    }           

        //}
        [HttpGet("GetAllMasterFields")]
        public async Task<IActionResult> GetAllMasterFields([FromQuery] MasterFilterDto filter)
        {
            return filter.Source switch
            {
                "UnitsMaster" => Ok(await _MastersService.GetUnitsAsync(filter)),
                "FacilityMaster-Area" => Ok(await _MastersService.GetFacilityAreasAsync(filter)),
                "FacilityMaster-Section" => Ok(await _MastersService.GetFacilitySectionsAsync(filter)),
                "ScenarioMaster" => Ok(await _MastersService.GetScenariosAsync(filter)),
                "Roles" => Ok(await _MastersService.GetRolesAsync(filter)),
                "Designation" => Ok(await _MastersService.GetDesignationsAsync(filter)),
                "Employees" => Ok(await _MastersService.GetEmployeesAsync(filter)),

                _ => BadRequest("Invalid source")
            };
        }


        [HttpPost("Create/{source}")]
        public async Task<IActionResult> Create(string source, [FromBody] JsonElement data)
        {
            var result = await _MastersService.CreateAsync(source, data);
            if (result.Success)
                return Ok(new { success = true, message = result.Message });
            else
                return BadRequest(new { success = false, message = result.Message });
            //return result
            //? Ok(new { message = "Saved" })
            //: BadRequest(new { message = "Failed to save" });
        }

        [HttpPut("Update/{source}/{id}")]
        public async Task<IActionResult> Update(string source, int id, [FromBody] JsonElement data)
        {
            var result = await _MastersService.UpdateAsync(source, id, data);
            if (result.Success)
                return Ok(new { success = true, message = result.Message });
            else
                return BadRequest(new { success = false, message = result.Message });
            //return result
            //? Ok(new { message = "Updated" })
            //: BadRequest(new { message = "Record not found" });
          
        }
        [HttpPut("updateStatus")]
        public async Task<IActionResult> UpdateStatus([FromBody] StatusUpdateDto dto)
        {
            // var userId = GetCurrentEmployeeId();

            //if (userId == null)
            //    return Unauthorized("User ID not found.");
            var result = await _MastersService.UpdateStatusAsync(dto);
            return result
            ? Ok(new { message = "Updated" })
            : BadRequest(new { message = "Record not found" });

        }
        [HttpGet("CheckHODEHS")]
        public async Task<IActionResult> CheckHODEHS([FromQuery] int configeleid, [FromQuery] int unitid)
        {
            //var currentEmployeeId = _httpContextAccessor.HttpContext?.Session.GetString("EmployeeId");

            //if (string.IsNullOrEmpty(currentEmployeeId))
            //    return Unauthorized("Session expired or EmployeeId not found");

            var ValuIds = await _MastersService.CheckHODEHSAsync(configeleid, unitid);
            return Ok(new { ValuIds });
        }
        [HttpGet("menus")]
        public async Task<IActionResult> GetMenus()
        {
            var menus = await _MastersService.GetMenusAsync();
            return Ok(menus);
        }
        [HttpGet("{roleId}/menu-ids")]
        public async Task<IActionResult> GetRoleMenuIds(int roleId)
        {
            var menuIds = await _context.RoleMenus
                .Where(rm => rm.RoleId == roleId)
                .Select(rm => rm.MenuId)
                .ToListAsync();

            return Ok(menuIds); // Example: [2, 4, 7, 13]
        }
        [HttpPost("SaveRoleMenuPermissions")]
        public async Task<IActionResult> SaveRoleMenuPermissions([FromBody] RoleMenuAssignmentDto dto)
        {
            await _MastersService.AssignMenusToRoleAsync(dto);
            return Ok(new { message = "Permissions updated successfully." });
        }
    }
}
