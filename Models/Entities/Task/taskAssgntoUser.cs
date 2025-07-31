using Microsoft.AspNetCore.Http.HttpResults;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;

namespace EmergencyManagement.Models.Entities.Task
{
    [Table("taskassgntouser", Schema = "public")]
    public class taskAssgntoUser
    {
        [Key]
        [Column("taskassgntouserid")]
        public int TaskAssignToUserId { get; set; }

        [Required]
        [Column("taskid")]
        public int TaskId { get; set; }

        [Required]
        [Column("empdeptid")]

        public int? EmpDeptId { get; set; }
        [Required]
        [Column("employeeid")]
        public int? EmployeeId { get; set; }

        [Column("remarks")]
        public string? Remarks { get; set; }
        [Required]
        [Column("usertaskstatusid")]
        public int? UserTaskStatusId { get; set; }
        [Required]
        [Column("createdby")]
        public int? CreatedBy { get; set; }
        [Required]
        [Column("createdon")]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        [Column("modifiedby")]
        public int? ModifiedBy { get; set; }

        [Column("modifiedon")]
        public DateTime? ModifiedOn { get; set; }

        [Required]
        [Column("isactive")]
        public bool IsActive { get; set; } = true;

        // Navigation property (optional, if you use relationships in EF)
        [ForeignKey("TaskId")]
        public virtual Tasks? Tasks { get; set; }

    }
}
