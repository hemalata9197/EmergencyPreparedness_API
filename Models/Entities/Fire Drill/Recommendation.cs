using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EmergencyManagement.Models.Entities.Task;

namespace EmergencyManagement.Models.Entities
{
    [Table("recommendation", Schema = "public")]
    public class Recommendation
    {
        [Key]
        [Column("recommendationid")]
        public int RecommendationId { get; set; }

        [Required]
        [ForeignKey("FireDrill")]
        [Column("firedrillid")]
        public int FireDrillId { get; set; }

        [Column("recommendationtext")]
        [Required]
        public string RecommendationText { get; set; } = string.Empty;

        [Column("responsibleuserid")]
        [Required]
        public int ResponsibleUserId { get; set; }

        [Column("targetdate")]
        [Required]
        public DateTime TargetDate { get; set; }

        [Column("actionstatusid")]
        [Required]
        public int ActionStatusId { get; set; }

        [Column("isactive")]
        [Required]
        public bool IsActive { get; set; } = true;

        [Column("createdby")]
        [Required]
        public int CreatedBy { get; set; }

        [Column("createdon")]
        [Required]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        [Column("modifiedby")]
        public int? ModifiedBy { get; set; }

        [Column("modifiedon")]
        public DateTime? ModifiedOn { get; set; }

        [Column("closedby")]
        public int? ClosedBy { get; set; }

        [Column("closedon")]
        public DateTime? ClosedOn { get; set; }

        [Column("closedremark")]
        [MaxLength(100)]
        public string? ClosedRemark { get; set; }
        [Column("severityid")]
        [Required]
        public int SeverityId { get; set; }

        // Navigation property
        public virtual FireDrill? FireDrill { get; set; }
        public ICollection<Tasks> Tasks { get; set; } = new List<Tasks>();

    }
}
