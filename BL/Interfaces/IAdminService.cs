using NewsSite.BL;
 
namespace NewsSite.BL.Services
{
    /// <summary>
    /// Defines the contract for admin-related operations in the application.
    /// This interface is placed here to centralize all admin service methods, making it easier to manage and implement admin functionality.
    /// Asynchronous functions are used to ensure that operations such as database queries, user management, and report handling do not block the main thread.
    /// This improves application responsiveness and scalability, especially when handling large datasets or long-running tasks.
    /// </summary>
    public interface IAdminService
    {
        Task<AdminDashboardStats> GetAdminDashboardStatsAsync();
        Task<List<AdminUserView>> GetAllUsersForAdminAsync(int page, int pageSize);
        Task<List<AdminUserView>> GetFilteredUsersForAdminAsync(int page, int pageSize, string search, string status, string joinDate);
        Task<int> GetFilteredUsersCountAsync(string search, string status, string joinDate);
        Task<AdminUserDetails> GetUserDetailsForAdminAsync(int userId);
        Task<bool> BanUserAsync(int userId, string reason, int durationDays, int adminId);
        Task<bool> UnbanUserAsync(int userId, int adminId);
        Task<bool> DeactivateUserAsync(int userId);
        Task<bool> ActivateUserAsync(int userId);
        Task<List<UserReport>> GetAllReportsAsync();
        Task<List<UserReport>> GetPendingReportsAsync();
        Task<bool> ResolveReportAsync(int reportId, string action, string notes, int adminId);
        Task<List<ActivityLog>> GetActivityLogsAsync(int page, int pageSize);
        Task<List<ActivityLog>> GetRecentActivityLogsAsync(int count);
        Task<bool> LogAdminActionAsync(int adminId, string action, string details);
    }
}
