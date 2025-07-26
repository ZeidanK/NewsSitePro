using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsSite.BL;
using NewsSitePro.Models;

namespace NewsSite.Pages
{
  

    public class AdminModel : PageModel
    {
        public HeaderViewModel HeaderData { get; set; } = new HeaderViewModel();
        public List<NewsArticle> Posts { get; set; } = new List<NewsArticle>();
        private readonly DBservices dbService;

        public AdminModel()
        {
            dbService = new DBservices();
        }

        // Properties for dashboard stats
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int BannedUsers { get; set; }
        public int TotalPosts { get; set; }
        public int TotalReports { get; set; }

        // Properties for user management
        public List<AdminUserView> Users { get; set; } = new List<AdminUserView>();
        public List<ActivityLog> RecentActivity { get; set; } = new List<ActivityLog>();
        public List<UserReport> PendingReports { get; set; } = new List<UserReport>();

        public IActionResult OnGetAsync()
        {
            Console.WriteLine("=== ADMIN PAGE ACCESSED ===");

            try
            {
                // Check if user is admin
                var jwt = Request.Cookies["jwtToken"];
                Console.WriteLine($"JWT Cookie exists: {!string.IsNullOrEmpty(jwt)}");

                if (string.IsNullOrEmpty(jwt))
                {
                    Console.WriteLine("No JWT token found, redirecting to login");
                    return RedirectToPage("/Login");
                }

                var user = new User().ExtractUserFromJWT(jwt);
                Console.WriteLine($"Extracted user ID: {user.Id}, Name: {user.Name}, IsAdmin from JWT: {user.IsAdmin}");

                var currentUser = dbService.GetUserById(user.Id);
                Console.WriteLine($"User from DB - ID: {currentUser?.Id}, Name: {currentUser?.Name}, IsAdmin: {currentUser?.IsAdmin}");

                if (currentUser?.IsAdmin != true)
                {
                    Console.WriteLine("User is not admin, returning Forbid");
                    return Forbid();
                }

                // Initialize with minimal data to avoid database issues
                Users = new List<AdminUserView>();
                RecentActivity = new List<ActivityLog>();
                PendingReports = new List<UserReport>();

                // Try to load real dashboard stats
                try
                {
                    var stats = dbService.GetAdminDashboardStats().Result;
                    TotalUsers = stats.TotalUsers;
                    ActiveUsers = stats.ActiveUsers;
                    BannedUsers = stats.BannedUsers;
                    TotalPosts = stats.TotalPosts;
                    TotalReports = stats.TotalReports;
                    Console.WriteLine($"Loaded stats: Users={TotalUsers}, Active={ActiveUsers}, Banned={BannedUsers}, Posts={TotalPosts}, Reports={TotalReports}");
                }
                catch (Exception statsEx)
                {
                    Console.WriteLine($"Error loading dashboard stats: {statsEx.Message}");
                    // Set default stats
                    TotalUsers = 0;
                    ActiveUsers = 0;
                    BannedUsers = 0;
                    TotalPosts = 0;
                    TotalReports = 0;
                }

                Console.WriteLine("Returning admin page successfully");
                return Page();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== ADMIN PAGE ERROR ===");
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");
                Console.WriteLine("=========================");
                TempData["Error"] = "Failed to load admin panel: " + ex.Message;
                return RedirectToPage("/Error");
            }
        }

        private async Task LoadDashboardStats()
        {
            var stats = await dbService.GetAdminDashboardStats();
            TotalUsers = stats.TotalUsers;
            ActiveUsers = stats.ActiveUsers;
            BannedUsers = stats.BannedUsers;
            TotalPosts = stats.TotalPosts;
            TotalReports = stats.TotalReports;
        }

        private bool IsCurrentUserAdmin()
        {
            try
            {
                var jwt = Request.Cookies["jwtToken"];
                if (string.IsNullOrEmpty(jwt))
                {
                    return false;
                }

                var user = new User().ExtractUserFromJWT(jwt);
                var currentUser = dbService.GetUserById(user.Id);

                return currentUser?.IsAdmin ?? false;
            }
            catch
            {
                return false;
            }
        }

        private int? GetCurrentUserId()
        {
            try
            {
                var jwt = Request.Cookies["jwtToken"];
                if (string.IsNullOrEmpty(jwt))
                {
                    return null;
                }

                var user = new User().ExtractUserFromJWT(jwt);
                return user.Id;
            }
            catch
            {
                return null;
            }
        }

