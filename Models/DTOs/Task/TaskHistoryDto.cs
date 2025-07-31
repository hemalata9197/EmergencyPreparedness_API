namespace EmergencyManagement.Models.DTOs.Task
{
    public class TaskHistoryDto
    {
        public string TaskStatus { get; set; } = string.Empty;
        public DateTime? TargetDate { get; set; }
        public string Remarks { get; set; } = string.Empty;
    }
}
