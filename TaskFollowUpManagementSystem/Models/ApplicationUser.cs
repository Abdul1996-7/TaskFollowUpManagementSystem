using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TaskFollowUpManagementSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Department { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();
        public virtual ICollection<TaskItem> CreatedTasks { get; set; } = new List<TaskItem>();
        public virtual ICollection<FollowUp> ManagerFollowUps { get; set; } = new List<FollowUp>();
        public virtual ICollection<FollowUp> EmployeeFollowUps { get; set; } = new List<FollowUp>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
