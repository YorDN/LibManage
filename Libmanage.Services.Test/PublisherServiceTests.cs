using LibManage.Common;
using LibManage.Data;
using LibManage.Data.Models.DTOs;
using LibManage.Data.Models.Library;
using LibManage.Services.Core;
using LibManage.Services.Core.Contracts;
using LibManage.ViewModels.Books;
using LibManage.ViewModels.Publishers;

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

using Moq;

using System.Text;

namespace Libmanage.Services.Test
{
    [TestFixture]
    public class PublisherServiceTests
    {
        private ApplicationDbContext _context;
        private Mock<IFileUploadService> _mockFileUploadService;
        private Mock<IBookService> _mockBookService;
        private PublisherService _publisherService;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _context = new ApplicationDbContext(options);
            _mockFileUploadService = new Mock<IFileUploadService>();
            _mockBookService = new Mock<IBookService>();

            _publisherService = new PublisherService(_context, _mockFileUploadService.Object, _mockBookService.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _context?.Dispose();
        }

        private Publisher CreateTestPublisher(string name = "Test Publisher", string logoUrl = "/uploads/pfps/publisher/test.png")
        {
            return new Publisher
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = "Test Description",
                Country = "Test Country",
                Website = "https://test.com",
                LogoUrl = logoUrl
            };
        }

        private Mock<IFormFile> CreateMockFile(string fileName, string content)
        {
            var mockFile = new Mock<IFormFile>();
            var bytes = Encoding.UTF8.GetBytes(content);
            var stream = new MemoryStream(bytes);

            mockFile.Setup(f => f.FileName).Returns(fileName);
            mockFile.Setup(f => f.Length).Returns(bytes.Length);
            mockFile.Setup(f => f.OpenReadStream()).Returns(stream);

            return mockFile;
        }

        private AddPublisherInputModel CreateAddPublisherModel(string name = "New Publisher", IFormFile logoFile = null)
        {
            return new AddPublisherInputModel
            {
                Name = name,
                Description = "New Description",
                Country = "New Country",
                Website = "https://new.com",
                LogoFile = logoFile
            };
        }

        private EditPublisherInputModel CreateEditPublisherModel(Guid id, IFormFile newLogo = null)
        {
            return new EditPublisherInputModel
            {
                Id = id,
                Name = "Updated Publisher",
                Description = "Updated Description",
                Country = "Updated Country",
                Website = "https://updated.com",
                NewLogo = newLogo
            };
        }

        [Test]
        public async Task AddPublisherAsync_ReturnsFalse_WhenPublisherNameAlreadyExists()
        {
            var existingPublisher = CreateTestPublisher("Existing Publisher");
            await _context.Publishers.AddAsync(existingPublisher);
            await _context.SaveChangesAsync();

            var model = CreateAddPublisherModel("Existing Publisher");

            var result = await _publisherService.AddPublisherAsync(model);

            Assert.IsFalse(result);
        }

