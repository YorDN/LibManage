using LibManage.Data.Models.DTOs;

namespace LibManage.ViewModels.Books
{
    public class PaginatedBooksViewModel
    {
        public IEnumerable<AllBooksViewModel> Books { get; set; } = new List<AllBooksViewModel>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public BookFilterOptions FilterOptions { get; set; } = new();

    }
}
