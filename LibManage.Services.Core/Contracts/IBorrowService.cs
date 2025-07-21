using LibManage.ViewModels.Borrows;

namespace LibManage.Services.Core.Contracts
{
    public interface IBorrowService
    {
        public Task<bool> RentBookAsync(Guid userId, Guid bookId);
        public Task<bool> HasActiveBorrowsAsync(Guid? userId, Guid bookId);
        public Task<IEnumerable<BorrowedBookViewModel>?> GetUsersBorrowedBooksAsync(Guid userId);
        public Task<bool> ReturnBookAsync(Guid userId, Guid bookId);
    }
}
