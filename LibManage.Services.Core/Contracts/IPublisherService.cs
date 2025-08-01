using LibManage.ViewModels.Publishers;

namespace LibManage.Services.Core.Contracts
{
    public interface IPublisherService
    {
        public Task<bool> AddPublisherAsync(AddPublisherInputModel model);
        public Task<IEnumerable<AllPublishersViewModel>> GetAllPublishersAsync();
        public Task<PublisherDetailsViewModel?> GetPublisherDetailsAsync(Guid id, Guid? userId = null);
    }
}
