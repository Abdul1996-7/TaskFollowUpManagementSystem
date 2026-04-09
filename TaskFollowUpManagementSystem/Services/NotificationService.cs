using Microsoft.EntityFrameworkCore;
using TaskFollowUpManagementSystem.Data;
using TaskFollowUpManagementSystem.Models;
using TaskFollowUpManagementSystem.Models.Enums;

namespace TaskFollowUpManagementSystem.Services
{
    public interface INotificationService
    {
        Task CreateNotificationAsync(string userId, int? taskItemId, string message, NotificationType type);
        Task<List<Notification>> GetUserNotificationsAsync(string userId, int count = 10);
        Task<int> GetUnreadCountAsync(string userId);
        Task MarkAsReadAsync(int notificationId, string userId);
        Task MarkAllAsReadAsync(string userId);
        Task CheckAndCreateOverdueNotificationsAsync();
    }

    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CreateNotificationAsync(string userId, int? taskItemId, string message, NotificationType type)
        {
            var notification = new Notification
            {
                UserId = userId,
                TaskItemId = taskItemId,
                Message = message,
                Type = type,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(string userId, int count = 10)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(count)
                .Include(n => n.TaskItem)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task MarkAsReadAsync(int notificationId, string userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.UserId == userId);

            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkAllAsReadAsync(string userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();
        }

        public async Task CheckAndCreateOverdueNotificationsAsync()
        {
            var overdueTasks = await _context.TaskItems
                .Where(t => t.DueDate < DateTime.UtcNow
                    && t.Status != TaskStatusEnum.Completed
                    && t.Status != TaskStatusEnum.Closed
                    && t.Status != TaskStatusEnum.Overdue)
                .ToListAsync();

            foreach (var task in overdueTasks)
            {
                task.Status = TaskStatusEnum.Overdue;
                task.UpdatedAt = DateTime.UtcNow;

                // Notify the employee
                await CreateNotificationAsync(
                    task.AssignedToId,
                    task.TaskId,
                    $"Task '{task.Title}' is overdue!",
                    NotificationType.TaskOverdue);

                // Notify the manager
                await CreateNotificationAsync(
                    task.AssignedById,
                    task.TaskId,
                    $"Task '{task.Title}' assigned to employee is overdue!",
                    NotificationType.TaskOverdue);
            }

            // Check for tasks nearing deadline (within 2 days)
            var nearingDeadlineTasks = await _context.TaskItems
                .Where(t => t.DueDate <= DateTime.UtcNow.AddDays(2)
                    && t.DueDate > DateTime.UtcNow
                    && t.Status != TaskStatusEnum.Completed
                    && t.Status != TaskStatusEnum.Closed)
                .ToListAsync();

            foreach (var task in nearingDeadlineTasks)
            {
                // Check if notification already exists for this task today
                var existingNotification = await _context.Notifications
                    .AnyAsync(n => n.TaskItemId == task.TaskId
                        && n.Type == NotificationType.TaskNearingDeadline
                        && n.CreatedAt.Date == DateTime.UtcNow.Date);

                if (!existingNotification)
                {
                    await CreateNotificationAsync(
                        task.AssignedToId,
                        task.TaskId,
                        $"Task '{task.Title}' is due on {task.DueDate:MMM dd, yyyy}!",
                        NotificationType.TaskNearingDeadline);
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
