using LibManage.Common;
using LibManage.Data;
using LibManage.Data.Models.DTOs;
using LibManage.Data.Models.Library;
using LibManage.Services.Core;
using LibManage.Services.Core.Contracts;
using LibManage.ViewModels.Books;

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

using Moq;


namespace Libmanage.Services.Test
{
    [TestFixture]
    class BookServiceTests
    {
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        private BookService CreateService(
            ApplicationDbContext context,
            Mock<IRatingService>? ratingServiceMock = null,
            Mock<IFileUploadService>? fileUploadServiceMock = null,
            Mock<IBorrowService>? borrowServiceMock = null)
        {
            return new BookService(
                context,
                ratingServiceMock?.Object ?? new Mock<IRatingService>().Object,
                fileUploadServiceMock?.Object ?? new Mock<IFileUploadService>().Object,
                borrowServiceMock?.Object ?? new Mock<IBorrowService>().Object
            );
        }
        private Author CreateAuthor()
        {
            return new Author
            {
                Id = Guid.NewGuid(),
                FullName = "Test Author",
                Photo = "test",
            };
        }

        private Publisher CreatePublisher()
        {
            return new Publisher
            {
                Id = Guid.NewGuid(),
                Name = "Test Publisher",
                LogoUrl = "test",
            };
        }


        [Test]
        public async Task CreateBookAsync_WithValidData_ReturnsTrue()
        {
            var context = GetDbContext();

            var fileUploadMock = new Mock<IFileUploadService>();
            fileUploadMock.Setup(x => x.UploadFileAsync(It.IsAny<IFormFile>(), Subfolders.Covers))
                .ReturnsAsync("/uploads/covers/test.png");

            var author = CreateAuthor();

            var publisher = CreatePublisher();

            context.Authors.Add(author);
            context.Publishers.Add(publisher);
            await context.SaveChangesAsync();

            var model = new AddBookInputModel
            {
                Title = "Test Book",
                ISBN = "1234567890",
                Language = "English",
                Type = "Digital",
                ReleaseDate = DateTime.UtcNow,
                Edition = "1st",
                Genre = "Fiction",
                Description = "Test description",
                AuthorId = author.Id,
                PublisherId = publisher.Id
            };

            var service = CreateService(context, fileUploadServiceMock: fileUploadMock);

            var result = await service.CreateBookAsync(model);

            Assert.IsTrue(result);
            Assert.AreEqual(1, context.Books.Count());
        }


        [Test]
        public async Task CreateBookAsync_WithDuplicateISBN_ReturnsFalse()
        {
            var context = GetDbContext();
            context.Books.Add(new Book { Id = Guid.NewGuid(), ISBN = "DUPLICATE", Cover = "test", Language = "test", Title = "test" });
            await context.SaveChangesAsync();

            var model = new AddBookInputModel { ISBN = "DUPLICATE" };

            var service = CreateService(context);

            var result = await service.CreateBookAsync(model);

            Assert.IsFalse(result);
        }

        [Test]
        public async Task DeleteBookAsync_WithValidId_DeletesBookAndReturnsTrue()
        {
            var context = GetDbContext();

            var fileMock = new Mock<IFileUploadService>();
            fileMock.Setup(f => f.DeleteFileAsync(It.IsAny<string>())).ReturnsAsync(true);

            var author = CreateAuthor();
            var publisher = CreatePublisher();

            var book = new Book
            {
                Id = Guid.NewGuid(),
                Title = "Book",
                Language = "test",
                ISBN = "testing",
                AuthorId = author.Id,
                PublisherId = publisher.Id,
                Cover = "test",
                BookFilePath = "test"
            };
            author.WrittenBooks.Add(book);
            publisher.Books.Add(book);

            context.Authors.Add(author);
            context.Publishers.Add(publisher);
            context.Books.Add(book);
            await context.SaveChangesAsync();

            var service = CreateService(context, fileUploadServiceMock: fileMock);

            var result = await service.DeleteBookAsync(book.Id);

            Assert.IsTrue(result);
            Assert.AreEqual(0, context.Books.Count());
        }

