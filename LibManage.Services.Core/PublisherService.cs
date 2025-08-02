using LibManage.Common;
using LibManage.Data;
using LibManage.Data.Models.Library;
using LibManage.Services.Core.Contracts;
using LibManage.ViewModels.Books;
using LibManage.ViewModels.Publishers;

using Microsoft.EntityFrameworkCore;

namespace LibManage.Services.Core
{
    public class PublisherService(ApplicationDbContext context, 
        IFileUploadService fileUploadService, 
        IBookService bookService) : IPublisherService
    {
        public async Task<bool> AddPublisherAsync(AddPublisherInputModel model)
        {
            if(context.Publishers.Any(p => p.Name == model.Name ))
                return false;
            
            string pfp;
            if (model.LogoFile is null || model.LogoFile.Length == 0)
            {
                pfp = "/uploads/pfps/publisher/DefaultPublisher.png";
            }
            else
            {
                pfp = await fileUploadService.UploadFileAsync(model.LogoFile, Subfolders.PublisherProfilePictures);
            }

            Publisher publisher = new Publisher()
            {
                Name = model.Name,
                Description = model.Description,
                LogoUrl = pfp,
                Country = model.Country,
                Website = model.Website
            };

            await context.Publishers.AddAsync(publisher);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeletePublisherAsync(Guid id)
        {
            Publisher? publisher = await context.Publishers
                .FirstOrDefaultAsync(p => p.Id == id);
            if(publisher == null)
                return false;

            using var transaction = await context.Database.BeginTransactionAsync();

            List<Book> books = publisher.Books.ToList();
            foreach (var book in books)
            {
                bool success = await bookService.DeleteBookAsync(book.Id);
                if (!success)
                {
                    await transaction.RollbackAsync();
                    return false;
                }
            }
            if (!string.IsNullOrEmpty(publisher.LogoUrl) && !publisher.LogoUrl.EndsWith("DefaultPublisher.png"))
            {
                bool photoDeleted = await fileUploadService.DeleteFileAsync(publisher.LogoUrl);
                if (!photoDeleted)
                {
                    await transaction.RollbackAsync();
                    return false;
                }
            }

            context.Publishers.Remove(publisher);
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }

        public async Task<bool> EditPublisherAsync(EditPublisherInputModel model)
        {
           Publisher? publisher = await context.Publishers
                .FirstOrDefaultAsync(p => p.Id == model.Id);
            if (publisher == null)
                return false;
            publisher.Name = model.Name;
            publisher.Description = model.Description;
            publisher.Country = model.Country;
            publisher.Website = model.Website;
            if (model.NewLogo != null)
            {
                bool deletionResult = await fileUploadService.DeleteFileAsync(publisher.LogoUrl);
                if (!deletionResult)
                    return false;
                publisher.LogoUrl = await fileUploadService
                    .UploadFileAsync(model.NewLogo, Subfolders.AuthorProfilePictures);
            }

            await context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<AllPublishersViewModel>> GetAllPublishersAsync()
        {
            IEnumerable<AllPublishersViewModel> publishers = await context.Publishers
                .OrderBy(p => p.Books.Count)
                .ThenBy(p => p.Name)
                .Select(p => new AllPublishersViewModel()
                {
                    Id = p.Id,
                    Name = p.Name,
                    Photo = p.LogoUrl,
                })
                .ToListAsync();
            return publishers;
        }

        public async Task<DeletePublisherViewModel?> GetDeletePublisherInfoAsync(Guid id)
        {
            Publisher? publisher = await context.Publishers
                .Include(p => p.Books)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (publisher == null)
                return null;
            return new DeletePublisherViewModel()
            {
                Id = publisher.Id,
                Name = publisher.Name,
                BooksCount = publisher.Books.Count,
                Logo = publisher.LogoUrl,
            };
        }

        public async Task<PublisherDetailsViewModel?> GetPublisherDetailsAsync(Guid id, Guid? userId = null)
        {
            Publisher? publisher = await context.Publishers
                .FirstOrDefaultAsync(p => p.Id == id);  
            if (publisher == null) 
                return null;
            List<AllBooksViewModel>? allPublishedBooks = await bookService.GetAllBooksFromPublisherAsync(id, userId);
            if (allPublishedBooks == null)
                return null;
            PublisherDetailsViewModel model = new PublisherDetailsViewModel() 
            {
                Id = publisher.Id,
                Name= publisher.Name,
                Country = publisher.Country,
                Description = publisher.Description,
                Logo = publisher.LogoUrl,
                Website = publisher.Website,
                PublishedBooks = allPublishedBooks
            };
            return model;
        }

        public async Task<EditPublisherInputModel?> GetPublisherEditInfoAsync(Guid id)
        {
            Publisher? publisher = await context.Publishers
                .FirstOrDefaultAsync(p => p.Id == id);
            if(publisher == null)
                return null;
            EditPublisherInputModel model = new EditPublisherInputModel()
            {
                Id = publisher.Id,
                Country = publisher.Country,
                Description = publisher.Description,
                ExistingLogoPath = publisher.LogoUrl,
                Name = publisher.Name,
                Website = publisher.Website
            }; 
            return model;
        }
    }
}
