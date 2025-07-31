using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmergencyManagement.Models.Entities
{
    [Table("form_fields")]
    public class FormField
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("section_id")]
        public int SectionId { get; set; }
        public FormSection Section { get; set; } = null!;
        [Column("label")]
        public string Label { get; set; } = string.Empty;
        [Column("name")]
        public string Name { get; set; } = string.Empty;
        [Column("type")]
        public string Type { get; set; } = string.Empty;
        [Column("placeholder")]
        public string? Placeholder { get; set; }
        [Column("default_value")]
        public string? DefaultValue { get; set; }
        [Column("order_index")]
        public int OrderIndex { get; set; }
        [Column("dropdown_source")]
        public string? DropdownSource { get; set; }
        [Column("is_repeatable")]
        public bool IsRepeatable { get; set; } = false;
        [Column("dateconstraint")]
        public string? DateConstraint { get; set; }

        [Column("isactive")]
        public bool IsActive { get; set; } = true;

        [Column("isdisabledonreview")]
        public bool isDisabledOnReview { get; set; } = true;

        [Column("isdisabledonrelease")]
        public bool isDisabledOnRelease { get; set; } = true;
        public ICollection<FormFieldValidation> Validations { get; set; } = new List<FormFieldValidation>();
    }
}
