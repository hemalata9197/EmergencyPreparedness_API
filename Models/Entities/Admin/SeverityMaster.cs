using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmergencyManagement.Models.Entities.Admin
{
    [Table("severitymaster")]
    public class SeverityMaster
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("level")]
        public string Level { get; set; } = string.Empty;
        [Column("isactive")]
        public bool IsActive { get; set; } = true;
    }
}
