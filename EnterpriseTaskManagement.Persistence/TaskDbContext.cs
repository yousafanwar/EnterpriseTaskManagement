using System;
using System.Collections.Generic;
using System.Text;
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
        }
    }
}
