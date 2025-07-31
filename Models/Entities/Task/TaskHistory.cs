using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmergencyManagement.Models.Entities.Task
{
    [Table("taskhistory")]
    public class TaskHistory
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("taskid")]
        [Required]
        public int TaskId { get; set; }
        [Column("taskstatusid")]
        [Required]
        public int taskStatusId { get; set; }
        [Column("remark")]
        public string? Remarks { get; set; }

        [Column("targetdate")]
        [Required]
        public DateTime? TargetDate { get; set; }

        [Column("isactive")]
        public bool IsActive { get; set; } = true;

        [Required]
        [Column("createdby")]
        public int CreatedBy { get; set; }

        [Required]
        [Column("createdon")]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    }
}

