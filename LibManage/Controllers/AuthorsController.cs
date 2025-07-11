using LibManage.Services.Core.Contracts;
using LibManage.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace LibManage.Web.Controllers
{
    public class AuthorsController(IAuthorService authorService) : BaseController
    {
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
            return this.RedirectToAction(nameof(Index), "Home"); ;
        }
    }
}
