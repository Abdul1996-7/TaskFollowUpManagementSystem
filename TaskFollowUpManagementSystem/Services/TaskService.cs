using Microsoft.EntityFrameworkCore;
using TaskFollowUpManagementSystem.Data;
using TaskFollowUpManagementSystem.Models;
using TaskFollowUpManagementSystem.Models.Enums;
using TaskFollowUpManagementSystem.ViewModels;

namespace TaskFollowUpManagementSystem.Services
{
    public interface ITaskService
    {
        Task<TaskItem> CreateTaskAsync(CreateTaskViewModel model, string managerId);
        Task<TaskItem?> GetTaskByIdAsync(int taskId);
        Task<TaskDetailsViewModel?> GetTaskDetailsAsync(int taskId);
        Task UpdateTaskAsync(EditTaskViewModel model);
        Task DeleteTaskAsync(int taskId);
        Task<TaskListViewModel> GetManagerTasksAsync(string managerId, string? search, string? employeeFilter,
            PriorityLevel? priority, TaskStatusEnum? status, DateTime? dueDate, int page = 1, int pageSize = 10);
        Task<TaskListViewModel> GetEmployeeTasksAsync(string employeeId, string? search,
            PriorityLevel? priority, TaskStatusEnum? status, int page = 1, int pageSize = 10);
        Task UpdateTaskStatusAsync(int taskId, TaskStatusEnum newStatus, string? progressNote, string employeeId);
        Task<ManagerDashboardViewModel> GetManagerDashboardAsync(string managerId);
        Task<EmployeeDashboardViewModel> GetEmployeeDashboardAsync(string employeeId);
        Task<AdminDashboardViewModel> GetAdminDashboardAsync();
    }

