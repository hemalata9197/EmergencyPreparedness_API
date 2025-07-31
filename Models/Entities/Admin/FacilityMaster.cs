using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmergencyManagement.Models.Entities.Admin
{
    [Table("facilitymaster")]
    public class FacilityMaster
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("name")]
        public string Name { get; set; } = string.Empty;
        [Column("parentid")]
        public int? ParentId { get; set; }
        public FacilityMaster? Parent { get; set; }

        [Column("facilityhead")]
        public int? FacilityHead { get; set; }
        public ICollection<FacilityMaster>? Children { get; set; }
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
        [Column("deletedby")]
        public int? DeletedBy { get; set; }
        [Column("deletedon")]
        public DateTime? DeletedOn { get; set; }
        [Column("isdeleted")]
        public bool IsDeleted { get; set; } = false;
        [Column("unitid")]
        public int? UnitId { get; set; }
    }
}
