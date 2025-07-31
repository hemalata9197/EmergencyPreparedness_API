using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EmergencyManagement.Models.Entities
{
    [Table("firedrill", Schema = "public")]
    public class FireDrill
    {
        [Key]
        [Column("firedrillid")]
        public int FireDrillId { get; set; }

        [Required]
        [Column("refno")]
        [MaxLength(50)]
        public string RefNo { get; set; } = string.Empty;

        [Required]
        [Column("unitid")]
        public int UnitId { get; set; }

        [Required]
        [Column("firedrilldate")]
        public DateTime FireDrillDate { get; set; }

        [Required]
        [Column("time")]
        [MaxLength(100)]
        public string Time { get; set; } = string.Empty;

        [Required]
        [Column("facility1id")]
        public int Facility1Id { get; set; }

        [Required]
        [Column("facility2id")]
        public int Facility2Id { get; set; }

        [Required]
        [Column("scenarioid")]
        public int ScenarioId { get; set; }

        [Required]
        [Column("actiontaken")]
        [MaxLength(500)]
        public string ActionTaken { get; set; } = string.Empty;

        [Required]
        [Column("entrystatus")]
        [MaxLength(10)]
        public string EntryStatus { get; set; } = string.Empty;

        [Required]
        [Column("isactive")]
        public bool IsActive { get; set; } = true;

        [Required]
        [Column("submittedby")]
        public int SubmittedBy { get; set; }

        [Column("submittedon")]
        public DateTime SubmittedOn { get; set; }

        [Column("modifiedby")]
        public int? ModifiedBy { get; set; }

        [Column("modifiedon")]
        public DateTime? ModifiedOn { get; set; }

        [Column("isreview")]
        public bool? IsReview { get; set; }

        [Column("reviewon")]
        public DateTime? ReviewOn { get; set; }

        [Column("reviewby")]
        public int? ReviewBy { get; set; }

        [Column("reviewremark")]
        [MaxLength(500)]
        public string? ReviewRemark { get; set; }

        [Column("isreleased")]
        public bool? IsReleased { get; set; }

        [Column("releasedon")]
        public DateTime? ReleasedOn { get; set; }

        [Column("releasedby")]
        public int? ReleasedBy { get; set; }

        [Column("callreceivedatfirestation")]
        [MaxLength(100)]
        public string? CallReceivedAtFireStation { get; set; }

        [Column("turnoutoffiretender")]
        [MaxLength(100)]
        public string? TurnOutOfFireTender { get; set; }

        [Column("securityreachedatsite")]
        [MaxLength(100)]
        public string? SecurityReachedAtSite { get; set; }

        [Column("firetenderreturnedatfirestation")]
        [MaxLength(100)]
        public string? FireTenderReturnedAtFireStation { get; set; }

        // Navigation property
        public ICollection<Recommendation> Recommendation { get; set; } = new List<Recommendation>();
    }
}
