namespace LibManage.ViewModels.Books
{
    public class AllBooksViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string AuthorName { get; set; }
        public string Cover { get; set; }
        public string BookType { get; set; }
        public int Rating { get; set; }
        public bool IsTaken { get; set; }
    }
}
