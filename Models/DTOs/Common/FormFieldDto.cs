using EmergencyManagement.Models.DTOs.Common;

namespace EmergencyManagement.Models.DTOs
{
    public class FormFieldDto
    {
        public string Label { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? Placeholder { get; set; }
        public bool IsRepeatable { get; set; }
        public string? DateConstraint { get; set; }
        public bool isDisabledOnReview { get; set; }
        public bool isDisabledOnRelease { get; set; }
        public List<ValidationDto> Validations { get; set; } = new();
        public List<DropdownOptionDto>? Options { get; set; }
    }
}
