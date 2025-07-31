using EmergencyManagement.Models.DTOs.Master;
using EmergencyManagement.Models.DTOs.Reports;

namespace EmergencyManagement.Services.Interfaces
{
    public interface IReportsService
    {
        Task<List<ReportFieldDto>> GetReportFieldsAsync(string reportType, int unitId);

        Task<PagedResult<Dictionary<string, object>>> GetFireDrillSummaryAsync(ReportFilterDto filter);
        Task<PagedResult<Dictionary<string, object>>> GetRecSummaryAsync(ReportFilterDto filter);
        Task<PagedResult<Dictionary<string, object>>> GetAreaWiseComplianceSummaryAsync(ReportFilterDto filter);
    }
}
