using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EmergencyManagement.Models.Entities
{
    [Table("designation")]
    public class Designation
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("name")]
        public string Name { get; set; } = string.Empty;       
       
        [Column("createdby")]
        public int? CreatedBy { get; set; }
        [Column("createdon")]
        public DateTime CreatedOn { get; set; }
        [Column("modifiedby")]
        public int? ModifiedBy { get; set; }
        [Column("modifiedon")]
        public DateTime? ModifiedOn { get; set; }
        [Column("unitid")]
        public int? UnitId { get; set; }
        [Column("isactive")]
        public bool IsActive { get; set; } = true;
    }
}
