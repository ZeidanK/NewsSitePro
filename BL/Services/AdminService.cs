using NewsSite.BL;

// ----------------------------------------------------------------------------------
// AdminService.cs
//
// This class implements admin-related business logic for the NewsSitePro application. It provides
// methods for managing users, handling bans/unbans, retrieving admin dashboard stats, processing reports,
// logging admin actions, and validating business rules. The service interacts with the database layer
// through DBservices and enforces rules to protect admin accounts and ensure proper moderation. All methods
// are asynchronous for efficient, non-blocking operations. Comments are added to key functions for clarity.
// ----------------------------------------------------------------------------------

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
        // Get statistics for the admin dashboard (user counts, activity, etc.)
        {
            return await _dbService.GetAdminDashboardStats();
        }

        public async Task<List<AdminUserView>> GetAllUsersForAdminAsync(int page, int pageSize)
        // Get a paginated list of all users for admin view
        {
            // Business logic validation
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20; // Limit page size

            return await Task.FromResult(_dbService.GetAllUsersForAdmin(page, pageSize));
        }

        public async Task<List<AdminUserView>> GetFilteredUsersForAdminAsync(int page, int pageSize, string search, string status, string joinDate)
        // Get a filtered, paginated list of users for admin view
        {
            // Business logic validation
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            return await _dbService.GetFilteredUsersForAdmin(page, pageSize, search, status, joinDate);
        }

        public async Task<int> GetFilteredUsersCountAsync(string search, string status, string joinDate)
        // Get the count of users matching filter criteria
        {
            return await _dbService.GetFilteredUsersCount(search, status, joinDate);
        }

        public async Task<AdminUserDetails> GetUserDetailsForAdminAsync(int userId)
        // Get detailed information about a specific user for admin
        {
            if (userId <= 0)
            {
                throw new ArgumentException("Valid User ID is required");
            }

            return await _dbService.GetUserDetailsForAdmin(userId);
        }

        public async Task<bool> BanUserAsync(int userId, string reason, int durationDays, int adminId)
        // Ban a user for a specified duration, with business rule checks
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

            if (durationDays < -1)
            {
                throw new ArgumentException("Ban duration must be positive or -1 for permanent");
            }

            try 
            {
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
            catch (Exception ex)
            {
                Console.WriteLine($"[AdminService] BanUserAsync error: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> UnbanUserAsync(int userId, int adminId)
        // Unban a user
        {
            if (userId <= 0)
            {
                throw new ArgumentException("Valid User ID is required");
            }

            if (adminId <= 0)
            {
                throw new ArgumentException("Valid Admin ID is required");
            }

            // Log admin action
            await LogAdminActionAsync(adminId, "Unban User", $"Unbanned user {userId}");

            return await _dbService.UnbanUser(userId, adminId);
        }

        public async Task<bool> DeactivateUserAsync(int userId)
        // Deactivate a user account
        { 
            if (userId <= 0)
            {
                throw new ArgumentException("Valid User ID is required");
            }

            return await _dbService.DeactivateUser(userId);
        }

        public async Task<bool> ActivateUserAsync(int userId)
        // Activate a user account
        { 
            if (userId <= 0)
            {
                throw new ArgumentException("Valid User ID is required");
            }

            return await _dbService.ActivateUser(userId);
        }

        public async Task<List<ActivityLog>> GetActivityLogsAsync(int page, int pageSize)
        // Get a paginated list of activity logs for admin review
        {
            // Business logic validation
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            return await _dbService.GetActivityLogs(page, pageSize);
        }

        public async Task<List<ActivityLog>> GetRecentActivityLogsAsync(int count)
        // Get a list of recent activity logs
        {
            if (count < 1 || count > 50) count = 10; // Limit count

            return await Task.FromResult(_dbService.GetRecentActivityLogs(count));
        }

        public async Task<List<UserReport>> GetPendingReportsAsync()
        // Get a list of user reports that are pending review
        {
            return await _dbService.GetPendingReports();
        }

        public async Task<List<UserReport>> GetAllReportsAsync()
        // Get a list of all user reports
        {
            return await _dbService.GetAllReports();
        }

        public async Task<bool> ResolveReportAsync(int reportId, string action, string notes, int adminId)
        // Resolve a user report with a specified action and notes
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
        // Log an admin action for auditing purposes
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
