using NewsSite.BL;

namespace NewsSite.BL.Services
{
    public interface IAdminService
    {
        Task<AdminDashboardStats> GetAdminDashboardStatsAsync();
        Task<List<AdminUserView>> GetAllUsersForAdminAsync(int page, int pageSize);
        Task<List<AdminUserView>> GetFilteredUsersForAdminAsync(int page, int pageSize, string search, string status, string joinDate);
        Task<AdminUserDetails> GetUserDetailsForAdminAsync(int userId);
        Task<List<UserReport>> GetAllReportsAsync();
        Task<List<UserReport>> GetPendingReportsAsync();
        Task<bool> ResolveReportAsync(int reportId, string action, string notes, int adminId);
        Task<List<ActivityLog>> GetActivityLogsAsync(int page, int pageSize);
        Task<List<ActivityLog>> GetRecentActivityLogsAsync(int count);
        Task<bool> LogAdminActionAsync(int adminId, string action, string details);
    }
}
