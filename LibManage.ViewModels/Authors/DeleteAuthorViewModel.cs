namespace LibManage.ViewModels.Authors
{
    public class DeleteAuthorViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Photo { get; set; }
        public bool ConfirmDeleteBooks { get; set; }
        public int BooksCount { get; set; }
    }
}
