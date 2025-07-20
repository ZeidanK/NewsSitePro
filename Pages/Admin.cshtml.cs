using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using NewsSite.BL;
//using NewsSite.DAL;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace NewsSite.Pages
{
    [Authorize]
    public class AdminModel : PageModel
    {
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

        public async Task<IActionResult> OnGetAsync()
        {
            // Check if user is admin
            if (!IsCurrentUserAdmin())
            {
                return Forbid();
            }

            try
            {
                // Load dashboard statistics
                await LoadDashboardStats();

                // Load initial data
                Users = await dbService.GetAllUsersForAdmin(1, 50); // First 50 users
                RecentActivity = await dbService.GetRecentActivityLogs(20); // Last 20 activities
                PendingReports = await dbService.GetPendingReports();

                return Page();
            }
            catch (Exception ex)
            {
                // Log error and redirect to error page
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
                // Check JWT claims
                var adminClaim = User.FindFirst("isAdmin");
                if (adminClaim != null && bool.TryParse(adminClaim.Value, out bool isAdmin))
                {
                    return isAdmin;
                }

                // Check cookies for JWT token
                var jwtCookie = Request.Cookies["jwtToken"];
                if (!string.IsNullOrEmpty(jwtCookie))
                {
                    var handler = new JwtSecurityTokenHandler();
                    if (handler.CanReadToken(jwtCookie))
                    {
                        var jsonToken = handler.ReadJwtToken(jwtCookie);
                        var adminTokenClaim = jsonToken.Claims.FirstOrDefault(x => x.Type == "isAdmin");
                        
                        if (adminTokenClaim != null && bool.TryParse(adminTokenClaim.Value, out bool tokenIsAdmin))
                        {
                            return tokenIsAdmin;
                        }
                    }
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private int? GetCurrentUserId()
        {
            try
            {
                // Try to get from JWT claims first
                var userIdClaim = User.FindFirst("id");
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    return userId;
                }

                // Check cookies for JWT token
                var jwtCookie = Request.Cookies["jwtToken"];
                if (!string.IsNullOrEmpty(jwtCookie))
                {
                    var handler = new JwtSecurityTokenHandler();
                    if (handler.CanReadToken(jwtCookie))
                    {
                        var jsonToken = handler.ReadJwtToken(jwtCookie);
                        var idClaim = jsonToken.Claims.FirstOrDefault(x => x.Type == "id");
                        
                        if (idClaim != null && int.TryParse(idClaim.Value, out int cookieUserId))
                        {
                            return cookieUserId;
                        }
                    }
                }

                return null;
            }
            catch (Exception)
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
