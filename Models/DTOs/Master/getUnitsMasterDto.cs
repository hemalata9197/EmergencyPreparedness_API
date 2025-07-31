namespace EmergencyManagement.Models.DTOs.Master
{
    public class getUnitsMasterDto
    {
        public int Id { get; set; }
        public string Label { get; set; } = string.Empty;
        public int UnitHeadId { get; set; }
        public string UnitHeadName { get; set; } = string.Empty;
        public bool isActive { get; set; }
        public bool? isTaskAssigned { get; set; }
    }
}