        // AJAX endpoints for admin actions
        public async Task<IActionResult> OnPostBanUserAsync([FromBody] BanUserRequest request)
        {
            if (!IsCurrentUserAdmin())
            {
                return Forbid();
            }

            try
            {
                var adminId = GetCurrentUserId();
                if (adminId == null)
                {
                    return Unauthorized();
                }

                bool success = await dbService.BanUser(request.UserId, request.Reason, request.Duration, adminId.Value);

                if (success)
                {
                    // Log admin action
                    await dbService.LogAdminAction(adminId.Value, "BAN_USER", $"Banned user {request.UserId} for {request.Duration} days. Reason: {request.Reason}");
                    return new JsonResult(new { success = true, message = "User banned successfully" });
                }
                else
                {
                    return new JsonResult(new { success = false, message = "Failed to ban user" });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = "Error: " + ex.Message });
            }
        }

        public async Task<IActionResult> OnPostUnbanUserAsync([FromBody] int userId)
        {
            if (!IsCurrentUserAdmin())
            {
                return Forbid();
            }

            try
            {
                var adminId = GetCurrentUserId();
                if (adminId == null)
                {
                    return Unauthorized();
                }

                bool success = await dbService.UnbanUser(userId);

                if (success)
                {
                    await dbService.LogAdminAction(adminId.Value, "UNBAN_USER", $"Unbanned user {userId}");
                    return new JsonResult(new { success = true, message = "User unbanned successfully" });
                }
                else
                {
                    return new JsonResult(new { success = false, message = "Failed to unban user" });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = "Error: " + ex.Message });
            }
        }

        public async Task<IActionResult> OnPostDeactivateUserAsync([FromBody] int userId)
        {
            if (!IsCurrentUserAdmin())
            {
                return Forbid();
            }

            try
            {
                var adminId = GetCurrentUserId();
                if (adminId == null)
                {
                    return Unauthorized();
                }

                bool success = await dbService.DeactivateUser(userId);

                if (success)
                {
                    await dbService.LogAdminAction(adminId.Value, "DEACTIVATE_USER", $"Deactivated user {userId}");
                    return new JsonResult(new { success = true, message = "User deactivated successfully" });
                }
                else
                {
                    return new JsonResult(new { success = false, message = "Failed to deactivate user" });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = "Error: " + ex.Message });
            }
        }

        public async Task<IActionResult> OnGetUsersAsync(int page = 1, int pageSize = 50, string search = "", string status = "", string joinDate = "")
        {
            if (!IsCurrentUserAdmin())
            {
                return Forbid();
            }

            try
            {
                var users = await dbService.GetFilteredUsersForAdmin(page, pageSize, search, status, joinDate);
                var totalUsers = await dbService.GetFilteredUsersCount(search, status, joinDate);

                return new JsonResult(new
                {
                    success = true,
                    users = users,
                    totalUsers = totalUsers,
                    totalPages = (int)Math.Ceiling((double)totalUsers / pageSize),
                    currentPage = page
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = "Error: " + ex.Message });
            }
        }

        public async Task<IActionResult> OnGetUserDetailsAsync(int userId)
        {
            if (!IsCurrentUserAdmin())
            {
                return Forbid();
            }

            try
            {
                var userDetails = await dbService.GetUserDetailsForAdmin(userId);
                return new JsonResult(new { success = true, user = userDetails });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = "Error: " + ex.Message });
            }
        }

        public async Task<IActionResult> OnGetActivityLogsAsync(int page = 1, int pageSize = 20)
        {
            if (!IsCurrentUserAdmin())
            {
                return Forbid();
            }

            try
            {
                var logs = await dbService.GetActivityLogs(page, pageSize);
                return new JsonResult(new { success = true, logs = logs });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = "Error: " + ex.Message });
            }
        }

        public async Task<IActionResult> OnGetReportsAsync()
        {
            if (!IsCurrentUserAdmin())
            {
                return Forbid();
            }

            try
            {
                var reports = await dbService.GetAllReports();
                return new JsonResult(new { success = true, reports = reports });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = "Error: " + ex.Message });
            }
        }

        public async Task<IActionResult> OnPostResolveReportAsync([FromBody] ResolveReportRequest request)
        {
            if (!IsCurrentUserAdmin())
            {
                return Forbid();
            }

            try
            {
                var adminId = GetCurrentUserId();
                if (adminId == null)
                {
                    return Unauthorized();
                }

                bool success = await dbService.ResolveReport(request.ReportId, request.Action, request.Notes, adminId.Value);

                if (success)
                {
                    return new JsonResult(new { success = true, message = "Report resolved successfully" });
                }
                else
                {
                    return new JsonResult(new { success = false, message = "Failed to resolve report" });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = "Error: " + ex.Message });
            }
        }
    }

    // Helper classes for admin requests
    public class BanUserRequest
    {
        public int UserId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public int Duration { get; set; } // Days, or -1 for permanent
    }

    public class ResolveReportRequest
    {
        public int ReportId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }
}
