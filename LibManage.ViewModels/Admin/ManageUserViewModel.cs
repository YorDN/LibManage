namespace LibManage.ViewModels.Admin
{
    public class ManageUserViewModel
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Role { get; set; } = null!;
        public bool IsDeleted { get; set; }
    }
}
