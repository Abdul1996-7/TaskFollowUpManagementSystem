namespace TaskFollowUpManagementSystem.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalManagers { get; set; }
        public int TotalEmployees { get; set; }
        public int TotalTasks { get; set; }
    }

    public class ManagerDashboardViewModel
    {
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int OverdueTasks { get; set; }
        public int NewTasks { get; set; }
        public int PendingReviewTasks { get; set; }
        public List<RecentFollowUpViewModel> RecentFollowUps { get; set; } = new();
        public List<TasksByEmployeeViewModel> TasksByEmployee { get; set; } = new();
    }

    public class EmployeeDashboardViewModel
    {
        public int AssignedTasks { get; set; }
        public int PendingFollowUps { get; set; }
        public int CompletedTasks { get; set; }
        public List<UpcomingDeadlineViewModel> UpcomingDeadlines { get; set; } = new();
        public List<TaskItemViewModel> RecentTasks { get; set; } = new();
    }

    public class RecentFollowUpViewModel
    {
        public int FollowUpId { get; set; }
        public string TaskTitle { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public DateTime FollowUpDate { get; set; }
        public bool HasResponse { get; set; }
    }

    public class TasksByEmployeeViewModel
    {
        public string EmployeeName { get; set; } = string.Empty;
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int OverdueTasks { get; set; }
        public int InProgressTasks { get; set; }
    }

    public class UpcomingDeadlineViewModel
    {
        public int TaskId { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public int DaysRemaining { get; set; }
    }
}
