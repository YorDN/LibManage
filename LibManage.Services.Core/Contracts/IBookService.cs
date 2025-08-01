using LibManage.Data.Models.DTOs;
using LibManage.Data.Models.Library;
using LibManage.ViewModels.Audio;
using LibManage.ViewModels.Books;

namespace LibManage.Services.Core.Contracts
{
    public interface IBookService
    {
        public Task<PaginatedBooksViewModel> GetAllBooksAsync(BookFilterOptions options, Guid? userId = null, int page = 1, int pageSize = 10);
        public Task<AddBookInputModel> GetBookInputModelAsync();
        public Task<bool> CreateBookAsync(AddBookInputModel model);
        public Task<BookDetailsViewModel?> GetBookDetailsAsync(Guid bookId, Guid? userId);
        public Task<DeleteBookViewModel?> GetDeletedBookDetailsAsync(Guid id);
        public Task<bool> DeleteBookAsync(Guid id);
        public Task<List<AllBooksViewModel>?> GetAllBooksFromAuthorAsync(Guid authorId, Guid? userId = null);
        public Task<List<AllBooksViewModel>?> GetAllBooksFromPublisherAsync(Guid publisherId, Guid? userId = null);
        public Task<bool> UpdateBookAsync(EditBookInputModel model);
        public Task<EditBookInputModel?> GetBookEditModelAsync(Guid id);
        public Task<Book?> GetBookByIdAsync(Guid id);
        public Task<AudioBookPlayerViewModel?> GetAudioBookPlayerViewModelAsync(Guid id);
    }
}
