namespace EmergencyManagement.Models.DTOs
{
    public class FireDrillFilterDto
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? Facility1Id { get; set; }
        public int? Facility2Id { get; set; }
        public int? ScenarioId { get; set; }
        public int UnitId { get; set; }
        public string? RefNo { get; set; }
        public int loginUserId { get; set; }
        public string role { get; set; }
    }
}
