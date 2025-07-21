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

    }
}
