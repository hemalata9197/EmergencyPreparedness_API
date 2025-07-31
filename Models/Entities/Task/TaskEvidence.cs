using Microsoft.AspNetCore.Http.HttpResults;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;
using System.Threading.Tasks;

namespace EmergencyManagement.Models.Entities.Task
{
    [Table("taskevidence")]
    public class TaskEvidence
    {
        [Key]
        [Column("documentid")]
        public int DocumentId { get; set; }

        [Required]
        [Column("taskid")]
        public int Taskid { get; set; }

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
    }

    
}
