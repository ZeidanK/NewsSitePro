/**
 * AdminController.cs
 * Purpose: Handles administrative operations, user management, and system monitoring
 * Responsibilities: User moderation, content management, system statistics, admin-only operations
 * Architecture: Uses AdminService and UserService from BL layer for business logic and data operations
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsSite.BL;
using NewsSite.BL.Services;
using NewsSite.Services;
using Microsoft.Extensions.Caching.Memory;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace NewsSite.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly IUserService _userService;

        public AdminController(IAdminService adminService, IUserService userService)
        {
            _adminService = adminService;
            _userService = userService;
        }

        // Use the centralized User class method for getting current user ID
        private int? GetCurrentUserId()
        {
            return BL.User.GetCurrentUserId(Request, User);
        }

        // Helper method to get current user from JWT
        private async Task<User?> GetCurrentUserFromJwt()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return null;

                var currentUser = await _userService.GetUserByIdAsync(userId.Value);
                return currentUser;
            }
            catch
            {
                return null;
            }
        }

        // Helper method to check if current user is admin
        private async Task<bool> IsCurrentUserAdminAsync()
        {
            var user = await GetCurrentUserFromJwt();
            return user?.IsAdmin ?? false;
        }

        // GET: api/admin/test - Test endpoint to verify API connectivity
        [HttpGet("test")]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                var user = await GetCurrentUserFromJwt();
                if (user == null)
                {
                    return StatusCode(403, new { 
                        success = false, 
                        message = "Authentication required" 
                    });
                }

                if (!user.IsAdmin)
                {
                    return StatusCode(403, new { 
                        success = false, 
                        message = "Admin access required" 
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Admin API is working",
                    userId = user.Id,
                    userName = user.Name,
                    isAdmin = user.IsAdmin,
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    success = false, 
                    message = "Error: " + ex.Message 
                });
            }
        }

        // Test endpoint to check authentication and API access
        [HttpGet("test2")]
        public async Task<IActionResult> Test()
        {
            try
            {
                var userId = GetCurrentUserId();
                var user = await GetCurrentUserFromJwt();
                var isAdmin = await IsCurrentUserAdminAsync();

                return Ok(new
                {
                    success = true,
                    message = "Admin API is working",
                    userId = userId,
                    userName = user?.Name,
                    isAdmin = isAdmin,
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    success = false,
                    message = "Error in test endpoint",
                    error = ex.Message,
                    timestamp = DateTime.Now
                });
            }
        }

        // GET: api/admin/dashboard-stats
        [HttpGet("dashboard-stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                if (!(await IsCurrentUserAdminAsync()))
                {
                    return StatusCode(403, new { success = false, message = "Admin access required" });
                }

                var stats = await _adminService.GetAdminDashboardStatsAsync();
                return Ok(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // GET: api/admin/users
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 50, 
            [FromQuery] string search = "", [FromQuery] string status = "", [FromQuery] string joinDate = "")
        {
            try
            {
                if (!(await IsCurrentUserAdminAsync()))
                {
                    return StatusCode(403, new { success = false, message = "Admin access required" });
                }

                var users = await _adminService.GetFilteredUsersForAdminAsync(page, pageSize, search, status, joinDate);
                var totalCount = await _adminService.GetFilteredUsersCountAsync(search, status, joinDate);
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                return Ok(new
                {
                    success = true,
                    users = users,
                    currentPage = page,
                    totalPages = totalPages,
                    totalCount = totalCount
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // GET: api/admin/users/{id}
        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUserDetails(int id)
        {
            try
            {
                if (!(await IsCurrentUserAdminAsync()))
                {
                    return StatusCode(403, new { success = false, message = "Admin access required" });
                }

                var userDetails = await _adminService.GetUserDetailsForAdminAsync(id);
                return Ok(new { success = true, data = userDetails });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // POST: api/admin/users/{id}/ban
        [HttpPost("users/{id}/ban")]
        public async Task<IActionResult> BanUser(int id, [FromBody] BanUserRequest request)
        {
            try
            {
                if (!(await IsCurrentUserAdminAsync()))
                {
                    return StatusCode(403, new { success = false, message = "Admin access required" });
                }

                var currentUserId = GetCurrentUserId();
                if (currentUserId == null)
                {
                    return Unauthorized(new { success = false, message = "Unable to identify admin user" });
                }

                var success = await _adminService.BanUserAsync(id, request.Reason, request.Duration, currentUserId.Value);
                
                if (success)
                {
                    return Ok(new { success = true, message = "User banned successfully" });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Failed to ban user" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // POST: api/admin/users/{id}/unban
        [HttpPost("users/{id}/unban")]
        public async Task<IActionResult> UnbanUser(int id)
        {
            try
            {
                if (!(await IsCurrentUserAdminAsync()))
                {
                    return StatusCode(403, new { success = false, message = "Admin access required" });
                }

                var success = await _adminService.UnbanUserAsync(id);
                
                if (success)
                {
                    return Ok(new { success = true, message = "User unbanned successfully" });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Failed to unban user" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // POST: api/admin/users/{id}/deactivate
        [HttpPost("users/{id}/deactivate")]
        public async Task<IActionResult> DeactivateUser(int id)
        {
            try
            {
                if (!(await IsCurrentUserAdminAsync()))
                {
                    return StatusCode(403, new { success = false, message = "Admin access required" });
                }

                var success = await _adminService.DeactivateUserAsync(id);
                
                if (success)
                {
                    return Ok(new { success = true, message = "User deactivated successfully" });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Failed to deactivate user" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // GET: api/admin/activity-logs
        [HttpGet("activity-logs")]
        public async Task<IActionResult> GetActivityLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                if (!(await IsCurrentUserAdminAsync()))
                {
                    return StatusCode(403, new { success = false, message = "Admin access required" });
                }

                var logs = await _adminService.GetActivityLogsAsync(page, pageSize);
                return Ok(new { success = true, data = logs });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // GET: api/admin/activity-logs/recent
        [HttpGet("activity-logs/recent")]
        public async Task<IActionResult> GetRecentActivityLogs([FromQuery] int count = 10)
        {
            try
            {
                if (!(await IsCurrentUserAdminAsync()))
                {
                    return StatusCode(403, new { success = false, message = "Admin access required" });
                }

                var logs = await _adminService.GetRecentActivityLogsAsync(count);
                return Ok(new { success = true, data = logs });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // GET: api/admin/reports
        [HttpGet("reports")]
        public async Task<IActionResult> GetReports([FromQuery] bool pendingOnly = false)
        {
            try
            {
                if (!(await IsCurrentUserAdminAsync()))
                {
                    return StatusCode(403, new { success = false, message = "Admin access required" });
                }

                var reports = pendingOnly 
                    ? await _adminService.GetPendingReportsAsync()
                    : await _adminService.GetAllReportsAsync();
                
                return Ok(new { success = true, data = reports });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // POST: api/admin/reports/{id}/resolve
        [HttpPost("reports/{id}/resolve")]
        public async Task<IActionResult> ResolveReport(int id, [FromBody] ResolveReportRequest request)
        {
            try
            {
                if (!(await IsCurrentUserAdminAsync()))
                {
                    return StatusCode(403, new { success = false, message = "Admin access required" });
                }

                var currentUserId = GetCurrentUserId();
                if (currentUserId == null)
                {
                    return Unauthorized(new { success = false, message = "Unable to identify admin user" });
                }

                var success = await _adminService.ResolveReportAsync(id, request.Action, request.Notes, currentUserId.Value);
                
                if (success)
                {
                    return Ok(new { success = true, message = "Report resolved successfully" });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Failed to resolve report" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // POST: api/admin/logs
        [HttpPost("logs")]
        public async Task<IActionResult> LogAdminAction([FromBody] AdminActionLogRequest request)
        {
            try
            {
                if (!(await IsCurrentUserAdminAsync()))
                {
                    return StatusCode(403, new { success = false, message = "Admin access required" });
                }

                var currentUserId = GetCurrentUserId();
                if (currentUserId == null)
                {
                    return Unauthorized(new { success = false, message = "Unable to identify admin user" });
                }

                var success = await _adminService.LogAdminActionAsync(currentUserId.Value, request.Action, request.Details);
                
                if (success)
                {
                    return Ok(new { success = true, message = "Action logged successfully" });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Failed to log action" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // POST: api/admin/users/bulk-action
        [HttpPost("users/bulk-action")]
        public async Task<IActionResult> BulkUserAction([FromBody] BulkUserActionRequest request)
        {
            try
            {
                if (!(await IsCurrentUserAdminAsync()))
                {
                    return StatusCode(403, new { success = false, message = "Admin access required" });
                }

                var currentUserId = GetCurrentUserId();
                if (currentUserId == null)
                {
                    return Unauthorized(new { success = false, message = "Unable to identify admin user" });
                }

                var results = new List<BulkActionResult>();

                foreach (var userId in request.UserIds)
                {
                    try
                    {
                        bool success = false;
                        string message = "";

                        switch (request.Action.ToLower())
                        {
                            case "ban":
                                success = await _adminService.BanUserAsync(userId, request.Reason ?? "Bulk action", request.Duration ?? 1, currentUserId.Value);
                                message = success ? "Banned successfully" : "Failed to ban";
                                break;
                            case "unban":
                                success = await _adminService.UnbanUserAsync(userId);
                                message = success ? "Unbanned successfully" : "Failed to unban";
                                break;
                            case "deactivate":
                                success = await _adminService.DeactivateUserAsync(userId);
                                message = success ? "Deactivated successfully" : "Failed to deactivate";
                                break;
                            default:
                                message = "Invalid action";
                                break;
                        }

                        results.Add(new BulkActionResult
                        {
                            UserId = userId,
                            Success = success,
                            Message = message
                        });
                    }
                    catch (Exception ex)
                    {
                        results.Add(new BulkActionResult
                        {
                            UserId = userId,
                            Success = false,
                            Message = ex.Message
                        });
                    }
                }

                var successCount = results.Count(r => r.Success);
                return Ok(new
                {
                    success = true,
                    message = $"Processed {results.Count} users. {successCount} succeeded, {results.Count - successCount} failed.",
                    results = results
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Background Service Control Endpoints
        [HttpGet("background-service/status")]
        public async Task<IActionResult> GetBackgroundServiceStatus()
        {
            try
            {
                if (!await IsCurrentUserAdminAsync())
                {
                    return StatusCode(403, new { success = false, message = "Admin access required" });
                }

                // Get background service status from cache first, then configuration
                var memoryCache = HttpContext.RequestServices.GetService<IMemoryCache>();
                var isEnabled = false; // Default to disabled

                if (memoryCache != null && memoryCache.TryGetValue("BackgroundService:NewsSync:Enabled", out var cachedValue))
                {
                    isEnabled = (bool)cachedValue;
                }
                else
                {
                    // Check configuration if no cache value exists
                    isEnabled = HttpContext.RequestServices.GetService<IConfiguration>()
                        ?.GetValue<bool>("BackgroundServices:NewsSync:Enabled") ?? false;
                    
                    // Store in cache for consistency
                    if (memoryCache != null)
                    {
                        memoryCache.Set("BackgroundService:NewsSync:Enabled", isEnabled, TimeSpan.FromHours(24));
                    }
                }

                return Ok(new
                {
                    success = true,
                    isEnabled = isEnabled,
                    lastSyncTime = DateTime.Now.AddHours(-12), // This could be retrieved from database
                    nextSyncTime = DateTime.Now.AddHours(12),
                    syncInterval = "24 hours"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost("background-service/toggle")]
        public async Task<IActionResult> ToggleBackgroundService([FromBody] ToggleServiceRequest request)
        {
            try
            {
                if (!await IsCurrentUserAdminAsync())
                {
                    return StatusCode(403, new { success = false, message = "Admin access required" });
                }

                // Store the setting in a cache for the background service to check
                var memoryCache = HttpContext.RequestServices.GetService<IMemoryCache>();
                if (memoryCache != null)
                {
                    // Store with a long expiration time (24 hours) to persist across requests
                    memoryCache.Set("BackgroundService:NewsSync:Enabled", request.Enabled, TimeSpan.FromHours(24));
                }

                // Log the admin action
                var user = await GetCurrentUserFromJwt();
                if (user != null)
                {
                    await _adminService.LogAdminActionAsync(user.Id, 
                        $"Background service {(request.Enabled ? "enabled" : "disabled")}", 
                        $"News sync background service was {(request.Enabled ? "enabled" : "disabled")} by admin");
                }

                return Ok(new
                {
                    success = true,
                    message = $"Background service {(request.Enabled ? "enabled" : "disabled")} successfully",
                    isEnabled = request.Enabled
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost("background-service/trigger-sync")]
        public async Task<IActionResult> TriggerManualSync()
        {
            try
            {
                if (!await IsCurrentUserAdminAsync())
                {
                    return StatusCode(403, new { success = false, message = "Admin access required" });
                }

                // Trigger manual sync using the NewsApiService
                var newsApiService = HttpContext.RequestServices.GetService<INewsApiService>();
                if (newsApiService == null)
                {
                    return StatusCode(500, new { success = false, message = "News API service not available" });
                }

                var articlesAdded = await newsApiService.SyncNewsArticlesToDatabase();

                // Log the admin action
                var user = await GetCurrentUserFromJwt();
                if (user != null)
                {
                    await _adminService.LogAdminActionAsync(user.Id, 
                        "Manual news sync triggered", 
                        $"Manual news sync completed. Added {articlesAdded} articles");
                }

                return Ok(new
                {
                    success = true,
                    message = $"Manual sync completed successfully",
                    articlesAdded = articlesAdded
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }

    // Request/Response Models
    public class BanUserRequest
    {
        public string Reason { get; set; } = string.Empty;
        public int Duration { get; set; } // Days
    }

    public class ResolveReportRequest
    {
        public string Action { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    public class AdminActionLogRequest
    {
        public string Action { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
    }

    public class BulkUserActionRequest
    {
        public List<int> UserIds { get; set; } = new List<int>();
        public string Action { get; set; } = string.Empty; // "ban", "unban", "deactivate"
        public string? Reason { get; set; }
        public int? Duration { get; set; }
    }

    public class BulkActionResult
    {
        public int UserId { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class ToggleServiceRequest
    {
        public bool Enabled { get; set; }
    }
}
