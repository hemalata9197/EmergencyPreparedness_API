using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EmergencyManagement.Models.Entities.Admin
{
    [Table("generalconfigelements")]
    public class GeneralConfigElements
    {
        [Key]
        [Column("connfigelementid")]
        public int ConnfigElementId { get; set; }

        [Required]
        [Column("elementdesc")]
        public string ElementDesc { get; set; } = string.Empty;

        [Required]
        [Column("moduleId")]
        public int ModuleId { get; set; }

        [Required]
        [Column("menuoptionid")]
        public int MenuOptionId { get; set; }



    }
}
