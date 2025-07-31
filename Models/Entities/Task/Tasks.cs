using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmergencyManagement.Models.Entities.Task
{
    [Table("tasks", Schema = "public")]
    public class Tasks
    {
        [Key]
        [Column("taskid")]
        public int TaskId { get; set; }

        [Required]
        [Column("taskcreatedforid")]
        public int TaskCreatedForId { get; set; }  // FK to recommendation.recommendationid

        [Column("taskdetails")]
        [Required]
        public string? TaskDetails { get; set; }

        [Column("targetdate")]
        [Required]
        public DateTime? TargetDate { get; set; }

        [Column("taskstatusid")]
        [Required]
        public int? TaskStatusId { get; set; }

        [Column("remarks")]
        public string? Remarks { get; set; }

        [Column("closedby")]
        public int? ClosedBy { get; set; }

        [Column("closedon")]
        public DateTime? ClosedOn { get; set; }

        [Column("taskmoduleid")]
        public int? TaskModuleId { get; set; }

        [Column("forsubmodule")]
        public string? ForSubModule { get; set; }

        [Required]
        [Column("createdby")]
        public int CreatedBy { get; set; }

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

        [Column("targetdatechangecount")]
        public int TargetDateChangeCount { get; set; }

        // Navigation Property (Optional)
        [ForeignKey("TaskCreatedForId")]
        public Recommendation? Recommendation { get; set; }
        public ICollection<taskAssgntoUser> taskAssgntoUsers { get; set; } = new List<taskAssgntoUser>();
    }
}
