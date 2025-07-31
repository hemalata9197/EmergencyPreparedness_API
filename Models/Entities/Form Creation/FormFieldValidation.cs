using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmergencyManagement.Models.Entities
{
    [Table("form_field_validations")]
    public class FormFieldValidation
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("field_id")]
        public int FieldId { get; set; }
        [Column("rule")]
        public string Rule { get; set; } = string.Empty;
        [Column("value")]
        public string Value { get; set; } = string.Empty;
    }
}
