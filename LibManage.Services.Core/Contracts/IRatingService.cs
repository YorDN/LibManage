
using LibManage.ViewModels.Manager;
using LibManage.ViewModels.Rating;

namespace LibManage.Services.Core.Contracts
{
    public interface IRatingService
    {
        public Task<double?> GetRatingForABookByIdAsync(Guid bookId);
        public Task<List<ReviewViewModel>> GetReviewsForABookAsync(Guid bookId, Guid currentUserId, int page = 1, int pageSize = 5);
        public Task<int> GetTotalReviewCountAsync(Guid bookId);
        public Task<bool> AddReviewAsync(ReviewInputModel model, Guid bookId, Guid userId);
        public Task<List<UnapprovedReviewViewModel>> GetUnapprovedReviewsAsync();
        public Task<bool> ApproveReviewAsync(Guid id);
        public Task<bool> DeleteReviewAsync(Guid id);
    }
}
