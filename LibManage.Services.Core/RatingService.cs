
using LibManage.Data;
using LibManage.Data.Models.Library;
using LibManage.Services.Core.Contracts;
using Microsoft.EntityFrameworkCore;

namespace LibManage.Services.Core
{
    public class RatingService(ApplicationDbContext context) : IRatingService
    {
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

            List<int> ratings = await context.Reviews
                .Include (r => r.Rating)
                .Select (r => r.Rating)
                .ToListAsync();

            double rating = ratings.Average();
            return rating;
        }
    }
}
