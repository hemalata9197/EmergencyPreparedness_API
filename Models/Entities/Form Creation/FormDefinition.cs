using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmergencyManagement.Models.Entities
{
    [Table("formdefinitions")]
    public class FormDefinition
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("formname")]
        public string FormName { get; set; } = string.Empty;
        [Column("description")]
        public string Description { get; set; } = string.Empty;
        [Column("createdon")]
        public DateTime CreatedOn { get; set; }
        [Column("createdby")]
        public int Createdby { get; set; }
        [Column("moduleid")]
        public int Moduleid { get; set; }
        [Column("isactive")]
        public bool IsActive { get; set; } = true;
        public ICollection<FormSection> Sections { get; set; } = new List<FormSection>();
    }
}
