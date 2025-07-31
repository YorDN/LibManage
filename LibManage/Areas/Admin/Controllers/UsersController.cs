using LibManage.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibManage.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : BaseController
    {
        public IActionResult Index()
        {
            return this.RedirectToAction("All");
        }
        public async Task<IActionResult> All()
        {
            return View();
        }
    }
}
