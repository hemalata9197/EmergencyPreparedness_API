using EmergencyManagement.Models.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmergencyManagement.Models.DTOs.Master
{
    public class getEmployeeMasterDto
    {
        public int Id { get; set; }
        public string Label { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;

        public int? RoleId { get; set; }
        public string Role { get; set; } = string.Empty;

        public int? areaId { get; set; }
        public string areaName { get; set; } = string.Empty;

        public int? desigId { get; set; }
        public string designationName { get; set; } = string.Empty;

        public string sectionName { get; set; } = string.Empty;

        public bool? isTaskAssigned { get; set; }   
        public bool isActive { get; set; }
        public bool isDeleted { get; set; }
    }
}
