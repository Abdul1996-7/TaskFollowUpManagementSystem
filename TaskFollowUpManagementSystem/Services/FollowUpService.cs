using Microsoft.EntityFrameworkCore;
using TaskFollowUpManagementSystem.Data;
using TaskFollowUpManagementSystem.Models;
using TaskFollowUpManagementSystem.Models.Enums;
using TaskFollowUpManagementSystem.ViewModels;

namespace TaskFollowUpManagementSystem.Services
{
    public interface IFollowUpService
    {
        Task<FollowUp> CreateFollowUpAsync(CreateFollowUpViewModel model, string managerId);
        Task<FollowUp?> GetFollowUpByIdAsync(int followUpId);
        Task RespondToFollowUpAsync(int followUpId, string response, string employeeId);
        Task<FollowUpListViewModel> GetManagerFollowUpsAsync(string managerId, int page = 1, int pageSize = 10);
        Task<FollowUpListViewModel> GetEmployeeFollowUpsAsync(string employeeId, int page = 1, int pageSize = 10);
        Task<List<FollowUp>> GetTaskFollowUpsAsync(int taskItemId);
    }

    public class FollowUpService : IFollowUpService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public FollowUpService(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<FollowUp> CreateFollowUpAsync(CreateFollowUpViewModel model, string managerId)
        {
            var task = await _context.TaskItems.FindAsync(model.TaskItemId);
            if (task == null) throw new ArgumentException("Task not found");

            var followUp = new FollowUp
            {
                TaskItemId = model.TaskItemId,
                ManagerId = managerId,
                EmployeeId = task.AssignedToId,
                FollowUpDate = model.FollowUpDate,
                Note = model.Note,
                CreatedAt = DateTime.UtcNow
            };

            _context.FollowUps.Add(followUp);
            await _context.SaveChangesAsync();

            // Notify employee
            await _notificationService.CreateNotificationAsync(
                task.AssignedToId,
                task.TaskId,
                $"New follow-up added for task: {task.Title}",
                NotificationType.FollowUpAdded);

            return followUp;
        }

        public async Task<FollowUp?> GetFollowUpByIdAsync(int followUpId)
        {
            return await _context.FollowUps
                .Include(f => f.TaskItem)
                .Include(f => f.Manager)
                .Include(f => f.Employee)
                .FirstOrDefaultAsync(f => f.FollowUpId == followUpId);
        }

        public async Task RespondToFollowUpAsync(int followUpId, string response, string employeeId)
        {
            var followUp = await _context.FollowUps
                .Include(f => f.TaskItem)
                .FirstOrDefaultAsync(f => f.FollowUpId == followUpId && f.EmployeeId == employeeId);

            if (followUp == null) return;

            followUp.EmployeeResponse = response;
            followUp.ResponseDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Notify manager
            await _notificationService.CreateNotificationAsync(
                followUp.ManagerId,
                followUp.TaskItemId,
                $"Employee responded to your follow-up on: {followUp.TaskItem?.Title}",
                NotificationType.FollowUpResponse);
        }

        public async Task<FollowUpListViewModel> GetManagerFollowUpsAsync(string managerId, int page = 1, int pageSize = 10)
        {
            var query = _context.FollowUps
                .Where(f => f.ManagerId == managerId)
                .Include(f => f.TaskItem)
                .Include(f => f.Employee)
                .Include(f => f.Manager)
                .OrderByDescending(f => f.CreatedAt);

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var followUps = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(f => new FollowUpListItemViewModel
                {
                    FollowUpId = f.FollowUpId,
                    TaskItemId = f.TaskItemId,
                    TaskTitle = f.TaskItem != null ? f.TaskItem.Title : "Unknown",
                    ManagerName = f.Manager != null ? f.Manager.FullName : "Unknown",
                    EmployeeName = f.Employee != null ? f.Employee.FullName : "Unknown",
                    FollowUpDate = f.FollowUpDate,
                    Note = f.Note,
                    HasResponse = f.EmployeeResponse != null,
                    CreatedAt = f.CreatedAt
                })
                .ToListAsync();

            return new FollowUpListViewModel
            {
                FollowUps = followUps,
                CurrentPage = page,
                TotalPages = totalPages
            };
        }

        public async Task<FollowUpListViewModel> GetEmployeeFollowUpsAsync(string employeeId, int page = 1, int pageSize = 10)
        {
            var query = _context.FollowUps
                .Where(f => f.EmployeeId == employeeId)
                .Include(f => f.TaskItem)
                .Include(f => f.Manager)
                .Include(f => f.Employee)
                .OrderByDescending(f => f.CreatedAt);

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var followUps = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(f => new FollowUpListItemViewModel
                {
                    FollowUpId = f.FollowUpId,
                    TaskItemId = f.TaskItemId,
                    TaskTitle = f.TaskItem != null ? f.TaskItem.Title : "Unknown",
                    ManagerName = f.Manager != null ? f.Manager.FullName : "Unknown",
                    EmployeeName = f.Employee != null ? f.Employee.FullName : "Unknown",
                    FollowUpDate = f.FollowUpDate,
                    Note = f.Note,
                    HasResponse = f.EmployeeResponse != null,
                    CreatedAt = f.CreatedAt
                })
                .ToListAsync();

            return new FollowUpListViewModel
            {
                FollowUps = followUps,
                CurrentPage = page,
                TotalPages = totalPages
            };
        }

        public async Task<List<FollowUp>> GetTaskFollowUpsAsync(int taskItemId)
        {
            return await _context.FollowUps
                .Where(f => f.TaskItemId == taskItemId)
                .Include(f => f.Manager)
                .Include(f => f.Employee)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }
    }
}
