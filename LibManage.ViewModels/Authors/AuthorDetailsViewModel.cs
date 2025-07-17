using LibManage.ViewModels.Books;

namespace LibManage.ViewModels.Authors
{
    public class AuthorDetailsViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Biography { get; set; }
        public string Photo { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime? DateOfDeath { get; set; }
        public List<AllBooksViewModel> WrittenBooks { get; set; }

    }
}
