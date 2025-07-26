using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsSite.BL;
using NewsSite.BL.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace NewsSite.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly DBservices _dbService;

        public AdminController(IAdminService adminService, DBservices dbService)
        {
            _adminService = adminService;
            _dbService = dbService;
        }

        // Use the centralized User class method for getting current user ID
        private int? GetCurrentUserId()
        {
            return BL.User.GetCurrentUserId(Request, User);
        }

        // Helper method to get current user from JWT
        private User? GetCurrentUserFromJwt()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return null;

                var currentUser = _dbService.GetUserById(userId.Value);
                return currentUser;
            }
            catch
            {
                return null;
            }
        }

        // Helper method to check if current user is admin
        private bool IsCurrentUserAdmin()
        {
            var user = GetCurrentUserFromJwt();
            return user?.IsAdmin ?? false;
        }

        // GET: api/admin/test - Test endpoint to verify API connectivity
        [HttpGet("test")]
        public IActionResult TestConnection()
        {
            try
            {
                var user = GetCurrentUserFromJwt();
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
        [HttpGet("test")]
        public IActionResult Test()
        {
            try
            {
                var userId = GetCurrentUserId();
                var user = GetCurrentUserFromJwt();
                var isAdmin = IsCurrentUserAdmin();

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
                if (!IsCurrentUserAdmin())
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
                if (!IsCurrentUserAdmin())
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
                if (!IsCurrentUserAdmin())
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
                if (!IsCurrentUserAdmin())
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
                if (!IsCurrentUserAdmin())
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
                if (!IsCurrentUserAdmin())
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
                if (!IsCurrentUserAdmin())
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
                if (!IsCurrentUserAdmin())
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
                if (!IsCurrentUserAdmin())
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
                if (!IsCurrentUserAdmin())
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
                if (!IsCurrentUserAdmin())
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
                if (!IsCurrentUserAdmin())
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
}
