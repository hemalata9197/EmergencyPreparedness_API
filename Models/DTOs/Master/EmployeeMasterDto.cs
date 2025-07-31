namespace EmergencyManagement.Models.DTOs.Master
{
    public class EmployeeMasterDto
    {
        public int Id { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string? MobileNumber { get; set; }
        public string? Email { get; set; }
        public int areaId { get; set; }
        public int DesigId { get; set; }
        public int RoleId { get; set; }        
        public int CreatedBy { get; set; }
        public int ModifiedBy { get; set; }
        public int UnitId { get; set; }
      

      
    }
}
