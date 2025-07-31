namespace EmergencyManagement.Models.DTOs.Master
{
    public class PaginatedResult<T>
    {
        public List<T> Data { get; set; } = new();
        public int TotalRecords { get; set; }
    }
}
