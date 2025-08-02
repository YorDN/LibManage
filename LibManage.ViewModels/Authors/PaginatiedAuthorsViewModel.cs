using LibManage.Data.Models.DTOs;

namespace LibManage.ViewModels.Authors
{
    public class PaginatiedAuthorsViewModel
    {
        public IEnumerable<AllAuthorsViewModel> Authors { get; set; } = new List<AllAuthorsViewModel>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public AuthorFilterOptions FilterOptions { get; set; } = new();
    }
}
