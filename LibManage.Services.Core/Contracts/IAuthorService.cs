using LibManage.Data.Models.DTOs;
using LibManage.ViewModels.Authors;

namespace LibManage.Services.Core.Contracts
{
    public interface IAuthorService
    {
        public Task<bool> CreateAuthorAsync(AddAuthorInputModel model);
        public Task<PaginatiedAuthorsViewModel> GetAllAuthorsAsync(AuthorFilterOptions options, int page = 1, int pageSize = 10);
        public Task<AuthorDetailsViewModel?> GetAuthorDetailsAsync(Guid id, Guid? userId = null);
        public Task<DeleteAuthorViewModel?> GetAuthorDeleteInfoAsync(Guid id);
        public Task<bool> DeleteAuthorAsync(Guid id);
        public Task<EditAuthorInputModel?> GetAuthorEditInfoAsync(Guid id);
        public Task<bool> EditAuthorAsync(EditAuthorInputModel model);
    }
}
