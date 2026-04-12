using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TaskFollowUpManagementSystem.Models.Enums;

namespace TaskFollowUpManagementSystem.Models
{
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        public int? TaskItemId { get; set; }

        [ForeignKey("TaskItemId")]
        public virtual TaskItem? TaskItem { get; set; }

        [Required]
        [StringLength(500)]
        public string Message { get; set; } = string.Empty;

        [Required]
        public NotificationType Type { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
