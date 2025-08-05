using LibManage.Services.Core;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

using Moq;

using System.Text;

namespace Libmanage.Services.Test
{
    [TestFixture]
    public class FileUploadServiceTests
    {
        private Mock<IWebHostEnvironment> _mockEnv;
        private FileUploadService _fileUploadService;
        private string _testWebRootPath;

        [SetUp]
        public void Setup()
        {
            _testWebRootPath = Path.Combine(Path.GetTempPath(), "test_webroot_" + Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testWebRootPath);

            _mockEnv = new Mock<IWebHostEnvironment>();
            _mockEnv.Setup(e => e.WebRootPath).Returns(_testWebRootPath);

            _fileUploadService = new FileUploadService(_mockEnv.Object);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_testWebRootPath))
            {
                Directory.Delete(_testWebRootPath, true);
            }
        }

        [Test]
        public async Task UploadFileAsync_ThrowsArgumentNullException_WhenFileIsNull()
        {
            IFormFile file = null;
            string subFolder = "images";

            var ex = Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _fileUploadService.UploadFileAsync(file, subFolder));

            Assert.That(ex.Message, Contains.Substring("Invalid file"));
        }

        [Test]
        public async Task UploadFileAsync_ThrowsArgumentNullException_WhenFileIsEmpty()
        {
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(0);
            mockFile.Setup(f => f.FileName).Returns("test.txt");

            var ex = Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _fileUploadService.UploadFileAsync(mockFile.Object, "documents"));

            Assert.That(ex.Message, Contains.Substring("Invalid file"));
        }

        [Test]
        public async Task UploadFileAsync_CreatesDirectoryIfNotExists()
        {
            var mockFile = CreateMockFile("test.txt", "Hello World");
            string subFolder = "new-folder";
            string expectedDir = Path.Combine(_testWebRootPath, "uploads", subFolder);

            Assert.IsFalse(Directory.Exists(expectedDir));

            await _fileUploadService.UploadFileAsync(mockFile.Object, subFolder);

            Assert.IsTrue(Directory.Exists(expectedDir));
        }

        [Test]
        public async Task UploadFileAsync_SavesFileWithGuidName()
        {
            var fileContent = "Test file content";
            var mockFile = CreateMockFile("original.txt", fileContent);
            string subFolder = "documents";

            string result = await _fileUploadService.UploadFileAsync(mockFile.Object, subFolder);

            Assert.IsNotNull(result);
            Assert.That(result, Does.StartWith("/uploads/documents/"));
            Assert.That(result, Does.EndWith(".txt"));

            string fileName = Path.GetFileNameWithoutExtension(result.Split('/').Last());
            Assert.IsTrue(Guid.TryParse(fileName, out _));
        }

        [Test]
        public async Task UploadFileAsync_SavesFileContent()
        {
            var fileContent = "This is test file content";
            var mockFile = CreateMockFile("test.txt", fileContent);
            string subFolder = "documents";

            string result = await _fileUploadService.UploadFileAsync(mockFile.Object, subFolder);

            string expectedPath = Path.Combine(_testWebRootPath, result.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
            Assert.IsTrue(File.Exists(expectedPath));

            string savedContent = await File.ReadAllTextAsync(expectedPath);
            Assert.AreEqual(fileContent, savedContent);
        }

        [Test]
        public async Task UploadFileAsync_PreservesFileExtension()
        {
            var mockFile = CreateMockFile("image.jpg", "fake image data");

            string result = await _fileUploadService.UploadFileAsync(mockFile.Object, "images");

            Assert.That(result, Does.EndWith(".jpg"));
        }

        [Test]
        public async Task UploadFileAsync_HandlesFileWithoutExtension()
        {
            var mockFile = CreateMockFile("README", "readme content");

            string result = await _fileUploadService.UploadFileAsync(mockFile.Object, "docs");

            Assert.That(result, Does.Not.EndWith("."));
            Assert.That(result, Does.StartWith("/uploads/docs/"));
        }

        [Test]
        public async Task UploadFileAsync_ReturnsForwardSlashPath()
        {
            var mockFile = CreateMockFile("test.pdf", "pdf content");

            string result = await _fileUploadService.UploadFileAsync(mockFile.Object, "pdfs");

            Assert.That(result, Does.Not.Contain("\\"));
            Assert.That(result, Does.Contain("/"));
            Assert.That(result, Does.StartWith("/uploads/pdfs/"));
        }

        [Test]
        public async Task UploadFileAsync_HandlesNestedSubFolders()
        {
            var mockFile = CreateMockFile("test.txt", "content");
            string subFolder = "documents/2024/january";

            string result = await _fileUploadService.UploadFileAsync(mockFile.Object, subFolder);

            Assert.That(result, Does.StartWith("/uploads/documents/2024/january/"));

            string expectedDir = Path.Combine(_testWebRootPath, "uploads", "documents", "2024", "january");
            Assert.IsTrue(Directory.Exists(expectedDir));
        }

        [Test]
        public async Task UploadFileAsync_HandlesSpecialCharactersInSubFolder()
        {
            var mockFile = CreateMockFile("test.txt", "content");
            string subFolder = "user-files_2024";

            string result = await _fileUploadService.UploadFileAsync(mockFile.Object, subFolder);

            Assert.That(result, Does.StartWith("/uploads/user-files_2024/"));
        }

        [Test]
        public async Task UploadFileAsync_GeneratesUniqueFileNames()
        {
            var mockFile1 = CreateMockFile("same.txt", "content1");
            var mockFile2 = CreateMockFile("same.txt", "content2");

            string result1 = await _fileUploadService.UploadFileAsync(mockFile1.Object, "test");
            string result2 = await _fileUploadService.UploadFileAsync(mockFile2.Object, "test");

            Assert.AreNotEqual(result1, result2);

            string path1 = Path.Combine(_testWebRootPath, result1.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
            string path2 = Path.Combine(_testWebRootPath, result2.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));

            Assert.IsTrue(File.Exists(path1));
            Assert.IsTrue(File.Exists(path2));
            Assert.AreNotEqual(await File.ReadAllTextAsync(path1), await File.ReadAllTextAsync(path2));
        }



        [Test]
        public async Task DeleteFileAsync_ReturnsTrue_WhenFileExists()
        {
            string relativePath = "/uploads/test/sample.txt";
            string fullPath = Path.Combine(_testWebRootPath, "uploads", "test", "sample.txt");

            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            await File.WriteAllTextAsync(fullPath, "test content");

            Assert.IsTrue(File.Exists(fullPath));

            bool result = await _fileUploadService.DeleteFileAsync(relativePath);

            Assert.IsTrue(result);
            Assert.IsFalse(File.Exists(fullPath));
        }

        [Test]
        public async Task DeleteFileAsync_ReturnsFalse_WhenFileDoesNotExist()
        {
            string relativePath = "/uploads/test/nonexistent.txt";

            bool result = await _fileUploadService.DeleteFileAsync(relativePath);

            Assert.IsFalse(result);
        }

        [Test]
        public async Task DeleteFileAsync_HandlesPathWithoutLeadingSlash()
        {
            string relativePath = "uploads/test/sample.txt";
            string fullPath = Path.Combine(_testWebRootPath, "uploads", "test", "sample.txt");

            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            await File.WriteAllTextAsync(fullPath, "test content");

            bool result = await _fileUploadService.DeleteFileAsync(relativePath);

            Assert.IsTrue(result);
            Assert.IsFalse(File.Exists(fullPath));
        }

        [Test]
        public async Task DeleteFileAsync_HandlesPathWithMultipleLeadingSlashes()
        {
            string relativePath = "///uploads/test/sample.txt";
            string fullPath = Path.Combine(_testWebRootPath, "uploads", "test", "sample.txt");

            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            await File.WriteAllTextAsync(fullPath, "test content");

            bool result = await _fileUploadService.DeleteFileAsync(relativePath);

            Assert.IsTrue(result);
            Assert.IsFalse(File.Exists(fullPath));
        }

        [Test]
        public async Task DeleteFileAsync_HandlesNestedDirectories()
        {
            string relativePath = "/uploads/deep/nested/path/file.txt";
            string fullPath = Path.Combine(_testWebRootPath, "uploads", "deep", "nested", "path", "file.txt");

            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            await File.WriteAllTextAsync(fullPath, "nested file content");

            bool result = await _fileUploadService.DeleteFileAsync(relativePath);

            Assert.IsTrue(result);
            Assert.IsFalse(File.Exists(fullPath));
        }

        [Test]
        public async Task DeleteFileAsync_DoesNotThrow_WhenDirectoryDoesNotExist()
        {
            string relativePath = "/uploads/nonexistent-folder/file.txt";

            Assert.DoesNotThrowAsync(async () =>
            {
                bool result = await _fileUploadService.DeleteFileAsync(relativePath);
                Assert.IsFalse(result);
            });
        }

        [Test]
        public async Task DeleteFileAsync_OnlyDeletesSpecifiedFile()
        {
            string dir = Path.Combine(_testWebRootPath, "uploads", "test");
            Directory.CreateDirectory(dir);

            string file1 = Path.Combine(dir, "file1.txt");
            string file2 = Path.Combine(dir, "file2.txt");

            await File.WriteAllTextAsync(file1, "content1");
            await File.WriteAllTextAsync(file2, "content2");

            bool result = await _fileUploadService.DeleteFileAsync("/uploads/test/file1.txt");

            Assert.IsTrue(result);
            Assert.IsFalse(File.Exists(file1));
            Assert.IsTrue(File.Exists(file2));
        }



        [Test]
        public async Task UploadThenDelete_WorksCorrectly()
        {
            var mockFile = CreateMockFile("integration.txt", "integration test content");

            string uploadedPath = await _fileUploadService.UploadFileAsync(mockFile.Object, "integration");

            string fullPath = Path.Combine(_testWebRootPath, uploadedPath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
            Assert.IsTrue(File.Exists(fullPath));

            bool deleteResult = await _fileUploadService.DeleteFileAsync(uploadedPath);

            Assert.IsTrue(deleteResult);
            Assert.IsFalse(File.Exists(fullPath));
        }

        [Test]
        public async Task MultipleUploadsAndDeletes_WorkCorrectly()
        {
            var files = new[]
            {
                CreateMockFile("file1.txt", "content1"),
                CreateMockFile("file2.jpg", "image data"),
                CreateMockFile("file3.pdf", "pdf data")
            };

            var uploadedPaths = new List<string>();
            foreach (var file in files)
            {
                string path = await _fileUploadService.UploadFileAsync(file.Object, "bulk-test");
                uploadedPaths.Add(path);
            }

            Assert.AreEqual(3, uploadedPaths.Count);
            foreach (var path in uploadedPaths)
            {
                string fullPath = Path.Combine(_testWebRootPath, path.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                Assert.IsTrue(File.Exists(fullPath));
            }

            bool deleteResult = await _fileUploadService.DeleteFileAsync(uploadedPaths[1]);

            Assert.IsTrue(deleteResult);

            string path1 = Path.Combine(_testWebRootPath, uploadedPaths[0].TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
            string path2 = Path.Combine(_testWebRootPath, uploadedPaths[1].TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
            string path3 = Path.Combine(_testWebRootPath, uploadedPaths[2].TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));

            Assert.IsTrue(File.Exists(path1));
            Assert.IsFalse(File.Exists(path2));
            Assert.IsTrue(File.Exists(path3));
        }

        private Mock<IFormFile> CreateMockFile(string fileName, string content)
        {
            var mockFile = new Mock<IFormFile>();
            var bytes = Encoding.UTF8.GetBytes(content);
            var stream = new MemoryStream(bytes);

            mockFile.Setup(f => f.FileName).Returns(fileName);
            mockFile.Setup(f => f.Length).Returns(bytes.Length);
            mockFile.Setup(f => f.OpenReadStream()).Returns(stream);
            mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Callback<Stream, CancellationToken>((targetStream, token) =>
                {
                    stream.Position = 0;
                    stream.CopyTo(targetStream);
                });

            return mockFile;
        }
    }
}