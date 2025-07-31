using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmergencyManagement.Models.Entities.Admin
{
    [Table("role")]
    public class Role
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

        [Column("isactive")]
        public bool IsActive { get; set; } = true;
        [Column("unitid")]
        public int? UnitId { get; set; }
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        // public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
        public ICollection<RoleMenu> RoleMenus { get; set; } = new List<RoleMenu>();
    }
}
