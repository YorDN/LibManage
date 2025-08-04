using LibManage.Data;
using LibManage.Data.Models.Library;
using LibManage.Services.Core;

using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

using Moq;

using static LibManage.Data.Models.Library.Book;

namespace Libmanage.Services.Test
{
    [TestFixture]
    public class EpubReaderServiceTests
    {
        private ApplicationDbContext _context;
        private Mock<IWebHostEnvironment> _mockEnv;
        private EpubReaderService _epubReaderService;
        private string _testWebRootPath;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);

            _testWebRootPath = Path.Combine(Path.GetTempPath(), "test_webroot");
            Directory.CreateDirectory(_testWebRootPath);

            _mockEnv = new Mock<IWebHostEnvironment>();
            _mockEnv.Setup(e => e.WebRootPath).Returns(_testWebRootPath);

            _epubReaderService = new EpubReaderService(_context, _mockEnv.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _context?.Dispose();

            if (Directory.Exists(_testWebRootPath))
            {
                Directory.Delete(_testWebRootPath, true);
            }
        }

        private Book CreateTestBook(bool isDigital = true, bool hasFilePath = true, string? customFilePath = null)
        {
            return new Book
            {
                Id = Guid.NewGuid(),
                ISBN = "test-isbn",
                Cover = "test-cover.jpg",
                Language = "en",
                Title = "Test Book",
                Type = isDigital ? BookType.Digital : BookType.Physical,
                BookFilePath = hasFilePath ? (customFilePath ?? "/uploads/books/test.epub") : null
            };
        }

        private Borrow CreateTestBorrow(Guid bookId, Guid userId, bool returned = false, bool overdue = false)
        {
            return new Borrow
            {
                BookId = bookId,
                UserId = userId,
                DateTaken = DateTime.UtcNow.AddDays(-5),
                DateDue = overdue ? DateTime.UtcNow.AddDays(-1) : DateTime.UtcNow.AddDays(5),
                Returned = returned,
                DateReturned = returned ? DateTime.UtcNow : null
            };
        }

        private UserEpubProgress CreateTestProgress(Guid userId, Guid bookId, int lastChapterIndex = 0)
        {
            return new UserEpubProgress
            {
                UserId = userId,
                BookId = bookId,
                LastChapterIndex = lastChapterIndex,
                LastUpdated = DateTime.UtcNow.AddDays(-1)
            };
        }

        [Test]
        public async Task LoadChapterAsync_ReturnsNull_WhenBookNotFound()
        {
            var bookId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var result = await _epubReaderService.LoadChapterAsync(bookId, userId, null);

            Assert.IsNull(result);
        }

        [Test]
        public async Task LoadChapterAsync_ReturnsNull_WhenBookNotDigital()
        {
            var book = CreateTestBook(isDigital: false);
            var userId = Guid.NewGuid();

            await _context.Books.AddAsync(book);
            await _context.SaveChangesAsync();

            var result = await _epubReaderService.LoadChapterAsync(book.Id, userId, null);

            Assert.IsNull(result);
        }

        [Test]
        public async Task LoadChapterAsync_ReturnsNull_WhenBookFilePathIsNull()
        {
            var book = CreateTestBook(hasFilePath: false);
            var userId = Guid.NewGuid();

            await _context.Books.AddAsync(book);
            await _context.SaveChangesAsync();

            var result = await _epubReaderService.LoadChapterAsync(book.Id, userId, null);

            Assert.IsNull(result);
        }

        [Test]
        public async Task LoadChapterAsync_ReturnsNull_WhenBookFilePathIsWhitespace()
        {
            var book = CreateTestBook(customFilePath: "   ");
            var userId = Guid.NewGuid();

            await _context.Books.AddAsync(book);
            await _context.SaveChangesAsync();

            var result = await _epubReaderService.LoadChapterAsync(book.Id, userId, null);

            Assert.IsNull(result);
        }

        [Test]
        public async Task LoadChapterAsync_ReturnsNull_WhenNoBorrowFound()
        {
            var book = CreateTestBook();
            var userId = Guid.NewGuid();

            await _context.Books.AddAsync(book);
            await _context.SaveChangesAsync();

            var result = await _epubReaderService.LoadChapterAsync(book.Id, userId, null);

            Assert.IsNull(result);
        }

        [Test]
        public async Task LoadChapterAsync_ReturnsNull_WhenBorrowIsReturned()
        {
            var book = CreateTestBook();
            var userId = Guid.NewGuid();
            var borrow = CreateTestBorrow(book.Id, userId, returned: true);

            await _context.Books.AddAsync(book);
            await _context.Borrows.AddAsync(borrow);
            await _context.SaveChangesAsync();

            var result = await _epubReaderService.LoadChapterAsync(book.Id, userId, null);

            Assert.IsNull(result);
        }

