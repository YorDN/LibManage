
using LibManage.Data;
using LibManage.Data.Models.Library;
using LibManage.Services.Core.Contracts;
using LibManage.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace LibManage.Services.Core
{
    public class BookService(ApplicationDbContext context,
        IRatingService ratingService, 
        IFileUploadService fileUploadService) : IBookService
    {
        public async Task<bool> CreateBookAsync(AddBookInputModel model)
        {
           if(context.Books.Any(b => b.ISBN == model.ISBN))
                return false;
            string cover;


            if (model.CoverFile is null || model.CoverFile.Length == 0)
            {
                cover = "/uploads/covers/no_cover_available.png";
            }
            else
            {
                cover = await fileUploadService.UploadFileAsync(model.CoverFile, "covers");
            }
            Book book = new Book()
           {
               ISBN = model.ISBN,
               Title = model.Title,
               AuthorId = model.AuthorId,
               PublisherId = model.PublisherId,
               Description = model.Description,
               Cover = cover,
               Duration = model.Duration,
               Genre = model.Genre,
               Language = model.Language,
               ReleaseDate = model.ReleaseDate,
               UploadDate = DateTime.Now,
               Edition = model.Edition,
               Files = new BookFile[]
               {

               }
           }
        }

        public async Task<IEnumerable<AllBooksViewModel>?> GetAllBooksAsync()
        {
            IEnumerable<AllBooksViewModel>? allBooksViewModel = await context.Books
                .Include(d => d.Author)
                .AsNoTracking()
                .Select(b => new AllBooksViewModel
                {
                    Id = b.Id,
                    Title = b.Title,
                    AuthorName = b.Author.FullName,
                    BookType = b.Type.ToString(),
                    Cover = b.Cover
                })
                .ToListAsync();
            if (allBooksViewModel is  null ) 
                return allBooksViewModel;

            for ( int i = 0; i < allBooksViewModel.Count(); i++ )
            {
                double? rating = await ratingService.GetRatingForABookByIdAsync(allBooksViewModel.ToList()[i].Id);
                if ( rating != null)
                {
                    allBooksViewModel.ToList()[i].Rating = (int)rating;
                }
            }

            return allBooksViewModel;
        }

        public async Task<AddBookInputModel> GetBookInputModelAsync()
        {
            AddBookInputModel model = new AddBookInputModel();
            model.Authors = await context.Authors
                .Select(a => new AddBookAuthorViewModel
                {
                    Id = a.Id,
                    Name = a.FullName,
                })
                .ToListAsync();
            model.Publishers = await context.Publishers
                .Select(p => new AddBookPublisherViewModel
                {
                    Id = p.Id,
                    Name= p.Name
                })
                .ToListAsync();
            return model;
        }
    }
}
