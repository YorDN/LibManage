namespace LibManage.ViewModels.Publishers
{
    public class DeletePublisherViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Logo { get; set; }
        public bool ConfirmDeleteBooks { get; set; }
        public int BooksCount { get; set; }
    }
}
