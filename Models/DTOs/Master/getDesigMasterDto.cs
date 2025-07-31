namespace EmergencyManagement.Models.DTOs.Master
{
    public class getDesigMasterDto
    {
        public int Id { get; set; }
        public string Label { get; set; } = string.Empty;
        public bool isActive { get; set; }
        public bool? isTaskAssigned { get; set; }
    }
}
