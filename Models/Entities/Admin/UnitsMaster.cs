using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmergencyManagement.Models.Entities.Admin
{
    [Table("unitsmaster")]
    public class UnitsMaster
    {
        [Key]
        [Column("unitid")]
        public int UnitId { get; set; }
        [Column("unitname")]
        public string UnitName { get; set; } = string.Empty;
        [Column("unithead")]
        public int UnitHead { get; set; }

        [Column("isactive")]
        public bool IsActive { get; set; } = true;
        [Column("createdby")]
        public int? CreatedBy { get; set; }
        [Column("createdon")]
        public DateTime CreatedOn { get; set; }
        [Column("modifiedby")]
        public int? ModifiedBy { get; set; }
        [Column("modifiedon")]
        public DateTime? ModifiedOn { get; set; }
    }
}