    public class TaskService : ITaskService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public TaskService(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<TaskItem> CreateTaskAsync(CreateTaskViewModel model, string managerId)
        {
            var task = new TaskItem
            {
                Title = model.Title,
                Description = model.Description,
                AssignedById = managerId,
                AssignedToId = model.AssignedToId,
                StartDate = model.StartDate,
                DueDate = model.DueDate,
                ExpectedDuration = model.ExpectedDuration,
                Priority = model.Priority,
                Status = model.Status,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.TaskItems.Add(task);
            await _context.SaveChangesAsync();

            // Create notification for assigned employee
            await _notificationService.CreateNotificationAsync(
                model.AssignedToId,
                task.TaskId,
                $"You have been assigned a new task: {task.Title}",
                NotificationType.TaskAssigned);

            return task;
        }

        public async Task<TaskItem?> GetTaskByIdAsync(int taskId)
        {
            return await _context.TaskItems
                .Include(t => t.AssignedBy)
                .Include(t => t.AssignedTo)
                .Include(t => t.FollowUps)
                .FirstOrDefaultAsync(t => t.TaskId == taskId);
        }

        public async Task<TaskDetailsViewModel?> GetTaskDetailsAsync(int taskId)
        {
            var task = await _context.TaskItems
                .Include(t => t.AssignedBy)
                .Include(t => t.AssignedTo)
                .Include(t => t.FollowUps)
                    .ThenInclude(f => f.Manager)
                .FirstOrDefaultAsync(t => t.TaskId == taskId);

            if (task == null) return null;

            return new TaskDetailsViewModel
            {
                TaskId = task.TaskId,
                Title = task.Title,
                Description = task.Description,
                AssignedByName = task.AssignedBy?.FullName ?? "Unknown",
                AssignedToName = task.AssignedTo?.FullName ?? "Unknown",
                AssignedToId = task.AssignedToId,
                StartDate = task.StartDate,
                DueDate = task.DueDate,
                ExpectedDuration = task.ExpectedDuration,
                Priority = task.Priority,
                Status = task.IsOverdue && task.Status != TaskStatusEnum.Overdue ? TaskStatusEnum.Overdue : task.Status,
                ProgressNote = task.ProgressNote,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt,
                IsOverdue = task.IsOverdue,
                FollowUps = task.FollowUps.OrderByDescending(f => f.CreatedAt).Select(f => new FollowUpViewModel
                {
                    FollowUpId = f.FollowUpId,
                    ManagerName = f.Manager?.FullName ?? "Unknown",
                    FollowUpDate = f.FollowUpDate,
                    Note = f.Note,
                    EmployeeResponse = f.EmployeeResponse,
                    ResponseDate = f.ResponseDate,
                    CreatedAt = f.CreatedAt
                }).ToList()
            };
        }

        public async Task UpdateTaskAsync(EditTaskViewModel model)
        {
            var task = await _context.TaskItems.FindAsync(model.TaskId);
            if (task == null) return;

            task.Title = model.Title;
            task.Description = model.Description;
            task.AssignedToId = model.AssignedToId;
            task.StartDate = model.StartDate;
            task.DueDate = model.DueDate;
            task.ExpectedDuration = model.ExpectedDuration;
            task.Priority = model.Priority;
            task.Status = model.Status;
            task.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteTaskAsync(int taskId)
        {
            var task = await _context.TaskItems
                .Include(t => t.Notifications)
                .FirstOrDefaultAsync(t => t.TaskId == taskId);
            if (task == null) return;

            _context.Notifications.RemoveRange(task.Notifications);
            _context.TaskItems.Remove(task);
            await _context.SaveChangesAsync();
        }

        public async Task<TaskListViewModel> GetManagerTasksAsync(string managerId, string? search,
            string? employeeFilter, PriorityLevel? priority, TaskStatusEnum? status, DateTime? dueDate,
            int page = 1, int pageSize = 10)
        {
            var query = _context.TaskItems
                .Include(t => t.AssignedTo)
                .Where(t => t.AssignedById == managerId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(t => t.Title.Contains(search) || (t.Description != null && t.Description.Contains(search)));

            if (!string.IsNullOrWhiteSpace(employeeFilter))
                query = query.Where(t => t.AssignedToId == employeeFilter);

            if (priority.HasValue)
                query = query.Where(t => t.Priority == priority.Value);

            if (status.HasValue)
                query = query.Where(t => t.Status == status.Value);

            if (dueDate.HasValue)
                query = query.Where(t => t.DueDate.Date == dueDate.Value.Date);

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var tasks = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new TaskItemViewModel
                {
                    TaskId = t.TaskId,
                    Title = t.Title,
                    AssignedToName = t.AssignedTo != null ? t.AssignedTo.FullName : "Unknown",
                    DueDate = t.DueDate,
                    Priority = t.Priority,
                    Status = t.Status,
                    IsOverdue = t.DueDate < DateTime.UtcNow && t.Status != TaskStatusEnum.Completed && t.Status != TaskStatusEnum.Closed,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();

            return new TaskListViewModel
            {
                Tasks = tasks,
                SearchTerm = search,
                EmployeeFilter = employeeFilter,
                PriorityFilter = priority,
                StatusFilter = status,
                DueDateFilter = dueDate,
                CurrentPage = page,
                TotalPages = totalPages,
                PageSize = pageSize
            };
        }

        public async Task<TaskListViewModel> GetEmployeeTasksAsync(string employeeId, string? search,
            PriorityLevel? priority, TaskStatusEnum? status, int page = 1, int pageSize = 10)
        {
            var query = _context.TaskItems
                .Include(t => t.AssignedBy)
                .Where(t => t.AssignedToId == employeeId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(t => t.Title.Contains(search) || (t.Description != null && t.Description.Contains(search)));

            if (priority.HasValue)
                query = query.Where(t => t.Priority == priority.Value);

            if (status.HasValue)
                query = query.Where(t => t.Status == status.Value);

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var tasks = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new TaskItemViewModel
                {
                    TaskId = t.TaskId,
                    Title = t.Title,
                    AssignedToName = t.AssignedTo != null ? t.AssignedTo.FullName : "Unknown",
                    DueDate = t.DueDate,
                    Priority = t.Priority,
                    Status = t.Status,
                    IsOverdue = t.DueDate < DateTime.UtcNow && t.Status != TaskStatusEnum.Completed && t.Status != TaskStatusEnum.Closed,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();

            return new TaskListViewModel
            {
                Tasks = tasks,
                SearchTerm = search,
                PriorityFilter = priority,
                StatusFilter = status,
                CurrentPage = page,
                TotalPages = totalPages,
                PageSize = pageSize
            };
        }

        public async Task UpdateTaskStatusAsync(int taskId, TaskStatusEnum newStatus, string? progressNote, string employeeId)
        {
            var task = await _context.TaskItems.FindAsync(taskId);
            if (task == null || task.AssignedToId != employeeId) return;

            task.Status = newStatus;
            task.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(progressNote))
                task.ProgressNote = progressNote;

            await _context.SaveChangesAsync();

            if (newStatus == TaskStatusEnum.Completed)
            {
                await _notificationService.CreateNotificationAsync(
                    task.AssignedById,
                    task.TaskId,
                    $"Task '{task.Title}' has been marked as completed.",
                    NotificationType.TaskCompleted);
            }
        }

        public async Task<ManagerDashboardViewModel> GetManagerDashboardAsync(string managerId)
        {
            var tasks = await _context.TaskItems
                .Where(t => t.AssignedById == managerId)
                .Include(t => t.AssignedTo)
                .ToListAsync();

            var recentFollowUps = await _context.FollowUps
                .Where(f => f.ManagerId == managerId)
                .Include(f => f.TaskItem)
                .Include(f => f.Employee)
                .OrderByDescending(f => f.CreatedAt)
                .Take(5)
                .ToListAsync();

            var tasksByEmployee = tasks
                .GroupBy(t => t.AssignedTo?.FullName ?? "Unknown")
                .Select(g => new TasksByEmployeeViewModel
                {
                    EmployeeName = g.Key,
                    TotalTasks = g.Count(),
                    CompletedTasks = g.Count(t => t.Status == TaskStatusEnum.Completed || t.Status == TaskStatusEnum.Closed),
                    OverdueTasks = g.Count(t => t.IsOverdue),
                    InProgressTasks = g.Count(t => t.Status == TaskStatusEnum.InProgress)
                })
                .ToList();

            return new ManagerDashboardViewModel
            {
                TotalTasks = tasks.Count,
                CompletedTasks = tasks.Count(t => t.Status == TaskStatusEnum.Completed || t.Status == TaskStatusEnum.Closed),
                InProgressTasks = tasks.Count(t => t.Status == TaskStatusEnum.InProgress),
                OverdueTasks = tasks.Count(t => t.IsOverdue),
                NewTasks = tasks.Count(t => t.Status == TaskStatusEnum.New || t.Status == TaskStatusEnum.Assigned),
                PendingReviewTasks = tasks.Count(t => t.Status == TaskStatusEnum.PendingReview),
                RecentFollowUps = recentFollowUps.Select(f => new RecentFollowUpViewModel
                {
                    FollowUpId = f.FollowUpId,
                    TaskTitle = f.TaskItem?.Title ?? "Unknown",
                    EmployeeName = f.Employee?.FullName ?? "Unknown",
                    FollowUpDate = f.FollowUpDate,
                    HasResponse = !string.IsNullOrEmpty(f.EmployeeResponse)
                }).ToList(),
                TasksByEmployee = tasksByEmployee
            };
        }

        public async Task<EmployeeDashboardViewModel> GetEmployeeDashboardAsync(string employeeId)
        {
            var tasks = await _context.TaskItems
                .Where(t => t.AssignedToId == employeeId)
                .ToListAsync();

            var pendingFollowUps = await _context.FollowUps
                .CountAsync(f => f.EmployeeId == employeeId && f.EmployeeResponse == null);

            var upcomingDeadlines = tasks
                .Where(t => t.DueDate >= DateTime.UtcNow && t.Status != TaskStatusEnum.Completed && t.Status != TaskStatusEnum.Closed)
                .OrderBy(t => t.DueDate)
                .Take(5)
                .Select(t => new UpcomingDeadlineViewModel
                {
                    TaskId = t.TaskId,
                    Title = t.Title,
                    DueDate = t.DueDate,
                    DaysRemaining = (int)(t.DueDate - DateTime.UtcNow).TotalDays
                })
                .ToList();

            var recentTasks = tasks
                .OrderByDescending(t => t.UpdatedAt)
                .Take(5)
                .Select(t => new TaskItemViewModel
                {
                    TaskId = t.TaskId,
                    Title = t.Title,
                    DueDate = t.DueDate,
                    Priority = t.Priority,
                    Status = t.Status,
                    IsOverdue = t.IsOverdue,
                    CreatedAt = t.CreatedAt
                })
                .ToList();

            return new EmployeeDashboardViewModel
            {
                AssignedTasks = tasks.Count(t => t.Status != TaskStatusEnum.Completed && t.Status != TaskStatusEnum.Closed),
                PendingFollowUps = pendingFollowUps,
                CompletedTasks = tasks.Count(t => t.Status == TaskStatusEnum.Completed || t.Status == TaskStatusEnum.Closed),
                UpcomingDeadlines = upcomingDeadlines,
                RecentTasks = recentTasks
            };
        }

        public async Task<AdminDashboardViewModel> GetAdminDashboardAsync()
        {
            var totalTasks = await _context.TaskItems.CountAsync();

            return new AdminDashboardViewModel
            {
                TotalTasks = totalTasks
            };
        }
    }
}
