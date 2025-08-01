using LibManage.Data.Models.Library;
using LibManage.Services.Core.Contracts;
using LibManage.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LibManage.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController(IAdminService adminService, UserManager<User> userManager) : BaseController
    {
        public IActionResult Index()
        {
            return this.RedirectToAction("All");
        }
        [HttpGet]
        public async Task<IActionResult> All(int page = 1, int pageSize = 10)
        {
            var (users, totalCount) = await adminService.GetAllUsersAsync(page, pageSize);
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            ViewBag.CurrentPage = page;
            return View(users);
        }
        [HttpPost]
        public async Task<IActionResult> ChangeRole(Guid userId, string newRole)
        {
            User? user = await userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return NotFound();
            var currentRoles = await userManager.GetRolesAsync(user);
            await userManager.RemoveFromRolesAsync(user, currentRoles);
            await userManager.AddToRoleAsync(user, newRole);
            return RedirectToAction(nameof(All));

        }
        [HttpPost]
        public async Task<IActionResult> DeleteUser(Guid userId)
        {
            User? user = await userManager.FindByIdAsync(userId.ToString());
            if(user == null) 
                return NotFound();
            var roles = await userManager.GetRolesAsync(user);
            if (roles.Contains("Admin"))
            {
                TempData["Error"] = "Cannot delete another Admin!";
                return RedirectToAction(nameof(All));
            }
            user.IsActive = false;
            await userManager.UpdateAsync(user);
            return RedirectToAction(nameof(All));
        }
    }
}
