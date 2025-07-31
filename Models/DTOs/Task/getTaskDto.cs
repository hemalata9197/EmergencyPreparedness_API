namespace EmergencyManagement.Models.DTOs
{
    public class getTaskDto
    {
        public int FireDrillId { get; set; }
        public int taskId { get; set; }
        public int taskCreatedForId { get; set; }
        public string RefNo { get; set; } = string.Empty;
        public string TaskDetails { get; set; } = string.Empty;
        public int ResponsiblePersonId { get; set; }   
        public string ResponsiblePerson { get; set; } = string.Empty;
        public int? UserAreaHead { get; set; }
        public DateTime? TargetDate { get; set; } 
        public DateTime? ExtendedTargetDate { get; set; }   
        public string TaskStatus { get; set; } = string.Empty;
        public string Remarks { get; set; }= string.Empty;
        public int? TaskStatusId { get; set; }
        public string ClosedRemark { get; set; } = string.Empty;
        public int? ApprovalStatusId { get; set; }
        public string? ApprovalStatus { get; set; }
        public int? RespUserHOD { get; set; }

        public string? FileName { get; set; } = string.Empty;
        public string? DocumentPath { get; set; } = string.Empty;


    }
}
