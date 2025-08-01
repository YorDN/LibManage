using LibManage.ViewModels.Admin;

namespace LibManage.Services.Core.Contracts
{
    public interface IAdminService
    {
        public Task<AdminDashboardViewModel> GetAdminDashboardDetailsAsync();
        public Task<(IEnumerable<ManageUserViewModel> Users, int TotalCount)> GetAllUsersAsync(int page, int pageSize);
    }
}
