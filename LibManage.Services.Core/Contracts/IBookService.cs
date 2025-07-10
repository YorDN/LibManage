
using LibManage.ViewModels;

namespace LibManage.Services.Core.Contracts
{
    public interface IBookService
    {
        public Task<IEnumerable<AllBooksViewModel>?> GetAllBooksAsync();
        public Task<bool> CreateBookAsync(AddBookInputModel model);
    }
}
