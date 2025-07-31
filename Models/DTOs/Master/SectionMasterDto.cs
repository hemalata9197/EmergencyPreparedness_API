namespace EmergencyManagement.Models.DTOs.Master
{
    public class SectionMasterDto
    {
        public int Id { get; set; }
        public string Label { get; set; } = string.Empty;
        public int FacilityHeadId { get; set; }
        public int ParentId { get; set; }   
        public int CreatedBy { get; set; }
        public int ModifiedBy { get; set; }
        public int UnitId { get; set; }
    }
}
