namespace EmergencyManagement.Models.DTOs.Master
{
    public class UnitsMasterDeto
    {
        public int Id { get; set; }
        public string Label { get; set; } = string.Empty;
        public int UnitHeadId { get; set; }
        public int CreatedBy { get; set; }
        public int ModifiedBy { get; set; }
    }
}
