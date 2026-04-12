using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskFollowUpManagementSystem.Models
{
    public class FollowUp
    {
        [Key]
        public int FollowUpId { get; set; }

        [Required]
        public int TaskItemId { get; set; }

        [ForeignKey("TaskItemId")]
        public virtual TaskItem? TaskItem { get; set; }

        [Required]
        public string ManagerId { get; set; } = string.Empty;

        [ForeignKey("ManagerId")]
        public virtual ApplicationUser? Manager { get; set; }

        [Required]
        public string EmployeeId { get; set; } = string.Empty;

        [ForeignKey("EmployeeId")]
        public virtual ApplicationUser? Employee { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime FollowUpDate { get; set; }

        [Required]
        [StringLength(2000)]
        public string Note { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? EmployeeResponse { get; set; }

        public DateTime? ResponseDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
