using LibManage.ViewModels.Authors;

namespace LibManage.Services.Core.Contracts
{
    public interface IAuthorService
    {
        public Task<bool> CreateAuthorAsync(AddAuthorInputModel model);
        public Task<IEnumerable<AllAuthorsViewModel>> GetAllAuthorsAsync();
        public Task<AuthorDetailsViewModel?> GetAuthorDetailsAsync(Guid id);
    }
}
