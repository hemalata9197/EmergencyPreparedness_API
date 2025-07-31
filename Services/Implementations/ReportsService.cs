using EmergencyManagement.Models.DTOs.Master;
using EmergencyManagement.Models.DTOs.Reports;
using EmergencyManagement.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using EmergencyManagement.Services.Interfaces;
using EmergencyManagement.Data;
using EmergencyManagement.Utilities;
using static EmergencyManagement.Utilities.DatabaseUtility;
using System.Linq;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using EmergencyManagement.Models.Entities;
using EmergencyManagement.Models.Entities.Admin;
using EmergencyManagement.Models.Entities.Task;

namespace EmergencyManagement.Services.Implementations
{
    public class ReportsService : IReportsService
    {
        private readonly EHSDbContext _context;
        private readonly IMastersService _MastersService;

        public ReportsService(EHSDbContext context, IMastersService mastersService)
        {
            _context = context;
            _MastersService = mastersService;
        }

        //public async Task<List<ReportFieldDto>> GetReportFieldsAsync(string reportType, int unitId)
        //{

        //    return reportType switch
        //    {
        //        "FireDrillSummary" => new List<ReportFieldDto>
        //    {

        //        new() { Name = "Reference No", Label = "Reference No", Type = "text"},
        //        new() { Name = "Filled By", Label = "Filled By", Type = "text" },
        //        new() { Name = "Filled On", Label = "Filled On", Type = "text" },
        //        new() { Name = "Fire Drill Date", Label = "Fire Drill Date", Type = "text" },
        //         new() { Name = "Time", Label = "Time", Type = "text" },
        //          new() { Name = "Area", Label = "Area", Type = "text" },
        //           new() { Name = "Section", Label = "Section", Type = "text" },
        //           new() { Name = "Scenario", Label = "Scenario", Type = "text" },
        //           new() { Name = "Action Taken", Label = "Action Taken", Type = "text" },
        //          new() { Name = "Review Remark", Label = "Review Remark", Type = "text" },
        //           new() { Name = "Review Status", Label = "Review Status", Type = "text" },
        //           new() { Name = "Review By", Label = "Review By", Type = "text" },
        //           new() { Name = "Review On", Label = "Review On", Type = "text" },
        //           new() { Name = "Released Status", Label = "Released Status", Type = "text" },
        //           new() { Name = "Released On", Label = "Released On", Type = "text" },
        //           new() { Name = "Released By", Label = "Released By", Type = "text" },


        //    },
        //        "RecommandationSummary" => new List<ReportFieldDto>
        //    { new() { Name = "Reference No", Label = "Reference No", Type = "text"},
        //      new() { Name = "Fire Drill Date", Label = "Fire Drill Date", Type = "text" },
        //        new() { Name = "Area", Label = "Area", Type = "text" },
        //        new() { Name = "recommendationtext", Label = "Recommendation", Type = "text" },
        //             new() { Name = "Responsible Person", Label = "Responsible Person", Type = "text" },
        //        new() { Name = "Target Date", Label = "Target Date", Type = "text" },
        //          new() { Name = "Action Status", Label = "Action Status", Type = "text" },
        //           new() { Name = "Closed By", Label = "Closed By", Type = "text" },
        //           new() { Name = "Closed On", Label = "Closed On", Type = "text" },
        //           new() { Name = "Closed Remark", Label = "Closed Remark", Type = "text" },

        //    },
        //        "ComplianceReport" => new List<ReportFieldDto>
        //    {
        //        new() { Name = "Area", Label = "Area", Type = "text" },
        //        new() { Name = "Total Recommendation", Label = "Total Tasks", Type = "text" },
        //             new() { Name = "Open Recommendation", Label = "Open Tasks", Type = "text" },
        //        new() { Name = "Closed Recommendation", Label = "Closed Tasks", Type = "text" },
        //          new() { Name = "Closure Compliance", Label = "Closure", Type = "text" },


