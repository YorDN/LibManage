namespace LibManage.ViewModels.Borrows
{
    public class BorrowedBookViewModel
    {
        public Guid BorrowId { get; set; }
        public Guid BookId { get; set; }
        public string Title { get; set; } = null!;
        public string Cover { get; set; } = null!;
        public DateTime DateTaken { get; set; }
        public DateTime DateDue { get; set; }
        public string BookType { get; set; } = null!;

    }
}
