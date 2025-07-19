using LibManage.Common;
using LibManage.Common.Enumerations;
using LibManage.Data;
using LibManage.Data.Models.Library;
using LibManage.Services.Core.Contracts;
using LibManage.ViewModels.Authors;
using LibManage.ViewModels.Books;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata.Ecma335;

namespace LibManage.Services.Core
{
    public class AuthorService(IFileUploadService fileUploadService, 
        ApplicationDbContext context, IBookService bookService) : IAuthorService
    {
        public async Task<bool> CreateAuthorAsync(AddAuthorInputModel model)
        {
            if (model is null)
                return false;
            if (context.Authors.Any(a => a.FullName == model.FullName))
                return false;

            string pfp;


            if (model.PhotoFile is null || model.PhotoFile.Length == 0)
            {
                pfp = "/uploads/pfps/author/DefaultAuthor.png";
            }
            else
            {
                pfp = await fileUploadService.UploadFileAsync(model.PhotoFile, "pfps/author");
            }

            Author author = new Author()
            {
                FullName = model.FullName,
                Photo = pfp,
                Biography = model.Biography,
                DateOfBirth = model.DateOfBirth,
                DateOfDeath = model.DateOfDeath
            };

            await context.Authors.AddAsync(author);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAuthorAsync(Guid id)
        {
            Author? author = await context.Authors
                .Include(a => a.WrittenBooks)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (author == null)
                return false;

            using var transaction = await context.Database.BeginTransactionAsync();

            List<Book> books = author.WrittenBooks.ToList();
            foreach (var book in books)
            {
                bool success = await bookService.DeleteBookAsync(book.Id);
                if (!success)
                {
                    await transaction.RollbackAsync();
                    return false;
                }
            }

            if (!string.IsNullOrEmpty(author.Photo) && !author.Photo.EndsWith("DefaultAuthor.png"))
            {
                bool photoDeleted = await fileUploadService.DeleteFileAsync(author.Photo);
                if (!photoDeleted)
                {
                    await transaction.RollbackAsync();
                    return false;
                }
            }

            context.Authors.Remove(author);
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }

        public async Task<bool> EditAuthorAsync(EditAuthorInputModel model)
        {
            Author? author = await context.Authors
                .FirstOrDefaultAsync(a => a.Id ==  model.Id);

            if (author == null) return false;

            author.FullName = model.FullName;
            author.DateOfDeath = model.DateOfDeath;
            author.DateOfBirth = model.DateOfBirth;
            author.Biography = model.Biography;

            if (model.NewPhoto != null)
            {
                bool deletionResult = await fileUploadService.DeleteFileAsync(author.Photo);
                if (!deletionResult) 
                    return false;
                author.Photo = await fileUploadService
                    .UploadFileAsync(model.NewPhoto, Subfolders.AuthorProfilePictures);
            }

            await context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<AllAuthorsViewModel>> GetAllAuthorsAsync()
        {
            IEnumerable<AllAuthorsViewModel> authors = await context.Authors
                .OrderBy(a => a.WrittenBooks.Count())
                .ThenBy(a => a.FullName)
                .Select(a => new AllAuthorsViewModel()
                {
                    Id = a.Id,
                    Name = a.FullName,
                    Photo = a.Photo,
                })
                .ToListAsync();
            return authors;
        }

        public async Task<DeleteAuthorViewModel?> GetAuthorDeleteInfoAsync(Guid id)
        {
            Author? author = await context.Authors
                .Include(a => a.WrittenBooks)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (author == null) 
                return null;

            DeleteAuthorViewModel deleteAuthorViewModel = new DeleteAuthorViewModel()
            {
                Id = author.Id,
                Name = author.FullName,
                Photo = author.Photo,
                BooksCount = author.WrittenBooks.Count(),
            };

            return deleteAuthorViewModel;
        }
        public async Task<AuthorDetailsViewModel?> GetAuthorDetailsAsync(Guid id)
        {
            Author? author = await context.Authors
                .FirstOrDefaultAsync(a => a.Id == id);

            if (author == null)
                return null;
            List<AllBooksViewModel>? books = await bookService.GetAllBooksFromAuthorAsync(id);
            if (books == null)
                return null;
            AuthorDetailsViewModel model = new AuthorDetailsViewModel()
            {
                Id = author.Id,
                Name = author.FullName,
                Photo = author.Photo,
                Biography = author.Biography,
                DateOfBirth = author.DateOfBirth,
                DateOfDeath = author.DateOfDeath,
                WrittenBooks = books
            };
            return model;
        }

        public async Task<EditAuthorInputModel?> GetAuthorEditInfoAsync(Guid id)
        {
            Author? author = await context.Authors
                .FirstOrDefaultAsync(a => a.Id == id);

            if(author == null) 
                return null;

            EditAuthorInputModel model = new EditAuthorInputModel()
            {
                Id = author.Id,
                Biography = author.Biography,
                DateOfBirth = author.DateOfBirth,
                DateOfDeath = author.DateOfDeath,
                FullName = author.FullName,
                ExistingPhotoPath = author.Photo
            };

            return model;
        }
    }
}
