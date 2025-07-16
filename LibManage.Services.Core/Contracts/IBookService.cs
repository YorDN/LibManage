using LibManage.ViewModels.Books;

namespace LibManage.Services.Core.Contracts
{
    public interface IBookService
    {
        public Task<IEnumerable<AllBooksViewModel>?> GetAllBooksAsync();
        public Task<AddBookInputModel> GetBookInputModelAsync();
        public Task<bool> CreateBookAsync(AddBookInputModel model);
        public Task<BookDetailsViewModel> GetBookDetailsAsync(Guid id);
    }
}
