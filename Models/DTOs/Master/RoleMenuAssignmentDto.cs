namespace EmergencyManagement.Models.DTOs.Master
{
    public class RoleMenuAssignmentDto
    {
        public int RoleId { get; set; }
        public List<int> MenuIds { get; set; } = new();
        public int SubmittedBy { get; set; }
        public int UnitId { get; set; }
    }
}
