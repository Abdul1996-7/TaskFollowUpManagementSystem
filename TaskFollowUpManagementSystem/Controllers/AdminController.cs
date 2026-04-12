using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFollowUpManagementSystem.Models;
using TaskFollowUpManagementSystem.Services;
using TaskFollowUpManagementSystem.ViewModels;

namespace TaskFollowUpManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ITaskService _taskService;

        public AdminController(UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager, ITaskService taskService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _taskService = taskService;
        }

        public async Task<IActionResult> Dashboard()
        {
            var dashboard = await _taskService.GetAdminDashboardAsync();

            var users = await _userManager.Users.ToListAsync();
            dashboard.TotalUsers = users.Count;

            int managerCount = 0;
            int employeeCount = 0;
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Manager")) managerCount++;
                if (roles.Contains("Employee")) employeeCount++;
            }
            dashboard.TotalManagers = managerCount;
            dashboard.TotalEmployees = employeeCount;

            return View(dashboard);
        }

        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            var userList = new List<UserListViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userList.Add(new UserListViewModel
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email ?? "",
                    Department = user.Department,
                    Role = roles.FirstOrDefault() ?? "None",
                    CreatedAt = user.CreatedAt
                });
            }

            return View(userList);
        }

        [HttpGet]
        public IActionResult CreateUser()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                Department = model.Department,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, model.Role);
                TempData["Success"] = $"User '{model.FullName}' created successfully.";
                return RedirectToAction("Users");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> AssignRole(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            var model = new AssignRoleViewModel
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? "",
                CurrentRole = roles.FirstOrDefault() ?? "None"
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRole(AssignRoleViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null) return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, model.NewRole);

            TempData["Success"] = $"Role for '{user.FullName}' updated to '{model.NewRole}'.";
            return RedirectToAction("Users");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Id == id)
            {
                TempData["Error"] = "You cannot delete your own account.";
                return RedirectToAction("Users");
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = $"User '{user.FullName}' deleted successfully.";
            }
            else
            {
                TempData["Error"] = "Failed to delete user. The user may have associated tasks.";
            }

            return RedirectToAction("Users");
        }
    }
}
