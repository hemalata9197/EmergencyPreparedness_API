namespace EmergencyManagement.Models.DTOs.Dashboard
{
    public class TrendFilterDto
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int UnitId { get; set; }
    }
}
