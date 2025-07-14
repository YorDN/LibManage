using LibManage.Data;
using LibManage.Data.Models.Library;
using LibManage.Services.Core.Contracts;
using LibManage.ViewModels;

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
                pfp = "/uploads/pfps/author/DefaultPublisher.png";
            }
            else
            {
                pfp = await fileUploadService.UploadFileAsync(model.LogoFile, "pfps/publisher");
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
    }
}
