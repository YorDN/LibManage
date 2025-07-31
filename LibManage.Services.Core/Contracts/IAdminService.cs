using LibManage.ViewModels.Admin;

namespace LibManage.Services.Core.Contracts
{
    public interface IAdminService
    {
        public Task<AdminDashboardViewModel> GetAdminDashboardDetailsAsync();
    }
}
