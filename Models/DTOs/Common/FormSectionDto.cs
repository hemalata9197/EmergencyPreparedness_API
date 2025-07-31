namespace EmergencyManagement.Models.DTOs.Common
{
    public class FormSectionDto
    {
        public int Id { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public int SectionOrder { get; set; }
        public bool IsRepeatable { get; set; }
        public List<FormFieldDto> Fields { get; set; } = new();
    }
}
