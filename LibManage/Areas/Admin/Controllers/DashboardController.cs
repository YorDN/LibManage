using LibManage.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibManage.Web.Areas.Admin.Controllers
{
    public class DashboardController : Controller
    {
        [Area("Admin")]
        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            // Dummy data
            AdminDashboardViewModel model = new AdminDashboardViewModel
            {
                AudioBooks = 10,
                DigitalBooks = 2,
                PhysicalBooks = 3,
                TotalAuthors = 20,
                TotalBooks = 2,
                TotalUsers = 200,
                TotalPublishers = 10
            };
            return View(model);
        }

    }
}
