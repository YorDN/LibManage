using LibManage.Data.Models.Library;
using LibManage.Data;
using LibManage.Services.Core;

using Microsoft.EntityFrameworkCore;

using static LibManage.Data.Models.Library.Book;

namespace Libmanage.Services.Test
{
    [TestFixture]
    public class BorrowServiceTests
    {
        private ApplicationDbContext _dbContext;
        private BorrowService _borrowService;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ApplicationDbContext(options);
            _borrowService = new BorrowService(_dbContext);
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Dispose();
        }

        private User CreateTestUser()
        {
            return new User
            {
                UserName = "testuser",
                ProfilePicture = "test",
                Email = "test@example.com",
                IsActive = true
            };
        }

        private Book CreateTestBook(BookType type = BookType.Physical)
        {
            return new Book
            {
                Title = "Test Book",
                ISBN = "1234567890123",
                Language = "English",
                Cover = "cover.jpg",
                Type = type,
                Author = new Author { FullName = "Test Author", Photo = "author.jpg" },
                Publisher = new Publisher { Name = "Test Publisher", LogoUrl = "logo.jpg" }
            };
        }

        private Borrow CreateTestBorrow(Guid userId, Guid bookId, bool returned = false)
        {
            return new Borrow
            {
                UserId = userId,
                BookId = bookId,
                DateTaken = DateTime.UtcNow.AddDays(-5),
                DateDue = DateTime.UtcNow.AddDays(9),
                Returned = returned
            };
        }

        [Test]
        public async Task GetUsersBorrowedBooksAsync_ReturnsActiveBorrows()
        {
            var user = CreateTestUser();
            var book1 = CreateTestBook();
            var book2 = CreateTestBook();
            await _dbContext.Users.AddAsync(user);
            await _dbContext.Books.AddRangeAsync(book1, book2);
            await _dbContext.Borrows.AddRangeAsync(
                CreateTestBorrow(user.Id, book1.Id),
                CreateTestBorrow(user.Id, book2.Id, returned: true)
            );
            await _dbContext.SaveChangesAsync();

            var result = await _borrowService.GetUsersBorrowedBooksAsync(user.Id);

            Assert.NotNull(result);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(book1.Title, result.First().Title);
        }

        [Test]
        public async Task GetUsersBorrowedBooksAsync_ReturnsNullForInvalidUser()
        {
            var result = await _borrowService.GetUsersBorrowedBooksAsync(Guid.NewGuid());

            Assert.Null(result);
        }

        [Test]
        public async Task GetUsersBorrowedBooksAsync_MarksOverdueBooksAsReturned()
        {
            var user = CreateTestUser();
            var book = CreateTestBook();
            var overdueBorrow = new Borrow
            {
                UserId = user.Id,
                BookId = book.Id,
                DateTaken = DateTime.UtcNow.AddDays(-15),
                DateDue = DateTime.UtcNow.AddDays(-1),
                Returned = false
            };

            await _dbContext.Users.AddAsync(user);
            await _dbContext.Books.AddAsync(book);
            await _dbContext.Borrows.AddAsync(overdueBorrow);
            await _dbContext.SaveChangesAsync();

            var result = await _borrowService.GetUsersBorrowedBooksAsync(user.Id);

            Assert.NotNull(result);
            Assert.AreEqual(0, result.Count());
            var updatedBorrow = await _dbContext.Borrows.FindAsync(overdueBorrow.Id);
            Assert.True(updatedBorrow.Returned);
        }

        [Test]
        public async Task HasActiveBorrowsAsync_ReturnsFalseForNullUserId()
        {
            var book = CreateTestBook();
            await _dbContext.Books.AddAsync(book);
            await _dbContext.SaveChangesAsync();

            var result = await _borrowService.HasActiveBorrowsAsync(null, book.Id);

            Assert.False(result);
        }

        [Test]
        public async Task HasActiveBorrowsAsync_ReturnsTrueForActiveBorrow()
        {
            var user = CreateTestUser();
            var book = CreateTestBook();
            await _dbContext.Users.AddAsync(user);
            await _dbContext.Books.AddAsync(book);
            await _dbContext.Borrows.AddAsync(CreateTestBorrow(user.Id, book.Id));
            await _dbContext.SaveChangesAsync();

            var result = await _borrowService.HasActiveBorrowsAsync(user.Id, book.Id);

            Assert.True(result);
        }

        [Test]
        public async Task RentBookAsync_ReturnsFalseForInvalidUserOrBook()
        {
            var result1 = await _borrowService.RentBookAsync(Guid.NewGuid(), Guid.NewGuid());
            var result2 = await _borrowService.RentBookAsync(Guid.NewGuid(), Guid.NewGuid());

            Assert.False(result1);
            Assert.False(result2);
        }

        [Test]
        public async Task RentBookAsync_ReturnsFalseIfAlreadyBorrowed()
        {
            var user = CreateTestUser();
            var book = CreateTestBook();
            await _dbContext.Users.AddAsync(user);
            await _dbContext.Books.AddAsync(book);
            await _dbContext.Borrows.AddAsync(CreateTestBorrow(user.Id, book.Id));
            await _dbContext.SaveChangesAsync();

            var result = await _borrowService.RentBookAsync(user.Id, book.Id);

            Assert.False(result);
        }

        [Test]
        public async Task RentBookAsync_ReturnsTrueForSuccessfulRental()
        {
            var user = CreateTestUser();
            var book = CreateTestBook();
            await _dbContext.Users.AddAsync(user);
            await _dbContext.Books.AddAsync(book);
            await _dbContext.SaveChangesAsync();

            var result = await _borrowService.RentBookAsync(user.Id, book.Id);

            Assert.True(result);
            var borrow = await _dbContext.Borrows.FirstOrDefaultAsync();
            Assert.NotNull(borrow);
            Assert.AreEqual(user.Id, borrow.UserId);
            Assert.AreEqual(book.Id, borrow.BookId);
        }

        [Test]
        public async Task ReturnBookAsync_ReturnsFalseForInvalidUserOrBorrow()
        {
            var result1 = await _borrowService.ReturnBookAsync(Guid.NewGuid(), Guid.NewGuid());
            var result2 = await _borrowService.ReturnBookAsync(Guid.NewGuid(), Guid.NewGuid());

            Assert.False(result1);
            Assert.False(result2);
        }

        [Test]
        public async Task ReturnBookAsync_ReturnsTrueForSuccessfulReturn()
        {
            var user = CreateTestUser();
            var book = CreateTestBook();
            var borrow = CreateTestBorrow(user.Id, book.Id);
            await _dbContext.Users.AddAsync(user);
            await _dbContext.Books.AddAsync(book);
            await _dbContext.Borrows.AddAsync(borrow);
            await _dbContext.SaveChangesAsync();

            var result = await _borrowService.ReturnBookAsync(user.Id, borrow.Id);

            Assert.True(result);
            var updatedBorrow = await _dbContext.Borrows.FindAsync(borrow.Id);
            Assert.True(updatedBorrow.Returned);
            Assert.NotNull(updatedBorrow.DateReturned);
        }

    }
}
