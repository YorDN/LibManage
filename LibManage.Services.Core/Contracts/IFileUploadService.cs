using Microsoft.AspNetCore.Http;

namespace LibManage.Services.Core.Contracts
{
    public interface IFileUploadService
    {
        public Task<string> UploadFileAsync(IFormFile file, string subFolder);
        public Task<bool> DeleteFileAsync(string relativePath);
    }
}
