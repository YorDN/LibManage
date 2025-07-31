namespace LibManage.ViewModels.Admin
{
    public class AdminDashboardViewModel
    {
        public int TotalBooks { get; set; }
        public int TotalUsers { get; set; }
        public int TotalAuthors { get; set; }
        public int TotalPublishers { get; set; }
        public int PhysicalBooks { get; set; }
        public int DigitalBooks { get; set; }
        public int AudioBooks { get; set; }
        public List<RecentBookViewModel> RecentBooks { get; set; } = new();
        public string? MostBorrowedBook { get; set; }
        public string? MostActiveUser { get; set; }
        public int ActiveUsersLast30Days { get; set; }
        public double RepeatBorrowersPercent { get; set; }
        public List<string> ReviewMonths { get; set; } = new();
        public List<int> ReviewCounts { get; set; } = new();

    }
}
