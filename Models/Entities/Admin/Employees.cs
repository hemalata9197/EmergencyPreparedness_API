using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EmergencyManagement.Models.Entities.Admin
{
    [Table("employees")]
    public class Employees
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("employeecode")]
        public string EmployeeCode { get; set; } = string.Empty;

        [Column("userid")]
        public int? UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual Users? User { get; set; }

        [Column("name")]
        public string FullName { get; set; } = string.Empty;
        [Column("deptid")]
        public int? DeptId { get; set; }
        [Column("designation")]
        public int? Designation { get; set; }
        [Column("email")]
        public string? Email { get; set; }
        [Column("mobilenumber")]
        public string? MobileNumber { get; set; }

        [Column("unitid")]
        public int? UnitId { get; set; }
        [Column("isactive")]
        public bool IsActive { get; set; } = true;
        [Column("createdby")]
        public int? CreatedBy { get; set; }
        [Column("createdon")]
        public DateTime CreatedOn { get; set; }
        [Column("modifiedby")]
        public int? ModifiedBy { get; set; }
        [Column("modifiedon")]
        public DateTime? ModifiedOn { get; set; }
        [Column("isdeleted")]
        public bool IsDeleted { get; set; } = false;

        [Column("deletedby")]
        public int? DeletedBy { get; set; }
        [Column("deletedon")]
        public DateTime? DeletedOn { get; set; }



    }
}
