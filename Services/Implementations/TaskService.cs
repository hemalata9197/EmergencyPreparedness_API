using EmergencyManagement.Data;
using EmergencyManagement.Models.DTOs;
using EmergencyManagement.Models.DTOs.Task;
using EmergencyManagement.Models.Entities;
using EmergencyManagement.Models.Entities.Task;
using EmergencyManagement.Services.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Threading.Tasks;
using static EmergencyManagement.Utilities.DatabaseUtility;
using static System.Runtime.InteropServices.JavaScript.JSType;
using TaskStatus = EmergencyManagement.Models.Entities.Task.TaskStatus;
//using TaskStatus = EmergencyManagement.Models.Entities.TaskStatus;

namespace EmergencyManagement.Services.Implementations
{
    public class TaskService : ITaskService
    {
        private readonly EHSDbContext _context;
        private readonly IConfiguration _configuration;

        public TaskService(EHSDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }      
        public async Task<TaskPagedResult> GetAllTaskAsync(TaskFilterDto filter)
        {
            try
            {
                var role = filter.role?.ToLower();
                var loginUserId = filter.loginUserId;

                // Get all facilities where the user is HOD
                var hodFacilityIds = await _context.FacilityMasters
                    .Where(f => !f.IsDeleted && f.FacilityHead == loginUserId)
                    .Select(f => f.Id)
                    .ToListAsync();


                // Build base query with joins
                var query = from a in _context.FireDrills
                            join p in _context.Recommendations on a.FireDrillId equals p.FireDrillId
                            join t in _context.Tasks on p.RecommendationId equals t.TaskCreatedForId
                            join e in _context.Employees on p.ResponsibleUserId equals e.Id
                            join tu in _context.taskAssgntoUsers on t.TaskId equals tu.TaskId into taskuserJoin
                            from tu in taskuserJoin.DefaultIfEmpty()
                            join apps in _context.ApprovalStatus on p.ActionStatusId equals apps.ApprovalStatusId into appsJoin
                            from apps in appsJoin.DefaultIfEmpty()
                            join ts in _context.TaskStatus on t.TaskStatusId equals ts.taskStatusId into tsJoin
                            from ts in tsJoin.DefaultIfEmpty()
                            where a.UnitId==filter.UnitId
                            && a.EntryStatus == "complete"
                               && a.IsReview == true
                               && a.IsReleased == true
                               && (!filter.AreaId.HasValue || a.Facility1Id == filter.AreaId)
                               && (!filter.SectionId.HasValue || a.Facility2Id == filter.SectionId)
                               && (!filter.taskStatusId.HasValue || t.TaskStatusId == filter.taskStatusId)
                               && (!filter.approvalStatusId.HasValue || p.ActionStatusId == filter.approvalStatusId)
                               && (string.IsNullOrEmpty(filter.RefNo) || a.RefNo == filter.RefNo)
                                && (!filter.ResponsiblePersonId.HasValue || tu.EmployeeId == filter.ResponsiblePersonId)
                            //          && (filter.FromDate.HasValue, fd => fd.SubmittedOn >= DateTime.SpecifyKind(filter.FromDate.Value, DateTimeKind.Utc))
                            //&& (filter.ToDate.HasValue, fd => fd.SubmittedOn < DateTime.SpecifyKind(filter.ToDate.Value.AddDays(1), DateTimeKind.Utc))
                            select new
                            {
                                a.FireDrillId,
                                a.SubmittedOn,
                                t.TaskId,
                                t.TaskCreatedForId,
                                a.RefNo,
                                TaskDetails = p.RecommendationText,
                                ResponsiblePerson = e.FullName,
                                TargetDate = t.TargetDate,
                                remark = t.Remarks,
                                taskStatusId = t.TaskStatusId,
                                TaskStatus = ts != null ? ts.taskStatus : "Not Assigned",
                                ApprovalStatus = apps != null ? apps.ApprovalStatusText : "Pending",
                                ClosedRemark = p.ClosedRemark,
                                ActionStatusId = p.ActionStatusId,
                                FacilityId = tu.EmpDeptId,
                                SubmittedBy = a.SubmittedBy,
                                 FileEvidence = _context.TaskEvidence
        .Where(te => te.Taskid == t.TaskId)
        .OrderByDescending(te => te.SubmittedOn)
        .Select(te => new {
            te.DocumentTitle,
            te.DocumentPath
        })
        .FirstOrDefault()
                            };


                if (filter.FromDate.HasValue)
                {
                    var fromDateUtc = DateTime.SpecifyKind(filter.FromDate.Value.Date, DateTimeKind.Utc);
                    query = query.Where(fd => fd.SubmittedOn >= fromDateUtc);
                }

                if (filter.ToDate.HasValue)
                {
                    var toDateUtc = DateTime.SpecifyKind(filter.ToDate.Value.Date.AddDays(1), DateTimeKind.Utc);
                    query = query.Where(fd => fd.SubmittedOn < toDateUtc);
                }
                // Apply role-based filter
                if (role == "admin" || role == "superadmin")
                {
                    // No additional filter
                }
                else if (hodFacilityIds.Any())
                {
                    query = query.Where(x => hodFacilityIds.Contains(x.FacilityId ?? 0));
                }
                else
                {
                    query = query.Where(x => x.SubmittedBy == loginUserId);
                }

                // Execute query and paginate
                var totalCount = await query.CountAsync();

                var pagedData = await query
                    .OrderBy(t => t.TaskId)
                    .Skip(filter.PageIndex * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                var facilityIds = pagedData
              .Select(x => x.FacilityId)
              .Distinct()
              .ToList();

                var facilities = await _context.FacilityMasters
                          .Where(f => facilityIds.Contains(f.Id) && !f.IsDeleted)
                          .ToDictionaryAsync(f => f.Id, f => f.FacilityHead);

                var result = pagedData.Select(d => new getTaskDto
                {
                    FireDrillId = d.FireDrillId,
                    taskId = d.TaskId,
                    taskCreatedForId = d.TaskCreatedForId,
                    RefNo = d.RefNo,
                    TaskDetails = d.TaskDetails,
                    ResponsiblePerson = d.ResponsiblePerson,
                    TargetDate = d.TargetDate,
                    Remarks = d.remark,
                    TaskStatusId = d.taskStatusId,
                    TaskStatus = d.TaskStatus,
                    ApprovalStatusId = d.ActionStatusId,
                    ApprovalStatus = d.ApprovalStatus,
                    ClosedRemark = d.ClosedRemark,
                    RespUserHOD = d.FacilityId.HasValue ? facilities.GetValueOrDefault(d.FacilityId.Value) : null,
                    FileName = d.FileEvidence?.DocumentTitle,
                    DocumentPath = d.FileEvidence?.DocumentPath
                }).ToList();

                return new TaskPagedResult
                {
                    Data = result,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while fetching task data.", ex);
            }
        }
        public async Task<List<TaskStatus>> GetFilteredTaskStatusAsync(string taskstatusFor)
        {
            IQueryable<TaskStatus> query = _context.TaskStatus.Where(ts => ts.IsActive == true);

            if (taskstatusFor == "ChangeStstus")
            {
                // Only return status where ID is NOT 2
                query = query.Where(ts => ts.taskStatusId != 2);
            }

            return await query.ToListAsync();
        }
        public async Task<List<ApprovalStatus>> GetFilteredApprovalStatusAsync()
        {
            return await _context.ApprovalStatus
                .Where(ts => ts.IsActive == true)
                .ToListAsync();
        }
        public async Task<bool> UpdateTaskSatusAsync(UpdateTaskStatusDto dto)
        {
            var existingTask = await _context.Tasks
                .FirstOrDefaultAsync(t => t.TaskId == dto.taskId);
            var assignedUser = await _context.taskAssgntoUsers
                .FirstOrDefaultAsync(u => u.TaskId == dto.taskId);

            if (existingTask == null || assignedUser == null)
                return false;

            var newTargetDate = DateTime.SpecifyKind(dto.TargetDate, DateTimeKind.Utc);

            // ✅ Only increment if the target date is being changed
            bool isDateChanged = existingTask.TargetDate != newTargetDate.Date;

            if (isDateChanged && existingTask.TargetDateChangeCount >= 2)
            {
                throw new InvalidOperationException("Target date can only be extend twice.");
            }

            // ✅ Task
            existingTask.TaskStatusId = dto.taskstatusId;
            existingTask.Remarks = dto.remark;
            existingTask.ModifiedBy = dto.modifiedBy;
            existingTask.ModifiedOn = DateTimeHelper.EnsureUtc(DateTime.UtcNow);
            existingTask.TargetDate = newTargetDate;
            existingTask.TargetDateChangeCount++;

            _context.Tasks.Update(existingTask);

            assignedUser.UserTaskStatusId = dto.taskstatusId;
            assignedUser.Remarks = dto.remark;
            assignedUser.ModifiedBy = dto.modifiedBy;
            assignedUser.ModifiedOn = DateTimeHelper.EnsureUtc(DateTime.UtcNow);

            _context.taskAssgntoUsers.Update(assignedUser);

            var drillTaskHistory = new TaskHistory
            {
                TaskId = dto.taskId,
                taskStatusId = dto.taskstatusId,
                TargetDate = newTargetDate,
                Remarks = dto.remark,
                CreatedBy = dto.modifiedBy,
                CreatedOn = DateTimeHelper.EnsureUtc(DateTime.UtcNow),
                IsActive = true
            };

            _context.TaskHistory.Add(drillTaskHistory);
            if (dto.TaskEvidence != null && !string.IsNullOrEmpty(dto.TaskEvidence.base64))
            {
                var folderPath = Path.Combine("Attachments", "TaskEvidence");  //, dto.FireDrillId.ToString()
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                var extension = GetFileExtensionFromBase64(dto.TaskEvidence.base64);
                var uniqueFileName = Path.GetFileNameWithoutExtension(dto.TaskEvidence.fileName) + "_" + Guid.NewGuid().ToString().Substring(0, 8) + "." + extension;
                var filePath = Path.Combine(folderPath, uniqueFileName);

                var base64Parts = dto.TaskEvidence.base64.Split(",");
                var base64Data = base64Parts.Length > 1 ? base64Parts[1] : base64Parts[0];
                var bytes = Convert.FromBase64String(base64Data);

                await File.WriteAllBytesAsync(filePath, bytes);


                var relativePath = filePath.Replace("\\", "/");
                var fullUrl = $"{_configuration["App:BaseUrl"]}/{relativePath}";

                var doc = new TaskEvidence
                {
                    Taskid = dto.taskId,
                    DocumentTitle = dto.TaskEvidence.fileName,
                    DocumentPath = fullUrl,
                    SubmittedBy = dto.modifiedBy,
                    SubmittedOn = DateTime.UtcNow,
                };

                _context.TaskEvidence.Add(doc);
            }


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
        public async Task<bool> UpdateTaskApprovalAsync(TaskApprovalDto dto)
        {

            var existingRec = await _context.Recommendations
                .FirstOrDefaultAsync(r => r.RecommendationId == dto.taskCreatedForId);

            if (existingRec == null)
                return false;


            var existingTask = await _context.Tasks
                .FirstOrDefaultAsync(t => t.TaskCreatedForId == dto.taskCreatedForId);

            if (existingTask == null)
                throw new Exception("Associated task not found.");


            if (dto.approvalStatusId == 1)
            {
                // Only update Recommendation
                existingRec.ActionStatusId = dto.approvalStatusId;
                existingRec.ClosedBy = dto.closedby;
                existingRec.ClosedOn = DateTimeHelper.EnsureUtc(DateTime.UtcNow);
                existingRec.ClosedRemark = dto.closedRemark;

                _context.Recommendations.Update(existingRec);
            }
            else if (dto.approvalStatusId == 2)
            {

                DateTime? newTargetDate = existingTask.TargetDate.HasValue
     ? DateTime.SpecifyKind(existingTask.TargetDate.Value, DateTimeKind.Utc)
     : (DateTime?)null;

                existingRec.ActionStatusId = dto.approvalStatusId;
                existingRec.ClosedBy = dto.closedby;
                existingRec.ClosedOn = DateTimeHelper.EnsureUtc(DateTime.UtcNow);
                existingRec.ClosedRemark = dto.closedRemark;

                _context.Recommendations.Update(existingRec);
                await _context.SaveChangesAsync();
                try
                {
                    // Update Task
                    existingTask.TaskStatusId = 3;
                    existingTask.ModifiedBy = dto.closedby;
                    existingTask.ModifiedOn = DateTime.UtcNow;
                    existingTask.TargetDate = newTargetDate;
                    _context.Tasks.Update(existingTask);
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    var inner = ex.InnerException?.Message;
                    throw new Exception("Database update failed: " + inner, ex);
                }


                // Update TaskAssignToUser
                var taskUser = await _context.taskAssgntoUsers
                    .FirstOrDefaultAsync(tu => tu.TaskId == existingTask.TaskId);

                if (taskUser != null)
                {
                    taskUser.UserTaskStatusId = 3;
                    taskUser.ModifiedBy = dto.closedby;
                    taskUser.ModifiedOn = DateTimeHelper.EnsureUtc(DateTime.UtcNow);
                    _context.taskAssgntoUsers.Update(taskUser);
                    await _context.SaveChangesAsync();
                }
            }

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
        public async Task<List<TaskHistoryDto>> getTaskHistoryAsync(int taskId)
        {
            var history = await (
       from t in _context.TaskHistory
       where t.TaskId == taskId
       join ts in _context.TaskStatus on t.taskStatusId equals ts.taskStatusId into tsJoin
       from ts in tsJoin.DefaultIfEmpty()
       orderby t.CreatedOn descending
       select new TaskHistoryDto
       {
           TaskStatus = ts != null ? ts.taskStatus : "Assign",
           TargetDate = t.TargetDate,
           Remarks = t.Remarks ?? string.Empty
       }
   ).ToListAsync();

            return history;
        }        
        public async Task<List<getTaskDto>> GetAllTaskForMailAsync(int fireDrillId)
        {
            var query = from a in _context.FireDrills
                        join p in _context.Recommendations on a.FireDrillId equals p.FireDrillId
                        join t in _context.Tasks on p.RecommendationId equals t.TaskCreatedForId into taskJoin
                        from t in taskJoin.DefaultIfEmpty()
                        join e in _context.Employees on p.ResponsibleUserId equals e.Id
                        join ts in _context.TaskStatus on t.TaskStatusId equals ts.taskStatusId into tsJoin
                        from ts in tsJoin.DefaultIfEmpty()
                        where a.FireDrillId == fireDrillId
                        select new
                        {
                            TaskDetails = p.RecommendationText,
                            ResponsiblePerson = e.FullName,
                            TargetDate = p.TargetDate,
                            Remarks = t != null ? t.Remarks : "",
                            TaskStatus = ts != null ? ts.taskStatus : "Not Assigned",
                            ClosedRemark = p.ClosedRemark,
                        };

            var allData = await query.ToListAsync();

            var result = allData.Select(d => new getTaskDto
            {

                TaskDetails = d.TaskDetails,
                ResponsiblePerson = d.ResponsiblePerson,
                TargetDate = d.TargetDate,
                Remarks = d.Remarks,
                TaskStatus = d.TaskStatus,
                ClosedRemark = d.ClosedRemark,
            }).ToList();

            return result;
        }
        public async Task<getTaskDto> GetAllTaskByTaskIdForMailAsync(int TaskId)
        {
            var query = from p in _context.Recommendations
                        join t in _context.Tasks on p.RecommendationId equals t.TaskCreatedForId into taskJoin
                        from t in taskJoin.DefaultIfEmpty()
                        join tu in _context.taskAssgntoUsers on t.TaskId equals tu.TaskId into taskuserJoin
                        from tu in taskuserJoin.DefaultIfEmpty()
                        join e in _context.Employees on tu.EmployeeId equals e.Id
                        join ts in _context.TaskStatus on t.TaskStatusId equals ts.taskStatusId into tsJoin
                        from ts in tsJoin.DefaultIfEmpty()
                        where t.TaskId == TaskId
                        select new
                        {
                            TaskDetails = p.RecommendationText,
                            ResponsiblePerson = e.FullName,
                            ResponsiblePersonId = tu.EmployeeId,
                            TargetDate = p.TargetDate,
                            Remarks = t != null ? t.Remarks : "",
                            TaskStatus = ts != null ? ts.taskStatus : "Not Assigned",
                            ClosedRemark = p.ClosedRemark,
                        };

            var data = await query.FirstOrDefaultAsync();

            if (data == null)
                return null;

            return new getTaskDto
            {
                TaskDetails = data.TaskDetails,
                ResponsiblePerson = data.ResponsiblePerson,
                ResponsiblePersonId = data.ResponsiblePersonId ?? 0,
                TargetDate = data.TargetDate,
                Remarks = data.Remarks,
                TaskStatus = data.TaskStatus,
                ClosedRemark = data.ClosedRemark
            };
        }
        public async Task<getTaskDto> GetAllTaskDeatilsAfterStatusUpdationByTaskIdForMailAsync(int TaskId)
        {
            var latestHistory = from th in _context.TaskHistory
                                where th.TaskId == TaskId
                                orderby th.CreatedOn descending
                                select th;
            var query = from p in _context.Recommendations
                        join t in _context.Tasks on p.RecommendationId equals t.TaskCreatedForId into taskJoin
                        from t in taskJoin.DefaultIfEmpty()
                        join tu in _context.taskAssgntoUsers on t.TaskId equals tu.TaskId into taskuserJoin
                        from tu in taskuserJoin.DefaultIfEmpty()
                        join e in _context.Employees on tu.EmployeeId equals e.Id
                        join ts in _context.TaskStatus on t.TaskStatusId equals ts.taskStatusId into tsJoin
                        from ts in tsJoin.DefaultIfEmpty()
                        join apps in _context.ApprovalStatus on p.ActionStatusId equals apps.ApprovalStatusId into appsJoin
                        from apps in appsJoin.DefaultIfEmpty()
                        join th in latestHistory.Take(1) on t.TaskId equals th.TaskId into thJoin
                        from th in thJoin.DefaultIfEmpty()
                        join f in _context.FacilityMasters on tu.EmpDeptId equals f.Id into facilityJoin
                        from f in facilityJoin.DefaultIfEmpty()
                        where t.TaskId == TaskId
                        select new
                        {
                            TaskDetails = p.RecommendationText,
                            ResponsiblePerson = e.FullName,
                            ResponsiblePersonId = tu.EmployeeId,
                            TargetDate = p.TargetDate,
                            Remarks = t != null ? t.Remarks : "",
                            TaskSatusId = t.TaskStatusId,
                            TaskStatus = ts != null ? ts.taskStatus : "Not Assigned",
                            ClosedRemark = p.ClosedRemark,
                            ExtendedTargetDate = th.TargetDate,
                            UserAreaHead = f.FacilityHead,
                            ApprovalStatusId = p.ActionStatusId,
                            ApprovalStatus = apps != null ? apps.ApprovalStatusText : "Not Approve",
                        };

            var data = await query.FirstOrDefaultAsync();

            if (data == null)
                return null;

            return new getTaskDto
            {
                TaskDetails = data.TaskDetails,
                ResponsiblePerson = data.ResponsiblePerson,
                ResponsiblePersonId = data.ResponsiblePersonId ?? 0,
                TargetDate = data.TargetDate,
                Remarks = data.Remarks,
                TaskStatusId = data.TaskSatusId,
                TaskStatus = data.TaskStatus,
                ClosedRemark = data.ClosedRemark,
                ExtendedTargetDate = data.ExtendedTargetDate,
                UserAreaHead = data.UserAreaHead,
                ApprovalStatus = data.ApprovalStatus,
                ApprovalStatusId = data.ApprovalStatusId,

            };
        }


    }
}
