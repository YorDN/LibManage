
using LibManage.ViewModels;

namespace LibManage.Services.Core.Contracts
{
    public interface IAuthorService
    {
        public Task<bool> CreateAuthorAsync(AddAuthorInputModel model);
    }
}
