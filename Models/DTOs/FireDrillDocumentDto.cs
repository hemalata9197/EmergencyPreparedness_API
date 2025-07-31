namespace EmergencyManagement.Models.DTOs
{
    public class FireDrillDocumentDto
    {
        public int FireDrillId { get; set; }
        public string DocumentTitle { get; set; } = string.Empty;  // file name
        public string Base64Content { get; set; } = string.Empty;
        public int SubmittedBy { get; set; }
    }
}
