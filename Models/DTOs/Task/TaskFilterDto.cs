namespace EmergencyManagement.Models.DTOs.Task
{
    public class TaskFilterDto
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? AreaId { get; set; }
        public int? SectionId { get; set; }
        public int? taskStatusId { get; set; }
        public int? approvalStatusId { get; set; }

        public string? RefNo { get; set; }
        //public int? ScenarioId { get; set; }
        public int? UnitId { get; set; }
        public int loginUserId { get; set; }
        public string role { get; set; }
        public int? ResponsiblePersonId { get; set; }
    }
}
