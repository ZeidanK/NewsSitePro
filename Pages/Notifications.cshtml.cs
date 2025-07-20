using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using NewsSite.BL;

namespace NewsSite.Pages
{
    public class NotificationsModel : PageModel
    {
        private readonly DBservices dbService;
        private readonly IConfiguration configuration;

        public NotificationsModel(DBservices dbService, IConfiguration configuration)
        {
            this.dbService = dbService;
            this.configuration = configuration;
        }

        public List<Notification> Notifications { get; set; } = new List<Notification>();
        public NotificationSummary Summary { get; set; } = new NotificationSummary();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public const int PageSize = 20;

        // Add UserPreferences for settings modal
        public Dictionary<string, NotificationPreferenceSettings> UserPreferences { get; set; } = new Dictionary<string, NotificationPreferenceSettings>();

        public async Task<IActionResult> OnGetAsync(int page = 1)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToPage("/Login");
            }

            CurrentPage = Math.Max(1, page);

            try
            {
                // Get notification summary
                Summary = await dbService.GetNotificationSummary(userId.Value);

                // Get paginated notifications
                Notifications = await dbService.GetUserNotifications(userId.Value, CurrentPage, PageSize);

                // Calculate total pages (simplified)
                TotalPages = Math.Max(1, (int)Math.Ceiling((double)Summary.TotalCount / PageSize));

                // Get user preferences
                // Preferences = await dbService.GetUserNotificationPreferences(userId.Value);
                // Also populate UserPreferences for settings modal
                UserPreferences = await dbService.GetUserNotificationPreferencesDictionary(userId.Value);

                return Page();
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error loading notifications: {ex.Message}");
                return Page();
            }
        }

        public async Task<IActionResult> OnPostMarkAsReadAsync([FromBody] int notificationId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            try
            {
                bool success = await dbService.MarkNotificationAsRead(notificationId, userId.Value);
                return new JsonResult(new { success = success });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> OnPostMarkAllAsReadAsync()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            try
            {
                bool success = await dbService.MarkAllNotificationsAsRead(userId.Value);
                return new JsonResult(new { success = success });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> OnPostUpdatePreferencesAsync([FromBody] UpdateNotificationPreferencesRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == null || userId.Value != request.UserID)
            {
                return Unauthorized();
            }

            try
            {
                bool success = await dbService.UpdateNotificationPreferences(request.UserID, request.Preferences);
                return new JsonResult(new { success = success });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> OnGetUnreadCountAsync()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return new JsonResult(new { count = 0 });
            }

            try
            {
                var summary = await dbService.GetNotificationSummary(userId.Value);
                return new JsonResult(new { count = summary.UnreadCount });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { count = 0, error = ex.Message });
            }
        }

        public async Task<IActionResult> OnGetRecentNotificationsAsync()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return new JsonResult(new { success = false, message = "Unauthorized" });
            }

            try
            {
                var notifications = await dbService.GetUserNotifications(userId.Value, 1, 10);
                return new JsonResult(new
                {
                    success = true,
                    notifications = notifications.Take(10).Select(n => new {
                        id = n.NotificationID,
                        title = n.Title,
                        message = n.Message,
                        isRead = n.IsRead,
                        createdAt = n.CreatedAt
                    })
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        private int? GetCurrentUserId()
        {
            try
            {
                // Try to get from JWT token in Authorization header first
                var authHeader = Request.Headers["Authorization"].FirstOrDefault();
                if (authHeader != null && authHeader.StartsWith("Bearer "))
                {
                    var token = authHeader.Substring("Bearer ".Length).Trim();
                    var handler = new JwtSecurityTokenHandler();
                    var jsonToken = handler.ReadJwtToken(token);
                    var userIdClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "userId" || c.Type == ClaimTypes.NameIdentifier);
                    if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int jwtUserId))
                    {
                        return jwtUserId;
                    }
                }

                // Fallback to cookie
                if (Request.Cookies.TryGetValue("jwtToken", out string? cookieToken) && !string.IsNullOrEmpty(cookieToken))
                {
                    var handler = new JwtSecurityTokenHandler();
                    var jsonToken = handler.ReadJwtToken(cookieToken);
                    var userIdClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "userId" || c.Type == ClaimTypes.NameIdentifier);
                    if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int cookieUserId))
                    {
                        return cookieUserId;
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}
