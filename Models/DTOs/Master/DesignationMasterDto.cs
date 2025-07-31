namespace EmergencyManagement.Models.DTOs.Master
{
    public class DesignationMasterDto
    {
        public int Id { get; set; }
        public string Label { get; set; } = string.Empty;
        public int CreatedBy { get; set; }
        public int ModifiedBy { get; set; }
        public int UnitId { get; set; }
    }
}
