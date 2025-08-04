using LibManage.Services.Core.Contracts;
using LibManage.ViewModels.Manager;
using LibManage.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LibManage.Web.Areas.Manager.Controllers
{
    [Area("Manager")]
    [Authorize(Roles = "Manager")]
    public class ModerationController(IRatingService ratingService) : BaseController
    {
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            List<UnapprovedReviewViewModel> reviews = await ratingService.GetUnapprovedReviewsAsync();
            return View(reviews);
        }
        [HttpPost]
        public async Task<IActionResult> Approve(Guid id)
        {
            await ratingService.ApproveReviewAsync(id);
            return this.RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            await ratingService.DeleteReviewAsync(id);
            return this.RedirectToAction(nameof(Index));
        }
    }
}
