using EmergencyManagement.Models.DTOs.Master;

namespace EmergencyManagement.Models.DTOs.Common
{
    public class UserInfoDto
    {
        public string Username { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public List<string> Permissions { get; set; } = new();
        public List<MenuDto> Menu { get; set; } = new();
    }
}
