namespace EmergencyManagement.Models.DTOs
{
    public class FireDrillPagedResult
    {
        public List<getFireDrillDto> Data { get; set; } = new();
        public int TotalCount { get; set; }
    }
}
