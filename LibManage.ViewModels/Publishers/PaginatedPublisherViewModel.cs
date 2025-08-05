using LibManage.Data.Models.DTOs;

namespace LibManage.ViewModels.Publishers
{
    public class PaginatedPublisherViewModel
    {
        public IEnumerable<AllPublishersViewModel> Publishers { get; set; } = new List<AllPublishersViewModel>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public PublisherFilterOptions FilterOptions { get; set; } = new();
    }
}
