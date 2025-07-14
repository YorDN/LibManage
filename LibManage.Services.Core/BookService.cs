
using LibManage.Data;
using LibManage.Data.Models.Library;
using LibManage.Services.Core.Contracts;
using LibManage.ViewModels;
using Microsoft.EntityFrameworkCore;
using static LibManage.Data.Models.Library.Book;

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


            if (model.CoverFile == null || model.CoverFile.Length == 0)
            {
                cover = "/uploads/covers/no_cover_available.png";
            }
            else
            {
                cover = await fileUploadService.UploadFileAsync(model.CoverFile, "covers");
            }
            if (!Enum.TryParse<BookType>(model.Type, true, out BookType type))
            {
                return false;
            }
            Book book = new Book()
            {
                Id = Guid.NewGuid(),
                Title = model.Title,
                ISBN = model.ISBN,
                Language = model.Language,
                Type = type,
                ReleaseDate = model.ReleaseDate,
                Edition = model.Edition,
                Genre = model.Genre,
                Description = model.Description,
                Duration = model.Duration,
                AuthorId = model.AuthorId,
                PublisherId = model.PublisherId,
                UploadDate = DateTime.UtcNow,
                Cover = cover,

            };
            if (model.BookFile != null)
            {
                string bookFileSubFolder = model.Type switch
                {
                    "Digital" => "files/digital",
                    "Audio" => "files/audio",
                    _ => "files"
                };

                string filePath = await fileUploadService.UploadFileAsync(model.BookFile, bookFileSubFolder);
                book.BookFilePath = filePath;
            }
            await context.Books.AddAsync(book);
            await context.SaveChangesAsync();
            return true;
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
