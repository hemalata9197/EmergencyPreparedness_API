using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EmergencyManagement.Models.Entities
{
    [Table("scenariomaster")]
    public class ScenarioMaster
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("scenarioname")]
        public string ScenarioName { get; set; } = string.Empty;
        [Column("description")]
        public string Description { get; set; } = string.Empty;

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
        [Column("unitid")]
        public int? UnitId { get; set; }

    }
}
