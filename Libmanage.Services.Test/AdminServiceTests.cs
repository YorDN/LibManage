using LibManage.Data.Models.Library;
using LibManage.Data;
using LibManage.Services.Core;

using Microsoft.EntityFrameworkCore;
using static LibManage.Data.Models.Library.Book;

namespace Libmanage.Services.Test
{
    [TestFixture]
    public class AdminServiceTests
    {
        private ApplicationDbContext _dbContext;
        private AdminService _adminService;

        [SetUp]
        public void Setup()
        {
            _dbContext = GetDbContext();
            _adminService = CreateService(_dbContext);
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

        private AdminService CreateService(ApplicationDbContext context)
        {
            return new AdminService(context);
        }

        private Author CreateTestAuthor()
        {
            return new Author
            {
                FullName = "Test Author",
                Photo = "author.jpg"
            };
        }

        private Publisher CreateTestPublisher()
        {
            return new Publisher
            {
                Name = "Test Publisher",
                LogoUrl = "logo.jpg"
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
                Author = CreateTestAuthor(),
                Publisher = CreateTestPublisher()
            };
        }

        private User CreateTestUser()
        {
            return new User
            {
                Id = Guid.NewGuid(),
                UserName = "testuser",
                Email = "test@example.com",
                ProfilePicture = "test",
                IsActive = true
            };
        }

        [Test]
        public async Task GetAdminDashboardDetailsAsync_ReturnsCorrectCounts()
        {
            var author1 = CreateTestAuthor();
            var author2 = CreateTestAuthor();
            var publisher1 = CreateTestPublisher();
            var publisher2 = CreateTestPublisher();
            var publisher3 = CreateTestPublisher();

            var users = new List<User>
            {
                CreateTestUser(),
                CreateTestUser()
            };

            var books = new List<Book>
            {
                new Book
                {
                    Title = "Book 1",
                    ISBN = "1111111111111",
                    Language = "English",
                    Cover = "cover1.jpg",
                    Type = BookType.Physical,
                    Author = author1,
                    Publisher = publisher1
                },
                new Book
                {
                    Title = "Book 2",
                    ISBN = "2222222222222",
                    Language = "English",
                    Cover = "cover2.jpg",
                    Type = BookType.Physical,
                    Author = author1,
                    Publisher = publisher1
                },
                new Book
                {
                    Title = "Book 3",
                    ISBN = "3333333333333",
                    Language = "English",
                    Cover = "cover3.jpg",
                    Type = BookType.Audio,
                    Author = author1,
                    Publisher = publisher1
                },
                new Book
                {
                    Title = "Book 4",
                    ISBN = "4444444444444",
                    Language = "English",
                    Cover = "cover4.jpg",
                    Type = BookType.Digital,
                    Author = author1,
                    Publisher = publisher1
                },
                new Book
                {
                    Title = "Book 5",
                    ISBN = "5555555555555",
                    Language = "English",
                    Cover = "cover5.jpg",
                    Type = BookType.Digital,
                    Author = author1,
                    Publisher = publisher1
                },
                new Book
                {
                    Title = "Book 6",
                    ISBN = "6666666666666",
                    Language = "English",
                    Cover = "cover6.jpg",
                    Type = BookType.Digital,
                    Author = author1,
                    Publisher = publisher1
                }
            };

            await _dbContext.Authors.AddRangeAsync(author1, author2);
            await _dbContext.Publishers.AddRangeAsync(publisher1, publisher2, publisher3);
            await _dbContext.Users.AddRangeAsync(users);
            await _dbContext.Books.AddRangeAsync(books);
            await _dbContext.SaveChangesAsync();

            var result = await _adminService.GetAdminDashboardDetailsAsync();

            Assert.That(result.PhysicalBooks, Is.EqualTo(2));
            Assert.That(result.AudioBooks, Is.EqualTo(1));
            Assert.That(result.DigitalBooks, Is.EqualTo(3));
            Assert.That(result.TotalUsers, Is.EqualTo(2));
            Assert.That(result.TotalPublishers, Is.EqualTo(3));
            Assert.That(result.TotalBooks, Is.EqualTo(6));
            Assert.That(result.TotalAuthors, Is.EqualTo(2));
        }

        [Test]
        public async Task GetAdminDashboardDetailsAsync_CalculatesActiveUsersCorrectly()
        {
            await _dbContext.Users.AddRangeAsync(
                new User
                {
                    UserName = "user1",
                    Email = "user1@test.com",
                    ProfilePicture = "test",
                    IsActive = true,
                    LastLogin = DateTime.UtcNow.AddDays(-10)
                },
                new User
                {
                    UserName = "user2",
                    Email = "user2@test.com",
                    ProfilePicture = "test",
                    IsActive = true,
                    LastLogin = DateTime.UtcNow.AddDays(-20)
                },
                new User
                {
                    UserName = "user3",
                    Email = "user3@test.com",
                    ProfilePicture = "test",
                    IsActive = true,
                    LastLogin = DateTime.UtcNow.AddDays(-40)
                }
            );

            await _dbContext.SaveChangesAsync();

            var result = await _adminService.GetAdminDashboardDetailsAsync();

            Assert.That(result.ActiveUsersLast30Days, Is.EqualTo(2));
        }

        [Test]
        public async Task GetAdminDashboardDetailsAsync_ReturnsMostActiveUser()
        {
            var user1 = CreateTestUser();
            var user2 = CreateTestUser();
            user2.UserName = "user2";
            user2.Email = "user2@test.com";

            var book = CreateTestBook();

            await _dbContext.Users.AddRangeAsync(user1, user2);
            await _dbContext.Books.AddAsync(book);
            await _dbContext.SaveChangesAsync();

            await _dbContext.Borrows.AddRangeAsync(
                new Borrow { User = user1, Book = book },
                new Borrow { User = user1, Book = book }
            );

            await _dbContext.SaveChangesAsync();

            var result = await _adminService.GetAdminDashboardDetailsAsync();

            Assert.That(result.MostActiveUser, Is.EqualTo("testuser"));
        }

        [Test]
        public async Task GetAllUsersAsync_ReturnsPaginatedResults()
        {
            await _dbContext.Users.AddRangeAsync(
                new User
                {
                    UserName = "user1",
                    Email = "user1@test.com",
                    ProfilePicture = "test",
                    IsActive = true
                },
                new User
                {
                    UserName = "user2",
                    ProfilePicture = "test",
                    Email = "user2@test.com",
                    IsActive = true
                },
                new User
                {
                    UserName = "user3",
                    Email = "user3@test.com",
                    ProfilePicture = "test",
                    IsActive = false
                }
            );

            await _dbContext.SaveChangesAsync();

            var (result, totalCount) = await _adminService.GetAllUsersAsync(1, 1);

            Assert.That(totalCount, Is.EqualTo(2));
            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.First().FullName, Is.EqualTo("user1"));
        }
    }
}
