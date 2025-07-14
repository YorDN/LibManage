
using LibManage.ViewModels;

namespace LibManage.Services.Core.Contracts
{
    public interface IPublisherService
    {
        public Task<bool> AddPublisherAsync(AddPublisherInputModel model);
    }
}
