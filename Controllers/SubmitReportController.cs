using EmergencyManagement.Data;
using EmergencyManagement.Models.DTOs.Task;
using EmergencyManagement.Models.Entities;
using EmergencyManagement.Services.Implementations;
using EmergencyManagement.Services.Interfaces;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Threading.Tasks;
using EmergencyManagement.Utilities;
using static EmergencyManagement.Utilities.DatabaseUtility;
using EmergencyManagement.Models.DTOs.Common;
using EmergencyManagement.Models.DTOs;
using static System.Collections.Specialized.BitVector32;
using EmergencyManagement.Models.Entities.Fire_Drill;


namespace EmergencyManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowAngularApp")]
    public class SubmitReportController : ControllerBase
    {
        private readonly ISubmitReportService _submitReportService;
        private readonly EHSDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IMastersService _MastersService;  // "[{""Employees"":[2,5,7,8]}]"


        public SubmitReportController(ISubmitReportService submitReportService, EHSDbContext context, IEmailService emailService, IMastersService mastersService)
        {
            _submitReportService = submitReportService;
            _context = context;
            _emailService = emailService;
            _MastersService = mastersService;
        }
        [HttpGet("fire-drill-form/{UnitId}")]

        public async Task<IActionResult> GetFireDrillForm(int UnitId)
        {
            

            // 1. Load all sections
            var sections = await _context.FormSections
                .Where(s => s.IsActive==true)
                .OrderBy(s => s.SectionOrder)
                .Select(s => new FormSectionDto
                {
                    Id = s.Id,
                    SectionName = s.SectionName,
                    SectionOrder = s.SectionOrder,
                    IsRepeatable = s.IsRepeatable,
                    Fields = new List<FormFieldDto>() // populate below
                }).ToListAsync();

            // 2. Load all fields and group by section
            var fields = await _context.FormFields
                .Where(f => f.IsActive == true)
                .Include(f => f.Validations)
                .OrderBy(f => f.OrderIndex)
                .ToListAsync();

            var finalSections = new List<FormSectionDto>();

            foreach (var section in sections)
            {
                var sectionFields = fields
                    .Where(f => f.SectionId == section.Id)
                    .Select(f => new FormFieldDto
                    {
                        Label = f.Label,
                        Name = f.Name,
                        Type = f.Type,
                        Placeholder = f.Placeholder,
                        IsRepeatable = f.IsRepeatable,
                        DateConstraint = f.DateConstraint,
                        isDisabledOnReview=f.isDisabledOnReview,
                        isDisabledOnRelease =f.isDisabledOnRelease,
                        Validations = f.Validations.Select(v => new ValidationDto
                        {
                            Rule = v.Rule,
                            Value = v.Value
                        }).ToList(),
                        Options = null // populated below
                    }).ToList();

                if (!sectionFields.Any())
                    continue; // skip this section if no fields

                // Populate dropdown options if needed
                foreach (var field in sectionFields)
                {
                    var source = fields.FirstOrDefault(x => x.Name == field.Name)?.DropdownSource;
                    if (!string.IsNullOrEmpty(source))
                    {
                        field.Options = await _MastersService.GetDropdownOptionsAsync(source, UnitId);
                    }
                }

                section.Fields = sectionFields;
                finalSections.Add(section); // only add sections with fields
            }

            return Ok(finalSections);
        }
        [HttpPost("submit")]
        public async Task<IActionResult> Submit([FromBody] FireDrillDto dto)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();  

            try
            {
                int fireDrillId = await _submitReportService.SaveFireDrillAsync(dto);

                if (dto.FormData.TryGetValue("section_1", out var section1Obj) &&
     section1Obj is JsonElement section1Element)
                {
                    if (section1Element.ValueKind == JsonValueKind.String)
                    {
                        // 🔹 Parse the string to JSON
                        string rawJson = section1Element.GetString() ?? "[]";
                        using var doc = JsonDocument.Parse(rawJson);
                        var root = doc.RootElement;

                        if (root.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var item in root.EnumerateArray())
                            {
                                if (item.ValueKind == JsonValueKind.Object &&
                                    item.TryGetProperty("Employees", out var employeesElement) &&
                                    employeesElement.ValueKind == JsonValueKind.Array)
                                {
                                    foreach (var empElement in employeesElement.EnumerateArray())
                                    {
                                        if (empElement.TryGetInt32(out int empId))
                                        {
                                            _context.FireDrillResposeEmp.Add(new FireDrillResposeEmp
                                            {
                                                FireDrillId = fireDrillId,
                                                EmployeeId = empId
                                            });
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (section1Element.ValueKind == JsonValueKind.Array)
                    {
                        // ✅ Normal array case (if backend sends correct JSON)
                        foreach (var item in section1Element.EnumerateArray())
                        {
                            if (item.ValueKind == JsonValueKind.Object &&
                                item.TryGetProperty("Employees", out var employeesElement) &&
                                employeesElement.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var empElement in employeesElement.EnumerateArray())
                                {
                                    if (empElement.TryGetInt32(out int empId))
                                    {
                                        _context.FireDrillResposeEmp.Add(new FireDrillResposeEmp
                                        {
                                            FireDrillId = fireDrillId,
                                            EmployeeId = empId
                                        });
                                    }
                                }
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                }

                if (dto.FormData.TryGetValue("section_3", out var section3Obj) && section3Obj is JsonElement section3Element && section3Element.ValueKind == JsonValueKind.Array)
            {

                foreach (var item in section3Element.EnumerateArray())
                {
                    var recommendedAction = item.GetProperty("recommendedAction").GetString();
                    if (!string.IsNullOrEmpty(recommendedAction))
                    {
                        var responsiblePersonStr = item.GetProperty("responsiblePerson").GetString();
                        int responsiblePerson = int.TryParse(responsiblePersonStr, out var parsedId) ? parsedId : 0;
                            var SeverityStr = item.GetProperty("Severity").GetString();
                            int SeverityId=int.TryParse(SeverityStr, out var passId) ? passId : 0;
                            //var targetDateStr = item.GetProperty("targetDate").GetString();
                            //var targetDate = DateTime.TryParse(targetDateStr, out var parsedDate)
                            //    ? DateTime.SpecifyKind(parsedDate, DateTimeKind.Local)  // Important for PostgreSQL
                            //    : throw new Exception("Invalid target date");
                            var targetDateStr = item.GetProperty("targetDate").GetString();

                        if (!DateTime.TryParse(targetDateStr, out var parsedDate))
                            return BadRequest("Invalid target date");

                        var targetDate = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);


                        var recommendationDto = new RecommendationDto
                        {
                            FireDrillId = fireDrillId,
                            ResponsibleUserId = responsiblePerson,
                            RecommendationText = recommendedAction,
                            TargetDate = targetDate,
                            ActionStatusId = 0,
                            SeverityId= SeverityId,
                            CreatedBy = dto.SubmittedBy
                        };

                        int RecommendationId = await _submitReportService.AddRecommendationAsync(recommendationDto);


                    }
                }


            }
            if (dto.FormData.TryGetValue("section_4", out var section4Obj)
                   && section4Obj is JsonElement section4Element
                   && section4Element.ValueKind == JsonValueKind.Array)
            {
                string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Attachments");

                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                foreach (var item in section4Element.EnumerateArray())
                {
                    if (item.TryGetProperty("documents", out var docProp) && docProp.ValueKind == JsonValueKind.Object)
                    {
                        var fileName = docProp.GetProperty("fileName").GetString();
                        var base64 = docProp.GetProperty("base64").GetString();

                        if (!string.IsNullOrWhiteSpace(base64))
                        {
                            var docDto = new FireDrillDocumentDto
                            {
                                FireDrillId = fireDrillId,
                                DocumentTitle = fileName,
                                Base64Content = base64,
                                SubmittedBy = dto.SubmittedBy
                            };

                            await _submitReportService.SaveFireDrillDocumentAsync(docDto);
                        }
                    }
                }

            }
            if (dto.EntryStatus == "complete")
            {
                await _emailService.SendMailFireDrillAfterSubmissionAsync(fireDrillId, dto.UnitId);
            }

                await transaction.CommitAsync();

                return Ok(new
            {
                Message = "Saved successfully",
                FireDrillId = fireDrillId
            });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // Optionally log exception
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost("update/{fireDrillId}")]
        public async Task<IActionResult> Update(int fireDrillId, [FromBody] FireDrillDto dto)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                await _submitReportService.UpdateFireDrillAsync(fireDrillId, dto);


                var existingResposTeam = await _context.FireDrillResposeEmp.Where(r => r.FireDrillId == fireDrillId).ToListAsync();

                if (existingResposTeam.Any())
                {
                    _context.FireDrillResposeEmp.RemoveRange(existingResposTeam);
                    await _context.SaveChangesAsync();
                }

                if (dto.FormData.TryGetValue("section_1", out var section1Obj) &&
    section1Obj is JsonElement section1Element)
                {
                    if (section1Element.ValueKind == JsonValueKind.String)
                    {
                        // 🔹 Parse the string to JSON
                        string rawJson = section1Element.GetString() ?? "[]";
                        using var doc = JsonDocument.Parse(rawJson);
                        var root = doc.RootElement;

                        if (root.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var item in root.EnumerateArray())
                            {
                                if (item.ValueKind == JsonValueKind.Object &&
                                    item.TryGetProperty("Employees", out var employeesElement) &&
                                    employeesElement.ValueKind == JsonValueKind.Array)
                                {
                                    foreach (var empElement in employeesElement.EnumerateArray())
                                    {
                                        if (empElement.TryGetInt32(out int empId))
                                        {
                                            _context.FireDrillResposeEmp.Add(new FireDrillResposeEmp
                                            {
                                                FireDrillId = fireDrillId,
                                                EmployeeId = empId
                                            });
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (section1Element.ValueKind == JsonValueKind.Array)
                    {
                        // ✅ Normal array case (if backend sends correct JSON)
                        foreach (var item in section1Element.EnumerateArray())
                        {
                            if (item.ValueKind == JsonValueKind.Object &&
                                item.TryGetProperty("Employees", out var employeesElement) &&
                                employeesElement.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var empElement in employeesElement.EnumerateArray())
                                {
                                    if (empElement.TryGetInt32(out int empId))
                                    {
                                        _context.FireDrillResposeEmp.Add(new FireDrillResposeEmp
                                        {
                                            FireDrillId = fireDrillId,
                                            EmployeeId = empId
                                        });
                                    }
                                }
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                }

                int RecommendationId = 0;
            var existingRecommendations = await _context.Recommendations.Where(r => r.FireDrillId == fireDrillId).ToListAsync();

            if (existingRecommendations.Any())
            {
                _context.Recommendations.RemoveRange(existingRecommendations);
                await _context.SaveChangesAsync();
            }

            var existingRecMap = existingRecommendations.ToDictionary(rec => rec.RecommendationId, rec => rec);

            if (dto.FormData.TryGetValue("section_3", out var section3Obj) && section3Obj is JsonElement section3Element && section3Element.ValueKind == JsonValueKind.Array)
            {
                int index = 0;
                foreach (var item in section3Element.EnumerateArray())
                {
                    var recommendedAction = item.GetProperty("recommendedAction").GetString();
                    if (!string.IsNullOrEmpty(recommendedAction))
                    {
                        var responsiblePersonStr = item.GetProperty("responsiblePerson").GetString();
                        int responsiblePerson = int.TryParse(responsiblePersonStr, out var parsedId) ? parsedId : 0;
                            var SeverityStr = item.GetProperty("Severity").GetString();
                            int SeverityId = int.TryParse(SeverityStr, out var passId) ? passId : 0;
                            var targetDateStr = item.GetProperty("targetDate").GetString();
                        if (!DateTime.TryParse(targetDateStr, out var parsedDate))
                            return BadRequest("Invalid target date");
                        parsedDate = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);
                        var targetDate = parsedDate;

                        var recommendationDto = new RecommendationDto
                        {
                            FireDrillId = fireDrillId,
                            ResponsibleUserId = responsiblePerson,
                            SeverityId=SeverityId,
                            RecommendationText = recommendedAction,
                            TargetDate = parsedDate,
                            ActionStatusId = 0,
                            CreatedBy = dto.SubmittedBy
                        };

                        RecommendationId = await _submitReportService.AddRecommendationAsync(recommendationDto);

                        if (dto.EntryStatus == "complete" && dto.Status == "released")
                        {
                            int TaskId;
                            var DrillTasksDto = new DrillTasksDto
                            {
                                taskCreatedForId = RecommendationId,
                                taskDetails = recommendedAction,
                                TargetDate = targetDate,
                                taskStatusId = 2,
                                taskModuleId = 1,
                                forsubmodule = "FD",
                                CreatedBy = dto.SubmittedBy
                            };

                            TaskId = await _submitReportService.AddTasksAsync(DrillTasksDto);

                            int deptId = await _context.Employees .Where(e => e.Id == responsiblePerson).Select(e => e.DeptId ?? 0).FirstOrDefaultAsync();

                            var taskAssgntoUserDto = new taskAssgntoUserDto
                            {
                                taskId = TaskId,
                                EmpdeptId = deptId,
                                EmployeeId = responsiblePerson,
                                userTaskStatusId = 2,
                                CreatedBy = dto.SubmittedBy
                            };

                            await _submitReportService.AddtaskAssgntoUserAsync(taskAssgntoUserDto);
                          

                            await _emailService.SendMailFireDrillAfterReleasedAsync(fireDrillId, TaskId, dto.UnitId);


                        }
                    }
                    index++;
                }

            }
                var existingDocuments = await _context.FireDrillDocuments.Where(r => r.FireDrillId == fireDrillId).ToListAsync();

                if (existingDocuments.Any())
                {
                    _context.FireDrillDocuments.RemoveRange(existingDocuments);
                    await _context.SaveChangesAsync();
                }
                if (dto.FormData.TryGetValue("section_4", out var section4Obj)
                      && section4Obj is JsonElement section4Element
                      && section4Element.ValueKind == JsonValueKind.Array)
                {
                    string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Attachments");

                    if (!Directory.Exists(uploadPath))
                        Directory.CreateDirectory(uploadPath);

                    foreach (var item in section4Element.EnumerateArray())
                    {
                        if (item.TryGetProperty("documents", out var docProp) && docProp.ValueKind == JsonValueKind.Object)
                        {
                            var fileName = docProp.GetProperty("fileName").GetString();
                            var base64 = docProp.GetProperty("base64").GetString();

                            if (!string.IsNullOrWhiteSpace(base64))
                            {
                                var docDto = new FireDrillDocumentDto
                                {
                                    FireDrillId = fireDrillId,
                                    DocumentTitle = fileName,
                                    Base64Content = base64,
                                    SubmittedBy = dto.SubmittedBy
                                };

                                await _submitReportService.SaveFireDrillDocumentAsync(docDto);
                            }
                        }
                    }

                }
                if (dto.Status == "released")
                {
                    await transaction.CommitAsync();
                    return Ok(new
                {

                    Message = "Released successfully",
                    FireDrillId = fireDrillId
                });
            }
            if (dto.Status == "review")
            {
                await _emailService.SendMailFireDrillAfterSubmissionAsync(fireDrillId, dto.UnitId);
                    await transaction.CommitAsync();
                    return Ok(new
                {

                    Message = "Review successfully",
                    FireDrillId = fireDrillId
                });
            }
            else
            {
                if (dto.EntryStatus == "complete")
                {
                    await _emailService.SendMailFireDrillAfterSubmissionAsync(fireDrillId, dto.UnitId);
                }
                    await transaction.CommitAsync();
                    return Ok(new
                {

                    Message = "Update successfully",
                    FireDrillId = fireDrillId
                });
            }
               

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // Optionally log exception
                return StatusCode(500, $"An error occurred: {ex.Message}");
    }

}

        [HttpDelete("DeleteFireDrill")]
        public async Task<IActionResult> DeleteFireDrill([FromQuery] int id, [FromQuery] int empId)
        {
            var result = await _submitReportService.DeleteFireDrillAsync(id, empId);
            if (!result)
            {
                return NotFound(new { message = "Fire Drill not found or already deleted." });
            }
            await _emailService.SendMailFireDrillDeletedAsync(id, empId);
            return Ok(new { message = "Fire Drill deleted successfully." });
        }


        //public async Task<IActionResult> Submit([FromBody] FireDrillDto dto)
        //{
        //    //using (var transaction = await _context.Database.BeginTransactionAsync())
        //    //{
        //    //    try
        //    //    {
        //    int RecommendationId = 0;
        //    // 1. Save FireDrill and get ID
        //    int fireDrillId = await _submitReportService.SubmitFireDrillAsync(dto);

        //    // 2. Extract and save recommendations from FormData["section_2"]
        //    if (dto.FormData.TryGetValue("section_2", out var section2Obj) && section2Obj is JsonElement section2Element && section2Element.ValueKind == JsonValueKind.Array)
        //    {
        //        foreach (var item in section2Element.EnumerateArray())
        //        {
        //            var recommendedAction = item.GetProperty("recommendedAction").GetString();
        //            if (!string.IsNullOrEmpty(recommendedAction))
        //            {
        //                var responsiblePersonStr = item.GetProperty("responsiblePerson").GetString();
        //                int responsiblePerson = int.TryParse(responsiblePersonStr, out var parsedId) ? parsedId : 0;
        //                //var targetDateStr = item.GetProperty("targetDate").GetString();
        //                //var targetDate = DateTime.TryParse(targetDateStr, out var parsedDate)
        //                //    ? DateTime.SpecifyKind(parsedDate, DateTimeKind.Local)  // Important for PostgreSQL
        //                //    : throw new Exception("Invalid target date");
        //                var targetDateStr = item.GetProperty("targetDate").GetString();

        //                if (!DateTime.TryParse(targetDateStr, out var parsedDate))
        //                    return BadRequest("Invalid target date");

        //                var targetDate = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);


        //                var recommendationDto = new RecommendationDto
        //                {
        //                    FireDrillId = fireDrillId,
        //                    ResponsibleUserId = responsiblePerson,
        //                    RecommendationText = recommendedAction,
        //                    TargetDate = targetDate,
        //                    ActionStatusId = 0,
        //                    CreatedBy = dto.SubmittedBy
        //                };

        //                RecommendationId = await _submitReportService.AddRecommendationAsync(recommendationDto);
        //                if (dto.EntryStatus == "complete")
        //                {
        //                    var DrillTasksDto = new DrillTasksDto
        //                    {
        //                        taskCreatedForId = RecommendationId,
        //                        taskDetails = recommendedAction,
        //                        severityId = 0,
        //                        TargetDate = targetDate,
        //                        taskStatusId = 2,
        //                        taskModuleId = 1,
        //                        forsubmodule = "FD",
        //                        CreatedBy = dto.SubmittedBy
        //                    };

        //                    int TaskId = await _submitReportService.AddTasksAsync(DrillTasksDto);
        //                    var taskAssgntoUserDto = new taskAssgntoUserDto
        //                    {
        //                        taskId = TaskId,
        //                        EmpdeptId = 0,
        //                        EmployeeId = responsiblePerson,
        //                        userTaskStatusId = 2,
        //                        CreatedBy = dto.SubmittedBy
        //                    };
        //                    await _submitReportService.AddtaskAssgntoUserAsync(taskAssgntoUserDto);
        //                }
        //            }
        //        }
        //    }

        //    return Ok(new
        //    {
        //        Message = "Saved successfully",
        //        FireDrillId = fireDrillId
        //    });
        //    //}
        //    //catch (Exception ex)
        //    //{
        //    //    await transaction.RollbackAsync();
        //    //    return StatusCode(500, $"Error Occurred:{ex.Message}");
        //    //}
        //    // }
        //}
        //[HttpPost("update/{fireDrillId}")]
        //public async Task<IActionResult> Update(int fireDrillId, [FromBody] FireDrillDto dto)
        //{
        //    using (var transaction = await _context.Database.BeginTransactionAsync())
        //    {
        //        try
        //        {
        //            int RecommendationId = 0;
        //            await _submitReportService.UpdateFireDrillAsync(fireDrillId, dto);

        //            var existingRecommendations = await _context.Recommendations.Where(r => r.FireDrillId == fireDrillId).ToListAsync();

        //            var existingRecMap = existingRecommendations.ToDictionary(rec => rec.RecommendationId, rec => rec);

        //            if (dto.FormData.TryGetValue("section_2", out var section2Obj) && section2Obj is JsonElement section2Element && section2Element.ValueKind == JsonValueKind.Array)
        //            {
        //                int index = 0;
        //                foreach (var item in section2Element.EnumerateArray())
        //                {
        //                    var recommendedAction = item.GetProperty("recommendedAction").GetString();
        //                    if (!string.IsNullOrEmpty(recommendedAction))
        //                    {
        //                        var responsiblePersonStr = item.GetProperty("responsiblePerson").GetString();
        //                        int responsiblePerson = int.TryParse(responsiblePersonStr, out var parsedId) ? parsedId : 0;
        //                        //var targetDateStr = item.GetProperty("targetDate").GetString();
        //                        //var targetDate = DateTime.TryParse(targetDateStr, out var parsedDate)
        //                        //               ? DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc)
        //                        //               : throw new Exception("Invalid target date");
        //                        var targetDateStr = item.GetProperty("targetDate").GetString();

        //                        if (!DateTime.TryParse(targetDateStr, out var parsedDate))
        //                            return BadRequest("Invalid target date");

        //                        var targetDate = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);



        //                        var existingRec = existingRecommendations.ElementAtOrDefault(index);
        //                        if (existingRec != null)
        //                        {

        //                            existingRec.ResponsibleUserId = responsiblePerson;
        //                            existingRec.RecommendationText = recommendedAction;
        //                            existingRec.TargetDate = targetDate;
        //                            existingRec.ModifiedBy = dto.SubmittedBy;
        //                            existingRec.ModifiedOn = DateTime.UtcNow;

        //                            _context.Recommendations.Update(existingRec);
        //                            RecommendationId = existingRec.RecommendationId;
        //                        }
        //                        else
        //                        {
        //                            var recommendationDto = new RecommendationDto
        //                            {
        //                                FireDrillId = fireDrillId,
        //                                ResponsibleUserId = responsiblePerson,
        //                                RecommendationText = recommendedAction,
        //                                TargetDate = targetDate,
        //                                ActionStatusId = 0,
        //                                CreatedBy = dto.SubmittedBy
        //                            };

        //                            RecommendationId = await _submitReportService.AddRecommendationAsync(recommendationDto);
        //                        }
        //                        if (dto.EntryStatus == "complete")
        //                        {
        //                            var existingTask = await _context.DrillTask.FirstOrDefaultAsync(t => t.taskCreatedForId == RecommendationId && t.taskModuleId == 1 && t.forsubmodule == "FD");
        //                            int TaskId;

        //                            if (existingTask != null)
        //                            {
        //                                existingTask.taskDetails = recommendedAction;
        //                                existingTask.TargetDate = targetDate;
        //                                existingTask.ModifiedBy = dto.SubmittedBy;
        //                                existingTask.ModifiedOn = DateTime.UtcNow;

        //                                _context.DrillTask.Update(existingTask);
        //                                TaskId = existingTask.taskId;
        //                            }
        //                            else
        //                            {
        //                                var DrillTasksDto = new DrillTasksDto
        //                                {
        //                                    taskCreatedForId = RecommendationId,
        //                                    taskDetails = recommendedAction,
        //                                    severityId = 0,
        //                                    TargetDate = targetDate,
        //                                    taskStatusId = 2,
        //                                    taskModuleId = 1,
        //                                    forsubmodule = "FD",
        //                                    CreatedBy = dto.SubmittedBy
        //                                };

        //                                TaskId = await _submitReportService.AddTasksAsync(DrillTasksDto);
        //                            }
        //                            var existsAssignment = await _context.taskAssgntoUsers.AnyAsync(a => a.taskId == TaskId && a.EmployeeId == responsiblePerson);

        //                            if (!existsAssignment)
        //                            {
        //                                var taskAssgntoUserDto = new taskAssgntoUserDto
        //                                {
        //                                    taskId = TaskId,
        //                                    EmpdeptId = 0,
        //                                    EmployeeId = responsiblePerson,
        //                                    userTaskStatusId = 2,
        //                                    CreatedBy = dto.SubmittedBy
        //                                };

        //                                await _submitReportService.AddtaskAssgntoUserAsync(taskAssgntoUserDto);
        //                            }
        //                        }
        //                    }
        //                }

        //                index++;
        //            }

        //            return Ok(new
        //            {
        //                Message = "Update successfully",
        //                FireDrillId = fireDrillId
        //            });
        //        }
        //        catch (Exception ex)
        //        {
        //            await transaction.RollbackAsync();
        //            return StatusCode(500, $"Error Occurred:{ex.Message}");
        //        }
        //    }
        //}


        [HttpGet("fire-drill/{id}")]
        public async Task<IActionResult> GetFireDrillById(int id)
        {
            var drill = await _submitReportService.GetFireDrillByIdAsync(id);
            if (drill == null)
                return NotFound();

            return Ok(drill);
        }

        [HttpGet("fire-drills")]

        public async Task<IActionResult> GetFireDrills([FromQuery] FireDrillFilterDto filter)  //[FromQuery] Dictionary<string, string?> filters
        {
            var result = await _submitReportService.GetFireDrillsAsync(filter);  //filters
            return Ok(result);
        }



        [HttpGet("download")]
        public IActionResult DownloadFile([FromQuery] string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return BadRequest("Path is required.");

            // Combine with your root directory
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), relativePath);

            if (!System.IO.File.Exists(fullPath))
                return NotFound();

            var fileName = Path.GetFileName(fullPath);
            var fileBytes = System.IO.File.ReadAllBytes(fullPath);

            // Force download
            return File(fileBytes, "application/octet-stream", fileName);
        }
    }
}
