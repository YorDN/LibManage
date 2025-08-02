using LibManage.Data.Models.Library;
using LibManage.Services.Core;
using LibManage.Services.Core.Contracts;
using LibManage.ViewModels.Authors;
using LibManage.ViewModels.Publishers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LibManage.Web.Controllers
{
    public class PublishersController (ICountryService countryService, IPublisherService publisherService, UserManager<User> userManager) : BaseController
    {
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return this.RedirectToAction(nameof(All));
        }
        [Authorize(Roles = "Admin, Manager")]
        [HttpGet]
        public async Task<IActionResult> Add()
        {
            var countries = await countryService
                .GetCountriesAsync();
            var selectItems = countries.Select(c => new SelectListItem
            {
                Text = c.name.common,
                Value = c.cca2.ToUpper()
            }).ToList();

            var flagsDict = countries.ToDictionary(
                c => c.cca2.ToUpper(),
                c => c.flags.png
            );

            ViewBag.Countries = selectItems;
            ViewBag.CountryFlags = flagsDict;

            return View();

        }
        [Authorize(Roles = "Admin, Manager")]
        [HttpPost]
        public async Task<IActionResult> Add(AddPublisherInputModel model)
        {
            if (!ModelState.IsValid)
                return this.RedirectToAction(nameof(Add));

            bool result = await publisherService.AddPublisherAsync(model);
            if (!result) 
                return this.RedirectToAction(nameof(Add));

            return this.RedirectToAction(nameof(All));
        }
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> All()
        {
            IEnumerable<AllPublishersViewModel> model = await publisherService
                .GetAllPublishersAsync();
            return View(model);
        } 
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            User? user = await userManager.GetUserAsync(User);
            PublisherDetailsViewModel? model = null;
            if (user != null)
            {
                model = await publisherService.GetPublisherDetailsAsync(id, user.Id);
            }
            else
            {
                model = await publisherService.GetPublisherDetailsAsync(id);
            }
            if (model == null)
                return this.NotFound();

            return View(model);
        }
        [Authorize(Roles = "Admin, Manager")]
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            EditPublisherInputModel? model = await publisherService.GetPublisherEditInfoAsync(id);
            if (model == null)
                return this.NotFound();
            var countries = await countryService
                .GetCountriesAsync();
            var selectItems = countries.Select(c => new SelectListItem
            {
                Text = c.name.common,
                Value = c.cca2.ToUpper()
            }).ToList();

            var flagsDict = countries.ToDictionary(
                c => c.cca2.ToUpper(),
                c => c.flags.png
            );

            ViewBag.Countries = selectItems;
            ViewBag.CountryFlags = flagsDict;
             
            return View(model);
        }
        [Authorize(Roles = "Admin, Manager")]
        [HttpPost]
        public async Task<IActionResult> Edit(EditPublisherInputModel model)
        {
            if (!ModelState.IsValid)
                return this.RedirectToAction(nameof(Edit), new { id = model.Id });
            bool result = await publisherService.EditPublisherAsync(model);
            if (!result)
                return this.RedirectToAction(nameof(Edit), new { id = model.Id });
            return this.RedirectToAction(nameof(Details), new { id = model.Id });
        }
        [Authorize(Roles = "Admin, Manager")]
        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            DeletePublisherViewModel? model = await publisherService.GetDeletePublisherInfoAsync(id);
            if (model == null)
                return this.NotFound();
            return View(model);
        }
        [Authorize(Roles = "Admin, Manager")]
        [HttpPost]
        public async Task<IActionResult> Delete(DeletePublisherViewModel model)
        {
            if (!model.ConfirmDeleteBooks)
            {
                TempData["Error"] = "Publisher could not be deleted. They may have associated books.";
                return this.RedirectToAction(nameof(Details), new { id = model.Id });
            }
            bool result = await publisherService.DeletePublisherAsync(model.Id);
            if (!result)
                return this.NotFound();
            return RedirectToAction("All");
        }
    }
}
