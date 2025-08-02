namespace LibManage.ViewModels.Rating
{
    public class ReviewViewModel
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Pfp { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsApproved { get; set; }
        public bool IsAuthor { get; set; }
    }
}
