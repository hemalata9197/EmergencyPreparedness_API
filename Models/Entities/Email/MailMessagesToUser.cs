using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmergencyManagement.Models.Entities.Email
{
    [Table("mail_messages_to_user")]
    public class MailMessagesToUser
    {
        [Key]
        [Column("mailmessagestousermailid")]
        public int MailMessagesToUserMailId { get; set; } 
        [Required]
        [ForeignKey("messageid")]
        [Column("messageid")]
        public int MessageId { get; set; }
        [Required]
        [Column("mailid")]
        public string? MailId { get; set; }
        [Required]
        [Column("mailidstatus")]
        public string? MailIdStatus { get; set; }   
        [Column("exceptionmessage")]
        public string? ExceptionMessage { get; set; }
        [Required]
        [Column("confcat")]
        public string? ConfCat { get; set; }

        // 🔹 Navigation property (Many-to-One)
        public MailMessages? MailMessages { get; set; }
    }
}
