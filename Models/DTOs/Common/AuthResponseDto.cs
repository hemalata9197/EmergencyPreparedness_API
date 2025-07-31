namespace EmergencyManagement.Models.DTOs.Common
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new List<string>();
        public string? EmployeeName { get; set; }
        public bool IsFirstLogin { get; set; }  
    }
}
