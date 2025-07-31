using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EmergencyManagement.Models.Entities.Admin
{
    [Table("generalconfigelementvalues")]
    public class GeneralConfigElementValues
    {
        [Key]
        [Column("configelementvalueid")]
        public int ConfigElementValueId { get; set; }
        [Required]
        [Column("connfigelementid")]
        public int ConnfigElementId { get; set; }
        [Required]
        [Column("valueid")]
        public int ValueId { get; set; }
        [Required]
        [Column("unitid")]
        public int UnitId { get; set; }
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
    }
}
