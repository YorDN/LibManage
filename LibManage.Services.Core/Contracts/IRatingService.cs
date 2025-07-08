
namespace LibManage.Services.Core.Contracts
{
    public interface IRatingService
    {
        public Task<double?> GetRatingForABookByIdAsync(Guid bookId);
    }
}
