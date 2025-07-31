namespace EmergencyManagement.Models.DTOs.Master
{
    public class MenuDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Route { get; set; } = string.Empty;
        public List<MenuDto> Children { get; set; } = new();
    }
}
