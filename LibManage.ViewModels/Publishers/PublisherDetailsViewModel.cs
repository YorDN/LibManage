using LibManage.ViewModels.Books;

namespace LibManage.ViewModels.Publishers
{
    public class PublisherDetailsViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Logo { get; set; } = null!;
        public string? Description { get; set; }
        public string? Country { get; set; }
        public string? Website { get; set; }
        public List<AllBooksViewModel> PublishedBooks { get; set; }
    }
}
