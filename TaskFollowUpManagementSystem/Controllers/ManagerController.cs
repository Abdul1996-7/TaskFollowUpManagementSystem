using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TaskFollowUpManagementSystem.Models;
using TaskFollowUpManagementSystem.Models.Enums;
using TaskFollowUpManagementSystem.Services;
using TaskFollowUpManagementSystem.ViewModels;

namespace TaskFollowUpManagementSystem.Controllers
{
    [Authorize(Roles = "Manager")]
    public class ManagerController : Controller
    {
        private readonly ITaskService _taskService;
        private readonly IFollowUpService _followUpService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ManagerController(ITaskService taskService, IFollowUpService followUpService,
            UserManager<ApplicationUser> userManager)
        {
            _taskService = taskService;
            _followUpService = followUpService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var dashboard = await _taskService.GetManagerDashboardAsync(user.Id);
            return View(dashboard);
        }

        public async Task<IActionResult> Tasks(string? search, string? employee, PriorityLevel? priority,
            TaskStatusEnum? status, DateTime? dueDate, int page = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var model = await _taskService.GetManagerTasksAsync(user.Id, search, employee, priority, status, dueDate, page);

            // Get employees for filter dropdown
            var employees = await _userManager.GetUsersInRoleAsync("Employee");
            ViewBag.Employees = employees.Select(e => new SelectListItem
            {
                Value = e.Id,
                Text = e.FullName,
                Selected = e.Id == employee
            }).ToList();

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> CreateTask()
        {
            var employees = await _userManager.GetUsersInRoleAsync("Employee");
            ViewBag.Employees = employees.Select(e => new SelectListItem
            {
                Value = e.Id,
                Text = e.FullName
            }).ToList();

            return View(new CreateTaskViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTask(CreateTaskViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var employees = await _userManager.GetUsersInRoleAsync("Employee");
                ViewBag.Employees = employees.Select(e => new SelectListItem
                {
                    Value = e.Id,
                    Text = e.FullName
                }).ToList();
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            await _taskService.CreateTaskAsync(model, user.Id);
            TempData["Success"] = "Task created successfully.";
            return RedirectToAction("Tasks");
        }

        [HttpGet]
        public async Task<IActionResult> EditTask(int id)
        {
            var task = await _taskService.GetTaskByIdAsync(id);
            if (task == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null || task.AssignedById != user.Id)
                return Forbid();

            var employees = await _userManager.GetUsersInRoleAsync("Employee");
            ViewBag.Employees = employees.Select(e => new SelectListItem
            {
                Value = e.Id,
                Text = e.FullName,
                Selected = e.Id == task.AssignedToId
            }).ToList();

            var model = new EditTaskViewModel
            {
                TaskId = task.TaskId,
                Title = task.Title,
                Description = task.Description,
                AssignedToId = task.AssignedToId,
                StartDate = task.StartDate,
                DueDate = task.DueDate,
                ExpectedDuration = task.ExpectedDuration,
                Priority = task.Priority,
                Status = task.Status
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTask(EditTaskViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var employees = await _userManager.GetUsersInRoleAsync("Employee");
                ViewBag.Employees = employees.Select(e => new SelectListItem
                {
                    Value = e.Id,
                    Text = e.FullName
                }).ToList();
                return View(model);
            }

            await _taskService.UpdateTaskAsync(model);
            TempData["Success"] = "Task updated successfully.";
            return RedirectToAction("Tasks");
        }

        public async Task<IActionResult> TaskDetails(int id)
        {
            var details = await _taskService.GetTaskDetailsAsync(id);
            if (details == null) return NotFound();

            return View(details);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTask(int id)
        {
            await _taskService.DeleteTaskAsync(id);
            TempData["Success"] = "Task deleted successfully.";
            return RedirectToAction("Tasks");
        }

        public async Task<IActionResult> FollowUps(int page = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var model = await _followUpService.GetManagerFollowUpsAsync(user.Id, page);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> AddFollowUp(int taskId)
        {
            var task = await _taskService.GetTaskByIdAsync(taskId);
            if (task == null) return NotFound();

            var model = new CreateFollowUpViewModel
            {
                TaskItemId = task.TaskId,
                TaskTitle = task.Title,
                EmployeeName = task.AssignedTo?.FullName ?? "Unknown",
                FollowUpDate = DateTime.Today
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFollowUp(CreateFollowUpViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var task = await _taskService.GetTaskByIdAsync(model.TaskItemId);
                if (task != null)
                {
                    model.TaskTitle = task.Title;
                    model.EmployeeName = task.AssignedTo?.FullName ?? "Unknown";
                }
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            await _followUpService.CreateFollowUpAsync(model, user.Id);
            TempData["Success"] = "Follow-up added successfully.";
            return RedirectToAction("TaskDetails", new { id = model.TaskItemId });
        }

        public async Task<IActionResult> Reports()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var dashboard = await _taskService.GetManagerDashboardAsync(user.Id);
            return View(dashboard);
        }
    }
}
