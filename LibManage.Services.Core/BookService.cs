
using LibManage.Common;
using LibManage.Data;
using LibManage.Data.Models.Library;
using LibManage.Services.Core.Contracts;
using LibManage.ViewModels.Books;
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
                cover = await fileUploadService.UploadFileAsync(model.CoverFile, Subfolders.Covers);
            }

            var author = await context.Authors
            .Include(a => a.WrittenBooks)
            .FirstOrDefaultAsync(a => a.Id == model.AuthorId);
            var publisher = await context.Publishers
             .Include(p => p.Books)
             .FirstOrDefaultAsync(p => p.Id == model.PublisherId);
            if (author == null || publisher == null)
            {
                return false;
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
                    "Digital" => Subfolders.DigitalFiles,
                    "Audio" => Subfolders.AudioFiles,
                    _ => Subfolders.DefaultUploads
                };

                string filePath = await fileUploadService.UploadFileAsync(model.BookFile, bookFileSubFolder);
                book.BookFilePath = filePath;
            }
            await context.Books.AddAsync(book);
            author.WrittenBooks.Add(book);
            publisher.Books.Add(book);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteBookAsync(Guid id)
        {
            Book? book = await context.Books.FirstOrDefaultAsync(b => b.Id == id);
            if (book == null)
                return false;
            var author = await context.Authors
                .Include(a => a.WrittenBooks)
                .FirstOrDefaultAsync(a => a.Id == book.AuthorId);
            var publisher = await context.Publishers
                 .Include(p => p.Books)
                 .FirstOrDefaultAsync(p => p.Id == book.PublisherId);
            if (author == null || publisher == null)
                return false;
            if (!string.IsNullOrEmpty(book.Cover) && !book.Cover.EndsWith("no_cover_available.png"))
            {
                bool coverDeletionResult = await fileUploadService.DeleteFileAsync(book.Cover);
                if (!coverDeletionResult)
                    return false;
            }
            if (!string.IsNullOrEmpty(book.BookFilePath))
            {
                bool fileDeletionResult = await fileUploadService.DeleteFileAsync(book.BookFilePath);
                if (!fileDeletionResult)
                    return false;
            }
            context.Books.Remove(book);
            author.WrittenBooks.Remove(book);
            publisher.Books.Remove(book);
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

        public async Task<List<AllBooksViewModel>?> GetAllBooksFromAuthorAsync(Guid authorId)
        {
            Author? author = await context.Authors
                .FirstOrDefaultAsync(a => a.Id == authorId);

            if( author == null )
                return null;

            List<AllBooksViewModel>? allAuthorBooks = await context.Books
                .Include(d => d.Author)
                .AsNoTracking()
                .Where(b => b.Author.Id == authorId)
                .Select(b => new AllBooksViewModel
                {
                    Id = b.Id,
                    Title = b.Title,
                    AuthorName = b.Author.FullName,
                    BookType = b.Type.ToString(),
                    Cover = b.Cover
                })
                .ToListAsync();

            if (allAuthorBooks is null)
                return allAuthorBooks;

            for (int i = 0; i < allAuthorBooks.Count(); i++)
            {
                double? rating = await ratingService.GetRatingForABookByIdAsync(allAuthorBooks.ToList()[i].Id);
                if (rating != null)
                {
                    allAuthorBooks.ToList()[i].Rating = (int)rating;
                }
            }

            return allAuthorBooks;
        }

        public async Task<BookDetailsViewModel?> GetBookDetailsAsync(Guid id)
        {
            var book = await context.Books
            .Include(b => b.Author)
            .Include(b => b.Publisher)
            .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
                return null;

            var isTaken = await context.Borrows.AnyAsync(r => r.BookId == id && r.DateDue == null);

            return new BookDetailsViewModel
            {
                Book = book,
                IsTaken = isTaken
            };

        }

        public async Task<EditBookInputModel?> GetBookEditModelAsync(Guid id)
        {
            Book? book = await context.Books
                .FirstOrDefaultAsync(b => b.Id == id);
            if (book == null)
                return null;

            EditBookInputModel model = new EditBookInputModel() 
            { 
                Id = id,
                ISBN = book.ISBN,
                Description = book.Description,
                Duration = book.Duration,
                Edition = book.Edition,
                Genre = book.Genre,
                AuthorId = book.AuthorId,
                ExistingCoverPath = book.Cover,
                Language = book.Language,
                ExistingFilePath = book.BookFilePath,
                ReleaseDate = book.ReleaseDate,
                Title = book.Title,
                Type = book.Type.ToString(),
                PublisherId = book.PublisherId,
            };
            model.Authors = await context.Authors
                .Select(p => new AddBookAuthorViewModel
                {
                    Id = p.Id,
                    Name = p.FullName
                })
                .ToListAsync();

            model.Publishers = await context.Publishers
                .Select(p => new AddBookPublisherViewModel
                {
                    Id = p.Id,
                    Name = p.Name
                })
                .ToListAsync();

            return model;
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

        public async Task<DeleteBookViewModel?> GetDeletedBookDetailsAsync(Guid id)
        {
            if (!context.Books.Any(b => b.Id == id))
                return null;
            Book? book = await context.Books
                .FirstOrDefaultAsync(b => b.Id == id);
            if (book == null) return null;

            DeleteBookViewModel model = new DeleteBookViewModel() 
            { 
                Id= book.Id,
                Title = book.Title,
                Cover = book.Cover,
            };

            return model;
        }

        public async Task<bool> UpdateBookAsync(EditBookInputModel model)
        {
            var book = await context.Books.FindAsync(model.Id);
            if (book == null) return false;

            book.Title = model.Title;
            book.ISBN = model.ISBN;
            book.ReleaseDate = model.ReleaseDate;
            book.Edition = model.Edition;
            book.Language = model.Language;
            book.Genre = model.Genre;
            book.Description = model.Description;
            book.Type = Enum.Parse<BookType>(model.Type);
            book.Duration = book.Type == BookType.Audio ? model.Duration : null;
           
            if (book.AuthorId != model.AuthorId)
            {
                Author? originalAuthor = await context.Authors.FirstOrDefaultAsync(a => a.Id == book.AuthorId);
                Author? newAuthor = context.Authors
                    .FirstOrDefault(a => a.Id == model.AuthorId);
                if (newAuthor == null || originalAuthor == null) 
                    return false;

                originalAuthor.WrittenBooks.Remove(book);
                newAuthor.WrittenBooks.Add(book);
                book.AuthorId = model.AuthorId;
                await context.SaveChangesAsync();
            }
            if(book.PublisherId != model.PublisherId)
            {
                Publisher? originalPublisher = await context.Publishers.FirstOrDefaultAsync(p => p.Id == book.PublisherId); ;
                Publisher? newPublisher = context.Publishers
                    .FirstOrDefault(p => p.Id == model.PublisherId);
                if (newPublisher == null || originalPublisher == null)
                    return false;
                originalPublisher.Books.Remove(book);
                newPublisher.Books.Add(book);
                book.PublisherId = model.PublisherId;
                await context.SaveChangesAsync();
            }

            if (model.NewCover != null)
            {
                await fileUploadService.DeleteFileAsync(model.ExistingCoverPath);
                book.Cover = await fileUploadService.UploadFileAsync(model.NewCover, Subfolders.Covers);
            }

            if ((book.Type == Book.BookType.Digital || book.Type == Book.BookType.Audio) && model.NewBookFile != null)
            {
                if (!string.IsNullOrEmpty(model.ExistingFilePath))
                    await fileUploadService.DeleteFileAsync(model.ExistingFilePath);


                string bookFileSubFolder = model.Type switch
                {
                    "Digital" => Subfolders.DigitalFiles,
                    "Audio" => Subfolders.AudioFiles,
                    _ => Subfolders.DefaultUploads
                };
                book.BookFilePath = await fileUploadService.UploadFileAsync(model.NewBookFile, bookFileSubFolder);
            }

            await context.SaveChangesAsync();
            return true;
        }

    }
}
