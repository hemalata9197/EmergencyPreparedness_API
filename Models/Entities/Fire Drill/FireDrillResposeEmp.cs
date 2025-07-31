using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EmergencyManagement.Models.Entities.Fire_Drill
{
    [Table("firedrillresposeemp")]
    public class FireDrillResposeEmp
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Required]
        [ForeignKey("FireDrill")]
        [Column("firedrillid")]
        public int FireDrillId { get; set; }
        [Required]
        [Column("employeeid")]
        public int EmployeeId { get; set; }

        public virtual FireDrill? FireDrill { get; set; }
    }
}
