namespace LibManage.Data.Models.DTOs
{
    public class BookFilterOptions
    {
        public string? SearchTerm { get; set; }
        public int? MinimumRating { get; set; }
        public string? BookType { get; set; }
        public bool? IsTaken { get; set; }
        public string? SortBy { get; set; } 
        public bool SortDescending { get; set; } = false;
    }
}
