using System.ComponentModel.DataAnnotations.Schema;

namespace EmergencyManagement.Models.DTOs.Task
{
    public class taskAssgntoUserDto
    {
        public int taskId { get; set; }
        public int EmpdeptId { get; set; }
        public int EmployeeId { get; set; }
        public int userTaskStatusId { get; set; }
        public int CreatedBy { get; set; }
    }
}
