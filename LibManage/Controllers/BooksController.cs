
using LibManage.Data.Models.DTOs;
using LibManage.Data.Models.Library;
using LibManage.Services.Core;
using LibManage.Services.Core.Contracts;
using LibManage.ViewModels.Audio;
using LibManage.ViewModels.Books;
using LibManage.ViewModels.Rating;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LibManage.Web.Controllers
{
    public class BooksController(IBookService bookService, IEpubReaderService epubReaderService, UserManager<User> userManager, IRatingService ratingService) : BaseController
    {
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> All([FromQuery] BookFilterOptions options, int page = 1)
        {
            User? user = await userManager.GetUserAsync(User);
            PaginatedBooksViewModel model;
            if (user != null)
            {
                model = await bookService.GetAllBooksAsync(options, user.Id, page);
            }
            else
            {
                model = await bookService.GetAllBooksAsync(options, null, page);
            }

            return View(model);
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
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Details(Guid id, int page = 1)
        {
            User? user = await userManager.GetUserAsync(User);
            BookDetailsViewModel? model;
            if (user != null)
            {
                model = await bookService.GetBookDetailsAsync(id, user.Id);
            }
            else
            {
                Guid? bId = null;
                model = await bookService.GetBookDetailsAsync(id, bId);
            }
            if (model == null)
                return NotFound();

            model.Reviews = await ratingService.GetReviewsForABookAsync(id, user == null ? Guid.NewGuid() : user.Id);
            model.TotalReviewCount = await ratingService.GetTotalReviewCountAsync(id);
            model.CurrentReviewPage = page;

            return View(model);
        }
        [Authorize(Roles = "Admin, Manager")]
        [HttpGet]
        public async Task<IActionResult> DeleteConfirmation(Guid id)
        {
            DeleteBookViewModel? model = await bookService.GetDeletedBookDetailsAsync(id);
            if (model == null)
                return NotFound();

            return View(model);
        }
        [Authorize(Roles = "Admin, Manager")]
        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await bookService.DeleteBookAsync(id);

            if (!result)
                return NotFound();

            return RedirectToAction("All");
        }
        [Authorize(Roles = "Admin, Manager")]
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var model = await bookService.GetBookEditModelAsync(id);
            if (model == null)
                return NotFound();

            ViewBag.Types = Enum.GetNames(typeof(Book.BookType));
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(EditBookInputModel model)
        {
            if (!ModelState.IsValid)
                return this.RedirectToAction(nameof(Edit), new { id = model.Id });
            bool result = await bookService.UpdateBookAsync(model);

            if (!result)
                return this.RedirectToAction(nameof(Edit), new { id = model.Id });
            return this.RedirectToAction(nameof(Details), new { id = model.Id });
        }
        [HttpGet]
        public async Task<IActionResult> Read(Guid id, int? chapterIndex = null)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return this.Unauthorized();
            var model = await epubReaderService.LoadChapterAsync(id, user.Id, chapterIndex);
            if (model == null)
            {
                TempData["Error"] = "Either the book is not available or your borrow has expired.";
                return RedirectToAction("All", "Borrows");
            }

            return View(model);

        }
        [HttpGet]
        public async Task<IActionResult> Listen(Guid id)
        {
            AudioBookPlayerViewModel? model = await bookService.GetAudioBookPlayerViewModelAsync(id);

            if (model == null)
                return NotFound();

            return View(model);
        }
        [HttpPost]
        [Route("[Controller]/Reviews/Add")]
        public async Task<IActionResult> SubmitReview(Guid id, ReviewInputModel input)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Details", new { id });

            User? user = await userManager.GetUserAsync(User);
            if (user is null)
                return Forbid();

            bool result = await ratingService.AddReviewAsync(input, id, user.Id);
            if (!result)
                return NotFound();

            return RedirectToAction("Details", new { id });
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
