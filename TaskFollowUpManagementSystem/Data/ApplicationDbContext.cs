using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskFollowUpManagementSystem.Models;

namespace TaskFollowUpManagementSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<TaskItem> TaskItems { get; set; }
        public DbSet<FollowUp> FollowUps { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // TaskItem - AssignedBy relationship
            builder.Entity<TaskItem>()
                .HasOne(t => t.AssignedBy)
                .WithMany(u => u.CreatedTasks)
                .HasForeignKey(t => t.AssignedById)
                .OnDelete(DeleteBehavior.Restrict);

            // TaskItem - AssignedTo relationship
            builder.Entity<TaskItem>()
                .HasOne(t => t.AssignedTo)
                .WithMany(u => u.AssignedTasks)
                .HasForeignKey(t => t.AssignedToId)
                .OnDelete(DeleteBehavior.Restrict);

            // FollowUp - TaskItem relationship
            builder.Entity<FollowUp>()
                .HasOne(f => f.TaskItem)
                .WithMany(t => t.FollowUps)
                .HasForeignKey(f => f.TaskItemId)
                .OnDelete(DeleteBehavior.Cascade);

            // FollowUp - Manager relationship
            builder.Entity<FollowUp>()
                .HasOne(f => f.Manager)
                .WithMany(u => u.ManagerFollowUps)
                .HasForeignKey(f => f.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            // FollowUp - Employee relationship
            builder.Entity<FollowUp>()
                .HasOne(f => f.Employee)
                .WithMany(u => u.EmployeeFollowUps)
                .HasForeignKey(f => f.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Notification - User relationship
            builder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Notification - TaskItem relationship
            builder.Entity<Notification>()
                .HasOne(n => n.TaskItem)
                .WithMany(t => t.Notifications)
                .HasForeignKey(n => n.TaskItemId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            builder.Entity<TaskItem>().HasIndex(t => t.Status);
            builder.Entity<TaskItem>().HasIndex(t => t.Priority);
            builder.Entity<TaskItem>().HasIndex(t => t.DueDate);
            builder.Entity<Notification>().HasIndex(n => n.IsRead);
            builder.Entity<Notification>().HasIndex(n => n.UserId);
        }
    }
}
