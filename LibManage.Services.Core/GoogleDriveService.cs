using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;

using LibManage.Common.Enumerations;
using LibManage.Services.Core.Contracts;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace LibManage.Services.Core
{
    public class GoogleDriveService : IGoogleDriveService
    {
        private readonly IConfiguration config;
        private readonly DriveService driveService;

        public GoogleDriveService(IConfiguration config)
        {
            this.config = config;
            var credentialsPath = config["GoogleDrive:CredentialsPath"];
            var credentials = GoogleCredential
                .FromFile(credentialsPath)
                .CreateScoped(DriveService.Scope.DriveFile);
            driveService = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credentials,
                ApplicationName = "LibManage"
            });
        }
        public async Task<string> UploadFileAsync(IFormFile file, DriveFolder folder)
        {
            string folderId = config[$"GoogleDrive:Folders:{folder}"]!;
            var fileData = new Google.Apis.Drive.v3.Data.File
            {
                Name = file.Name,
                Parents = new List<string> { folderId },
            };
            using var stream = file.OpenReadStream();
            var request = driveService.Files.Create(fileData, stream, file.ContentType);
            request.Fields = "id";
            await request.UploadAsync();

            var uploadedFile = request.ResponseBody;

            var permission = new Permission
            {
                Type = "anyone",
                Role = "reader"
            };
            await driveService.Permissions.Create(permission, uploadedFile.Id).ExecuteAsync();

            return $"https://drive.google.com/uc?id={uploadedFile.Id}";
        }
    }
}