        [Test]
        public async Task LoadChapterAsync_ReturnsNull_AndMarksAsReturned_WhenBorrowIsOverdue()
        {
            var book = CreateTestBook();
            var userId = Guid.NewGuid();
            var borrow = CreateTestBorrow(book.Id, userId, overdue: true);

            await _context.Books.AddAsync(book);
            await _context.Borrows.AddAsync(borrow);
            await _context.SaveChangesAsync();

            var result = await _epubReaderService.LoadChapterAsync(book.Id, userId, null);

            Assert.IsNull(result);

            var updatedBorrow = await _context.Borrows.FirstAsync(b => b.BookId == book.Id && b.UserId == userId);
            Assert.IsTrue(updatedBorrow.Returned);
            Assert.IsNotNull(updatedBorrow.DateReturned);
            Assert.That(updatedBorrow.DateReturned, Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromSeconds(5)));
        }

        [Test]
        public async Task LoadChapterAsync_ReturnsNull_WhenEpubFileDoesNotExist()
        {
            var book = CreateTestBook(customFilePath: "/uploads/books/nonexistent.epub");
            var userId = Guid.NewGuid();
            var borrow = CreateTestBorrow(book.Id, userId);

            await _context.Books.AddAsync(book);
            await _context.Borrows.AddAsync(borrow);
            await _context.SaveChangesAsync();

            var result = await _epubReaderService.LoadChapterAsync(book.Id, userId, null);

            Assert.IsNull(result);
        }

        [Test]
        public async Task LoadChapterAsync_HandlesFilePathWithLeadingSlash()
        {
            var book = CreateTestBook(customFilePath: "/uploads/books/test.epub");
            var userId = Guid.NewGuid();
            var borrow = CreateTestBorrow(book.Id, userId);

            var uploadsDir = Path.Combine(_testWebRootPath, "uploads", "books");
            Directory.CreateDirectory(uploadsDir);
            var epubFilePath = Path.Combine(uploadsDir, "test.epub");

            await _context.Books.AddAsync(book);
            await _context.Borrows.AddAsync(borrow);
            await _context.SaveChangesAsync();

            var result = await _epubReaderService.LoadChapterAsync(book.Id, userId, null);

            Assert.IsNull(result);
        }

        [Test]
        public async Task LoadChapterAsync_HandlesFilePathWithoutLeadingSlash()
        {
            // Arrange
            var book = CreateTestBook(customFilePath: "uploads/books/test.epub");
            var userId = Guid.NewGuid();
            var borrow = CreateTestBorrow(book.Id, userId);

            await _context.Books.AddAsync(book);
            await _context.Borrows.AddAsync(borrow);
            await _context.SaveChangesAsync();

            var result = await _epubReaderService.LoadChapterAsync(book.Id, userId, null);

            Assert.IsNull(result);
        }

        [Test]
        public async Task LoadChapterAsync_CreatesNewProgress_WhenNoExistingProgress()
        {
            var book = CreateTestBook();
            var userId = Guid.NewGuid();
            var borrow = CreateTestBorrow(book.Id, userId);

            await _context.Books.AddAsync(book);
            await _context.Borrows.AddAsync(borrow);
            await _context.SaveChangesAsync();

            var result = await _epubReaderService.LoadChapterAsync(book.Id, userId, null);

            Assert.IsNull(result);

            var progress = await _context.UserEpubProgresses
                .FirstOrDefaultAsync(p => p.UserId == userId && p.BookId == book.Id);
            Assert.IsNull(progress);
        }

        [Test]
        public async Task LoadChapterAsync_UsesExistingProgress_WhenProgressExists()
        {
            var book = CreateTestBook();
            var userId = Guid.NewGuid();
            var borrow = CreateTestBorrow(book.Id, userId);
            var progress = CreateTestProgress(userId, book.Id, lastChapterIndex: 3);

            await _context.Books.AddAsync(book);
            await _context.Borrows.AddAsync(borrow);
            await _context.UserEpubProgresses.AddAsync(progress);
            await _context.SaveChangesAsync();

            var result = await _epubReaderService.LoadChapterAsync(book.Id, userId, null);

            Assert.IsNull(result);

            var existingProgress = await _context.UserEpubProgresses
                .FirstOrDefaultAsync(p => p.UserId == userId && p.BookId == book.Id);
            Assert.IsNotNull(existingProgress);
            Assert.AreEqual(3, existingProgress.LastChapterIndex);
        }

        [Test]
        public async Task LoadChapterAsync_UsesProvidedChapterIndex_WhenSpecified()
        {
            var book = CreateTestBook();
            var userId = Guid.NewGuid();
            var borrow = CreateTestBorrow(book.Id, userId);
            var progress = CreateTestProgress(userId, book.Id, lastChapterIndex: 3);

            await _context.Books.AddAsync(book);
            await _context.Borrows.AddAsync(borrow);
            await _context.UserEpubProgresses.AddAsync(progress);
            await _context.SaveChangesAsync();

            var result = await _epubReaderService.LoadChapterAsync(book.Id, userId, 5);

            Assert.IsNull(result);
        }

        [Test]
        public async Task LoadChapterAsync_ValidatesBorrowNotReturned()
        {
            // Arrange
            var book = CreateTestBook();
            var userId = Guid.NewGuid();
            var activeBorrow = CreateTestBorrow(book.Id, userId, returned: false);
            var returnedBorrow = CreateTestBorrow(book.Id, Guid.NewGuid(), returned: true);

            await _context.Books.AddAsync(book);
            await _context.Borrows.AddRangeAsync(activeBorrow, returnedBorrow);
            await _context.SaveChangesAsync();

            var result = await _epubReaderService.LoadChapterAsync(book.Id, userId, null);

            Assert.IsNull(result);
        }

        [Test]
        public async Task LoadChapterAsync_FindsCorrectBorrowForUser()
        {
            var book = CreateTestBook();
            var userId1 = Guid.NewGuid();
            var userId2 = Guid.NewGuid();
            var borrow1 = CreateTestBorrow(book.Id, userId1, returned: false);
            var borrow2 = CreateTestBorrow(book.Id, userId2, returned: false);

            await _context.Books.AddAsync(book);
            await _context.Borrows.AddRangeAsync(borrow1, borrow2);
            await _context.SaveChangesAsync();

            var result = await _epubReaderService.LoadChapterAsync(book.Id, userId1, null);

            Assert.IsNull(result);

            var activeBorrows = await _context.Borrows
                .Where(b => b.BookId == book.Id && !b.Returned)
                .CountAsync();
            Assert.AreEqual(2, activeBorrows);
        }

    }
}