        [Test]
        public async Task GetAllBooksFromAuthorAsync_WithInvalidId_ReturnsNull()
        {
            var context = GetDbContext();
            var ratingMock = new Mock<IRatingService>();
            var service = CreateService(context, ratingServiceMock: ratingMock);

            var result = await service.GetAllBooksFromAuthorAsync(Guid.NewGuid());

            Assert.IsNull(result);
        }

        [Test]
        public async Task GetBookDetailsAsync_WithInvalidId_ReturnsNull()
        {
            var context = GetDbContext();
            var borrowMock = new Mock<IBorrowService>();
            var service = CreateService(context, borrowServiceMock: borrowMock);

            var result = await service.GetBookDetailsAsync(Guid.NewGuid());

            Assert.IsNull(result);
        }
        [Test]
        public async Task EditBookAsync_ShouldUpdateBookAndReturnTrue()
        {
            var context = GetDbContext();

            var author = CreateAuthor();
            var publisher = CreatePublisher();

            var book = new Book
            {
                Id = Guid.NewGuid(),
                Title = "Original Title",
                ISBN = "1111111111",
                Language = "English",
                Cover = "/covers/old.png",
                Type = Book.BookType.Digital,
                Author = author,
                Publisher = publisher
            };

            context.Authors.Add(author);
            context.Publishers.Add(publisher);
            context.Books.Add(book);
            await context.SaveChangesAsync();

            var model = new EditBookInputModel
            {
                Id = book.Id,
                Title = "Updated Title",
                ISBN = "2222222222",
                Language = "French",
                Type = "Physical",
                AuthorId = author.Id,
                PublisherId = publisher.Id,
                Description = "Updated description",
                NewCover = null, // simulate no new cover
                NewBookFile = null   // simulate no new book file
            };

            var fileUploadMock = new Mock<IFileUploadService>();
            var service = CreateService(context, fileUploadServiceMock: fileUploadMock);

            var result = await service.UpdateBookAsync(model);

            Assert.IsTrue(result);
            var updatedBook = await context.Books.FindAsync(book.Id);
            Assert.AreEqual("Updated Title", updatedBook?.Title);
            Assert.AreEqual("2222222222", updatedBook?.ISBN);
            Assert.AreEqual(Book.BookType.Physical, updatedBook?.Type);
        }
        [Test]
        public async Task EditBookAsync_ShouldReturnFalseIfBookNotFound()
        {
            var context = GetDbContext();

            var model = new EditBookInputModel
            {
                Id = Guid.NewGuid(), // nonexistent book
                Title = "Title",
                ISBN = "ISBN",
                Language = "English",
                Type = "Audio",
                AuthorId = Guid.NewGuid(),
                PublisherId = Guid.NewGuid()
            };

            var service = CreateService(context);

            var result = await service.UpdateBookAsync(model);

            Assert.IsFalse(result);
        }
        [Test]
        public async Task DeleteBookAsync_DeletesBookSuccessfully()
        {
            var context = GetDbContext();
            var fileUploadMock = new Mock<IFileUploadService>();
            fileUploadMock
                .Setup(f => f.DeleteFileAsync(It.IsAny<string>()))
                .ReturnsAsync(true); // <-- FIX

            var service = CreateService(context, fileUploadServiceMock: fileUploadMock);

            var author = CreateAuthor();
            var publisher = CreatePublisher();

            var book = new Book
            {
                Id = Guid.NewGuid(),
                Title = "Book to Delete",
                ISBN = "1234567890",
                Language = "English",
                Type = Book.BookType.Digital,
                Cover = "delete",
                AuthorId = author.Id,
                Author = author,
                PublisherId = publisher.Id,
                Publisher = publisher
            };

            context.Authors.Add(author);
            context.Publishers.Add(publisher);
            context.Books.Add(book);
            await context.SaveChangesAsync();

            var result = await service.DeleteBookAsync(book.Id);

            var deletedBook = await context.Books.FindAsync(book.Id);
            Assert.IsTrue(result);
            Assert.IsNull(deletedBook);
        }

