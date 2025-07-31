namespace EmergencyManagement.Models.DTOs.Reports
{
    public class ReportFilterDto
    {
        public string ReportType { get; set; } = string.Empty;
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? Facility1Id { get; set; }
        public int? Facility2Id { get; set; }
        public int? ScenarioId { get; set; }
        public int? UnitId { get; set; }
        public string? RefNo { get; set; }
    }
}
