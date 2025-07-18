﻿using LibManage.ViewModels.Books;

namespace LibManage.Services.Core.Contracts
{
    public interface IBookService
    {
        public Task<IEnumerable<AllBooksViewModel>?> GetAllBooksAsync();
        public Task<AddBookInputModel> GetBookInputModelAsync();
        public Task<bool> CreateBookAsync(AddBookInputModel model);
        public Task<BookDetailsViewModel?> GetBookDetailsAsync(Guid id);
        public Task<DeleteBookViewModel?> GetDeletedBookDetailsAsync(Guid id);
        public Task<bool> DeleteBookAsync(Guid id);
        public Task<List<AllBooksViewModel>?> GetAllBooksFromAuthorAsync(Guid authorId); 
        public Task<bool> UpdateBookAsync(EditBookInputModel model);
        public Task<EditBookInputModel?> GetBookEditModelAsync(Guid id);
    }
}
