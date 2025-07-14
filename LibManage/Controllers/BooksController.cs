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
            AddBookInputModel model = await bookService.GetBookInputModelAsync();
            return View(model);
        }
        [Authorize(Roles = "Admin, Manager")]
        [HttpPost]
        public async Task<IActionResult> Add(AddBookInputModel model)
        {
            if (model.Type == "Digital")
            {
                if (model.BookFile == null)
                    ModelState.AddModelError(nameof(model.BookFile), "Please upload an EPUB file for digital books.");
                else if (!IsEpub(model.BookFile))
                    ModelState.AddModelError(nameof(model.BookFile), "Only .epub files are allowed.");
            }

            if (model.Type == "Audio")
            {
                if (model.BookFile == null)
                    ModelState.AddModelError(nameof(model.BookFile), "Please upload an MP3 file for audio books.");
                else if (!IsMp3(model.BookFile))
                    ModelState.AddModelError(nameof(model.BookFile), "Only .mp3 files are allowed.");

                if (model.Duration == null)
                    ModelState.AddModelError(nameof(model.Duration), "Please provide the duration of the audio book.");
            }

            if (!ModelState.IsValid)
                return this.RedirectToAction(nameof(Add));

            bool result = await bookService.CreateBookAsync(model);
            if (!result)
                return this.RedirectToAction(nameof(Add));
            return this.RedirectToAction(nameof(All));
        }
        private bool IsEpub(IFormFile file)
        {
            var allowedExtensions = new[] { ".epub" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return allowedExtensions.Contains(extension);
        }

        private bool IsMp3(IFormFile file)
        {
            var allowedExtensions = new[] { ".mp3" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return allowedExtensions.Contains(extension);
        }

    }
}
