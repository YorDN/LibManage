using LibManage.Services.Core.Contracts;
using LibManage.ViewModels.Authors;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibManage.Web.Controllers
{
    public class AuthorsController(IAuthorService authorService) : BaseController
    {
        [HttpGet]
        public IActionResult Index()
        {
            return this.RedirectToAction(nameof(All));
        }
        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Add(AddAuthorInputModel model)
        {
            if (!ModelState.IsValid)
            {
                return this.RedirectToAction(nameof(Add));
            }
            bool result = await authorService.CreateAuthorAsync(model);
            if (!result)
            {
                return this.RedirectToAction(nameof(Add));
            }
            return this.RedirectToAction(nameof(All)); ;
        }
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> All()
        {
            IEnumerable<AllAuthorsViewModel> model = await authorService
                .GetAllAuthorsAsync();

            return View(model);
        }
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            AuthorDetailsViewModel? model = await authorService
                .GetAuthorDetailsAsync(id);
            if (model == null) 
                return this.NotFound();

            return View(model);
        }
        [Authorize(Roles = "Admin, Manager")]
        [HttpGet]
        public async Task<IActionResult> Delete(Guid Id)
        {
            DeleteAuthorViewModel? model = await authorService
                .GetAuthorDeleteInfoAsync(Id);

            if (model == null)
                return this.NotFound();

            return View(model);
        }
        [Authorize(Roles = "Admin, Manager")]
        [HttpPost]
        public async Task<IActionResult> Delete(DeleteAuthorViewModel model)
        {
            if (!model.ConfirmDeleteBooks)
            {
                TempData["Error"] = "Author could not be deleted. They may have associated books.";
                return this.RedirectToAction(nameof(Details), new { id = model.Id });
            }
            bool result = await authorService.DeleteAuthorAsync(model.Id);
            if (!result)
                return this.NotFound();
            return RedirectToAction("All");
        }
        [Authorize(Roles = "Admin, Manager")]
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            EditAuthorInputModel? model = await authorService.GetAuthorEditInfoAsync(id);
            if (model == null)
                return this.NotFound();

            return View(model);
        }
        [Authorize(Roles = "Admin, Manager")]
        [HttpPost]
        public async Task<IActionResult> Edit(EditAuthorInputModel model)
        {
            if(!ModelState.IsValid)
                return this.RedirectToAction(nameof(Edit));

            bool result = await authorService
                .EditAuthorAsync(model);
            if (!result)
                return this.RedirectToAction(nameof(Edit), new {id = model.Id});

            return this.RedirectToAction(nameof(Details), new {id = model.Id});
        }
    }
}
