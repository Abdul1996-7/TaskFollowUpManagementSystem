using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TaskFollowUpManagementSystem.Models.Enums;

namespace TaskFollowUpManagementSystem.Models
{
    public class TaskItem
    {
        [Key]
        public int TaskId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        [Required]
        public string AssignedById { get; set; } = string.Empty;

        [ForeignKey("AssignedById")]
        public virtual ApplicationUser? AssignedBy { get; set; }

        [Required]
        public string AssignedToId { get; set; } = string.Empty;

        [ForeignKey("AssignedToId")]
        public virtual ApplicationUser? AssignedTo { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }

        [StringLength(100)]
        public string? ExpectedDuration { get; set; }

        [Required]
        public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;

        [Required]
        public TaskStatusEnum Status { get; set; } = TaskStatusEnum.New;

        [StringLength(2000)]
        public string? ProgressNote { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<FollowUp> FollowUps { get; set; } = new List<FollowUp>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

        // Computed property for overdue check
        [NotMapped]
        public bool IsOverdue => DueDate < DateTime.UtcNow
            && Status != TaskStatusEnum.Completed
            && Status != TaskStatusEnum.Closed;
    }
}
