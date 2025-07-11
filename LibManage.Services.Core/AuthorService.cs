using LibManage.Common.Enumerations;
using LibManage.Data;
using LibManage.Data.Models.Library;
using LibManage.Services.Core.Contracts;
using LibManage.ViewModels;

namespace LibManage.Services.Core
{
    public class AuthorService(IFileUploadService fileUploadService, 
        ApplicationDbContext context) : IAuthorService
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
    }
}
