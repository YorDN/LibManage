namespace LibManage.ViewModels.Manager
{
    public class UnapprovedReviewViewModel
    {
        public Guid Id { get; set; }
        public string BookTitle { get; set; } = null!;
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public string Username { get; set; } = "Unknown";
        public DateTime CreatedAt { get; set; }

    }
}
