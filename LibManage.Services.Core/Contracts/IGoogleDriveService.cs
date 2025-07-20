
using LibManage.Common.Enumerations;

using Microsoft.AspNetCore.Http;

namespace LibManage.Services.Core.Contracts
{
    public interface IGoogleDriveService
    {
        public Task<string> UploadFileAsync(IFormFile file, DriveFolder folder);
    }
}
