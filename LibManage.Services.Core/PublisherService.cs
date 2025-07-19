using LibManage.Common;
using LibManage.Data;
using LibManage.Data.Models.Library;
using LibManage.Services.Core.Contracts;
using LibManage.ViewModels.Publishers;
using Microsoft.EntityFrameworkCore;

namespace LibManage.Services.Core
{
    public class PublisherService(ApplicationDbContext context, IFileUploadService fileUploadService) : IPublisherService
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
    }
}
