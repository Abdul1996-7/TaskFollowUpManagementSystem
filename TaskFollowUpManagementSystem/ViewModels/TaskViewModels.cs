using System.ComponentModel.DataAnnotations;
using TaskFollowUpManagementSystem.Models.Enums;

namespace TaskFollowUpManagementSystem.ViewModels
{
    public class CreateTaskViewModel
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Assign To")]
        public string AssignedToId { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Due Date")]
        public DateTime DueDate { get; set; } = DateTime.Today.AddDays(7);

        [StringLength(100)]
        [Display(Name = "Expected Duration")]
        public string? ExpectedDuration { get; set; }

        [Required]
        public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;

        [Required]
        public TaskStatusEnum Status { get; set; } = TaskStatusEnum.Assigned;
    }

    public class EditTaskViewModel
    {
        public int TaskId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Assign To")]
        public string AssignedToId { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Due Date")]
        public DateTime DueDate { get; set; }

        [StringLength(100)]
        [Display(Name = "Expected Duration")]
        public string? ExpectedDuration { get; set; }

        [Required]
        public PriorityLevel Priority { get; set; }

        [Required]
        public TaskStatusEnum Status { get; set; }
    }

    public class TaskDetailsViewModel
    {
        public int TaskId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string AssignedByName { get; set; } = string.Empty;
        public string AssignedToName { get; set; } = string.Empty;
        public string AssignedToId { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public string? ExpectedDuration { get; set; }
        public PriorityLevel Priority { get; set; }
        public TaskStatusEnum Status { get; set; }
        public string? ProgressNote { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsOverdue { get; set; }
        public List<FollowUpViewModel> FollowUps { get; set; } = new();
    }

    public class TaskListViewModel
    {
        public List<TaskItemViewModel> Tasks { get; set; } = new();
        public string? SearchTerm { get; set; }
        public string? EmployeeFilter { get; set; }
        public PriorityLevel? PriorityFilter { get; set; }
        public TaskStatusEnum? StatusFilter { get; set; }
        public DateTime? DueDateFilter { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 10;
    }

    public class TaskItemViewModel
    {
        public int TaskId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string AssignedToName { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public PriorityLevel Priority { get; set; }
        public TaskStatusEnum Status { get; set; }
        public bool IsOverdue { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UpdateTaskStatusViewModel
    {
        public int TaskId { get; set; }
        public string Title { get; set; } = string.Empty;
        public TaskStatusEnum CurrentStatus { get; set; }

        [Required]
        public TaskStatusEnum NewStatus { get; set; }

        [StringLength(2000)]
        [Display(Name = "Progress Note")]
        public string? ProgressNote { get; set; }
    }

    public class FollowUpViewModel
    {
        public int FollowUpId { get; set; }
        public string ManagerName { get; set; } = string.Empty;
        public DateTime FollowUpDate { get; set; }
        public string Note { get; set; } = string.Empty;
        public string? EmployeeResponse { get; set; }
        public DateTime? ResponseDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
