
using LibManage.ViewModels.Rating;

namespace LibManage.Services.Core.Contracts
{
    public interface IRatingService
    {
        public Task<double?> GetRatingForABookByIdAsync(Guid bookId);
        public Task<List<ReviewViewModel>> GetReviewsForABookAsync(Guid bookId, int page = 1, int pageSize = 5);
        public Task<int> GetTotalReviewCountAsync(Guid bookId);
        public Task<bool> AddReviewAsync(ReviewInputModel model, Guid bookId, Guid userId);
    }
}
