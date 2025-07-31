using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmergencyManagement.Models.Entities.Admin
{
    [Table("users")]
    public class Users
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("username")]
        public string Username { get; set; } = string.Empty;
        [Column("passwordhash")]
        public string PasswordHash { get; set; } = string.Empty;
        [Column("roleid")]
        public int RoleId { get; set; }

        [Column("isactive")]
        public bool IsActive { get; set; } = true;

        [Column("isfirstlogin")]
        public bool IsFirstLogin { get; set; } = true;
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public virtual Employees? Employee { get; set; }
    }
}
