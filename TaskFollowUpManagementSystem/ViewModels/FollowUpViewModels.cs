using System.ComponentModel.DataAnnotations;

namespace TaskFollowUpManagementSystem.ViewModels
{
    public class CreateFollowUpViewModel
    {
        [Required]
        public int TaskItemId { get; set; }

        public string TaskTitle { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Follow-Up Date")]
        public DateTime FollowUpDate { get; set; } = DateTime.Today;

        [Required]
        [StringLength(2000)]
        [Display(Name = "Follow-Up Note")]
        public string Note { get; set; } = string.Empty;
    }

    public class RespondFollowUpViewModel
    {
        public int FollowUpId { get; set; }
        public string TaskTitle { get; set; } = string.Empty;
        public string ManagerName { get; set; } = string.Empty;
        public DateTime FollowUpDate { get; set; }
        public string Note { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        [Display(Name = "Your Response")]
        public string EmployeeResponse { get; set; } = string.Empty;
    }

    public class FollowUpListViewModel
    {
        public List<FollowUpListItemViewModel> FollowUps { get; set; } = new();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
    }

    public class FollowUpListItemViewModel
    {
        public int FollowUpId { get; set; }
        public int TaskItemId { get; set; }
        public string TaskTitle { get; set; } = string.Empty;
        public string ManagerName { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public DateTime FollowUpDate { get; set; }
        public string Note { get; set; } = string.Empty;
        public bool HasResponse { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
