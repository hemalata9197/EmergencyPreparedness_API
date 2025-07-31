using EmergencyManagement.Data;
using EmergencyManagement.Models.DTOs;
using EmergencyManagement.Services.Interfaces;
using EmergencyManagement.Token;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using EmergencyManagement.Models.Entities;
using EmergencyManagement.Models.Entities.Task;
//using Newtonsoft.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Threading.Tasks;
using EmergencyManagement.Utilities;
using static EmergencyManagement.Utilities.DatabaseUtility;
using EmergencyManagement.Models.DTOs.Master;
using EmergencyManagement.Models.DTOs.Reports;
using System.Data;
using EmergencyManagement.Models.DTOs.Task;
using EmergencyManagement.Models.DTOs.Common;
using EmergencyManagement.Models.Entities.Fire_Drill;

namespace EmergencyManagement.Services.Implementations
{
    public class SubmitReportService : ISubmitReportService
    {
        private readonly EHSDbContext _context;
        private readonly IMastersService _MastersService;

        public SubmitReportService(EHSDbContext context, IMastersService mastersService)
        {
            _context = context;
            _MastersService = mastersService;
        }
        public async Task<UserInfoDto?> GetUserInfoAsync(string username)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return null;

            // 1. Get directly assigned menu IDs
            var assignedMenuIds = await _context.RoleMenus
                .Where(rm => rm.RoleId == user.RoleId && rm.IsActive)
                .Select(rm => rm.MenuId)
                .ToListAsync();

            // 2. Fetch all menus to traverse parent-child relations
            var allMenus = await _context.Menus
                .Where(m => m.IsActive)
                .ToListAsync();

            // 3. Recursive function to collect parent IDs
            HashSet<int> GetWithParents(IEnumerable<int> menuIds)
            {
                var allIds = new HashSet<int>(menuIds);
                var menuDict = allMenus.ToDictionary(m => m.Id, m => m.ParentId);

                foreach (var id in menuIds)
                {
                    var currentId = id;
                    while (menuDict.ContainsKey(currentId) && menuDict[currentId] != null && !allIds.Contains(menuDict[currentId]!.Value))
                    {
                        var parentId = menuDict[currentId];
                        if (parentId.HasValue)
                        {
                            allIds.Add(parentId.Value);
                            currentId = parentId.Value;
                        }
                        else break;
                    }
                }

                return allIds;
            }

            // 4. Include all parent menus
            var menuIdsWithParents = GetWithParents(assignedMenuIds);

            // 5. Get all required menus for the tree
            var menusToShow = allMenus
                .Where(m => menuIdsWithParents.Contains(m.Id))
                .OrderBy(m => m.SortOrder)
                .ToList();

            // 6. Build tree
            List<MenuDto> BuildMenuTree(List<Menu> menus, int? parentId)
            {
                return menus
                    .Where(m => m.ParentId == parentId)
                    .OrderBy(m => m.SortOrder)
                    .Select(m => new MenuDto
                    {
                        Id = m.Id,
                        Name = m.Title,
                        Route = m.Route,
                        Children = BuildMenuTree(menus, m.Id)
                    })
                    .ToList();
            }

            var menuTree = BuildMenuTree(menusToShow, null);

            return new UserInfoDto
            {
                Username = user.Username,
                Roles = new List<string>(),
                Permissions = new List<string>(),
                Menu = menuTree
            };
        }
        public async Task<int> SaveFireDrillAsync(FireDrillDto dto)
        {
            var formData = dto.FormData;

            var section0 = DatabaseUtility.GetFirstSection(formData, "section_0");

            var section2 = DatabaseUtility.GetFirstSection(formData, "section_2");

            var refNo = section0?.GetValueOrDefault("Reference No")?.ToString();
            if (string.IsNullOrWhiteSpace(refNo) || refNo == "Auto")
                refNo = null;
            var fireDrill = new FireDrill
            {
                RefNo = refNo,
                //FireDrillDate = DatabaseUtility.ParseDate(section0?.GetValueOrDefault("Fire Drill Date")) ?? DateTime.MinValue,
                FireDrillDate = DatabaseUtility.ParseDate(section0?.GetValueOrDefault("Fire Drill Date")) ?? DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc),
                Time = section0?.GetValueOrDefault("Time")?.ToString() ?? string.Empty,
                Facility1Id = DatabaseUtility.ParseInt(section0?.GetValueOrDefault("area")) ?? 0,
                Facility2Id = DatabaseUtility.ParseInt(section0?.GetValueOrDefault("section")) ?? 0,
                ScenarioId = DatabaseUtility.ParseInt(section0?.GetValueOrDefault("scenario")) ?? 0,
                CallReceivedAtFireStation = section0?.GetValueOrDefault("Call received at Fire Station / Security")?.ToString() ?? string.Empty,
                TurnOutOfFireTender = section0?.GetValueOrDefault("Turn Out of Fire Tender")?.ToString() ?? string.Empty,
                SecurityReachedAtSite = section0?.GetValueOrDefault("Fire / Security reached at Site")?.ToString() ?? string.Empty,
                FireTenderReturnedAtFireStation = section0?.GetValueOrDefault("Fire Tender returned at Fire Station / Security")?.ToString() ?? string.Empty,
                ActionTaken = section2?.GetValueOrDefault("actions")?.ToString() ?? "",
                EntryStatus = dto.EntryStatus,
                UnitId = dto.UnitId,
                SubmittedBy = dto.SubmittedBy,
                SubmittedOn = DateTime.UtcNow,
                IsActive = true,

            };
            _context.FireDrills.Add(fireDrill);


