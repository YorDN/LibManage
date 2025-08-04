using LibManage.Data;
using LibManage.Data.Models.DTOs;
using LibManage.Data.Models.Library;
using LibManage.Services.Core;
using LibManage.Services.Core.Contracts;
using LibManage.ViewModels.Authors;
using LibManage.ViewModels.Books;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

using Moq;

using static LibManage.Data.Models.Library.Book;

namespace Libmanage.Services.Test
{
    [TestFixture]
    public class AuthorServiceTests
    {
        private ApplicationDbContext _dbContext;
        private AuthorService _authorService;
        private Mock<IFileUploadService> _mockFileUploadService;
        private Mock<IBookService> _mockBookService;

        [SetUp]
        public void Setup()
        {
            _dbContext = GetDbContext();
            _mockFileUploadService = new Mock<IFileUploadService>();
            _mockBookService = new Mock<IBookService>();
            _authorService = CreateService(_dbContext, _mockFileUploadService, _mockBookService);
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Dispose();
        }

        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        private AuthorService CreateService(
            ApplicationDbContext context,
            Mock<IFileUploadService>? fileUploadServiceMock = null,
            Mock<IBookService>? bookServiceMock = null)
        {
            return new AuthorService(
                fileUploadServiceMock?.Object ?? new Mock<IFileUploadService>().Object,
                context,
                bookServiceMock?.Object ?? new Mock<IBookService>().Object
            );
        }

        private Author CreateTestAuthor()
        {
            return new Author
            {
                FullName = "Test Author",
                Photo = "/uploads/pfps/author/test.jpg",
                Biography = "Test Biography",
                DateOfBirth = new DateTime(1980, 1, 1)
            };
        }

        private Book CreateTestBook(Author author)
        {
            return new Book
            {
                Title = "Test Book",
                ISBN = "1234567890123",
                Language = "English",
                Cover = "cover.jpg",
                Type = BookType.Physical,
                Author = author,
                Publisher = new Publisher
                {
                    Name = "Test Publisher",
                    LogoUrl = "logo.jpg"
                }
            };
        }

        [Test]
        public async Task CreateAuthorAsync_WithValidData_ReturnsTrue()
        {
            var model = new AddAuthorInputModel
            {
                FullName = "New Author",
                PhotoFile = null,
                Biography = "Test Bio",
                DateOfBirth = new DateTime(1980, 1, 1)
            };
            var result = await _authorService.CreateAuthorAsync(model);

            Assert.IsTrue(result);
            Assert.AreEqual(1, await _dbContext.Authors.CountAsync());
        }

        [Test]
        public async Task CreateAuthorAsync_WithDuplicateName_ReturnsFalse()
        {
            await _dbContext.Authors.AddAsync(CreateTestAuthor());
            await _dbContext.SaveChangesAsync();

            var model = new AddAuthorInputModel
            {
                FullName = "Test Author",
                PhotoFile = null
            };

            var result = await _authorService.CreateAuthorAsync(model);

            Assert.IsFalse(result);
        }

        [Test]
        public async Task DeleteAuthorAsync_WithNoBooks_ReturnsTrue()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            using var dbContext = new ApplicationDbContext(options);
            var author = CreateTestAuthor();
            await dbContext.Authors.AddAsync(author);
            await dbContext.SaveChangesAsync();

            var mockFileUploadService = new Mock<IFileUploadService>();
            mockFileUploadService.Setup(x => x.DeleteFileAsync(author.Photo))
                .ReturnsAsync(true);

            var service = new AuthorService(
                mockFileUploadService.Object,
                dbContext,
                new Mock<IBookService>().Object
            );

            var result = await service.DeleteAuthorAsync(author.Id);

            Assert.IsTrue(result);
            Assert.AreEqual(0, await dbContext.Authors.CountAsync());
        }

        [Test]
        public async Task DeleteAuthorAsync_WithBooks_DeletesBooksFirst()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            using var dbContext = new ApplicationDbContext(options);
            var mockFileUploadService = new Mock<IFileUploadService>();
            var mockBookService = new Mock<IBookService>();

            var author = CreateTestAuthor();
            var book = CreateTestBook(author);
            await dbContext.Authors.AddAsync(author);
            await dbContext.Books.AddAsync(book);
            await dbContext.SaveChangesAsync();

            mockBookService.Setup(x => x.DeleteBookAsync(book.Id))
                .ReturnsAsync(true);
            mockFileUploadService.Setup(x => x.DeleteFileAsync(author.Photo))
                .ReturnsAsync(true);

