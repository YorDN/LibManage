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
            if (string.IsNullOrWhiteSpace(credentialsPath))
            {
                throw new InvalidOperationException("GoogleDrive:CredentialsPath is not configured.");
            }

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
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty or not provided.");

            string folderId = config[$"GoogleDrive:Folders:{folder}"];
            if (string.IsNullOrWhiteSpace(folderId))
                throw new InvalidOperationException($"Google Drive folder ID for '{folder}' is not configured.");

            var fileMeta = new Google.Apis.Drive.v3.Data.File
            {
                Name = file.FileName,
                Parents = new List<string> { folderId },
            };

            using var originalStream = file.OpenReadStream();
            using var memoryStream = new MemoryStream();
            await originalStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            try
            {
                var request = driveService.Files.Create(fileMeta, memoryStream, file.ContentType);
                request.Fields = "id";

                var progress = await request.UploadAsync();

                if (progress.Status != Google.Apis.Upload.UploadStatus.Completed)
                {
                    throw new ApplicationException($"Upload failed: {progress.Exception?.Message}");
                }

                var uploadedFile = request.ResponseBody;

                if (uploadedFile == null || string.IsNullOrWhiteSpace(uploadedFile.Id))
                    throw new InvalidOperationException("Upload failed — response file ID is null.");

                var permission = new Permission
                {
                    Type = "anyone",
                    Role = "reader"
                };
                await driveService.Permissions.Create(permission, uploadedFile.Id).ExecuteAsync();

                return $"https://drive.google.com/uc?id={uploadedFile.Id}";
            }
            catch (Exception ex)
            {
                Console.WriteLine("Google Drive upload failed: " + ex.Message);
                throw new ApplicationException("An error occurred during file upload to Google Drive.", ex);
            }
        }

    }
}
