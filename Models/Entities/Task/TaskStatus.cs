using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmergencyManagement.Models.Entities.Task
{
    [Table("taskstatus")]
    public class TaskStatus
    {
        [Key]
        [Column("taskstatusid")]
        public int taskStatusId { get; set; }
        [Column("taskstatus")]
        public string taskStatus { get; set; } = string.Empty;
        [Column("isactive")]
        public bool IsActive { get; set; } = true;
    }
}
