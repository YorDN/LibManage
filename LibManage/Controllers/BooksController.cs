using LibManage.Data.Models.Library;
using LibManage.Services.Core;
using LibManage.Services.Core.Contracts;
using LibManage.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LibManage.Web.Controllers
{
    public class BooksController(IBookService bookService) : BaseController
    {
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> All()
        {
            IEnumerable<AllBooksViewModel>? books = await bookService
                .GetAllBooksAsync();
            return View(books);
        }
        [Authorize(Roles = "Admin, Manager")]
        [HttpGet]
        public async Task<IActionResult> Add()
        {
            ViewBag.Types = Enum.GetNames(typeof(Book.BookType));
            return View();
        }
    }
}
