namespace EmergencyManagement.Models.DTOs
{
    public class TaskApprovalDto
    {
        public int fireDrillId { get; set; }
        public int taskId { get; set; }
        public int taskCreatedForId { get; set; }
        public int approvalStatusId { get; set; }
        public string closedRemark { get; set; } = string.Empty;
        public int closedby { get; set; }
    }
}
