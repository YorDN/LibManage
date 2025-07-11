using LibManage.Services.Core.Contracts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace LibManage.Services.Core
{
    public class FileUploadService(IWebHostEnvironment env) : IFileUploadService
    {
        public async Task<string> UploadFileAsync(IFormFile file, string subFolder)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentNullException("Invalid file");

            string uploadsPath = Path.Combine(env.WebRootPath, "uploads", subFolder);
            Directory.CreateDirectory(uploadsPath);

            string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            string filePath = Path.Combine(uploadsPath, fileName);
            using (FileStream stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/{subFolder}/{fileName}".Replace("\\", "/");
        }
    }
}
