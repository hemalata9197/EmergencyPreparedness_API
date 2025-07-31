using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;
using EmergencyManagement.Models.Entities.Admin;

namespace EmergencyManagement.Models.Entities
{
    [Table("menu")]
    public class Menu
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }


        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Column("route")]
        public string Route { get; set; } = string.Empty;
        
        [Column("parentid")]
        public int? ParentId { get; set; }
        [Column("sortorder")]
        public int SortOrder { get; set; }
        [Column("isactive")]
        public bool IsActive { get; set; }
     
        public virtual Menu? Parent { get; set; }
        public virtual ICollection<Menu> Children { get; set; } = new List<Menu>();
        public ICollection<RoleMenu> RoleMenus { get; set; } = new List<RoleMenu>();
    }
}
