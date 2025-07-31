using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmergencyManagement.Models.Entities.Email
{
    [Table("mailmessages")]
    public class MailMessages
    {
        [Key]
        [Column("messageid")]
        public int MessageId { get; set; }

        [Required]
        [Column("frommailid")]
        public string? FromMailId { get; set; }
        [Column("subject")]
        [Required]
        public string? Subject { get; set; }
        [Column("body")]
        [Required]
        public string? Body { get; set; }
        [Column("sendondate")]
        [Required]
        public DateTime SendOnDate { get; set; } = DateTime.UtcNow;
        
        [Column("sendstatus")]
        [Required]
        public string? SendStatus { get; set; }
        
        [Column("unitid")]
        [Required]
        public decimal? UnitId { get; set; }
        [Column("frommod")]
        public string? FromMod { get; set; }
        [Column("id")]
        public decimal? Id { get; set; }
        [Column("exceptionmessage")]
        public string? ExceptionMessage { get; set; }

        // 🔹 Navigation property (One-to-Many)
        public ICollection<MailMessagesToUser> MailMessagesToUsers { get; set; }
    }
}