            //var employeeIds = section1?["Employees"]?.ToString();

            //if (!string.IsNullOrWhiteSpace(employeeIds))
            //{
            //    var ids = JsonSerializer.Deserialize<List<int>>(employeeIds);
            //    if (ids != null)
            //    {
            //        foreach (var empId in ids)
            //        {
            //            _context.FireDrillResposeEmp.Add(new FireDrillResposeEmp
            //            {
            //                FireDrillId = fireDrill.FireDrillId,
            //                EmployeeId = empId
            //            });
            //        }

            //        await _context.SaveChangesAsync();
            //    }
            //}
            try
            {
                await _context.SaveChangesAsync();
                await _context.Entry(fireDrill).ReloadAsync();
            }
            catch (DbUpdateException ex)
            {
                var inner = ex.InnerException?.Message;
                throw new Exception("Database update failed: " + inner, ex);
            }
            return fireDrill.FireDrillId;
        }
        public async Task<bool> UpdateFireDrillAsync(int fireDrillId, FireDrillDto dto)
        {
            var existing = await _context.FireDrills.FirstOrDefaultAsync(fd => fd.FireDrillId == fireDrillId);
            if (existing == null)
                return false;

            var formData = dto.FormData;

            var section0 = DatabaseUtility.GetFirstSection(formData, "section_0");
            var section2 = DatabaseUtility.GetFirstSection(formData, "section_2");
            var section5 = DatabaseUtility.GetFirstSection(formData, "section_5");


            //existing.FireDrillDate = DatabaseUtility.ParseDate(section0?.GetValueOrDefault("Fire Drill Date"))
            //                         ?? DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);
            existing.Time = section0?.GetValueOrDefault("Time")?.ToString() ?? string.Empty;
            existing.Facility1Id = DatabaseUtility.ParseInt(section0?.GetValueOrDefault("area")) ?? 0;
            existing.Facility2Id = DatabaseUtility.ParseInt(section0?.GetValueOrDefault("section")) ?? 0;
            existing.ScenarioId = DatabaseUtility.ParseInt(section0?.GetValueOrDefault("scenario")) ?? 0;
            existing.CallReceivedAtFireStation = section0?.GetValueOrDefault("Call received at Fire Station / Security")?.ToString() ?? string.Empty;
            existing.TurnOutOfFireTender = section0?.GetValueOrDefault("Turn Out of Fire Tender")?.ToString() ?? string.Empty;
            existing.SecurityReachedAtSite = section0?.GetValueOrDefault("Fire / Security reached at Site")?.ToString() ?? string.Empty;
            existing.FireTenderReturnedAtFireStation = section0?.GetValueOrDefault("Fire Tender returned at Fire Station / Security")?.ToString() ?? string.Empty;
            existing.ActionTaken = section2?.GetValueOrDefault("actions")?.ToString() ?? "";

            existing.EntryStatus = dto.EntryStatus;
            existing.ModifiedBy = dto.SubmittedBy;
            existing.ModifiedOn = DateTime.UtcNow;

            if (dto.Status == "review")
            {
                existing.ReviewBy = dto.SubmittedBy;
                existing.ReviewOn = DateTime.UtcNow;
                existing.IsReview = true;
                existing.ReviewRemark = section5?.GetValueOrDefault("Review Remarks")?.ToString() ?? "";
            }
            else if (dto.Status == "released")
            {
                existing.ReleasedBy = dto.SubmittedBy;
                existing.ReleasedOn = DateTime.UtcNow;
                existing.IsReleased = true;
            }
            _context.FireDrills.Update(existing);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                var inner = ex.InnerException?.Message;
                throw new Exception("Database update failed: " + inner, ex);
            }

            return true;
        }
        public async Task<int> AddRecommendationAsync(RecommendationDto dto)
        {
            var Recommendation = new Recommendation
            {
                FireDrillId = dto.FireDrillId,
                ResponsibleUserId = dto.ResponsibleUserId,
                RecommendationText = dto.RecommendationText,
                SeverityId=dto.SeverityId,  
                TargetDate = dto.TargetDate,
                ActionStatusId = dto.ActionStatusId,
                CreatedBy = dto.CreatedBy,
                CreatedOn = DateTime.UtcNow

            };

            _context.Recommendations.Add(Recommendation);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                var inner = ex.InnerException?.Message;
                throw new Exception("Database update failed: " + inner, ex);
            }
            //await _context.SaveChangesAsync();
            return Recommendation.RecommendationId;
        }
        public async Task<int> AddTasksAsync(DrillTasksDto dto)
        {
            try
            {
                // Ensure UTC for all DateTime values
                var targetDate = DateTime.SpecifyKind(dto.TargetDate, DateTimeKind.Utc);
                var createdOn = DateTime.UtcNow;

                var drillTask = new Tasks
                {
                    TaskCreatedForId = dto.taskCreatedForId,
                    TaskDetails = dto.taskDetails,
                    TargetDate = targetDate,
                    TaskStatusId = dto.taskStatusId,
                    TaskModuleId = dto.taskModuleId,
                    ForSubModule = dto.forsubmodule,
                    CreatedBy = dto.CreatedBy,
                    CreatedOn = createdOn,
                    IsActive = true
                };

                _context.Tasks.Add(drillTask);
                await _context.SaveChangesAsync();


                var drillTaskHistory = new TaskHistory
                {
                    TaskId = drillTask.TaskId,
                    taskStatusId = dto.taskStatusId,
                    TargetDate = targetDate,
                    Remarks = "",
                    CreatedBy = dto.CreatedBy,
                    CreatedOn = createdOn,
                    IsActive = true
                };

                _context.TaskHistory.Add(drillTaskHistory);
                await _context.SaveChangesAsync();

                return drillTask.TaskId;
            }
            catch (Exception ex)
            {
                Console.WriteLine("EF Save Error: " + ex.Message);
                if (ex.InnerException != null)
                    Console.WriteLine("Inner: " + ex.InnerException.Message);
                throw;
            }
        }
        public async Task AddtaskAssgntoUserAsync(taskAssgntoUserDto dto)
        {
            var taskAssgntoUser = new taskAssgntoUser
            {
                TaskId = dto.taskId,
                EmpDeptId = dto.EmpdeptId,
                EmployeeId = dto.EmployeeId,
                UserTaskStatusId = dto.userTaskStatusId,
                CreatedBy = dto.CreatedBy,
            };

            _context.taskAssgntoUsers.Add(taskAssgntoUser);
            await _context.SaveChangesAsync();

        }
        public async Task<bool> DeleteFireDrillAsync(int id, int empId)
        {
            var fireDrill = await _context.FireDrills.FindAsync(id);
            if (fireDrill == null || !fireDrill.IsActive)
                return false;

            fireDrill.IsActive = false;
            fireDrill.ModifiedBy = empId;
            fireDrill.ModifiedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<object?> GetFireDrillByIdAsync(int id)
        {
            var fireDrill = await _context.FireDrills.FindAsync(id);
            if (fireDrill == null)
                return null;

            var recommendations = await _context.Recommendations
                .Where(r => r.FireDrillId == id)
                .ToListAsync();

            var documents = await _context.FireDrillDocuments
                .Where(d => d.FireDrillId == id)
                .ToListAsync();

            //var employeeIds = new List<string> { "1", "5", "2", "3", "4", "6" };
            var employeeIds = await _context.FireDrillResposeEmp
    .Where(fe => fe.FireDrillId == fireDrill.FireDrillId)
    .Select(fe => fe.EmployeeId.ToString())
    .ToListAsync();


            var employeeName = await _context.Employees
    .Where(e => e.Id == fireDrill.SubmittedBy)
    .Select(e => e.FullName)
    .FirstOrDefaultAsync() ?? "Unknown";

            var formData = new Dictionary<string, List<Dictionary<string, string>>>
            {
                ["section_0"] = new List<Dictionary<string, string>>
                {
                      new Dictionary<string, string>
                      {
                          { "Reference No", fireDrill.RefNo },
                          { "Filled By", employeeName  },
                          { "Filled On",fireDrill.SubmittedOn.ToString("dd.MMM.yyyy")  },
                          { "Fire Drill Date", fireDrill.FireDrillDate.ToString("yyyy-MM-dd") },
                          { "Time", fireDrill.Time },
                          { "area", fireDrill.Facility1Id.ToString() },
                          { "section", fireDrill.Facility2Id.ToString() },
                          { "scenario", fireDrill.ScenarioId.ToString() },
                          { "Call received at Fire Station / Security", fireDrill.CallReceivedAtFireStation?? "" },
                          { "Turn Out of Fire Tender", fireDrill.TurnOutOfFireTender??"" },
                          { "Fire / Security reached at Site", fireDrill.SecurityReachedAtSite ?? "" },
                          { "Fire Tender returned at Fire Station / Security", fireDrill.FireTenderReturnedAtFireStation ?? "" },
                      }
                },
                ["section_1"] = new List<Dictionary<string, string>>
                {
                      new Dictionary<string, string>
                      {
                          //{ "actions", fireDrill.ActionTaken }
                           { "Employees", string.Join(",", employeeIds) }
                      }
                },
                ["section_2"] = new List<Dictionary<string, string>>
                {
                      new Dictionary<string, string>
                      {
                          { "actions", fireDrill.ActionTaken }
                      }
                },
                ["section_3"] = recommendations.Select(r => new Dictionary<string, string>
                {
                        { "recommendedAction", r.RecommendationText },
                        { "responsiblePerson", r.ResponsibleUserId.ToString() },
                        { "targetDate", r.TargetDate.ToString("yyyy-MM-dd") },
                        { "Severity",r.SeverityId.ToString() }

                }).ToList(),
                ["section_4"] = documents.Select(d => new Dictionary<string, string>
                {
                          { "documents", d.DocumentTitle }, // this will go into file name
                          { "documentPath", d.DocumentPath }
                }).ToList(),
                ["section_5"] = new List<Dictionary<string, string>>
                {
                      new Dictionary<string, string>
                      {
                          { "Review Remarks", fireDrill.ReviewRemark ?? ""}
                      }
                },
            };

            return new
            {
                fireDrill.FireDrillId,
                fireDrill.RefNo,
                fireDrill.EntryStatus,
                fireDrill.SubmittedBy,
                fireDrill.UnitId,
                fireDrill.SubmittedOn,
                formData
            };
        }
        public async Task<FireDrillPagedResult> GetFireDrillsAsync(FireDrillFilterDto filter)
        {
            try
            {
                var role = filter.role?.ToLower();
                var loginUserId = filter.loginUserId;

                var hodFacilityIds = await _context.FacilityMasters
    .Where(f => !f.IsDeleted && f.FacilityHead == loginUserId)
    .Select(f => f.Id)
    .ToListAsync();
                // Step 1: Build filtered query directly in DB
                var query = _context.FireDrills
                    .Where(fd => fd.IsActive && fd.UnitId == filter.UnitId)
                    .WhereIf(filter.Facility1Id.HasValue, fd => fd.Facility1Id == filter.Facility1Id)
                    .WhereIf(filter.Facility2Id.HasValue, fd => fd.Facility2Id == filter.Facility2Id)
                    .WhereIf(filter.ScenarioId.HasValue, fd => fd.ScenarioId == filter.ScenarioId)
                    .WhereIf(!string.IsNullOrWhiteSpace(filter.RefNo), fd => fd.RefNo == filter.RefNo)
                    .WhereIf(filter.FromDate.HasValue, fd => fd.SubmittedOn >= DateTime.SpecifyKind(filter.FromDate.Value, DateTimeKind.Utc))
                   .WhereIf(filter.ToDate.HasValue, fd => fd.SubmittedOn < DateTime.SpecifyKind(filter.ToDate.Value.AddDays(1), DateTimeKind.Utc));


                if (role.Trim().ToLower() == "admin" || role.Trim().ToLower() == "superadmin")
                {
                    // Admins see all
                    // No filter needed
                }
                else if (hodFacilityIds.Any())
                {
                    // If user is HOD of at least one facility, show those fire drills
                    query = query.Where(fd => hodFacilityIds.Contains(fd.Facility1Id));
                }
                else
                {
                    // Otherwise treat them as a normal user — only show their submissions
                    query = query.Where(fd => fd.SubmittedBy == loginUserId);
                }

                // Step 2: Count total for pagination
                var total = await query.CountAsync();

                // Step 3: Fetch paginated data from DB (no materialization until here)
                var pageData = await query
                    .OrderBy(fd => fd.FireDrillId)
                    .Skip(filter.PageIndex * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(fd => new
                    {
                        fd.FireDrillId,
                        fd.RefNo,
                        fd.EntryStatus,
                        fd.SubmittedBy,
                        fd.UnitId,
                        fd.SubmittedOn,
                        fd.IsReview,
                        fd.IsReleased,
                        fd.FireDrillDate,
                        fd.Time,
                        fd.Facility1Id,
                        fd.Facility2Id,
                        fd.ScenarioId,
                        Status = fd.EntryStatus == "inprogress" ? "In Progress"
            : fd.EntryStatus == "complete" && (fd.IsReview != true) ? "Review Pending"
            : fd.IsReview == true && (fd.IsReleased != true) ? "Release Pending"
            : fd.IsReleased == true ? "Released"
            : "-"
                    })
                    .ToListAsync();

                if (!pageData.Any())
                {
                    return new FireDrillPagedResult
                    {
                        Data = new List<getFireDrillDto>(),
                        TotalCount = 0
                    };
                }

                // Step 4: Collect related IDs
                var facilityIds = pageData
          .Select(x => x.Facility1Id)
          .Concat(pageData.Select(x => x.Facility2Id))
          .Distinct()
          .ToList();

                var scenarioIds = pageData.Select(x => x.ScenarioId).Distinct().ToList();



                // Step 5: Lookup tables (only needed records)
                //var facilities = await _context.FacilityMasters
                //    .Where(f => facilityIds.Contains(f.Id) && !f.IsDeleted)
                //    .ToDictionaryAsync(f => f.Id, f => f.Name);

                var facilities = await _context.FacilityMasters
                      .Where(f => facilityIds.Contains(f.Id) && !f.IsDeleted)
                      .ToDictionaryAsync(f => f.Id, f => new { f.Name, f.FacilityHead });

                var scenarios = await _context.ScenarioMasters
                    .Where(s => scenarioIds.Contains(s.Id) && s.IsActive)
                    .ToDictionaryAsync(s => s.Id, s => s.ScenarioName);

                int configElementId = 1;
                var rawValueIds = await _MastersService.CheckHODEHSAsync(configElementId, filter.UnitId);
                var hodEhsValueIds = rawValueIds?
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse)
                    .ToList() ?? new List<int>();

                //          var filteredData = pageData.Where(fd =>
                //          role == "admin" || role == "superadmin" ||
                //    ( facilities.TryGetValue(fd.Facility1Id, out var area) && area.FacilityHead == loginUserId) ||
                //    (role == "user" && fd.SubmittedBy == loginUserId)
                //).ToList();


                // Step 6: Map to result
                var result = pageData.Select(fd => new getFireDrillDto
                {
                    FireDrillId = fd.FireDrillId,
                    RefNo = fd.RefNo,
                    EntryStatus = fd.EntryStatus,
                    SubmittedBy = fd.SubmittedBy,
                    UnitId = fd.UnitId,
                    SubmittedOn = fd.SubmittedOn,
                    IsReview = fd.IsReview ?? false,
                    IsReleased = fd.IsReleased ?? false,
                    FireDrillDate = fd.FireDrillDate.ToString("yyyy-MM-dd"),
                    Time = fd.Time,
                    AreaName = facilities.GetValueOrDefault(fd.Facility1Id)?.Name ?? "-",
                    SectionName = facilities.GetValueOrDefault(fd.Facility2Id)?.Name ?? "-",
                    ScenarioName = scenarios.GetValueOrDefault(fd.ScenarioId) ?? "-",
                    AreaHOD = facilities.GetValueOrDefault(fd.Facility1Id)?.FacilityHead ?? 0,
                    ValueIds = string.Join(",", hodEhsValueIds),
                    Status = fd.Status
                }).ToList();
                return new FireDrillPagedResult
                {
                    Data = result,
                    TotalCount = total
                };
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while fetching fire drill data.", ex);
            }

        }
        public async Task<getFireDrillDto> GetFireDrillsformailAsync(int fireDrillId)
        {

            var fd = await _context.FireDrills
       .Where(f => f.FireDrillId == fireDrillId)
       .FirstOrDefaultAsync();

            if (fd == null)
                return null;

            var scenarioName = await _context.ScenarioMasters
        .Where(s => s.Id == fd.ScenarioId && s.IsActive)
        .Select(s => s.ScenarioName)
        .FirstOrDefaultAsync();

            var facilityInfo = await _context.FacilityMasters
     .Where(f => f.Id == fd.Facility1Id && !f.IsDeleted)
     .Select(f => new { f.Name, f.FacilityHead })
     .FirstOrDefaultAsync();

            string AreaName = facilityInfo?.Name;
            int areaHead = facilityInfo?.FacilityHead ?? 0;

            var SectionName = await _context.FacilityMasters
      .Where(s => s.Id == fd.Facility2Id && s.IsActive)
      .Select(s => s.Name)
      .FirstOrDefaultAsync();

            //  Fetch attachment paths for this FireDrill
            var attachments = await _context.FireDrillDocuments
                .Where(a => a.FireDrillId == fireDrillId)
                .Select(a => a.DocumentPath)
                .ToListAsync();


            return new getFireDrillDto
            {
                FireDrillId = fd.FireDrillId,
                RefNo = fd.RefNo,
                EntryStatus = fd.EntryStatus,
                SubmittedBy = fd.SubmittedBy,
                UnitId = fd.UnitId,
                SubmittedOn = fd.SubmittedOn,
                IsReview = fd.IsReview ?? false,
                IsReleased = fd.IsReleased ?? false,
                FireDrillDate = fd.FireDrillDate.ToString("yyyy-MM-dd"),
                Time = fd.Time,
                AreaName = AreaName,
                SectionName = SectionName,
                ScenarioName = scenarioName,
                ReviewRemark = fd.ReviewRemark ?? "",
                AreaHOD = areaHead,
                ReviewBy = fd.ReviewBy,
                ReleasedBy = fd.ReleasedBy,
                AttachmentPaths = attachments

            };
        }
        public async Task SaveFireDrillDocumentAsync(FireDrillDocumentDto dto)
        {
            var folderPath = Path.Combine("Attachments", "FireDrill");  //, dto.FireDrillId.ToString()
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var extension = GetFileExtensionFromBase64(dto.Base64Content);
            var uniqueFileName = Path.GetFileNameWithoutExtension(dto.DocumentTitle) + "_" + Guid.NewGuid().ToString().Substring(0, 8) + "." + extension;
            var filePath = Path.Combine(folderPath, uniqueFileName);

            var base64Parts = dto.Base64Content.Split(",");
            var base64Data = base64Parts.Length > 1 ? base64Parts[1] : base64Parts[0];
            var bytes = Convert.FromBase64String(base64Data);

            await File.WriteAllBytesAsync(filePath, bytes);

            var doc = new FireDrillDocuments
            {
                FireDrillId = dto.FireDrillId,
                DocumentTitle = dto.DocumentTitle,
                DocumentPath = filePath.Replace("\\", "/"),
                SubmittedBy = dto.SubmittedBy,
                SubmittedOn = DateTime.UtcNow
            };

            _context.FireDrillDocuments.Add(doc);
            await _context.SaveChangesAsync();
        }
        private string GetFileExtensionFromBase64(string mimeTypePart)
        {
            if (mimeTypePart.StartsWith("data:") && mimeTypePart.Contains(";"))
            {
                var mime = mimeTypePart.Split(':')[1].Split(';')[0];
                return mime switch
                {
                    // Image formats
                    "image/png" => "png",
                    "image/jpeg" => "jpg",
                    "image/gif" => "gif",
                    "image/bmp" => "bmp",

                    // Document formats
                    "application/pdf" => "pdf",
                    "application/msword" => "doc",
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => "docx",
                    "application/vnd.ms-excel" => "xls",
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" => "xlsx",
                    "text/plain" => "txt",

                    // Video formats
                    "video/mp4" => "mp4",
                    "video/webm" => "webm",
                    "video/ogg" => "ogg",
                    "video/x-msvideo" => "avi",
                    "video/quicktime" => "mov",

                    // Audio formats
                    "audio/mpeg" => "mp3",
                    "audio/wav" => "wav",
                    "audio/ogg" => "ogg",
                    "audio/webm" => "webm",
                    "audio/aac" => "aac",
                    "audio/x-ms-wma" => "wma",

                    _ => "bin"
                };
            }
            return "bin";
        }


    }
}