        //    },
        //        _ => new List<ReportFieldDto>()
        //    };
        //}
        public Task<List<ReportFieldDto>> GetReportFieldsAsync(string reportType, int unitId)
        {
            var result = reportType switch
            {
                "FireDrillSummary" => new List<ReportFieldDto>
        {
            new() { Name = "Reference No", Label = "Reference No", Type = "text"},
            new() { Name = "Filled By", Label = "Filled By", Type = "text" },
            new() { Name = "Filled On", Label = "Filled On", Type = "text" },
            new() { Name = "Fire Drill Date", Label = "Fire Drill Date", Type = "text" },
            new() { Name = "Time", Label = "Fire Drill Time", Type = "text" },
            new() { Name = "CallReceivedAtFireStation", Label = "Call received at Fire Station", Type = "text" },
            new() { Name = "TurnOutOfFireTender", Label = "Turn Out of Fire Tender", Type = "text" },
            new() { Name = "SecurityReachedAtSite", Label = "Fire / Security reached at Site", Type = "text" },
            new() { Name = "FireTenderReturnedAtFireStation", Label = "Fire Tender returned at Fire Station / Security", Type = "text" },
            new() { Name = "Area", Label = "Area", Type = "text" },
            new() { Name = "Section", Label = "Section", Type = "text" },
            new() { Name = "Scenario", Label = "Scenario", Type = "text" },
            new() { Name = "Action Taken", Label = "Action Taken", Type = "text" },
            new() { Name = "Review Remark", Label = "Review Remark", Type = "text" },
            new() { Name = "Review Status", Label = "Review Status", Type = "text" },
            new() { Name = "Review By", Label = "Review By", Type = "text" },
            new() { Name = "Review On", Label = "Review On", Type = "text" },
            new() { Name = "Released Status", Label = "Released Status", Type = "text" },
            new() { Name = "Released On", Label = "Released On", Type = "text" },
            new() { Name = "Released By", Label = "Released By", Type = "text" },
        },

                "RecommandationSummary" => new List<ReportFieldDto>
        {
            new() { Name = "Reference No", Label = "Reference No", Type = "text"},
            new() { Name = "Fire Drill Date", Label = "Fire Drill Date", Type = "text" },
            new() { Name = "Area", Label = "Area", Type = "text" },
            new() { Name = "recommendationtext", Label = "Recommendation", Type = "text" },
            new() { Name = "Responsible Person", Label = "Responsible Person", Type = "text" },
            new() { Name = "Target Date", Label = "Target Date", Type = "text" },
            new() { Name = "Action Status", Label = "Action Status", Type = "text" },
            new() { Name = "Closed By", Label = "Closed By", Type = "text" },
            new() { Name = "Closed On", Label = "Closed On", Type = "text" },
            new() { Name = "Closed Remark", Label = "Closed Remark", Type = "text" },
        },

                "ComplianceReport" => new List<ReportFieldDto>
        {
            new() { Name = "Area", Label = "Area", Type = "text" },
            new() { Name = "Total Recommendation", Label = "Total Recommendation", Type = "text" },
            new() { Name = "Open Recommendation", Label = "Open Recommendation", Type = "text" },
            new() { Name = "Closed Recommendation", Label = "Closed Recommendation", Type = "text" },
            new() { Name = "Overdue Recommendation", Label = "Overdue Recommendation", Type = "text" },
            new() { Name = "Recommendation Closure %", Label = "Recommendation Closure %", Type = "text" }, 
        },

                _ => new List<ReportFieldDto>()
            };

            return Task.FromResult(result);
        }
        public async Task<PagedResult<Dictionary<string, object>>> GetFireDrillSummaryAsync(ReportFilterDto filter)
        {
            // Step 1: Build filtered query directly in DB
            var query = _context.FireDrills
                .Where(fd => fd.IsActive && fd.EntryStatus == "complete")
                .WhereIf(filter.UnitId != 3, fd => fd.UnitId == filter.UnitId)
                .WhereIf(filter.Facility1Id.HasValue, fd => fd.Facility1Id == filter.Facility1Id)
                .WhereIf(filter.Facility2Id.HasValue, fd => fd.Facility2Id == filter.Facility2Id)
                .WhereIf(filter.ScenarioId.HasValue, fd => fd.ScenarioId == filter.ScenarioId)
                .WhereIf(!string.IsNullOrWhiteSpace(filter.RefNo), fd => fd.RefNo == filter.RefNo)
                .WhereIf(filter.FromDate.HasValue, fd => fd.SubmittedOn >= DateTime.SpecifyKind(filter.FromDate.Value, DateTimeKind.Utc))
                .WhereIf(filter.ToDate.HasValue, fd => fd.SubmittedOn <= DateTime.SpecifyKind(filter.ToDate.Value.AddDays(1), DateTimeKind.Utc));

            // Step 2: Count total for pagination
            var total = await query.CountAsync();

            

            // Step 3: Fetch paginated data from DB (no materialization until here)
            var pageData = await query
                .OrderBy(fd => fd.FireDrillId)
                .Skip(filter.PageSize == 0 ? 0 : filter.PageIndex * filter.PageSize)
                .Take(filter.PageSize == 0 ? int.MaxValue : filter.PageSize)
                .Select(fd => new
                {
                    fd.FireDrillId,
                    fd.RefNo,
                    fd.SubmittedBy,
                    fd.SubmittedOn,
                    fd.FireDrillDate,
                    fd.Time,
                    fd.Facility1Id,
                    fd.Facility2Id,
                    fd.ScenarioId,
                    fd.ActionTaken,
                    fd.ReviewRemark,
                    fd.IsReview,
                    fd.ReviewBy,
                    fd.ReviewOn,
                    fd.IsReleased,
                    fd.ReleasedOn,
                    fd.ReleasedBy,
                    fd.CallReceivedAtFireStation,
                    fd.TurnOutOfFireTender,
                    fd.SecurityReachedAtSite,
                    fd.FireTenderReturnedAtFireStation
                })
                .ToListAsync();

            if (!pageData.Any())
                return new PagedResult<Dictionary<string, object>> { Data = [], TotalCount = 0 };

            // Step 4: Collect related IDs
            var facilityIds = pageData
      .Select(x => x.Facility1Id)
      .Concat(pageData.Select(x => x.Facility2Id))
      .Distinct()
      .ToList();

            var scenarioIds = pageData.Select(x => x.ScenarioId).Distinct().ToList();

            var userIds = pageData.Select(x => x.SubmittedBy)
                .Concat(pageData.Where(x => x.ReviewBy.HasValue).Select(x => x.ReviewBy.Value))
                .Concat(pageData.Where(x => x.ReleasedBy.HasValue).Select(x => x.ReleasedBy.Value))
                .Distinct()
                .ToList();

            // Step 5: Lookup tables (only needed records)
            var facilities = await _context.FacilityMasters
                .Where(f => facilityIds.Contains(f.Id) && !f.IsDeleted)
                .ToDictionaryAsync(f => f.Id, f => f.Name);

            var scenarios = await _context.ScenarioMasters
                .Where(s => scenarioIds.Contains(s.Id) && s.IsActive)
                .ToDictionaryAsync(s => s.Id, s => s.ScenarioName);

            var employees = await _context.Employees
                .Where(e => userIds.Contains(e.Id) && e.IsActive)
                .ToDictionaryAsync(e => e.Id, e => e.FullName);

            // Step 6: Map to result
            var result = pageData.Select(item => new Dictionary<string, object>
            {  


                ["Reference No"] = item.RefNo ?? "",
                ["Filled By"] = employees.GetValueOrDefault(item.SubmittedBy) ?? "-",
                ["Filled On"] = item.SubmittedOn.ToString("dd MMM yy"),
                ["Fire Drill Date"] = item.FireDrillDate.ToString("dd MMM yy"),
                ["Time"] = item.Time ?? "",
                ["CallReceivedAtFireStation"]=item.CallReceivedAtFireStation??"-",
                ["TurnOutOfFireTender"]=item.TurnOutOfFireTender??"-",
                ["SecurityReachedAtSite"]=item.SecurityReachedAtSite??"-",
                ["FireTenderReturnedAtFireStation"]=item.FireTenderReturnedAtFireStation??"-",
                ["Area"] = facilities.GetValueOrDefault(item.Facility1Id ) ?? "-",
                ["Section"] = facilities.GetValueOrDefault(item.Facility2Id) ?? "-",
                ["Scenario"] = scenarios.GetValueOrDefault(item.ScenarioId) ?? "-",
                ["Action Taken"] = item.ActionTaken ?? "",
                ["Review Remark"] = item.ReviewRemark ?? "",
                ["Review Status"] = (item.IsReview ?? false) ? "Reviewed" : "Pending",
                ["Review By"] = item.ReviewBy.HasValue ? employees.GetValueOrDefault(item.ReviewBy.Value) ?? "-" : "-",
                ["Review On"] = item.ReviewOn.HasValue ? item.ReviewOn.Value.ToString("dd MMM yy") : "-",
                ["Released Status"] = (item.IsReleased ?? false) ? "Released" : "Pending",
                ["Released On"] = item.ReleasedOn.HasValue ? item.ReleasedOn.Value.ToString("dd MMM yy") : "-",
                ["Released By"] = item.ReleasedBy.HasValue ? employees.GetValueOrDefault(item.ReleasedBy.Value) ?? "" : "-"
            }).ToList();

            // Step 7: Return result
            return new PagedResult<Dictionary<string, object>>
            {
                Data = result,
                TotalCount = total
            };
        }
        public async Task<PagedResult<Dictionary<string, object>>> GetRecSummaryAsync(ReportFilterDto filter)
        {
            // Step 1: Join FireDrill + Recommendation with filters applied at DB level
            var query = from fd in _context.FireDrills
                        join r in _context.Recommendations on fd.FireDrillId equals r.FireDrillId
                        join t in _context.Tasks on r.RecommendationId equals t.TaskCreatedForId into tJoin
                        from t in tJoin.DefaultIfEmpty()
                        join apps in _context.TaskStatus on t.TaskStatusId equals apps.taskStatusId into appsJoin
                        from apps in appsJoin.DefaultIfEmpty()
                        where fd.IsActive                             
                              && (filter.UnitId == 3 || fd.UnitId == filter.UnitId)
                              && fd.EntryStatus == "complete"
                              && fd.IsReleased == true
                              && r.IsActive
                              && (!filter.Facility1Id.HasValue || fd.Facility1Id == filter.Facility1Id)
                              && (string.IsNullOrEmpty(filter.RefNo) || fd.RefNo == filter.RefNo)
                              && (!filter.FromDate.HasValue || fd.SubmittedOn >= DateTime.SpecifyKind(filter.FromDate.Value, DateTimeKind.Utc))
                              && (!filter.ToDate.HasValue || fd.SubmittedOn <= DateTime.SpecifyKind(filter.ToDate.Value.AddDays(1), DateTimeKind.Utc))

                              
            select new
                        {
                            fd.FireDrillId,
                            fd.RefNo,
                            fd.FireDrillDate,
                            fd.Facility1Id,
                            r.RecommendationText,
                            r.ResponsibleUserId,
                            t.TargetDate,
                            r.ActionStatusId,
                            r.ClosedBy,
                            r.ClosedOn,
                            r.ClosedRemark,
                            apps.taskStatus
                        };

            // Step 2: Get total count for pagination
            var totalCount = await query.CountAsync();

            // Step 3: Get paginated data
            var dataPage = await query
                .Skip(filter.PageIndex * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            if (!dataPage.Any())
            {
                return new PagedResult<Dictionary<string, object>>
                {
                    Data = new List<Dictionary<string, object>>(),
                    TotalCount = 0
                };
            }

            // Step 4: Collect required IDs
            var facilityIds = dataPage.Select(x => x.Facility1Id).Distinct().ToList();
            var userIds = dataPage
                .Select(x => x.ResponsibleUserId)
                .Concat(dataPage.Where(x => x.ClosedBy.HasValue).Select(x => x.ClosedBy.Value))
                .Distinct()
                .ToList();

            // 🔁 Sequentially load lookup data to avoid DbContext concurrency issues
            var facilities = await _context.FacilityMasters
                .Where(f => facilityIds.Contains(f.Id) && !f.IsDeleted)
                .ToDictionaryAsync(f => f.Id, f => f.Name);

            var employees = await _context.Employees
                .Where(e => userIds.Contains(e.Id) && e.IsActive)
                .ToDictionaryAsync(e => e.Id, e => e.FullName);

            // Step 5: Map results
            var resultRows = dataPage.Select(item => new Dictionary<string, object>
            {
                ["Reference No"] = item.RefNo ?? "-",
                ["Fire Drill Date"] = item.FireDrillDate.ToString("dd MMM yy"),
                ["Area"] = facilities.GetValueOrDefault(item.Facility1Id) ?? "-",
                ["recommendationtext"] = item.RecommendationText ?? "-",
                ["Responsible Person"] = employees.GetValueOrDefault(item.ResponsibleUserId) ?? "-",
                ["Target Date"] = item.TargetDate.HasValue
    ? item.TargetDate.Value.ToString("dd MMM yy")
    : "-",
                //["Action Status"] = item.ActionStatusId == 1 ? "Verified and Closed" : "Open",
                ["Action Status"] = item.taskStatus==null?"Open": item. taskStatus,
                ["Closed By"] = item.ClosedBy.HasValue ? employees.GetValueOrDefault(item.ClosedBy.Value) ?? "-" : "-",
                ["Closed On"] = item.ClosedOn.HasValue ? item.ClosedOn.Value.ToString("dd MMM yy") : "-",
                ["Closed Remark"] = item.ClosedRemark ?? "-"

            
            }).ToList();

            // Step 6: Return paged result
            return new PagedResult<Dictionary<string, object>>
            {
                Data = resultRows,
                TotalCount = totalCount
            };
        }
        public async Task<PagedResult<Dictionary<string, object>>> GetAreaWiseComplianceSummaryAsync(ReportFilterDto filter)
        {
            // Step 1: Build fire drill query with all filters applied in DB
            var fireDrillQuery = _context.FireDrills
                .Where(fd => fd.IsActive && (filter.UnitId == 3 || fd.UnitId == filter.UnitId)
                 && fd.EntryStatus == "complete" && fd.IsReleased == true)
                .WhereIf(filter.Facility1Id.HasValue, fd => fd.Facility1Id == filter.Facility1Id)

             .WhereIf(filter.FromDate.HasValue, fd => fd.SubmittedOn >= DateTime.SpecifyKind(filter.FromDate.Value, DateTimeKind.Utc))
                   .WhereIf(filter.ToDate.HasValue, fd => fd.SubmittedOn < DateTime.SpecifyKind(filter.ToDate.Value.AddDays(1), DateTimeKind.Utc));

            
            var filteredFireDrills = await fireDrillQuery.ToListAsync();

            var fireDrillIds = filteredFireDrills.Select(fd => fd.FireDrillId).ToList();
            var facility1Ids = filteredFireDrills.Select(fd => fd.Facility1Id).Distinct().ToList();

            List<FacilityMaster> facilityList = new List<FacilityMaster>();
            if (facility1Ids.Count > 0)
            {
                facilityList = await _context.FacilityMasters
                    .Where(f => facility1Ids.Contains(f.Id) && !f.IsDeleted)
                    .ToListAsync();  //  fully materialized before proceeding
            }

            var facilities = facilityList.ToDictionary(f => f.Id, f => f.Name);

            //  Get recommendations
            List<Recommendation> recommendations = new List<Recommendation>();
            if (fireDrillIds.Count > 0)
            {
                recommendations = await _context.Recommendations
                    .Where(r => fireDrillIds.Contains(r.FireDrillId) && r.IsActive)
                    .ToListAsync();  // ✅ waited
            }
            var recIds = recommendations.Select(r => r.RecommendationId).ToList();
            List<Tasks> tasks = new List<Tasks>();
            if (recIds.Count > 0)
            {
                tasks = await _context.Tasks
                    .Where(t => recIds.Contains(t.TaskCreatedForId) && t.TaskModuleId == 1)
                    .ToListAsync();
            }

            // Join and aggregate
            var recWithArea = from rec in recommendations
                              join fd in filteredFireDrills on rec.FireDrillId equals fd.FireDrillId
                              select new
                              {
                                  AreaId = fd.Facility1Id,
                                  rec.ActionStatusId,
                                   Tasks = tasks.Where(t => t.TaskCreatedForId == rec.RecommendationId).ToList()
                              };



            var grouped = recWithArea
                .GroupBy(x => x.AreaId)
                .Select(g =>
                {
                    var total = g.Count();
                    var closed = g.Count(x => x.ActionStatusId == 1);
                    var open = total - closed;
                    var overdue = g.SelectMany(x => x.Tasks)
                      .Count(t => t.TargetDate.HasValue
                                  && t.TargetDate.Value.Date < DateTime.UtcNow.Date
                                  && t.TaskStatusId != 1);
                    var closurePercent = total > 0 ? Math.Round((double)closed * 100 / total, 2) : 0;

                    return new Dictionary<string, object>
                    {
                        ["Area"] = facilities.GetValueOrDefault(g.Key) ?? "Unknown",
                        ["Total Recommendation"] = total,
                        ["Open Recommendation"] = open,
                        ["Closed Recommendation"] = closed,
                        ["Overdue Recommendation"] = overdue,
                        ["Recommendation Closure %"] = $"{closurePercent} %"
                    };
                })
                .ToList();

            // Apply pagination
            var pagedData = grouped
                .Skip(filter.PageIndex * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            return new PagedResult<Dictionary<string, object>>
            {
                Data = pagedData,
                TotalCount = grouped.Count
            };
        }

        //    public async Task<PagedResult<Dictionary<string, object>>> GetFireDrillSummaryAsync(ReportFilterDto filter)
        //    {

        //        var facilities = await _context.FacilityMasters
        //            .Where(f => !f.IsDeleted)
        //            .ToDictionaryAsync(f => f.Id, f => f.Name);

        //        var scenarios = await _context.ScenarioMasters
        //            .Where(s => s.IsActive)
        //            .Select(s => new { s.Id, s.ScenarioName })
        //            .ToDictionaryAsync(x => x.Id, x => x.ScenarioName);


        //        var employees = await _context.Employees
        //            .Where(s => s.IsActive)
        //            .Select(s => new { s.Id, s.FullName })
        //            .ToDictionaryAsync(x => x.Id, x => x.FullName);

        //        var query = _context.FireDrills
        //           .Where(fd => fd.IsActive && fd.UnitId == filter.UnitId && fd.EntryStatus == "complete")
        //           .WhereIf(filter.Facility1Id.HasValue, fd => fd.Facility1Id == filter.Facility1Id)
        //           .WhereIf(filter.Facility2Id.HasValue, fd => fd.Facility2Id == filter.Facility2Id)
        //           .WhereIf(filter.ScenarioId.HasValue, fd => fd.ScenarioId == filter.ScenarioId)
        //         .WhereIf(!string.IsNullOrWhiteSpace(filter.RefNo), fd => fd.RefNo == filter.RefNo);


        //        var allData = await query.ToListAsync();
        //        var filtered = allData
        //           .Where(x =>
        //               (!filter.FromDate.HasValue || x.SubmittedOn.Date >= filter.FromDate.Value) &&
        //               (!filter.ToDate.HasValue || x.SubmittedOn.Date <= filter.ToDate.Value)
        //           )
        //           .ToList();

        //        var total = allData.Count;
        //        var pagedData = filtered
        //             .OrderBy(fd => fd.FireDrillId)
        //            .Skip(filter.PageIndex * filter.PageSize)
        //            .Take(filter.PageSize)
        //            .Select(item => new Dictionary<string, object>
        //            {
        //                ["Reference No"] = item.RefNo ?? "",
        //                ["Filled By"] = employees.GetValueOrDefault(item.SubmittedBy) ?? "",
        //                ["Filled On"] = item.SubmittedOn.ToString("dd MMM yy"),
        //                ["Fire Drill Date"] = item.FireDrillDate.ToString("dd MMM yy"),
        //                ["Time"] = item.Time,
        //                ["Area"] = facilities.GetValueOrDefault(item.Facility1Id) ?? "",
        //                ["Section"] = facilities.GetValueOrDefault(item.Facility2Id) ?? "",
        //                ["Scenario"] = scenarios.GetValueOrDefault(item.ScenarioId) ?? "",
        //                ["Action Taken"] = item.ActionTaken,
        //                ["Review Remark"] = item.ReviewRemark ?? "",
        //                ["Review Status"] = item.IsReview.ToString() ?? "",
        //                ["Review By"] = item.ReviewBy != null
        //? employees.GetValueOrDefault<int, string>(item.ReviewBy.Value) ?? ""
        //: "",
        //                ["Review On"] = item.ReviewOn.HasValue
        //? item.ReviewOn.Value.ToString("dd MMM yy")
        //: "",
        //                ["Released Status"] = item.IsReleased.ToString() ?? "",

        //                ["Released On"] = item.ReleasedOn.HasValue
        //? item.ReleasedOn.Value.ToString("dd MMM yy")
        //: "",
        //                ["Released By"] = item.ReleasedBy != null
        //? employees.GetValueOrDefault<int, string>(item.ReleasedBy.Value) ?? ""
        //: "",

        //            })
        //            .ToList();

        //        return new PagedResult<Dictionary<string, object>>
        //        {
        //            Data = pagedData,
        //            TotalCount = total
        //        };
        //    }

        //    public async Task<PagedResult<Dictionary<string, object>>> GetRecSummaryAsync(ReportFilterDto filter)
        //    {
        //        // Lookup tables
        //        var employees = await _context.Employees
        //            .Where(s => s.IsActive)
        //            .ToDictionaryAsync(x => x.Id, x => x.FullName);

        //        var facilities = await _context.FacilityMasters
        //            .Where(f => !f.IsDeleted)
        //            .ToDictionaryAsync(f => f.Id, f => f.Name);

        //        var scenarios = await _context.ScenarioMasters
        //            .Where(s => s.IsActive)
        //            .ToDictionaryAsync(x => x.Id, x => x.ScenarioName);

        //        // Filter FireDrills
        //        var query = _context.FireDrills
        //            .Where(fd => fd.IsActive && fd.UnitId == filter.UnitId && fd.EntryStatus == "complete" && fd.IsReleased == true)
        //            .WhereIf(filter.Facility1Id.HasValue, fd => fd.Facility1Id == filter.Facility1Id)
        //            .WhereIf(filter.Facility2Id.HasValue, fd => fd.Facility2Id == filter.Facility2Id)
        //            .WhereIf(filter.ScenarioId.HasValue, fd => fd.ScenarioId == filter.ScenarioId)
        //            .WhereIf(!string.IsNullOrWhiteSpace(filter.RefNo), fd => fd.RefNo == filter.RefNo);

        //        var allData = await query.ToListAsync();

        //        var filtered = allData
        //            .Where(x =>
        //                (!filter.FromDate.HasValue || x.SubmittedOn.Date >= filter.FromDate.Value) &&
        //                (!filter.ToDate.HasValue || x.SubmittedOn.Date <= filter.ToDate.Value))
        //            .ToList();

        //        var fireDrillIds = filtered.Select(x => x.FireDrillId).ToList();

        //        // Get recommendations
        //        var recommendations = await _context.Recommendations
        //            .Where(r => fireDrillIds.Contains(r.FireDrillId) && r.IsActive == true)
        //            .ToListAsync();

        //        var rows = new List<Dictionary<string, object>>();

        //        foreach (var fireDrill in filtered)
        //        {
        //            var recs = recommendations
        //                .Where(r => r.FireDrillId == fireDrill.FireDrillId)
        //                .ToList();

        //            if (recs.Any())
        //            {
        //                foreach (var rec in recs)
        //                {
        //                    rows.Add(new Dictionary<string, object>
        //                    {
        //                        ["Reference No"] = fireDrill.RefNo ?? "",
        //                        ["Fire Drill Date"] = fireDrill.FireDrillDate.ToString("dd MMM yy"),
        //                        ["Area"] = facilities.GetValueOrDefault(fireDrill.Facility1Id) ?? "",
        //                        ["recommendationtext"] = rec.RecommendationText ?? "",
        //                        ["Responsible Person"] = employees.GetValueOrDefault(rec.ResponsibleUserId) ?? "",
        //                        ["Target Date"] = rec.TargetDate.ToString("dd MMM yy"),
        //                        ["Action Status"] = rec.ActionStatusId == 1 ? "Closed" : "Open",
        //                        ["Closed By"] = rec.ClosedBy != null
        //? employees.GetValueOrDefault<int, string>(rec.ClosedBy.Value) ?? ""
        //: "-",
        //                        ["Closed On"] = rec.ClosedBy.HasValue
        //? rec.ClosedBy.Value.ToString("dd MMM yy")
        //: "-",
        //                        ["Closed Remark"] = rec.ClosedRemark ?? "-",
        //                    });
        //                }
        //            }
        //        }

        //        // Pagination (after filtering rows with recommendations)
        //        var pagedRows = rows
        //            .Skip(filter.PageIndex * filter.PageSize)
        //            .Take(filter.PageSize)
        //            .ToList();

        //        return new PagedResult<Dictionary<string, object>>
        //        {
        //            Data = pagedRows,
        //            TotalCount = rows.Count
        //        };
        //    }
        //public async Task<PagedResult<Dictionary<string, object>>> GetRecSummaryAsync(ReportFilterDto filter)
        //{
        //    // Step 1: Fetch lookup data in advance (fully awaited, separate queries)
        //    var employeeList = await _context.Employees
        //        .Where(s => s.IsActive)
        //        .ToListAsync();

        //    var facilityList = await _context.FacilityMasters
        //        .Where(f => !f.IsDeleted)
        //        .ToListAsync();

        //    var scenarioList = await _context.ScenarioMasters
        //        .Where(s => s.IsActive)
        //        .ToListAsync();

        //    // Convert to dictionaries in-memory
        //    var employees = employeeList.ToDictionary(x => x.Id, x => x.FullName);
        //    var facilities = facilityList.ToDictionary(f => f.Id, f => f.Name);
        //    var scenarios = scenarioList.ToDictionary(x => x.Id, x => x.ScenarioName);

        //    // Step 2: Build FireDrill query with filters
        //    var fireDrillQuery = _context.FireDrills
        //        .Where(fd => fd.IsActive && fd.UnitId == filter.UnitId && fd.EntryStatus == "complete" && fd.IsReleased==true)
        //        .WhereIf(filter.Facility1Id.HasValue, fd => fd.Facility1Id == filter.Facility1Id)
        //        .WhereIf(filter.Facility2Id.HasValue, fd => fd.Facility2Id == filter.Facility2Id)
        //        .WhereIf(filter.ScenarioId.HasValue, fd => fd.ScenarioId == filter.ScenarioId)
        //        .WhereIf(!string.IsNullOrWhiteSpace(filter.RefNo), fd => fd.RefNo == filter.RefNo);

        //    var fireDrillList = await fireDrillQuery.ToListAsync(); // Await fire drills

        //    // Step 3: In-memory date filter
        //    var filtered = fireDrillList
        //        .Where(x =>
        //            (!filter.FromDate.HasValue || x.SubmittedOn.Date >= filter.FromDate.Value.Date) &&
        //            (!filter.ToDate.HasValue || x.SubmittedOn.Date <= filter.ToDate.Value.Date))
        //        .ToList();

        //    // Step 4: Get recommendations based on filtered FireDrillIds
        //    var fireDrillIds = filtered.Select(x => x.FireDrillId).ToList();

        //    var recommendationList = fireDrillIds.Any()
        //        ? await _context.Recommendations
        //            .Where(r => fireDrillIds.Contains(r.FireDrillId) && r.IsActive)
        //            .ToListAsync()
        //        : new List<Recommendation>();

        //    // Step 5: Build result rows
        //    var rows = new List<Dictionary<string, object>>();

        //    foreach (var fireDrill in filtered)
        //    {
        //        var recs = recommendationList
        //            .Where(r => r.FireDrillId == fireDrill.FireDrillId)
        //            .ToList();

        //        foreach (var rec in recs)
        //        {
        //            rows.Add(new Dictionary<string, object>
        //            {
        //                ["Reference No"] = fireDrill.RefNo ?? "",
        //                ["Fire Drill Date"] = fireDrill.FireDrillDate.ToString("dd MMM yy"),
        //                ["Area"] = facilities.GetValueOrDefault(fireDrill.Facility1Id) ?? "",
        //                ["recommendationtext"] = rec.RecommendationText ?? "",
        //                ["Responsible Person"] = employees.GetValueOrDefault(rec.ResponsibleUserId) ?? "",
        //                ["Target Date"] = rec.TargetDate.ToString("dd MMM yy"),
        //                ["Action Status"] = rec.ActionStatusId == 1 ? "Closed" : "Open",
        //                ["Closed By"] = rec.ClosedBy.HasValue
        //                    ? employees.GetValueOrDefault(rec.ClosedBy.Value) ?? "-"
        //                    : "-",
        //                ["Closed On"] = rec.ClosedOn.HasValue
        //                    ? rec.ClosedOn.Value.ToString("dd MMM yy")
        //                    : "-",
        //                ["Closed Remark"] = rec.ClosedRemark ?? "-"
        //            });
        //        }
        //    }

        //    // Step 6: Pagination
        //    var pagedRows = rows
        //        .Skip(filter.PageIndex * filter.PageSize)
        //        .Take(filter.PageSize)
        //        .ToList();

        //    return new PagedResult<Dictionary<string, object>>
        //    {
        //        Data = pagedRows,
        //        TotalCount = rows.Count
        //    };
        //}
        //public async Task<PagedResult<Dictionary<string, object>>> GetRecSummaryAsync(ReportFilterDto filter)
        //{
        //    // Step 1: Filter FireDrills in DB with date handling
        //    var fireDrillQuery = _context.FireDrills
        //        .Where(fd => fd.IsActive && fd.UnitId == filter.UnitId && fd.EntryStatus == "complete" && fd.IsReleased==true)
        //        .WhereIf(filter.Facility1Id.HasValue, fd => fd.Facility1Id == filter.Facility1Id)
        //        .WhereIf(filter.Facility2Id.HasValue, fd => fd.Facility2Id == filter.Facility2Id)
        //        .WhereIf(filter.ScenarioId.HasValue, fd => fd.ScenarioId == filter.ScenarioId)
        //        .WhereIf(!string.IsNullOrWhiteSpace(filter.RefNo), fd => fd.RefNo == filter.RefNo);

        //    if (filter.FromDate.HasValue)
        //    {
        //        var fromDateUtc = DateTime.SpecifyKind(filter.FromDate.Value, DateTimeKind.Utc);
        //        fireDrillQuery = fireDrillQuery.Where(fd => fd.SubmittedOn >= fromDateUtc);
        //    }

        //    if (filter.ToDate.HasValue)
        //    {
        //        var toDateUtc = DateTime.SpecifyKind(filter.ToDate.Value, DateTimeKind.Utc);
        //        fireDrillQuery = fireDrillQuery.Where(fd => fd.SubmittedOn <= toDateUtc);
        //    }

        //    var fireDrills = await fireDrillQuery
        //        .Select(fd => new { fd.FireDrillId, fd.RefNo, fd.FireDrillDate, fd.Facility1Id })
        //        .ToListAsync();

        //    var fireDrillIds = fireDrills.Select(fd => fd.FireDrillId).ToList();

        //    if (!fireDrillIds.Any())
        //        return new PagedResult<Dictionary<string, object>> { Data = new(), TotalCount = 0 };

        //    // Step 2: Filter and fetch recommendations
        //    var recs = await _context.Recommendations
        //        .Where(r => fireDrillIds.Contains(r.FireDrillId) && r.IsActive)
        //        .ToListAsync();

        //    // Step 3: Lookup IDs (only those needed)
        //    var responsibleIds = recs.Select(r => r.ResponsibleUserId)
        //        .Union(recs.Where(r => r.ClosedBy.HasValue).Select(r => r.ClosedBy.Value))
        //        .Distinct()
        //        .ToList();

        //    var employees = await _context.Employees
        //        .Where(e => responsibleIds.Contains(e.Id))
        //        .ToDictionaryAsync(e => e.Id, e => e.FullName);

        //    var facilityIds = fireDrills.Select(fd => fd.Facility1Id).Distinct().ToList();

        //    var facilities = await _context.FacilityMasters
        //        .Where(f => facilityIds.Contains(f.Id) && !f.IsDeleted)
        //        .ToDictionaryAsync(f => f.Id, f => f.Name);

        //    // Step 4: Build result rows
        //    var joined = from fd in fireDrills
        //                 join r in recs on fd.FireDrillId equals r.FireDrillId
        //                 select new Dictionary<string, object>
        //                 {
        //                     ["Reference No"] = fd.RefNo ?? "",
        //                     ["Fire Drill Date"] = fd.FireDrillDate.ToString("dd MMM yy"),
        //                     ["Area"] = facilities.GetValueOrDefault(fd.Facility1Id) ?? "",
        //                     ["recommendationtext"] = r.RecommendationText ?? "",
        //                     ["Responsible Person"] = employees.GetValueOrDefault(r.ResponsibleUserId) ?? "",
        //                     ["Target Date"] = r.TargetDate.ToString("dd MMM yy"),
        //                     ["Action Status"] = r.ActionStatusId == 1 ? "Closed" : "Open",
        //                     ["Closed By"] = r.ClosedBy.HasValue ? employees.GetValueOrDefault(r.ClosedBy.Value) ?? "-" : "-",
        //                     ["Closed On"] = r.ClosedOn.HasValue ? r.ClosedOn.Value.ToString("dd MMM yy") : "-",
        //                     ["Closed Remark"] = r.ClosedRemark ?? "-"
        //                 };

        //    var rows = joined.ToList();

        //    // Step 5: Pagination
        //    var paged = rows
        //        .Skip(filter.PageIndex * filter.PageSize)
        //        .Take(filter.PageSize)
        //        .ToList();

        //    return new PagedResult<Dictionary<string, object>>
        //    {
        //        Data = paged,
        //        TotalCount = rows.Count
        //    };
        //}

        //public async Task<PagedResult<Dictionary<string, object>>> GetAreaWiseComplianceSummaryAsync(ReportFilterDto filter)
        //{
        //    // Get only Facility1Ids used in FireDrill
        //    var fireDrillQuery = _context.FireDrills
        //        .Where(fd => fd.IsActive && fd.UnitId == filter.UnitId && fd.EntryStatus == "complete" && fd.IsReleased==true)
        //        .WhereIf(filter.Facility1Id.HasValue, fd => fd.Facility1Id == filter.Facility1Id)
        //        .WhereIf(filter.Facility2Id.HasValue, fd => fd.Facility2Id == filter.Facility2Id)
        //        .WhereIf(filter.ScenarioId.HasValue, fd => fd.ScenarioId == filter.ScenarioId)
        //        .WhereIf(!string.IsNullOrWhiteSpace(filter.RefNo), fd => fd.RefNo == filter.RefNo);


        //    if (filter.FromDate.HasValue)
        //        fireDrillQuery = fireDrillQuery.Where(fd => fd.SubmittedOn.Date >= filter.FromDate.Value);
        //    if (filter.ToDate.HasValue)
        //        fireDrillQuery = fireDrillQuery.Where(fd => fd.SubmittedOn.Date <= filter.ToDate.Value);

        //    var filteredFireDrills = await fireDrillQuery.ToListAsync();

        //    //var fireDrills = await fireDrillQuery.ToListAsync();

        //    //var filteredFireDrills = fireDrills
        //    //    .Where(fd =>
        //    //        (!filter.FromDate.HasValue || fd.SubmittedOn.Date >= filter.FromDate.Value) &&
        //    //        (!filter.ToDate.HasValue || fd.SubmittedOn.Date <= filter.ToDate.Value))
        //    //    .ToList();

        //    var fireDrillIds = filteredFireDrills.Select(fd => fd.FireDrillId).ToList();

        //    // Get only facility names used in FireDrills
        //    var facility1Ids = filteredFireDrills.Select(fd => fd.Facility1Id).Distinct().ToList();

        //    var facilities = await _context.FacilityMasters
        //        .Where(f => facility1Ids.Contains(f.Id) && !f.IsDeleted)
        //        .ToDictionaryAsync(f => f.Id, f => f.Name);

        //    // Get active recommendations
        //    var recommendations = await _context.Recommendations
        //        .Where(r => fireDrillIds.Contains(r.FireDrillId) && r.IsActive)
        //        .ToListAsync();

        //    // Join recommendations to FireDrills
        //    var recWithArea = from rec in recommendations
        //                      join fd in filteredFireDrills on rec.FireDrillId equals fd.FireDrillId
        //                      select new
        //                      {
        //                          AreaId = fd.Facility1Id,
        //                          rec.ActionStatusId
        //                      };

        //    // Group and summarize
        //    var grouped = recWithArea
        //        .GroupBy(x => x.AreaId)
        //        .Select(g =>
        //        {
        //            var total = g.Count();
        //            var closed = g.Count(x => x.ActionStatusId == 1);
        //            var open = total - closed;
        //            var closurePercent = total > 0 ? Math.Round((double)closed * 100 / total, 2) : 0;

        //            return new Dictionary<string, object>
        //            {
        //                ["Area"] = facilities.GetValueOrDefault(g.Key) ?? "Unknown",
        //                ["Total Recommendation"] = total,
        //                ["Open Recommendation"] = open,
        //                ["Closed Recommendation"] = closed,
        //                ["Closure Compliance %"] = $"{closurePercent} %"
        //            };
        //        })
        //        .ToList();

        //    // Pagination
        //    var pagedData = grouped
        //        .Skip(filter.PageIndex * filter.PageSize)
        //        .Take(filter.PageSize)
        //        .ToList();

        //    return new PagedResult<Dictionary<string, object>>
        //    {
        //        Data = pagedData,
        //        TotalCount = grouped.Count
        //    };
        //}


        //public async Task<PagedResult<Dictionary<string, object>>> GetCAPADetailsAsync(ReportFilterDto filter)
        //{
        //    var query = _context.FireDrills
        //        .Where(c => c.UnitId == filter.UnitId)
        //        .Select(c => new Dictionary<string, object>
        //        {
        //            ["capaId"] = c.CAPAId.ToString(),
        //            ["description"] = c.Description,
        //            ["assignedTo"] = c.AssignedTo
        //        });

        //    var total = await query.CountAsync();
        //    var data = await query.Skip((filter.Page - 1) * filter.Size).Take(filter.Size).ToListAsync();

        //    return new PagedResult<Dictionary<string, object>> { Data = data, TotalCount = total };
        //}
    }
}
