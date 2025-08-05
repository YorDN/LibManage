using LibManage.Data.Models.DTOs;
using LibManage.ViewModels.Authors;
using LibManage.ViewModels.Publishers;

namespace LibManage.Services.Core.Contracts
{
    public interface IPublisherService
    {
        public Task<bool> AddPublisherAsync(AddPublisherInputModel model);
        public Task<PaginatedPublisherViewModel> GetAllPublishersAsync(PublisherFilterOptions options, int page = 1, int pageSize = 10);
        public Task<PublisherDetailsViewModel?> GetPublisherDetailsAsync(Guid id, Guid? userId = null);
        public Task<EditPublisherInputModel?> GetPublisherEditInfoAsync(Guid id);
        public Task<bool> EditPublisherAsync(EditPublisherInputModel model);
        public Task<DeletePublisherViewModel?> GetDeletePublisherInfoAsync(Guid id);
        public Task<bool> DeletePublisherAsync(Guid id);
    }
}
