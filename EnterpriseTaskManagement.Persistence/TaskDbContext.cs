using EnterpriseTaskManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseTaskManagement.Persistence
{
    public class TaskDbContext : DbContext
    {
        public TaskDbContext(DbContextOptions<TaskDbContext> options) : base(options)
        {

        }

        public DbSet<TaskItem> Tasks { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<TaskHistory> TaskHistories { get; set; }
        public DbSet<Skill> Skills { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectUser> ProjectUsers { get; set; }
        public DbSet<ProjectSkill> ProjectSkills { get; set; }
        public DbSet<UserSkill> UserSkills { get; set; }
        public DbSet<TaskSkill> TaskSkills { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TaskItem>()
                .HasOne(task => task.Assignee)
                .WithMany()
                .HasForeignKey(task => task.AssigneeId)
                .OnDelete(DeleteBehavior.SetNull);
            
            modelBuilder.Entity<User>()
                .HasOne(user => user.Role)
                .WithMany()
                .HasForeignKey(user => user.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<RefreshToken>()
                .HasOne(r => r.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Task)
                .WithMany(t => t.Comments)
                .HasForeignKey(c => c.TaskId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskHistory>()
                .HasOne(h => h.Task)
                .WithMany(t => t.History)
                .HasForeignKey(h => h.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TaskHistory>()
                .HasOne(h => h.User)
                .WithMany(u => u.TaskHistories)
                .HasForeignKey(h => h.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Skill
            modelBuilder.Entity<Skill>()
                .HasIndex(s => s.Name)
                .IsUnique();

            // UserSkill
            modelBuilder.Entity<UserSkill>()
                .HasKey(us => new { us.UserId, us.SkillId });

            modelBuilder.Entity<UserSkill>()
                .HasOne(us => us.User)
                .WithMany()
                .HasForeignKey(us => us.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserSkill>()
                .HasOne(us => us.Skill)
                .WithMany(s => s.UserSkills)
                .HasForeignKey(us => us.SkillId)
                .OnDelete(DeleteBehavior.Cascade);

            // ProjectSkill
            modelBuilder.Entity<ProjectSkill>()
                .HasKey(ps => new { ps.ProjectId, ps.SkillId });

            modelBuilder.Entity<ProjectSkill>()
                .HasOne(ps => ps.Project)
                .WithMany()
                .HasForeignKey(ps => ps.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProjectSkill>()
                .HasOne(ps => ps.Skill)
                .WithMany(s => s.ProjectSkills)
                .HasForeignKey(ps => ps.SkillId)
                .OnDelete(DeleteBehavior.Cascade);

            // ProjectUser
            modelBuilder.Entity<ProjectUser>()
                .HasKey(pu => new { pu.ProjectId, pu.UserId });

            modelBuilder.Entity<ProjectUser>()
                .HasOne(pu => pu.Project)
                .WithMany()
                .HasForeignKey(pu => pu.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProjectUser>()
                .HasOne(pu => pu.User)
                .WithMany()
                .HasForeignKey(pu => pu.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskItem>()
                .HasOne(task => task.Project)
                .WithMany(project => project.Tasks)
                .HasForeignKey(task => task.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TaskSkill>()
                .HasKey(ts => new { ts.TaskId, ts.SkillId });

            modelBuilder.Entity<TaskSkill>()
                .HasOne(ts => ts.Task)
                .WithMany(t => t.RequiredSkills)
                .HasForeignKey(ts => ts.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TaskSkill>()
                .HasOne(ts => ts.Skill)
                .WithMany()
                .HasForeignKey(ts => ts.SkillId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
