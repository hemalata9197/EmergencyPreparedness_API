namespace EmergencyManagement.Models.DTOs.Master
{
    public class PagedResult<T>
    {
        public List<T> Data { get; set; }
        public int TotalCount { get; set; }
    }
}
