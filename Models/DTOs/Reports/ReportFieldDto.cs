namespace EmergencyManagement.Models.DTOs.Reports
{
    public class ReportFieldDto
    {
        public string Name { get; set; }
        public string Label { get; set; }
        public string Type { get; set; } = "text"; // text, dropdown, date etc.
       // public bool Filterable { get; set; } = false;
        //public List<DropdownOptionDto>? Options { get; set; } = null;
    }
}
