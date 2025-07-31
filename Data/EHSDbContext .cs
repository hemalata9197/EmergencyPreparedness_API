using EmergencyManagement.Models.Entities;
using EmergencyManagement.Models.Entities.Admin;
using EmergencyManagement.Models.Entities.Email;
using EmergencyManagement.Models.Entities.Fire_Drill;
using EmergencyManagement.Models.Entities.Incident;
using EmergencyManagement.Models.Entities.Task;
using Microsoft.EntityFrameworkCore;
using System;

namespace EmergencyManagement.Data
{
    public class EHSDbContext : DbContext
    {
        public EHSDbContext(DbContextOptions<EHSDbContext> options) : base(options)
        {
        }
        //Role Permission
        public DbSet<Users> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Menu> Menus { get; set; }
        public DbSet<RoleMenu> RoleMenus { get; set; }
        //Form Created table 
        public DbSet<FormDefinition> FormDefinitions { get; set; }
        public DbSet<FormSection> FormSections { get; set; }
        public DbSet<FormField> FormFields { get; set; }
        public DbSet<FormFieldValidation> FormFieldValidations { get; set; }

        // Add master tables for dropdowns
        public DbSet<UnitsMaster> UnitsMaster { get; set; }
        public DbSet<FacilityMaster> FacilityMasters { get; set; }
        public DbSet<ScenarioMaster> ScenarioMasters { get; set; }
        public DbSet<Employees> Employees { get; set; }
        public DbSet<Models.Entities.Task.TaskStatus> TaskStatus { get; set; }
        public DbSet<ApprovalStatus> ApprovalStatus { get; set; }
        public DbSet<Designation> Designations { get; set; }
        public DbSet<GeneralConfigElements> GeneralConfigElements { get; set; }
        public DbSet<GeneralConfigElementValues> GeneralConfigElementValues { get; set; }
        public DbSet<SeverityMaster> SeverityMaster { get; set; }   
        //Form Submission table 
        public DbSet<FireDrill> FireDrills { get; set; }
        public DbSet<FireDrillResposeEmp> FireDrillResposeEmp { get; set; }
        public DbSet<Recommendation> Recommendations { get; set; }
        public DbSet<Tasks> Tasks { get; set; }
        public DbSet<taskAssgntoUser> taskAssgntoUsers { get; set; }
        public DbSet<TaskEvidence> TaskEvidence { get; set; }   
        public DbSet<FireDrillDocuments> FireDrillDocuments { get; set; }
        public DbSet<IncidentDetails> IncidentDetails { get; set; }
        public DbSet<TaskHistory> TaskHistory { get; set; }
        // Mail table
        public DbSet<MailMessages> MailMessages { get; set; }
        public DbSet<MailMessagesToUser> MailMessagesToUser { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserRole>().HasKey(ur => new { ur.UserId, ur.RoleId });
            modelBuilder.Entity<RoleMenu>().HasKey(rm => new { rm.RoleId, rm.MenuId });
            modelBuilder.Entity<FormDefinition>().ToTable("formdefinitions");
            modelBuilder.Entity<FormSection>().ToTable("form_sections");
            modelBuilder.Entity<FormField>().ToTable("form_fields");
            modelBuilder.Entity<FormFieldValidation>().ToTable("form_field_validations");
            modelBuilder.Entity<FacilityMaster>().ToTable("facilitymaster");

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);

            modelBuilder.Entity<RoleMenu>()
                .HasOne(rm => rm.Role)
                .WithMany(r => r.RoleMenus)
                .HasForeignKey(rm => rm.RoleId);

            modelBuilder.Entity<RoleMenu>()
                .HasOne(rm => rm.Menu)
                .WithMany(m => m.RoleMenus)
                .HasForeignKey(rm => rm.MenuId);

            modelBuilder.Entity<Menu>()
                .HasOne(m => m.Parent)
                .WithMany(m => m.Children)
                .HasForeignKey(m => m.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FormFieldValidation>()
                .HasOne<FormField>()
                .WithMany(f => f.Validations)
                .HasForeignKey(v => v.FieldId);


            modelBuilder.Entity<FacilityMaster>()
              .HasOne(e => e.Parent)
              .WithMany(e => e.Children)
              .HasForeignKey(e => e.ParentId)
              .HasConstraintName("fk_facilitymaster_parent")
              .OnDelete(DeleteBehavior.Restrict);


             modelBuilder.Entity<FormSection>()
             .HasMany(s => s.Fields)
             .WithOne(f => f.Section)
             .HasForeignKey(f => f.SectionId)
             .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<Tasks>()
     .HasOne(t => t.Recommendation)
     .WithMany(r => r.Tasks)
     .HasForeignKey(t => t.TaskCreatedForId);

            modelBuilder.Entity<taskAssgntoUser>()
     .HasOne(t => t.Tasks)
     .WithMany(r => r.taskAssgntoUsers)
     .HasForeignKey(t => t.TaskId);

            modelBuilder.Entity<MailMessages>()
            .HasKey(f => f.MessageId);

            modelBuilder.Entity<MailMessagesToUser>()
                .HasKey(m => m.MailMessagesToUserMailId);

            modelBuilder.Entity<MailMessagesToUser>()
                .HasOne(m => m.MailMessages)
                .WithMany(f => f.MailMessagesToUsers)
                .HasForeignKey(m => m.MessageId)
                .OnDelete(DeleteBehavior.Cascade);


            base.OnModelCreating(modelBuilder);



        }
    }
}