        [Test]
        public async Task AddPublisherAsync_UsesDefaultLogo_WhenLogoFileIsNull()
        {
            var model = CreateAddPublisherModel(logoFile: null);

            var result = await _publisherService.AddPublisherAsync(model);

            Assert.IsTrue(result);

            var savedPublisher = await _context.Publishers.FirstAsync(p => p.Name == model.Name);
            Assert.AreEqual("/uploads/pfps/publisher/DefaultPublisher.png", savedPublisher.LogoUrl);

            _mockFileUploadService.Verify(f => f.UploadFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task AddPublisherAsync_UsesDefaultLogo_WhenLogoFileIsEmpty()
        {
            var emptyFile = CreateMockFile("empty.png", "");
            emptyFile.Setup(f => f.Length).Returns(0);
            var model = CreateAddPublisherModel(logoFile: emptyFile.Object);

            var result = await _publisherService.AddPublisherAsync(model);

            Assert.IsTrue(result);

            var savedPublisher = await _context.Publishers.FirstAsync(p => p.Name == model.Name);
            Assert.AreEqual("/uploads/pfps/publisher/DefaultPublisher.png", savedPublisher.LogoUrl);
        }

        [Test]
        public async Task AddPublisherAsync_UploadsLogo_WhenLogoFileProvided()
        {
            var logoFile = CreateMockFile("logo.png", "logo content");
            var model = CreateAddPublisherModel(logoFile: logoFile.Object);
            var uploadedPath = "/uploads/pfps/publisher/uploaded-logo.png";

            _mockFileUploadService.Setup(f => f.UploadFileAsync(logoFile.Object, Subfolders.PublisherProfilePictures))
                .ReturnsAsync(uploadedPath);

            var result = await _publisherService.AddPublisherAsync(model);

            Assert.IsTrue(result);

            var savedPublisher = await _context.Publishers.FirstAsync(p => p.Name == model.Name);
            Assert.AreEqual(uploadedPath, savedPublisher.LogoUrl);

            _mockFileUploadService.Verify(f => f.UploadFileAsync(logoFile.Object, Subfolders.PublisherProfilePictures), Times.Once);
        }

        [Test]
        public async Task AddPublisherAsync_SavesToDatabase_WithCorrectProperties()
        {
            var model = CreateAddPublisherModel();

            var result = await _publisherService.AddPublisherAsync(model);

            Assert.IsTrue(result);

            var savedPublisher = await _context.Publishers.FirstAsync(p => p.Name == model.Name);
            Assert.AreEqual(model.Name, savedPublisher.Name);
            Assert.AreEqual(model.Description, savedPublisher.Description);
            Assert.AreEqual(model.Country, savedPublisher.Country);
            Assert.AreEqual(model.Website, savedPublisher.Website);
        }

        [Test]
        public async Task DeletePublisherAsync_ReturnsFalse_WhenPublisherNotFound()
        {
            var nonExistentId = Guid.NewGuid();

            var result = await _publisherService.DeletePublisherAsync(nonExistentId);

            Assert.IsFalse(result);
        }

        [Test]
        public async Task DeletePublisherAsync_DeletesPublisherAndBooks_WhenSuccessful()
        {
            var publisher = CreateTestPublisher();
            var book1 = new Book {Id = Guid.NewGuid(), PublisherId = publisher.Id, Cover = "test", ISBN = "test", Language = "testt", Title = "test1" };
            var book2 = new Book {Id = Guid.NewGuid(), PublisherId = publisher.Id, Cover = "test", ISBN = "test", Language = "testt", Title = "test2" };

            publisher.Books = new List<Book> { book1, book2 };

            await _context.Publishers.AddAsync(publisher);
            await _context.SaveChangesAsync();

            _mockBookService.Setup(b => b.DeleteBookAsync(book1.Id)).ReturnsAsync(true);
            _mockBookService.Setup(b => b.DeleteBookAsync(book2.Id)).ReturnsAsync(true);
            _mockFileUploadService.Setup(f => f.DeleteFileAsync(publisher.LogoUrl)).ReturnsAsync(true);

            var result = await _publisherService.DeletePublisherAsync(publisher.Id);

            Assert.IsTrue(result);

            var deletedPublisher = await _context.Publishers.FirstOrDefaultAsync(p => p.Id == publisher.Id);
            Assert.IsNull(deletedPublisher);

            _mockBookService.Verify(b => b.DeleteBookAsync(book1.Id), Times.Once);
            _mockBookService.Verify(b => b.DeleteBookAsync(book2.Id), Times.Once);
            _mockFileUploadService.Verify(f => f.DeleteFileAsync(publisher.LogoUrl), Times.Once);
        }

        [Test]
        public async Task DeletePublisherAsync_RollsBackTransaction_WhenBookDeletionFails()
        {
            var publisher = CreateTestPublisher();
            var book = new Book {Id = Guid.NewGuid(), PublisherId = publisher.Id, Cover = "test", ISBN = "test", Language = "testt", Title = "test1" };
            publisher.Books = new List<Book> { book };

            await _context.Publishers.AddAsync(publisher);
            await _context.SaveChangesAsync();

            _mockBookService.Setup(b => b.DeleteBookAsync(book.Id)).ReturnsAsync(false);

            var result = await _publisherService.DeletePublisherAsync(publisher.Id);

            Assert.IsFalse(result);

            var existingPublisher = await _context.Publishers.FirstOrDefaultAsync(p => p.Id == publisher.Id);
            Assert.IsNotNull(existingPublisher);
        }

        [Test]
        public async Task DeletePublisherAsync_RollsBackTransaction_WhenFileDeletionFails()
        {
            var publisher = CreateTestPublisher();
            await _context.Publishers.AddAsync(publisher);
            await _context.SaveChangesAsync();

            _mockFileUploadService.Setup(f => f.DeleteFileAsync(publisher.LogoUrl)).ReturnsAsync(false);

            var result = await _publisherService.DeletePublisherAsync(publisher.Id);

            Assert.IsFalse(result);

            var existingPublisher = await _context.Publishers.FirstOrDefaultAsync(p => p.Id == publisher.Id);
            Assert.IsNotNull(existingPublisher);
        }

        [Test]
        public async Task DeletePublisherAsync_DoesNotDeleteFile_WhenUsingDefaultLogo()
        {
            var publisher = CreateTestPublisher(logoUrl: "/uploads/pfps/publisher/DefaultPublisher.png");
            await _context.Publishers.AddAsync(publisher);
            await _context.SaveChangesAsync();

            var result = await _publisherService.DeletePublisherAsync(publisher.Id);

            Assert.IsTrue(result);

            _mockFileUploadService.Verify(f => f.DeleteFileAsync(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task DeletePublisherAsync_DoesNotDeleteFile_WhenLogoUrlIsEmpty()
        {
            var publisher = CreateTestPublisher(logoUrl: "");
            await _context.Publishers.AddAsync(publisher);
            await _context.SaveChangesAsync();

            var result = await _publisherService.DeletePublisherAsync(publisher.Id);

            Assert.IsTrue(result);

            _mockFileUploadService.Verify(f => f.DeleteFileAsync(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task DeletePublisherAsync_DoesNotDeleteFile_WhenLogoUrlIsNull()
        {
            var publisher = CreateTestPublisher(logoUrl: "");
            await _context.Publishers.AddAsync(publisher);
            await _context.SaveChangesAsync();

            var result = await _publisherService.DeletePublisherAsync(publisher.Id);

            Assert.IsTrue(result);

            _mockFileUploadService.Verify(f => f.DeleteFileAsync(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task EditPublisherAsync_ReturnsFalse_WhenPublisherNotFound()
        {
            var model = CreateEditPublisherModel(Guid.NewGuid());

            var result = await _publisherService.EditPublisherAsync(model);

            Assert.IsFalse(result);
        }

        [Test]
        public async Task EditPublisherAsync_UpdatesProperties_WithoutNewLogo()
        {
            var publisher = CreateTestPublisher();
            await _context.Publishers.AddAsync(publisher);
            await _context.SaveChangesAsync();

            var model = CreateEditPublisherModel(publisher.Id);
            var originalLogoUrl = publisher.LogoUrl;

            var result = await _publisherService.EditPublisherAsync(model);

            Assert.IsTrue(result);

            var updatedPublisher = await _context.Publishers.FirstAsync(p => p.Id == publisher.Id);
            Assert.AreEqual(model.Name, updatedPublisher.Name);
            Assert.AreEqual(model.Description, updatedPublisher.Description);
            Assert.AreEqual(model.Country, updatedPublisher.Country);
            Assert.AreEqual(model.Website, updatedPublisher.Website);
            Assert.AreEqual(originalLogoUrl, updatedPublisher.LogoUrl);

            _mockFileUploadService.Verify(f => f.DeleteFileAsync(It.IsAny<string>()), Times.Never);
            _mockFileUploadService.Verify(f => f.UploadFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task EditPublisherAsync_UpdatesLogoAndDeletesOld_WhenNewLogoProvided()
        {
            var publisher = CreateTestPublisher();
            await _context.Publishers.AddAsync(publisher);
            await _context.SaveChangesAsync();

            var oldLogoPath = publisher.LogoUrl;

            var newLogoFile = CreateMockFile("new-logo.png", "new logo content");
            var model = CreateEditPublisherModel(publisher.Id, newLogoFile.Object);
            var newLogoPath = "/uploads/pfps/publisher/new-logo.png";

            _mockFileUploadService.Setup(f => f.DeleteFileAsync(oldLogoPath)).ReturnsAsync(true);
            _mockFileUploadService.Setup(f => f.UploadFileAsync(newLogoFile.Object, Subfolders.AuthorProfilePictures))
                .ReturnsAsync(newLogoPath);

            var result = await _publisherService.EditPublisherAsync(model);

            Assert.IsTrue(result);

            var updatedPublisher = await _context.Publishers.FirstAsync(p => p.Id == publisher.Id);
            Assert.AreEqual(newLogoPath, updatedPublisher.LogoUrl);

            _mockFileUploadService.Verify(f => f.DeleteFileAsync(oldLogoPath), Times.Once);
            _mockFileUploadService.Verify(f => f.UploadFileAsync(newLogoFile.Object, Subfolders.AuthorProfilePictures), Times.Once);
        }


        [Test]
        public async Task EditPublisherAsync_ReturnsFalse_WhenOldLogoDeletionFails()
        {
            var publisher = CreateTestPublisher();
            await _context.Publishers.AddAsync(publisher);
            await _context.SaveChangesAsync();

            var newLogoFile = CreateMockFile("new-logo.png", "new logo content");
            var model = CreateEditPublisherModel(publisher.Id, newLogoFile.Object);

            _mockFileUploadService.Setup(f => f.DeleteFileAsync(publisher.LogoUrl)).ReturnsAsync(false);

            var result = await _publisherService.EditPublisherAsync(model);

            Assert.IsFalse(result);

            _mockFileUploadService.Verify(f => f.UploadFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task GetAllPublishersAsync_ReturnsEmptyList_WhenNoPublishers()
        {
            var result = await _publisherService.GetAllPublishersAsync(new PublisherFilterOptions());

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Publishers.Any());
        }

        [Test]
        public async Task GetAllPublishersAsync_ReturnsPublishers_OrderedByBookCountThenName()
        {
            var publisher1 = CreateTestPublisher("Publisher C");
            var publisher2 = CreateTestPublisher("Publisher A");
            var publisher3 = CreateTestPublisher("Publisher B");

            publisher1.Books = new List<Book> { new Book {Id = Guid.NewGuid(), Cover = "test", ISBN = "test", Language = "testt", Title = "test1" } }; // 1 book
            publisher2.Books = new List<Book>();
            publisher3.Books = new List<Book>();

            await _context.Publishers.AddRangeAsync(publisher1, publisher2, publisher3);
            await _context.SaveChangesAsync();

            var result = await _publisherService.GetAllPublishersAsync(new PublisherFilterOptions());

            var publishers = result.Publishers.ToList();
            Assert.AreEqual(3, publishers.Count);

            Assert.AreEqual("Publisher A", publishers[0].Name);
            Assert.AreEqual("Publisher B", publishers[1].Name);  
            Assert.AreEqual("Publisher C", publishers[2].Name);
        }

        [Test]
        public async Task GetAllPublishersAsync_ReturnsCorrectProperties()
        {
            var publisher = CreateTestPublisher();
            await _context.Publishers.AddAsync(publisher);
            await _context.SaveChangesAsync();

            var result = await _publisherService.GetAllPublishersAsync(new PublisherFilterOptions());

            var publisherViewModel = result.Publishers.First();
            Assert.AreEqual(publisher.Id, publisherViewModel.Id);
            Assert.AreEqual(publisher.Name, publisherViewModel.Name);
            Assert.AreEqual(publisher.LogoUrl, publisherViewModel.Photo);
        }
        [Test]
        public async Task GetAllPublishersAsync_ReturnsPaginatedResults()
        {

            var publishers = new List<Publisher>
                {
                    new Publisher { Name = "Author C", LogoUrl = "photo1.jpg" },
                    new Publisher { Name = "Author A", LogoUrl = "photo2.jpg" },
                    new Publisher { Name = "Author B", LogoUrl = "photo3.jpg" }
                };

            await _context.Publishers.AddRangeAsync(publishers);
            await _context.SaveChangesAsync();

            var filterOptions = new PublisherFilterOptions()
            {
                SearchTerm = "",
                SortBy = "Name",
                SortDescending = false
            };

            var result = await _publisherService.GetAllPublishersAsync(filterOptions, 1, 2);

            var resultPublishers = result.Publishers.ToList();

            Assert.AreEqual(2, result.TotalPages);
            Assert.AreEqual(1, result.CurrentPage);

            Assert.AreEqual(2, resultPublishers.Count);

            Assert.AreEqual("Author A", resultPublishers[0].Name);
            Assert.AreEqual("Author B", resultPublishers[1].Name);

            var page2 = await _publisherService.GetAllPublishersAsync(filterOptions, 2, 2);
            var page2Publishers = page2.Publishers.ToList();
            Assert.AreEqual(1, page2Publishers.Count);
            Assert.AreEqual("Author C", page2Publishers[0].Name);
            Assert.AreEqual(2, page2.CurrentPage);
        }
        [Test]
        public async Task GetDeletePublisherInfoAsync_ReturnsNull_WhenPublisherNotFound()
        {
            var nonExistentId = Guid.NewGuid();

            var result = await _publisherService.GetDeletePublisherInfoAsync(nonExistentId);

            Assert.IsNull(result);
        }

        [Test]
        public async Task GetDeletePublisherInfoAsync_ReturnsCorrectInfo_WhenPublisherExists()
        {
            var publisher = CreateTestPublisher();
            var book1 = new Book { Id = Guid.NewGuid(), PublisherId = publisher.Id,
             Cover = "test", ISBN  = "test", Language = "testt", Title = "test1"};
            var book2 = new Book { Id = Guid.NewGuid(), PublisherId = publisher.Id,
                Cover = "test",
                ISBN = "testaa",
                Language = "testt",
                Title = "test2"
            };
            publisher.Books = new List<Book> { book1, book2 };

            await _context.Publishers.AddAsync(publisher);
            await _context.SaveChangesAsync();

            var result = await _publisherService.GetDeletePublisherInfoAsync(publisher.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual(publisher.Id, result.Id);
            Assert.AreEqual(publisher.Name, result.Name);
            Assert.AreEqual(2, result.BooksCount);
            Assert.AreEqual(publisher.LogoUrl, result.Logo);
        }

        [Test]
        public async Task GetPublisherDetailsAsync_ReturnsNull_WhenPublisherNotFound()
        {
            var nonExistentId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var result = await _publisherService.GetPublisherDetailsAsync(nonExistentId, userId);

            Assert.IsNull(result);
        }

        [Test]
        public async Task GetPublisherDetailsAsync_ReturnsNull_WhenBookServiceReturnsNull()
        {
            var publisher = CreateTestPublisher();
            await _context.Publishers.AddAsync(publisher);
            await _context.SaveChangesAsync();

            var userId = Guid.NewGuid();
            _mockBookService.Setup(b => b.GetAllBooksFromPublisherAsync(publisher.Id, userId))
                .ReturnsAsync((List<AllBooksViewModel>)null);

            var result = await _publisherService.GetPublisherDetailsAsync(publisher.Id, userId);

            Assert.IsNull(result);
        }

        [Test]
        public async Task GetPublisherDetailsAsync_ReturnsCorrectDetails_WhenSuccessful()
        {
            var publisher = CreateTestPublisher();
            await _context.Publishers.AddAsync(publisher);
            await _context.SaveChangesAsync();

            var userId = Guid.NewGuid();
            var publishedBooks = new List<AllBooksViewModel>
            {
                new AllBooksViewModel { Id = Guid.NewGuid(), Title = "Book 1" },
                new AllBooksViewModel { Id = Guid.NewGuid(), Title = "Book 2" }
            };

            _mockBookService.Setup(b => b.GetAllBooksFromPublisherAsync(publisher.Id, userId))
                .ReturnsAsync(publishedBooks);

            var result = await _publisherService.GetPublisherDetailsAsync(publisher.Id, userId);

            Assert.IsNotNull(result);
            Assert.AreEqual(publisher.Id, result.Id);
            Assert.AreEqual(publisher.Name, result.Name);
            Assert.AreEqual(publisher.Country, result.Country);
            Assert.AreEqual(publisher.Description, result.Description);
            Assert.AreEqual(publisher.LogoUrl, result.Logo);
            Assert.AreEqual(publisher.Website, result.Website);
            Assert.AreEqual(publishedBooks, result.PublishedBooks);

            _mockBookService.Verify(b => b.GetAllBooksFromPublisherAsync(publisher.Id, userId), Times.Once);
        }

        [Test]
        public async Task GetPublisherDetailsAsync_WorksWithoutUserId()
        {
            var publisher = CreateTestPublisher();
            await _context.Publishers.AddAsync(publisher);
            await _context.SaveChangesAsync();

            var publishedBooks = new List<AllBooksViewModel>();
            _mockBookService.Setup(b => b.GetAllBooksFromPublisherAsync(publisher.Id, null))
                .ReturnsAsync(publishedBooks);

            var result = await _publisherService.GetPublisherDetailsAsync(publisher.Id);

            Assert.IsNotNull(result);
            _mockBookService.Verify(b => b.GetAllBooksFromPublisherAsync(publisher.Id, null), Times.Once);
        }

        [Test]
        public async Task GetPublisherEditInfoAsync_ReturnsNull_WhenPublisherNotFound()
        {
            var nonExistentId = Guid.NewGuid();

            var result = await _publisherService.GetPublisherEditInfoAsync(nonExistentId);

            Assert.IsNull(result);
        }

        [Test]
        public async Task GetPublisherEditInfoAsync_ReturnsCorrectInfo_WhenPublisherExists()
        {
            var publisher = CreateTestPublisher();
            await _context.Publishers.AddAsync(publisher);
            await _context.SaveChangesAsync();

            var result = await _publisherService.GetPublisherEditInfoAsync(publisher.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual(publisher.Id, result.Id);
            Assert.AreEqual(publisher.Country, result.Country);
            Assert.AreEqual(publisher.Description, result.Description);
            Assert.AreEqual(publisher.LogoUrl, result.ExistingLogoPath);
            Assert.AreEqual(publisher.Name, result.Name);
            Assert.AreEqual(publisher.Website, result.Website);
        }
    }
}
