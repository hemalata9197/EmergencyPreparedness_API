namespace EmergencyManagement.Models.DTOs
{
    public class DropdownOptionDto
    {
        public int Id { get; set; }
        public string Label { get; set; } = string.Empty;
        public int? ParentId { get; set; }= 0;
    }
}
