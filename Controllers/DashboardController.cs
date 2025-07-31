using EmergencyManagement.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EmergencyManagement.Services.Interfaces;
using EmergencyManagement.Models.DTOs.Dashboard;
using EmergencyManagement.Models.DTOs.Master;
using EmergencyManagement.Utilities;
using Microsoft.AspNetCore.Cors;

namespace EmergencyManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowAngularApp")]
    public class DashboardController : ControllerBase
    {
        private readonly EHSDbContext _context;
        private readonly IMastersService _MastersService;
        public DashboardController(EHSDbContext context, IMastersService MastersService)
        {
            _context = context;
            _MastersService = MastersService;
        }


        [HttpGet("GetSummary")]
        public async Task<IActionResult> GetSummary([FromQuery] TrendFilterDto dto)
        {
            var query = _context.FireDrills
               .Where(fd => fd.IsActive )
               .WhereIf(dto.UnitId != 3, fd => fd.UnitId == dto.UnitId);
            var allData = await query.ToListAsync();

            var filtered = allData
                .Where(x =>
                    (!dto.FromDate.HasValue || x.SubmittedOn.Date >= dto.FromDate.Value) &&
                    (!dto.ToDate.HasValue || x.SubmittedOn.Date <= dto.ToDate.Value)
                )
                .ToList();

            var totalDrills = filtered
                .Where(f => f.IsActive)
                .Count();

            var totalSubmitted = filtered
                .Where(f => f.IsActive && f.EntryStatus == "complete")
                .Count();

            var totalDraft = filtered
                .Where(f => f.IsActive && f.EntryStatus == "inprogress")
                .Count();

            var totalReview = filtered
                .Where(f => f.IsActive && f.IsReview == true)

                .Count();

            var totalReleased = filtered
              .Where(f => f.IsActive && f.IsReleased == true)
              .Count();




            return Ok(new
            {
                totalDrills,
                totalSubmitted,
                totalDraft,
                totalReview,
                totalReleased
            });
        }
        [HttpGet("area-summary")]
        public async Task<IActionResult> GetAreaDrillSummary([FromQuery] TrendFilterDto dto)
        {
            if (dto.UnitId == 0)
                return BadRequest("UnitId is required");

            var query = _context.FireDrills
                .Where(fd => fd.IsActive )
                  .WhereIf(dto.UnitId != 3, fd => fd.UnitId == dto.UnitId);

            var allData = await query.ToListAsync();
            
            var filtered = allData
                .Where(x =>
                    (!dto.FromDate.HasValue || x.SubmittedOn.Date >= dto.FromDate.Value) &&
                    (!dto.ToDate.HasValue || x.SubmittedOn.Date <= dto.ToDate.Value)
                )
                .ToList();

            var result = filtered
          .GroupBy(fd => fd.Facility1Id)
          .Select(g => new
          {
              AreaId = g.Key,
              AreaName = _context.FacilityMasters
                  .Where(f => f.Id == g.Key)
                  .Select(f => f.Name)
                  .FirstOrDefault(),
              Count = g.Count()
          })
          .ToList();

            return Ok(result);
        }        

        [HttpGet("section-summary")]
        public async Task<IActionResult> GetSectionDrillSummary([FromQuery] SectionFilterDto dto)
        {
            if (dto.areaId == 0)
                return BadRequest("AreaId is required");

            var query = _context.FireDrills
                .Where(fd => fd.IsActive  && fd.Facility1Id == dto.areaId)
                  .WhereIf(dto.UnitId != 3, fd => fd.UnitId == dto.UnitId);
            var allData = await query.ToListAsync();

            var filtered = allData
                .Where(x =>
                    (!dto.FromDate.HasValue || x.SubmittedOn.Date >= dto.FromDate.Value) &&
                    (!dto.ToDate.HasValue || x.SubmittedOn.Date <= dto.ToDate.Value)
                )
                .ToList();

            var result = filtered
                .GroupBy(fd => fd.Facility2Id)
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    SectionId = g.Key,
                    SectionName = _context.FacilityMasters
                        .Where(f => f.Id == g.Key)
                        .Select(f => f.Name)
                        .FirstOrDefault(),
                    Count = g.Count()
                })
                .ToList();

            return Ok(result);

            
        }
       
        [HttpGet("getDrillsByScenario")]
        public async Task<IActionResult> getDrillsByScenario([FromQuery] TrendFilterDto dto)
        {
            if (dto.UnitId == 0)
                return BadRequest("UnitId is required");

            var query = _context.FireDrills
                .Where(fd => fd.IsActive )
                 .WhereIf(dto.UnitId != 3, fd => fd.UnitId == dto.UnitId);
            var allData = await query.ToListAsync();

            // Now filter in-memory
            var filtered = allData
                .Where(x =>
                    (!dto.FromDate.HasValue || x.SubmittedOn.Date >= dto.FromDate.Value) &&
                    (!dto.ToDate.HasValue || x.SubmittedOn.Date <= dto.ToDate.Value)
                )
                .ToList();

            var result = filtered
                .GroupBy(fd => fd.ScenarioId)
                .Select(g => new {
                    Id = g.Key,
                    ScenarioName = _context.ScenarioMasters
                                    .Where(f => f.Id == g.Key)
                                    .Select(f => f.ScenarioName)
                                    .FirstOrDefault(),
                    Count = g.Count()
                }).ToList();

            return Ok(result);
        }

        [HttpGet("firedrill-dashboard")]
        public async Task<IActionResult> GetFiredrillDashboard([FromQuery] PendingTaskFilterDto dto)
        {
            var data = await _MastersService.GetFiredrillDashboardAsync(dto);
            return Ok(data);
        }
        [HttpGet("GetFireHeatmap")]
        public async Task<IActionResult> GetFireHeatmap(int unitId, DateTime? fromDate, DateTime? toDate)
        {
            //  Fetch Areas/Sections
            var areas = await _context.FacilityMasters
     .Where(f => f.ParentId == null && f.IsActive )
      .WhereIf(unitId != 3, fd => fd.UnitId == unitId)
     .Select(f => new { f.Id, f.Name })
     .OrderBy(f => f.Id)
     .ToListAsync();

            var sections = await _context.FacilityMasters
                .Where(f => f.ParentId != null && f.IsActive )
                 .WhereIf(unitId != 3, fd => fd.UnitId == unitId)
                .Select(f => new { f.Id, f.Name, f.ParentId })
                .OrderBy(f => f.Id)
                .ToListAsync();

            // Get Incidents
            var incidents = await (from fi in _context.IncidentDetails
                                   join area in _context.FacilityMasters on fi.Facility1Id equals area.Id
                                   join section in _context.FacilityMasters on fi.Facility2Id equals section.Id into sec
                                   from section in sec.DefaultIfEmpty() // Left Join in case section is null
                                   where (unitId ==3 || fi.UnitId == unitId) && fi.IncidentType==2
                                             && (!fromDate.HasValue || fi.SubmittedOn >= DateTime.SpecifyKind(fromDate.Value, DateTimeKind.Utc))
                                               && (!toDate.HasValue || fi.SubmittedOn <= DateTime.SpecifyKind(toDate.Value.AddDays(1), DateTimeKind.Utc))
                                  
                                   select new
                                   {
                                       AreaName = area.Name,
                                       SectionName = section != null ? section.Name : null
                                   }).ToListAsync();


            // ✅ Get Fire Drills with Area & Section Names
            var drills = await (from fd in _context.FireDrills
                                join area in _context.FacilityMasters on fd.Facility1Id equals area.Id
                                join section in _context.FacilityMasters on fd.Facility2Id equals section.Id into sec
                                from section in sec.DefaultIfEmpty()
                                where (unitId ==3 || fd.UnitId == unitId)
                                 && (!fromDate.HasValue || fd.SubmittedOn >= DateTime.SpecifyKind(fromDate.Value, DateTimeKind.Utc))
                                               && (!toDate.HasValue || fd.SubmittedOn <= DateTime.SpecifyKind(toDate.Value.AddDays(1), DateTimeKind.Utc))
                                select new
                                {
                                    AreaName = area.Name,
                                    SectionName = section != null ? section.Name : null
                                }).ToListAsync();

            var heatmapData = new List<FireHeatmapDto>();
            foreach (var area in areas)
            {
                var areaSections = sections.Where(s => s.ParentId == area.Id);

                foreach (var section in areaSections)
                {
                    // 🔴 Add all incidents for this section
                    if (incidents.Any(i => i.AreaName == area.Name && i.SectionName == section.Name))
                    {
                        heatmapData.Add(new FireHeatmapDto
                        {
                            Area = area.Name,
                            Section = section.Name,
                            Status = "incident"
                        });
                    }

                    // 🟡 Add all drills for this section
                    if (drills.Any(d => d.AreaName == area.Name && d.SectionName == section.Name))
                    {
                        heatmapData.Add(new FireHeatmapDto
                        {
                            Area = area.Name,
                            Section = section.Name,
                            Status = "drill"
                        });
                    }

                    // ⚪ If no incident and no drill, add "none"
                    if (!incidents.Any(i => i.AreaName == area.Name && i.SectionName == section.Name) &&
                        !drills.Any(d => d.AreaName == area.Name && d.SectionName == section.Name))
                    {
                        heatmapData.Add(new FireHeatmapDto
                        {
                            Area = area.Name,
                            Section = section.Name,
                            Status = "none"
                        });
                    }
                }
            }
            //foreach (var area in areas)
            //{
            //    var areaSections = sections.Where(s => s.ParentId == area.Id);

            //    foreach (var section in areaSections)
            //    {
            //        string status = "none";

            //        if (incidents.Any(i => i.AreaName == area.Name && i.SectionName == section.Name))
            //            status = "incident";
            //        else if (drills.Any(d => d.AreaName == area.Name && d.SectionName == section.Name))
            //            status = "drill";

            //        heatmapData.Add(new FireHeatmapDto
            //        {
            //            Area = area.Name,
            //            Section = section.Name,
            //            Status = status
            //        });
            //    }
            //}

            return Ok(heatmapData);
        }

    }
}