        [Test]
        public async Task GetAllBooksAsync_ReturnsCorrectBooksBasedOnFilters()
        {
            var context = GetDbContext();
            var author = CreateAuthor();
            var book = new Book
            {
                Id = Guid.NewGuid(),
                Title = "The Great Book",
                ISBN = "111111",
                Language = "English",
                Type = Book.BookType.Digital,
                Author = author,
                AuthorId = author.Id,
                Publisher = CreatePublisher(),
                PublisherId = Guid.NewGuid(),
                Cover = "delete"
            };

            context.Authors.Add(author);
            context.Books.Add(book);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var options = new BookFilterOptions
            {
                SearchTerm = "Great",
                BookType = "Digital"
            };

            var result = await service.GetAllBooksAsync(options);

            Assert.That(result.Books.Count, Is.EqualTo(1));
            Assert.That(result.Books.ToList()[0].Title, Is.EqualTo("The Great Book"));
        }
        [Test]
        public async Task GetAllBooksFromPublisherAsync_WithValidPublisherId_ReturnsBooks()
        {
            var context = GetDbContext();
            var ratingMock = new Mock<IRatingService>();
            ratingMock.Setup(r => r.GetRatingForABookByIdAsync(It.IsAny<Guid>())).ReturnsAsync(4);

            var publisher = CreatePublisher();
            var author = CreateAuthor();
            var userId = Guid.NewGuid();

            var book = new Book
            {
                Id = Guid.NewGuid(),
                Title = "Publisher's Book",
                ISBN = "12345678",
                Language = "English",
                Type = Book.BookType.Digital,
                Publisher = publisher,
                PublisherId = publisher.Id,
                Author = author,
                AuthorId = author.Id,
                Cover = "test",
                Borrows = new List<Borrow>()
            };

            context.Publishers.Add(publisher);
            context.Authors.Add(author);
            context.Books.Add(book);
            await context.SaveChangesAsync();

            var service = CreateService(context, ratingServiceMock: ratingMock);

            var result = await service.GetAllBooksFromPublisherAsync(publisher.Id, userId);

            Assert.IsNotNull(result);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Title, Is.EqualTo("Publisher's Book"));
            Assert.That(result[0].Rating, Is.EqualTo(4));
        }
        [Test]
        public async Task GetAllBooksFromPublisherAsync_WithInvalidPublisherId_ReturnsNull()
        {
            var context = GetDbContext();
            var ratingMock = new Mock<IRatingService>();
            var service = CreateService(context, ratingServiceMock: ratingMock);

            var result = await service.GetAllBooksFromPublisherAsync(Guid.NewGuid());

            Assert.IsNull(result);
        }
        [Test]
        public async Task GetAudioBookPlayerViewModelAsync_WithValidAudioBook_ReturnsModel()
        {
            var context = GetDbContext();
            var author = CreateAuthor();

            var audioBook = new Book
            {
                Id = Guid.NewGuid(),
                ISBN = "12232323",
                Title = "Audio Book",
                Author = author,
                AuthorId = author.Id,
                Language = "English",
                Type = Book.BookType.Audio,
                Cover = "uploads/covers/audio.jpg",
                Description = "Listen to this",
                Duration = TimeSpan.FromMinutes(90),
                UploadDate = DateTime.UtcNow,
                BookFilePath = "audio"
            };

            context.Authors.Add(author);
            context.Books.Add(audioBook);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var result = await service.GetAudioBookPlayerViewModelAsync(audioBook.Id);

            Assert.IsNotNull(result);
            Assert.That(result.Title, Is.EqualTo("Audio Book"));
            Assert.That(result.FilePath, Is.EqualTo("audio"));
            Assert.That(result.Duration, Is.EqualTo(TimeSpan.FromMinutes(90)));
        }
        [Test]
        public async Task GetAudioBookPlayerViewModelAsync_WithInvalidId_ReturnsNull()
        {
            var context = GetDbContext();
            var service = CreateService(context);

            var result = await service.GetAudioBookPlayerViewModelAsync(Guid.NewGuid());

            Assert.IsNull(result);
        }
        [Test]
        public async Task GetAudioBookPlayerViewModelAsync_WithNonAudioBook_ReturnsNull()
        {
            var context = GetDbContext();
            var author = CreateAuthor();

            var book = new Book
            {
                Id = Guid.NewGuid(),
                ISBN = "Sdsdsd",
                Cover = "sdsdsd",
                Language = "en",
                Title = "Not Audio",
                Author = author,
                AuthorId = author.Id,
                Type = Book.BookType.Physical
            };

            context.Authors.Add(author);
            context.Books.Add(book);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var result = await service.GetAudioBookPlayerViewModelAsync(book.Id);

            Assert.IsNull(result);
        }
        [Test]
        public async Task GetBookByIdAsync_WithValidId_ReturnsBook()
        {
            var context = GetDbContext();
            var author = CreateAuthor();

            var book = new Book
            {
                Id = Guid.NewGuid(),
                ISBN = "23sodio2",
                Cover = "test",
                Language = "en",
                Title = "Find Me",
                Author = author,
                AuthorId = author.Id
            };

            context.Authors.Add(author);
            context.Books.Add(book);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var result = await service.GetBookByIdAsync(book.Id);

            Assert.IsNotNull(result);
            Assert.That(result.Title, Is.EqualTo("Find Me"));
        }
        [Test]
        public async Task GetBookByIdAsync_WithInvalidId_ReturnsNull()
        {
            var context = GetDbContext();
            var service = CreateService(context);

            var result = await service.GetBookByIdAsync(Guid.NewGuid());

            Assert.IsNull(result);
        }
        [Test]
        public async Task GetBookEditModelAsync_WithValidId_ReturnsEditModel()
        {
            var context = GetDbContext();
            var author = CreateAuthor();
            var publisher = CreatePublisher();

            var book = new Book
            {
                Id = Guid.NewGuid(),
                Title = "Editable Book",
                ISBN = "EDIT123",
                Language = "English",
                Type = Book.BookType.Audio,
                Cover = "cover",
                BookFilePath = "bookFile",
                Description = "Description",
                Genre = "Sci-Fi",
                Edition = "2nd",
                ReleaseDate = DateTime.UtcNow,
                Duration = TimeSpan.FromMinutes(90),
                Author = author,
                AuthorId = author.Id,
                Publisher = publisher,
                PublisherId = publisher.Id
            };

            context.Authors.Add(author);
            context.Publishers.Add(publisher);
            context.Books.Add(book);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var result = await service.GetBookEditModelAsync(book.Id);

            Assert.IsNotNull(result);
            Assert.That(result.Title, Is.EqualTo("Editable Book"));
            Assert.That(result.ExistingCoverPath, Is.EqualTo("cover"));
            Assert.That(result.ExistingFilePath, Is.EqualTo("bookFile"));
            Assert.That(result.Authors.Count, Is.EqualTo(1));
            Assert.That(result.Publishers.Count, Is.EqualTo(1));
        }
        [Test]
        public async Task GetBookEditModelAsync_WithInvalidId_ReturnsNull()
        {
            var context = GetDbContext();
            var service = CreateService(context);

            var result = await service.GetBookEditModelAsync(Guid.NewGuid());

            Assert.IsNull(result);
        }
        [Test]
        public async Task GetBookInputModelAsync_ReturnsAuthorsAndPublishers()
        {
            var context = GetDbContext();

            var author = CreateAuthor();
            var publisher = CreatePublisher();

            context.Authors.Add(author);
            context.Publishers.Add(publisher);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var result = await service.GetBookInputModelAsync();

            Assert.IsNotNull(result);
            Assert.That(result.Authors.Count, Is.EqualTo(1));
            Assert.That(result.Authors[0].Id, Is.EqualTo(author.Id));
            Assert.That(result.Publishers.Count, Is.EqualTo(1));
            Assert.That(result.Publishers[0].Id, Is.EqualTo(publisher.Id));
        }
        [Test]
        public async Task GetDeletedBookDetailsAsync_WithValidId_ReturnsModel()
        {
            var context = GetDbContext();
            var author = CreateAuthor();
            var publisher = CreatePublisher();

            var book = new Book
            {
                Id = Guid.NewGuid(),
                ISBN = "test",
                Language = "en",
                Title = "Deletable Book",
                Cover = "test",
                Author = author,
                Publisher = publisher
            };

            context.Authors.Add(author);
            context.Publishers.Add(publisher);
            context.Books.Add(book);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var result = await service.GetDeletedBookDetailsAsync(book.Id);

            Assert.IsNotNull(result);
            Assert.That(result.Id, Is.EqualTo(book.Id));
            Assert.That(result.Title, Is.EqualTo("Deletable Book"));
            Assert.That(result.Cover, Is.EqualTo("test"));
        }


    }
}
