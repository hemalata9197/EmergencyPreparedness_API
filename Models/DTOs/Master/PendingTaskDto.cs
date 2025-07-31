namespace EmergencyManagement.Models.DTOs.Master
{
    public class PendingTaskDto
    {
        public string RefNo { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;

        public int FireDrillId { get; set; }
        public int taskId { get; set; }
        public int taskCreatedForId { get; set; }
        public string TaskDetails { get; set; } = string.Empty;
        public int ResponsiblePersonId { get; set; }
        public DateTime? TargetDate { get; set; }
        public string Remarks { get; set; } = string.Empty;
        public int? TaskStatusId { get; set; }
        public string ClosedRemark { get; set; } = string.Empty;
        public int? ApprovalStatusId { get; set; }

    }
}
