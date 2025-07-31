namespace EmergencyManagement.Models.DTOs.Master
{
    public class getFacilityMasterDto
    {
        public int Id { get; set; }
        public string Label { get; set; } = string.Empty;       
        public int? ParentId { get; set; } = 0;
        public string ParentName { get; set; } = string.Empty;
        public int FacilityHeadId { get; set; } 
        public string FacilityHeadName { get; set; }= string.Empty;
        public bool isActive { get; set; }
        public bool? isTaskAssigned { get; set; }
    }
}
