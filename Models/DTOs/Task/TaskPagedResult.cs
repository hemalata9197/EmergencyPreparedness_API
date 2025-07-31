namespace EmergencyManagement.Models.DTOs.Task
{
    public class TaskPagedResult
    {
        public List<getTaskDto> Data { get; set; } = new();
        public int TotalCount { get; set; }
    }
}
