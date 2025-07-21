using LibManage.Data.Models.Library;
using LibManage.Services.Core.Contracts;
using LibManage.ViewModels.Borrows;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LibManage.Web.Controllers
{
    public class BorrowsController(UserManager<User> userManager, 
        IBorrowService borrowService) : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Rent(Guid Id)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();
            bool success = await borrowService.RentBookAsync(user.Id, Id);
            if (!success)
                TempData["Error"] = "You already have this book rented.";

            return RedirectToAction("All", "Borrows");
        }
        [HttpGet]
        [Route("[Controller]/User/All")]
        public async Task<IActionResult> All()
        {
            User? user = await userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();
            IEnumerable<BorrowedBookViewModel>? model = await borrowService.GetUsersBorrowedBooksAsync(user.Id);
            if (model == null)
                return Unauthorized();
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Return(Guid Id)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();
            bool success = await borrowService.ReturnBookAsync(user.Id, Id);
            if (!success)
                TempData["Error"] = "You already have this book returned.";

            return RedirectToAction("All", "Borrows");
        }
    }
}
