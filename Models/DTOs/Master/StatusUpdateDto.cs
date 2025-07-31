namespace EmergencyManagement.Models.DTOs.Master
{
    public class StatusUpdateDto
    {
        public string source { get; set; }
        public int id { get; set; }
        public bool isActive { get; set; }
        public int ModifiedBy { get; set; } 
    }
}
