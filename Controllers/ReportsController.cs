using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EmergencyManagement.Services.Interfaces;
using EmergencyManagement.Services.Implementations;
using EmergencyManagement.Models.DTOs.Reports;
using Microsoft.AspNetCore.Cors;

namespace EmergencyManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowAngularApp")]
    public class ReportsController : ControllerBase
    {
        private readonly  IReportsService _reportsService;  

        public ReportsController(IReportsService reportsService)
        {
            _reportsService = reportsService;
        }
        [HttpGet("GetReportFields")]
        public async Task<IActionResult> GetReportFields([FromQuery] string reportType, [FromQuery] int unitId)
        {
            var fields = await _reportsService.GetReportFieldsAsync(reportType, unitId);
            return Ok(new { fields });
        }

        [HttpGet("GetReportData")]
        public async Task<IActionResult> GetReportData([FromQuery] ReportFilterDto filter)
        {
            return filter.ReportType switch
            {
                "FireDrillSummary" => Ok(await _reportsService.GetFireDrillSummaryAsync(filter)),
                "RecommandationSummary" => Ok(await _reportsService.GetRecSummaryAsync(filter)),
                "ComplianceReport" => Ok(await _reportsService.GetAreaWiseComplianceSummaryAsync(filter)),
                _ => BadRequest("Invalid report type")
            };
        }
    }
}
