
using LibManage.Data;
using LibManage.Data.Models.Library;
using LibManage.Services.Core.Contracts;
using LibManage.ViewModels.Rating;

using Microsoft.EntityFrameworkCore;

namespace LibManage.Services.Core
{
    public class RatingService(ApplicationDbContext context) : IRatingService
    {
        public async Task<bool> AddReviewAsync(ReviewInputModel model, Guid bookId, Guid userId)
        {
            Book? book = await context.Books
                .FirstOrDefaultAsync(b => b.Id == bookId);
            if (book == null) 
                return false;

            User? user = await context.Users.FindAsync(userId);
            if (user == null) 
                return false;

            if (await context.Reviews.AnyAsync(r => r.BookId == bookId && r.UserId == userId))
                return false;

            Review review = new Review
            {
                BookId = bookId,
                UserId = userId,
                Rating = model.Rating,
                Comment = model.Comment,
                CreatedAt = DateTime.UtcNow
            };
            context.Reviews.Add(review);
            await context.SaveChangesAsync();

            return true;
        }

        public async Task<double?> GetRatingForABookByIdAsync(Guid bookId)
        {
            Book? book = await context.Books
                .FirstOrDefaultAsync(book => book.Id == bookId);

            if (book is null) 
                return null;

            bool hasReviews = await context.Reviews
                .AnyAsync(r => r.BookId == bookId);

            if (!hasReviews) 
                return 0;

            return await context.Reviews
                .Where(r => r.BookId == bookId && r.IsApproved)
                .Select(r => (double?)r.Rating)
                .AverageAsync();
        }

        public async Task<List<ReviewViewModel>> GetReviewsForABookAsync(Guid bookId, Guid currentUserId, int page = 1, int pageSize = 5)
        {
            Book? book = await context.Books
                .Include (book => book.Reviews)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(book => book.Id == bookId);

            if(book is null)
                return new List<ReviewViewModel>();

            return book.Reviews
                .Where(r => r.BookId == book.Id)
                .OrderByDescending (r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select (r => new ReviewViewModel
                {
                    Id = r.Id,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt,
                    Pfp = r.User.ProfilePicture,
                    Rating = r.Rating,
                    Username = r.User.UserName ?? "Unknown",
                    IsApproved = r.IsApproved,
                    IsAuthor = r.UserId == currentUserId

                })
                .ToList();
        }
        public async Task<int> GetTotalReviewCountAsync(Guid bookId)
        {
            return await context.Reviews
                .Where(r => r.BookId == bookId && r.IsApproved)
                .CountAsync();
        }

    }
}
