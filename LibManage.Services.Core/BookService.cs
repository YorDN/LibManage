
using LibManage.Common;
using LibManage.Data;
using LibManage.Data.Models.DTOs;
using LibManage.Data.Models.Library;
using LibManage.Services.Core.Contracts;
using LibManage.ViewModels.Audio;
using LibManage.ViewModels.Books;
using LibManage.ViewModels.Rating;
using Microsoft.EntityFrameworkCore;

using static LibManage.Data.Models.Library.Book;

namespace LibManage.Services.Core
{
    public class BookService(ApplicationDbContext context,
        IRatingService ratingService, 
        IFileUploadService fileUploadService,
        IBorrowService borrowService) : IBookService
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

        public async Task<PaginatedBooksViewModel> GetAllBooksAsync(BookFilterOptions options, Guid? userId = null, int page = 1, int pageSize = 10)
        {
            var query = context.Books
                .Include(b => b.Author)
                .Include(b => b.Borrows)
                .Include(b => b.Reviews)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(options.SearchTerm))
                query = query.Where(b =>
                    b.Title.Contains(options.SearchTerm) ||
                    b.Author.FullName.Contains(options.SearchTerm));

            if (options.MinimumRating.HasValue)
                query = query.Where(b => b.Reviews.Any() &&
                                         b.Reviews.Average(r => r.Rating) >= options.MinimumRating.Value);

            if (!string.IsNullOrEmpty(options.BookType) &&
                Enum.TryParse<BookType>(options.BookType, out var parsedType))
            {
                query = query.Where(b => b.Type == parsedType);
            }

            if (options.IsTaken.HasValue)
            {
                query = query.Where(b =>
                    b.Type == BookType.Physical
                        ? b.Borrows.Any(br => !br.Returned) == options.IsTaken.Value
                        : (userId.HasValue && b.Borrows.Any(br => br.UserId == userId && !br.Returned) == options.IsTaken.Value));
            }

            query = options.SortBy switch
            {
                "Title" => options.SortDescending ? query.OrderByDescending(b => b.Title) : query.OrderBy(b => b.Title),
                "Rating" => options.SortDescending
                    ? query.OrderByDescending(b => b.Reviews.Any() ? b.Reviews.Average(r => r.Rating) : 0)
                    : query.OrderBy(b => b.Reviews.Any() ? b.Reviews.Average(r => r.Rating) : 0),
                _ => query.OrderBy(b => b.Title)
            };

            int totalBooks = await query.CountAsync();

            var books = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new AllBooksViewModel
                {
                    Id = b.Id,
                    Title = b.Title,
                    AuthorName = b.Author.FullName,
                    BookType = b.Type.ToString(),
                    Cover = b.Cover,
                    Rating = b.Reviews.Any(r => r.IsApproved) ? (int)b.Reviews.Average(r => r.Rating) : 0,
                    IsTaken = b.Type == BookType.Physical
                        ? b.Borrows.Any(br => !br.Returned)
                        : (userId.HasValue && b.Borrows.Any(br => br.UserId == userId && !br.Returned))
                })
                .ToListAsync();

            return new PaginatedBooksViewModel
            {
                Books = books,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalBooks / (double)pageSize),
                FilterOptions = options
            };

        }


        public async Task<List<AllBooksViewModel>?> GetAllBooksFromAuthorAsync(Guid authorId, Guid? userId = null)
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
                    Cover = b.Cover,
                    IsTaken = b.Type == BookType.Physical
                    ? b.Borrows.Any(br => !br.Returned)
                    : (userId.HasValue && b.Borrows.Any(br => br.UserId == userId && !br.Returned))
                })
                .ToListAsync();
            if (allAuthorBooks is null)
                return allAuthorBooks;
            for (int i = 0; i < allAuthorBooks.Count(); i++)
            {
                double? rating = await ratingService.GetRatingForABookByIdAsync(allAuthorBooks[i].Id);
                if (rating != null)
                    allAuthorBooks[i].Rating = (int)rating;
            }

            return allAuthorBooks;
        }

        public async Task<List<AllBooksViewModel>?> GetAllBooksFromPublisherAsync(Guid publisherId, Guid? userId = null)
        {
            Publisher? publisher = await context.Publishers
                .FirstOrDefaultAsync(p => p.Id == publisherId);
            if (publisher == null)
                return null;
            List<AllBooksViewModel>? publishedBooks = await context.Books
                .Include(p => p.Publisher)
                .AsNoTracking()
                .Where(b => b.Publisher.Id == publisherId)
                .Select(b => new AllBooksViewModel()
                {
                    Id = b.Id,
                    Title = b.Title,
                    AuthorName = b.Author.FullName,
                    BookType = b.Type.ToString(),
                    Cover = b.Cover,
                    IsTaken = b.Type == BookType.Physical
                    ? b.Borrows.Any(br => !br.Returned)
                    : (userId.HasValue && b.Borrows.Any(br => br.UserId == userId && !br.Returned))
                }).ToListAsync();
            if (publishedBooks == null)
                return null;
            for (int i = 0; i < publishedBooks.Count; i++)
            {
                double? rating = await ratingService.GetRatingForABookByIdAsync(publishedBooks[i].Id);
                if (rating != null)
                    publishedBooks[i].Rating = (int) rating;
            }

            return publishedBooks;
        }

        public async Task<AudioBookPlayerViewModel?> GetAudioBookPlayerViewModelAsync(Guid id)
        {
            Book? book = await context.Books
                .Include(b => b.Author)
                .FirstOrDefaultAsync(b => b.Id == id);
            
            if (book == null) 
                return null;
            if (book.Type != BookType.Audio) 
                return null;

            AudioBookPlayerViewModel model = new AudioBookPlayerViewModel() 
            { 
                Id = book.Id,
                Title = book.Title,
                AuthorId = book.AuthorId,
                AuthorName = book.Author.FullName,
                Language = book.Language,
                Cover = book.Cover,
                Description = book.Description,
                Duration = book.Duration,
                UploadDate = book.UploadDate,
                FilePath = book.BookFilePath
            };

            return model;
        }

        public Task<Book?> GetBookByIdAsync(Guid id)
        {
            return context.Books.Include(b => b.Author).FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<BookDetailsViewModel?> GetBookDetailsAsync(Guid bookId, Guid? userId = null)
        {
            Book? book = await context.Books
            .Include(b => b.Author)
            .Include(b => b.Publisher)
            .Include(b => b.Reviews)
            .FirstOrDefaultAsync(b => b.Id == bookId);

            if (book == null)
                return null;
            bool isTaken = false;
            if (book.Type != BookType.Physical)
            {
                if (userId != null)
                {
                    isTaken = await borrowService.HasActiveBorrowsAsync(userId, bookId);
                }
            }
            else
            {
                isTaken = context.Borrows.Any(b => b.BookId == bookId && b.Returned == false);
            }

            bool isTakenByUser = false;
            bool canReview = false;
            if (userId != null)
            {
                isTakenByUser = await borrowService.HasActiveBorrowsAsync(userId, bookId);
                canReview = !book.Reviews.Any(r => r.UserId == userId);
            }

            return new BookDetailsViewModel
            {
                Book = book,
                IsTaken = isTaken,
                IsTakenByUser = isTakenByUser,
                AverageRating = book.Reviews.Any(r => r.IsApproved) ? book.Reviews.Average(r => r.Rating) : 0,
                CanReview = canReview
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
