using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmergencyManagement.Models.Entities.Incident
{
    [Table("incidentdetails")]
    public class IncidentDetails
    {
        [Key]
        [Column("incidentid")]
        public int IncidentId { get; set; }
        [Required]
        [Column("facility1id")]
        public int Facility1Id { get; set; }

        [Required]
        [Column("facility2id")]
        public int Facility2Id { get; set; }
        [Required]
        [Column("entrystatus")]
        [MaxLength(10)]
        public string EntryStatus { get; set; } = string.Empty;

        [Required]
        [Column("isactive")]
        public bool IsActive { get; set; } = true;

        [Required]
        [Column("unitid")]
        public int UnitId { get; set; }
        [Required]
        [Column("incidentdate")]
        public DateTime IncidentDate { get; set; }
        [Column("submittedon")]
        public DateTime SubmittedOn { get; set; }

        [Column("incidenttype")]
        public int IncidentType { get; set; }
    }
}
