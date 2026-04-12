using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskFollowUpManagementSystem.Models;
using TaskFollowUpManagementSystem.Models.Enums;
using TaskFollowUpManagementSystem.Services;
using TaskFollowUpManagementSystem.ViewModels;

namespace TaskFollowUpManagementSystem.Controllers
{
    [Authorize(Roles = "Employee")]
    public class EmployeeController : Controller
    {
        private readonly ITaskService _taskService;
        private readonly IFollowUpService _followUpService;
        private readonly UserManager<ApplicationUser> _userManager;

        public EmployeeController(ITaskService taskService, IFollowUpService followUpService,
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

            var dashboard = await _taskService.GetEmployeeDashboardAsync(user.Id);
            return View(dashboard);
        }

        public async Task<IActionResult> MyTasks(string? search, PriorityLevel? priority,
            TaskStatusEnum? status, int page = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var model = await _taskService.GetEmployeeTasksAsync(user.Id, search, priority, status, page);
            return View(model);
        }

        public async Task<IActionResult> TaskDetails(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var details = await _taskService.GetTaskDetailsAsync(id);
            if (details == null) return NotFound();

            // Ensure employee can only view their own tasks
            if (details.AssignedToId != user.Id)
                return Forbid();

            return View(details);
        }

        [HttpGet]
        public async Task<IActionResult> UpdateStatus(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var task = await _taskService.GetTaskByIdAsync(id);
            if (task == null) return NotFound();

            if (task.AssignedToId != user.Id)
                return Forbid();

            var model = new UpdateTaskStatusViewModel
            {
                TaskId = task.TaskId,
                Title = task.Title,
                CurrentStatus = task.Status,
                NewStatus = task.Status,
                ProgressNote = task.ProgressNote
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(UpdateTaskStatusViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            await _taskService.UpdateTaskStatusAsync(model.TaskId, model.NewStatus, model.ProgressNote, user.Id);
            TempData["Success"] = "Task status updated successfully.";
            return RedirectToAction("TaskDetails", new { id = model.TaskId });
        }

        public async Task<IActionResult> FollowUps(int page = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var model = await _followUpService.GetEmployeeFollowUpsAsync(user.Id, page);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> RespondFollowUp(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var followUp = await _followUpService.GetFollowUpByIdAsync(id);
            if (followUp == null) return NotFound();

            if (followUp.EmployeeId != user.Id)
                return Forbid();

            var model = new RespondFollowUpViewModel
            {
                FollowUpId = followUp.FollowUpId,
                TaskTitle = followUp.TaskItem?.Title ?? "Unknown",
                ManagerName = followUp.Manager?.FullName ?? "Unknown",
                FollowUpDate = followUp.FollowUpDate,
                Note = followUp.Note,
                EmployeeResponse = followUp.EmployeeResponse ?? ""
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RespondFollowUp(RespondFollowUpViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            await _followUpService.RespondToFollowUpAsync(model.FollowUpId, model.EmployeeResponse, user.Id);
            TempData["Success"] = "Response submitted successfully.";
            return RedirectToAction("FollowUps");
        }
    }
}
