using EmergencyManagement.Models.Entities.Task;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmergencyManagement.Models.DTOs.Task
{
    public class DrillTasksDto
    {
        public int taskCreatedForId { get; set; }
        public string taskDetails { get; set; } = string.Empty;
        public int severityId { get; set; }
        public DateTime TargetDate { get; set; }
        public int taskStatusId { get; set; }
        public int taskModuleId { get; set; }
        public string? forsubmodule { get; set; }
        public int CreatedBy { get; set; }

        public List<taskAssgntoUser> taskAssgntoUsers { get; set; } = new();

    }
}
