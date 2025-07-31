using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EmergencyManagement.Models.Entities.Admin
{
    [Table("rolemenu")]
    public class RoleMenu

    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Column("roleid")]
        public int RoleId { get; set; }

        [ForeignKey("RoleId")]
        public Role Role { get; set; } = default!;

        [Column("menuid")]
        public int MenuId { get; set; }

        [ForeignKey("MenuId")]
        public Menu Menu { get; set; } = default!;

        [Column("submittedby")]
        public int SubmittedBy { get; set; }

        [Column("submittedon")]
        public DateTime SubmittedOn { get; set; }

        [Column("isactive")]
        public bool IsActive { get; set; } = true;

        [Column("unitid")]
        public int UnitId { get; set; }
    }
}

