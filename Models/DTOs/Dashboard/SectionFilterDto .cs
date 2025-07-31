namespace EmergencyManagement.Models.DTOs.Dashboard
{
    public class SectionFilterDto
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int UnitId { get; set; }
        public int areaId { get; set; }
    }
}
