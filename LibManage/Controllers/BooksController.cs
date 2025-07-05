using LibManage.Data.Models.Library;
using LibManage.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace LibManage.Web.Controllers
{
    public class BooksController : Controller
    {
        public IActionResult All()
        {
            // Just for testing perposes untill the services are created
            IEnumerable<AllBooksViewModel> books = new List<AllBooksViewModel>(){
                new AllBooksViewModel()
                {
                    Title = "To kill a mockingbird",
                    AuthorName = "Herper Lee",
                    BookType = "physical",
                    Rating = 5,
                    IsTaken = false,
                    Cover = "https://images-na.ssl-images-amazon.com/images/S/compressed.photo.goodreads.com/books/1612238791i/56916837.jpg",
                    Id = Guid.NewGuid()
                },
                new AllBooksViewModel()
                {
                    Title = "Programming C# 10",
                    AuthorName = "Ian Griffiths",
                    BookType = "physical",
                    Rating = 4,
                    IsTaken = true,
                    Cover = "https://libris.to/media/jacket/38834523o.jpg",
                    Id = Guid.NewGuid()
                },
            }; 

            return View(books);
        }
    }
}