            var service = new AuthorService(
                mockFileUploadService.Object,
                dbContext,
                mockBookService.Object
            );

            var result = await service.DeleteAuthorAsync(author.Id);

            Assert.IsTrue(result);
            mockBookService.Verify(x => x.DeleteBookAsync(book.Id), Times.Once);

            Assert.IsNull(await dbContext.Authors.FindAsync(author.Id));
        }

        [Test]
        public async Task EditAuthorAsync_WithValidData_ReturnsTrue()
        {
            var author = CreateTestAuthor();
            await _dbContext.Authors.AddAsync(author);
            await _dbContext.SaveChangesAsync();

            var model = new EditAuthorInputModel
            {
                Id = author.Id,
                FullName = "Updated Name",
                Biography = "Updated Bio",
                DateOfBirth = new DateTime(1990, 1, 1)
            };

            var result = await _authorService.EditAuthorAsync(model);

            Assert.IsTrue(result);
            var updatedAuthor = await _dbContext.Authors.FindAsync(author.Id);
            Assert.AreEqual("Updated Name", updatedAuthor?.FullName);
        }

        [Test]
        public async Task GetAllAuthorsAsync_ReturnsPaginatedResults()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var dbContext = new ApplicationDbContext(options);

            var authors = new List<Author>
                {
                    new Author { FullName = "Author C", Photo = "photo1.jpg" },
                    new Author { FullName = "Author A", Photo = "photo2.jpg" },
                    new Author { FullName = "Author B", Photo = "photo3.jpg" }
                };

            await dbContext.Authors.AddRangeAsync(authors);
            await dbContext.SaveChangesAsync();

            var service = new AuthorService(
                new Mock<IFileUploadService>().Object,
                dbContext,
                new Mock<IBookService>().Object
            );

            var filterOptions = new AuthorFilterOptions
            {
                SearchTerm = "",
                SortBy = "Name",
                SortDescending = false
            };

            var result = await service.GetAllAuthorsAsync(filterOptions, 1, 2);

            var resultAuthors = result.Authors.ToList();

            Assert.AreEqual(2, result.TotalPages);
            Assert.AreEqual(1, result.CurrentPage);

            Assert.AreEqual(2, resultAuthors.Count);

            Assert.AreEqual("Author A", resultAuthors[0].Name);
            Assert.AreEqual("Author B", resultAuthors[1].Name);

            var page2 = await service.GetAllAuthorsAsync(filterOptions, 2, 2);
            var page2Authors = page2.Authors.ToList();
            Assert.AreEqual(1, page2Authors.Count);
            Assert.AreEqual("Author C", page2Authors[0].Name);
            Assert.AreEqual(2, page2.CurrentPage);
        }
        [Test]
        public async Task GetAuthorDeleteInfoAsync_ReturnsCorrectInfo()
        {
            var author = CreateTestAuthor();
            var book = CreateTestBook(author);
            await _dbContext.Authors.AddAsync(author);
            await _dbContext.Books.AddAsync(book);
            await _dbContext.SaveChangesAsync();

            var result = await _authorService.GetAuthorDeleteInfoAsync(author.Id);

            Assert.NotNull(result);
            Assert.AreEqual(author.FullName, result?.Name);
            Assert.AreEqual(1, result?.BooksCount);
        }

        [Test]
        public async Task GetAuthorDetailsAsync_ReturnsCorrectDetails()
        {
            var author = CreateTestAuthor();
            var book = CreateTestBook(author);
            await _dbContext.Authors.AddAsync(author);
            await _dbContext.Books.AddAsync(book);
            await _dbContext.SaveChangesAsync();

            _mockBookService.Setup(x => x.GetAllBooksFromAuthorAsync(author.Id, null))
                .ReturnsAsync(new List<AllBooksViewModel>
                {
                    new() { Title = book.Title }
                });

            var result = await _authorService.GetAuthorDetailsAsync(author.Id);

            Assert.NotNull(result);
            Assert.AreEqual(author.FullName, result?.Name);
            Assert.AreEqual(1, result?.WrittenBooks.Count);
        }

        [Test]
        public async Task GetAuthorEditInfoAsync_ReturnsCorrectInfo()
        {
            var author = CreateTestAuthor();
            await _dbContext.Authors.AddAsync(author);
            await _dbContext.SaveChangesAsync();

            var result = await _authorService.GetAuthorEditInfoAsync(author.Id);

            Assert.NotNull(result);
            Assert.AreEqual(author.FullName, result?.FullName);
            Assert.AreEqual(author.Biography, result?.Biography);
        }
    }
}
