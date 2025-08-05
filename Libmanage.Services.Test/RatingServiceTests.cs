using LibManage.Data.Models.Library;
using LibManage.Data;
using LibManage.Services.Core;
using LibManage.ViewModels.Rating;
using Microsoft.EntityFrameworkCore;

namespace Libmanage.Services.Test
{
    [TestFixture]
    public class RatingServiceTests
    {
        private ApplicationDbContext _context;
        private RatingService _service;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _service = new RatingService(_context);
        }
        [TearDown]
        public void TearDown()
        {
            _context?.Dispose();
        }

        private User CreateUser() => new User { Id = Guid.NewGuid(), UserName = "testuser", ProfilePicture = "pfp.png" };

        private Book CreateBook() => new Book { Id = Guid.NewGuid(), Title = "Test Book", Cover = "test", ISBN = "test", Language = "jshd" };

        [Test]
        public async Task AddReviewAsync_WithValidData_AddsReview()
        {
            var book = CreateBook();
            var user = CreateUser();

            await _context.Books.AddAsync(book);
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var model = new ReviewInputModel { Rating = 5, Comment = "Great book!" };
            var result = await _service.AddReviewAsync(model, book.Id, user.Id);

            Assert.IsTrue(result);
            Assert.AreEqual(1, _context.Reviews.Count());
        }

        [Test]
        public async Task AddReviewAsync_WithExistingReview_ReturnsFalse()
        {
            var book = CreateBook();
            var user = CreateUser();
            var review = new Review { BookId = book.Id, UserId = user.Id, Rating = 5, Comment = "Test", CreatedAt = DateTime.UtcNow };

            await _context.Books.AddAsync(book);
            await _context.Users.AddAsync(user);
            await _context.Reviews.AddAsync(review);
            await _context.SaveChangesAsync();

            var model = new ReviewInputModel { Rating = 4, Comment = "Another review" };
            var result = await _service.AddReviewAsync(model, book.Id, user.Id);

            Assert.IsFalse(result);
        }

        [Test]
        public async Task ApproveReviewAsync_WithValidId_ApprovesReview()
        {
            var review = new Review { Id = Guid.NewGuid(), Rating = 4, IsApproved = false, BookId = Guid.NewGuid(), UserId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow };
            await _context.Reviews.AddAsync(review);
            await _context.SaveChangesAsync();

            var result = await _service.ApproveReviewAsync(review.Id);

            Assert.IsTrue(result);
            Assert.IsTrue(_context.Reviews.First().IsApproved);
        }

        [Test]
        public async Task ApproveReviewAsync_WithInvalidId_ReturnsFalse()
        {
            var result = await _service.ApproveReviewAsync(Guid.NewGuid());
            Assert.IsFalse(result);
        }

        [Test]
        public async Task DeleteReviewAsync_WithValidId_DeletesReview()
        {
            var review = new Review { Id = Guid.NewGuid(), Rating = 3, BookId = Guid.NewGuid(), UserId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow };
            await _context.Reviews.AddAsync(review);
            await _context.SaveChangesAsync();

            var result = await _service.DeleteReviewAsync(review.Id);

            Assert.IsTrue(result);
            Assert.AreEqual(0, _context.Reviews.Count());
        }

        [Test]
        public async Task GetRatingForABookByIdAsync_WithApprovedReviews_ReturnsAverage()
        {
            var book = CreateBook();
            await _context.Books.AddAsync(book);

            await _context.Reviews.AddRangeAsync(
                new Review { BookId = book.Id, Rating = 4, IsApproved = true },
                new Review { BookId = book.Id, Rating = 2, IsApproved = true },
                new Review { BookId = book.Id, Rating = 5, IsApproved = false }
            );
            await _context.SaveChangesAsync();

            var rating = await _service.GetRatingForABookByIdAsync(book.Id);

            Assert.AreEqual(3, rating);
        }

        [Test]
        public async Task GetReviewsForABookAsync_WithReviews_ReturnsCorrectPage()
        {
            var user = CreateUser();
            var book = CreateBook();
            book.Reviews = new List<Review>();

            for (int i = 0; i < 10; i++)
            {
                book.Reviews.Add(new Review
                {
                    Id = Guid.NewGuid(),
                    BookId = book.Id,
                    UserId = user.Id,
                    Rating = 4,
                    Comment = $"Review {i}",
                    CreatedAt = DateTime.UtcNow.AddMinutes(-i),
                    User = user
                });
            }

            await _context.Users.AddAsync(user);
            await _context.Books.AddAsync(book);
            await _context.SaveChangesAsync();

            var reviews = await _service.GetReviewsForABookAsync(book.Id, user.Id, page: 1, pageSize: 5);

            Assert.AreEqual(5, reviews.Count);
        }

        [Test]
        public async Task GetTotalReviewCountAsync_ReturnsApprovedCount()
        {
            var book = CreateBook();

            await _context.Books.AddAsync(book);
            await _context.Reviews.AddRangeAsync(
                new Review { BookId = book.Id, Rating = 5, IsApproved = true },
                new Review { BookId = book.Id, Rating = 4, IsApproved = false }
            );
            await _context.SaveChangesAsync();

            var count = await _service.GetTotalReviewCountAsync(book.Id);

            Assert.AreEqual(1, count);
        }

        [Test]
        public async Task GetUnapprovedReviewsAsync_ReturnsCorrectList()
        {
            var user = CreateUser();
            var book = CreateBook();
            var review = new Review
            {
                BookId = book.Id,
                UserId = user.Id,
                Rating = 3,
                Comment = "Needs work",
                CreatedAt = DateTime.UtcNow,
                IsApproved = false,
                User = user,
                Book = book
            };

            await _context.Users.AddAsync(user);
            await _context.Books.AddAsync(book);
            await _context.Reviews.AddAsync(review);
            await _context.SaveChangesAsync();

            var result = await _service.GetUnapprovedReviewsAsync();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Needs work", result.First().Comment);
        }
    }
}
