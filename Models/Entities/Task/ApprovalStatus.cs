using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EmergencyManagement.Models.Entities.Task
{
    [Table("approvalstatus")]
    public class ApprovalStatus
    {
        [Key]
        [Column("approvalstatusid")]
        public int ApprovalStatusId { get; set; }
        [Column("approvalstatus")]
        public string ApprovalStatusText { get; set; } = string.Empty;

        [Column("isactive")]
        public bool IsActive { get; set; } = true;
    }
}
