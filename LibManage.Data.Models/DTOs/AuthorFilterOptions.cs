namespace LibManage.Data.Models.DTOs
{
    public class AuthorFilterOptions
    {
        public string? SearchTerm { get; set; }
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; } = false;
    }
}
