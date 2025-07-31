using LibManage.Services.Core.Contracts;
using LibManage.ViewModels.Admin;
using LibManage.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibManage.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController(IAdminService adminService) : BaseController
    {
        public async Task<IActionResult> Index()
        {
            AdminDashboardViewModel model = await adminService.GetAdminDashboardDetailsAsync();
            return View(model);
        }

    }
}
