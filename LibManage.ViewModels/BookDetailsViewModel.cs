using LibManage.Data.Models.Library;

namespace LibManage.ViewModels
{
    public class BookDetailsViewModel
    {
        public Book Book { get; set; }
        public bool IsTaken { get; set; }
    }
}
