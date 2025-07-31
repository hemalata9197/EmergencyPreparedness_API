namespace EmergencyManagement.Models.DTOs.Dashboard
{
    public class FireHeatmapDto
    {
        public string Area { get; set; }
        public string Section { get; set; }
        public string Status { get; set; }  // "incident", "drill", "none"
    }
}
