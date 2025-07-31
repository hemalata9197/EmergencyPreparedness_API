using System.Text.Json;

namespace EmergencyManagement.Models.DTOs
{
    public class getFireDrillDto
    {
        public int FireDrillId { get; set; }
        public string? RefNo { get; set; }
        public string? EntryStatus { get; set; }
        public int? SubmittedBy { get; set; }
        public int UnitId { get; set; }
        public DateTime SubmittedOn { get; set; }
        public string? FormData { get; set; }
        public string? AreaName { get; set; }
        public string? SectionName { get; set; }
        public string? ScenarioName { get; set; }
        public string? Time { get; set; }
        public string? FireDrillDate { get; set; } = null;
        public bool IsReview { get; set; }
        public bool IsReleased { get; set; }
        public int? AreaHOD { get; set; }
        public string? ValueIds { get; set; }
        public string? ReviewRemark { get; set;}
        public int? ReviewBy { get; set; }
        public int? ReleasedBy { get; set; }
        public string? Status { get; set; }
        public List<string> AttachmentPaths { get; set; } = new List<string>();
    }
}
