namespace LibManage.ViewModels.Audio
{
    public class AudioBookPlayerViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Cover { get; set; }
        public string Language { get; set; }
        public Guid AuthorId { get; set; }
        public string AuthorName { get; set; }
        public string? Description { get; set; }
        public TimeSpan? Duration { get; set; }
        public DateTime UploadDate { get; set; }
        public string? FilePath { get; set; }
    }
}
