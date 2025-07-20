namespace LibManage.ViewModels.Reader
{
    public class EpubReadViewModel
    {
        public Guid BookId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int ChapterIndex { get; set; }
        public int ChapterCount { get; set; }
        public string ChapterTitle { get; set; } = string.Empty;
        public string ChapterHtmlContent { get; set; } = string.Empty;

        public List<string> TableOfContents { get; set; } = new();

    }
}
