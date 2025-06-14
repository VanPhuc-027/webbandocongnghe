using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using _2280613193_webdocongnghe.Models;

namespace _2280613193_webdocongnghe.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UserManagementController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserManagementController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.ToList();
            var filteredUsers = new List<ApplicationUser>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (!roles.Contains("Admin"))
                {
                    filteredUsers.Add(user);
                }
            }

            return View(filteredUsers);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleLock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.Now)
            {
                // Mở khóa
                user.LockoutEnd = DateTimeOffset.Now;
            }
            else
            {
                // Khóa trong 100 năm
                user.LockoutEnd = DateTimeOffset.Now.AddYears(100);
            }

            await _userManager.UpdateAsync(user);
            return RedirectToAction("Index");
        }
    }
}
