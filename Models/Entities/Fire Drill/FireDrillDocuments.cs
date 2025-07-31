using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Metadata;

namespace EmergencyManagement.Models.Entities
{
    [Table("firedrilldocuments", Schema = "public")]
    public class FireDrillDocuments
    {
        [Key]
        [Column("documentid")]
        public int DocumentId { get; set; }

        [Required]
        [ForeignKey("FireDrill")]
        [Column("firedrillid")]
        public int FireDrillId { get; set; }

        [Column("documenttitle")]
        [Required]
        public string DocumentTitle { get; set; } = string.Empty;

        [Column("documentpath")]
        [Required]
        public string DocumentPath { get; set; } = string.Empty;

        [Column("isactive")]
        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        [Column("submittedby")]
        public int SubmittedBy { get; set; }

        [Column("submittedon")]
        public DateTime SubmittedOn { get; set; } = DateTime.UtcNow;

        [Column("modifiedby")]
        public int? ModifiedBy { get; set; }

        [Column("modifiedon")]
        public DateTime? ModifiedOn { get; set; }
        public virtual FireDrill? FireDrill { get; set; }

    }
}
