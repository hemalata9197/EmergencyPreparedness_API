using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmergencyManagement.Models.Entities
{
    [Table("form_sections")]
    public class FormSection
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("form_id")]
        public int FormId { get; set; }
        [Column("section_name")]
        public string SectionName { get; set; } = string.Empty;
        [Column("section_order")]
        public int SectionOrder { get; set; }
        [Column("is_repeatable")]
        public bool IsRepeatable { get; set; } = false;
        [Column("isactive")]
        public bool IsActive { get; set; } = true;
        public ICollection<FormField> Fields { get; set; } = new List<FormField>();
    }
}
