using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmergencyManagement.Models.Entities.Admin
{
    [Table("userrole")]
    public class UserRole
    {
        [Key]
        [Column("userid")]
        public int UserId { get; set; }
        public Users User { get; set; } = default!;
        [Column("roleid")]
        public int RoleId { get; set; }
        public Role Role { get; set; } = default!;
    }
}
