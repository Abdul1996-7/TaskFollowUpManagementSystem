using Microsoft.AspNetCore.Identity;
using TaskFollowUpManagementSystem.Models;
using TaskFollowUpManagementSystem.Models.Enums;

namespace TaskFollowUpManagementSystem.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // Seed roles
            string[] roles = { "Admin", "Manager", "Employee" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Seed Admin user
            var adminUser = await userManager.FindByEmailAsync("admin@taskmanager.com");
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = "admin@taskmanager.com",
                    Email = "admin@taskmanager.com",
                    FullName = "System Administrator",
                    Department = "IT",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };
                await userManager.CreateAsync(adminUser, "Admin@123");
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            // Seed Manager user
            var managerUser = await userManager.FindByEmailAsync("manager@taskmanager.com");
            if (managerUser == null)
            {
                managerUser = new ApplicationUser
                {
                    UserName = "manager@taskmanager.com",
                    Email = "manager@taskmanager.com",
                    FullName = "John Manager",
                    Department = "Operations",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };
                await userManager.CreateAsync(managerUser, "Manager@123");
                await userManager.AddToRoleAsync(managerUser, "Manager");
            }

            // Seed Employee 1
            var employee1 = await userManager.FindByEmailAsync("employee1@taskmanager.com");
            if (employee1 == null)
            {
                employee1 = new ApplicationUser
                {
                    UserName = "employee1@taskmanager.com",
                    Email = "employee1@taskmanager.com",
                    FullName = "Alice Employee",
                    Department = "Development",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };
                await userManager.CreateAsync(employee1, "Employee@123");
                await userManager.AddToRoleAsync(employee1, "Employee");
            }

            // Seed Employee 2
            var employee2 = await userManager.FindByEmailAsync("employee2@taskmanager.com");
            if (employee2 == null)
            {
                employee2 = new ApplicationUser
                {
                    UserName = "employee2@taskmanager.com",
                    Email = "employee2@taskmanager.com",
                    FullName = "Bob Employee",
                    Department = "Marketing",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };
                await userManager.CreateAsync(employee2, "Employee@123");
                await userManager.AddToRoleAsync(employee2, "Employee");
            }

            // Seed sample tasks if none exist
            if (!context.TaskItems.Any())
            {
                var tasks = new List<TaskItem>
                {
                    new TaskItem
                    {
                        Title = "Design Landing Page",
                        Description = "Create a modern and responsive landing page design for the company website.",
                        AssignedById = managerUser.Id,
                        AssignedToId = employee1.Id,
                        StartDate = DateTime.UtcNow.AddDays(-5),
                        DueDate = DateTime.UtcNow.AddDays(10),
                        ExpectedDuration = "5 days",
                        Priority = PriorityLevel.High,
                        Status = TaskStatusEnum.InProgress,
                        ProgressNote = "Working on the initial mockups",
                        CreatedAt = DateTime.UtcNow.AddDays(-5),
                        UpdatedAt = DateTime.UtcNow.AddDays(-2)
                    },
                    new TaskItem
                    {
                        Title = "Write API Documentation",
                        Description = "Document all REST API endpoints with request/response examples.",
                        AssignedById = managerUser.Id,
                        AssignedToId = employee1.Id,
                        StartDate = DateTime.UtcNow.AddDays(-10),
                        DueDate = DateTime.UtcNow.AddDays(-2),
                        ExpectedDuration = "7 days",
                        Priority = PriorityLevel.Medium,
                        Status = TaskStatusEnum.Overdue,
                        ProgressNote = "Completed 60% of the endpoints",
                        CreatedAt = DateTime.UtcNow.AddDays(-10),
                        UpdatedAt = DateTime.UtcNow.AddDays(-3)
                    },
                    new TaskItem
                    {
                        Title = "Prepare Marketing Report",
                        Description = "Compile Q1 marketing performance metrics and create a presentation.",
                        AssignedById = managerUser.Id,
                        AssignedToId = employee2.Id,
                        StartDate = DateTime.UtcNow.AddDays(-3),
                        DueDate = DateTime.UtcNow.AddDays(7),
                        ExpectedDuration = "4 days",
                        Priority = PriorityLevel.Medium,
                        Status = TaskStatusEnum.Assigned,
                        CreatedAt = DateTime.UtcNow.AddDays(-3),
                        UpdatedAt = DateTime.UtcNow.AddDays(-3)
                    },
                    new TaskItem
                    {
                        Title = "Database Optimization",
                        Description = "Optimize slow queries and add necessary indexes to improve performance.",
                        AssignedById = managerUser.Id,
                        AssignedToId = employee1.Id,
                        StartDate = DateTime.UtcNow.AddDays(-15),
                        DueDate = DateTime.UtcNow.AddDays(-5),
                        ExpectedDuration = "10 days",
                        Priority = PriorityLevel.Critical,
                        Status = TaskStatusEnum.Completed,
                        ProgressNote = "All queries optimized and tested",
                        CreatedAt = DateTime.UtcNow.AddDays(-15),
                        UpdatedAt = DateTime.UtcNow.AddDays(-6)
                    },
                    new TaskItem
                    {
                        Title = "Social Media Campaign",
                        Description = "Plan and execute social media campaign for product launch.",
                        AssignedById = managerUser.Id,
                        AssignedToId = employee2.Id,
                        StartDate = DateTime.UtcNow,
                        DueDate = DateTime.UtcNow.AddDays(14),
                        ExpectedDuration = "14 days",
                        Priority = PriorityLevel.High,
                        Status = TaskStatusEnum.New,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }
                };

                context.TaskItems.AddRange(tasks);
                await context.SaveChangesAsync();

                // Seed follow-ups
                var task1 = tasks[0];
                var task2 = tasks[1];

                var followUps = new List<FollowUp>
                {
                    new FollowUp
                    {
                        TaskItemId = task1.TaskId,
                        ManagerId = managerUser.Id,
                        EmployeeId = employee1.Id,
                        FollowUpDate = DateTime.UtcNow.AddDays(-2),
                        Note = "How is the landing page design progressing? Please share the mockups when ready.",
                        EmployeeResponse = "I have completed the initial wireframes. Will share the high-fidelity mockups by tomorrow.",
                        ResponseDate = DateTime.UtcNow.AddDays(-1),
                        CreatedAt = DateTime.UtcNow.AddDays(-2)
                    },
                    new FollowUp
                    {
                        TaskItemId = task2.TaskId,
                        ManagerId = managerUser.Id,
                        EmployeeId = employee1.Id,
                        FollowUpDate = DateTime.UtcNow.AddDays(-3),
                        Note = "The API documentation is past due. What is the status?",
                        CreatedAt = DateTime.UtcNow.AddDays(-3)
                    },
                    new FollowUp
                    {
                        TaskItemId = task1.TaskId,
                        ManagerId = managerUser.Id,
                        EmployeeId = employee1.Id,
                        FollowUpDate = DateTime.UtcNow,
                        Note = "Please provide an update on the landing page. Are we on track for the deadline?",
                        CreatedAt = DateTime.UtcNow
                    }
                };

                context.FollowUps.AddRange(followUps);
                await context.SaveChangesAsync();

                // Seed notifications
                var notifications = new List<Notification>
                {
                    new Notification
                    {
                        UserId = employee1.Id,
                        TaskItemId = task1.TaskId,
                        Message = "You have been assigned a new task: Design Landing Page",
                        Type = NotificationType.TaskAssigned,
                        IsRead = true,
                        CreatedAt = DateTime.UtcNow.AddDays(-5)
                    },
                    new Notification
                    {
                        UserId = employee1.Id,
                        TaskItemId = task2.TaskId,
                        Message = "Task 'Write API Documentation' is overdue!",
                        Type = NotificationType.TaskOverdue,
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow.AddDays(-2)
                    },
                    new Notification
                    {
                        UserId = employee1.Id,
                        TaskItemId = task1.TaskId,
                        Message = "New follow-up added for task: Design Landing Page",
                        Type = NotificationType.FollowUpAdded,
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Notification
                    {
                        UserId = managerUser.Id,
                        TaskItemId = task1.TaskId,
                        Message = "Alice Employee responded to your follow-up on: Design Landing Page",
                        Type = NotificationType.FollowUpResponse,
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow.AddDays(-1)
                    },
                    new Notification
                    {
                        UserId = employee2.Id,
                        TaskItemId = tasks[2].TaskId,
                        Message = "You have been assigned a new task: Prepare Marketing Report",
                        Type = NotificationType.TaskAssigned,
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow.AddDays(-3)
                    }
                };

                context.Notifications.AddRange(notifications);
                await context.SaveChangesAsync();
            }
        }
    }
}
