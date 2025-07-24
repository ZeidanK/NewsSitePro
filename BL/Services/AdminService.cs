using NewsSite.BL;

namespace NewsSite.BL.Services
{
    /// <summary>
    /// Admin Service - Business Logic Layer
    /// Implements admin-related business operations and validation
    /// </summary>
    public class AdminService : IAdminService
    {
        private readonly DBservices _dbService;

        public AdminService(DBservices dbService)
        {
            _dbService = dbService;
        }

        public async Task<AdminDashboardStats> GetAdminDashboardStatsAsync()
        {
            return await _dbService.GetAdminDashboardStats();
        }

        public async Task<List<AdminUserView>> GetAllUsersForAdminAsync(int page, int pageSize)
        {
            // Business logic validation
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20; // Limit page size

            return await _dbService.GetAllUsersForAdmin(page, pageSize);
        }

        public async Task<List<AdminUserView>> GetFilteredUsersForAdminAsync(int page, int pageSize, string search, string status, string joinDate)
        {
            // Business logic validation
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            return await _dbService.GetFilteredUsersForAdmin(page, pageSize, search, status, joinDate);
        }

        public async Task<int> GetFilteredUsersCountAsync(string search, string status, string joinDate)
        {
            return await _dbService.GetFilteredUsersCount(search, status, joinDate);
        }

        public async Task<AdminUserDetails> GetUserDetailsForAdminAsync(int userId)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("Valid User ID is required");
            }

            return await _dbService.GetUserDetailsForAdmin(userId);
        }

        public async Task<bool> BanUserAsync(int userId, string reason, int durationDays, int adminId)
        {
            // Business logic validation
            if (userId <= 0 || adminId <= 0)
            {
                throw new ArgumentException("Valid User ID and Admin ID are required");
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                throw new ArgumentException("Ban reason is required");
            }

            if (durationDays < 0)
            {
                throw new ArgumentException("Ban duration must be positive");
            }

            // Business rule: Cannot ban admin users
            var userDetails = await GetUserDetailsForAdminAsync(userId);
            if (userDetails.IsAdmin)
            {
                throw new InvalidOperationException("Cannot ban admin users");
            }

            // Business rule: Cannot ban yourself
            if (userId == adminId)
            {
                throw new InvalidOperationException("Cannot ban yourself");
            }

            // Log admin action
            await LogAdminActionAsync(adminId, "Ban User", $"Banned user {userId} for {durationDays} days. Reason: {reason}");

            return await _dbService.BanUser(userId, reason, durationDays, adminId);
        }

        public async Task<bool> UnbanUserAsync(int userId)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("Valid User ID is required");
            }

            return await _dbService.UnbanUser(userId);
        }

        public async Task<bool> DeactivateUserAsync(int userId)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("Valid User ID is required");
            }

            // Business rule: Cannot deactivate admin users
            var userDetails = await GetUserDetailsForAdminAsync(userId);
            if (userDetails.IsAdmin)
            {
                throw new InvalidOperationException("Cannot deactivate admin users");
            }

            return await _dbService.DeactivateUser(userId);
        }

        public async Task<List<ActivityLog>> GetActivityLogsAsync(int page, int pageSize)
        {
            // Business logic validation
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            return await _dbService.GetActivityLogs(page, pageSize);
        }

        public async Task<List<ActivityLog>> GetRecentActivityLogsAsync(int count)
        {
            if (count < 1 || count > 50) count = 10; // Limit count

            return await _dbService.GetRecentActivityLogs(count);
        }

        public async Task<List<UserReport>> GetPendingReportsAsync()
        {
            return await _dbService.GetPendingReports();
        }

        public async Task<List<UserReport>> GetAllReportsAsync()
        {
            return await _dbService.GetAllReports();
        }

        public async Task<bool> ResolveReportAsync(int reportId, string action, string notes, int adminId)
        {
            // Business logic validation
            if (reportId <= 0 || adminId <= 0)
            {
                throw new ArgumentException("Valid Report ID and Admin ID are required");
            }

            if (string.IsNullOrWhiteSpace(action))
            {
                throw new ArgumentException("Resolution action is required");
            }

            // Valid actions
            var validActions = new[] { "Resolved", "Dismissed", "Warning Issued", "Content Removed" };
            if (!validActions.Contains(action))
            {
                throw new ArgumentException("Invalid resolution action");
            }

            // Log admin action
            await LogAdminActionAsync(adminId, "Resolve Report", $"Resolved report {reportId} with action: {action}. Notes: {notes}");

            return await _dbService.ResolveReport(reportId, action, notes, adminId);
        }

        public async Task<bool> LogAdminActionAsync(int adminId, string action, string details)
        {
            if (adminId <= 0)
            {
                throw new ArgumentException("Valid Admin ID is required");
            }

            if (string.IsNullOrWhiteSpace(action))
            {
                throw new ArgumentException("Action is required");
            }

            return await _dbService.LogAdminAction(adminId, action, details);
        }
    }
}
