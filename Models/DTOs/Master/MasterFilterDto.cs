namespace EmergencyManagement.Models.DTOs.Master
{
    public class MasterFilterDto
    {
        public string Source { get; set; } = string.Empty;
        public int UnitId { get; set; }
        public int Page { get; set; } = 1;
        public int Size { get; set; } = 10;
        //public string SearchText { get; set; } = string.Empty;
       // public bool? IsActive { get; set; }
    }
}
