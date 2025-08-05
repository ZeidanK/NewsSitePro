/**
 * NotificationController.cs
 * Purpose: Handles notification operations and user notification management
 * Responsibilities: Notification CRUD operations, notification reading, notification preferences
 * Architecture: Uses NotificationService from BL layer for notification business logic and data operations
 */

using Microsoft.AspNetCore.Mvc;
using NewsSite.BL;
using NewsSite.BL.Services;

namespace NewsSite.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly NotificationService _notificationService;

        public NotificationController(NotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <summary>
        /// Gets paginated notifications for the current user
        /// </summary>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Number of notifications per page (default: 20)</param>
        /// <returns>List of notifications</returns>
        [HttpGet]
        public async Task<IActionResult> GetNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { success = false, message = "Authentication required" });
            }

            try
            {
                var notifications = await _notificationService.GetUserNotificationsAsync(userId.Value, page, pageSize);
                return Ok(new { success = true, notifications = notifications, page = page, pageSize = pageSize });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Gets notification summary for the current user
        /// </summary>
        /// <returns>Notification summary including unread count and recent notifications</returns>
        [HttpGet("summary")]
        public async Task<IActionResult> GetNotificationSummary()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { success = false, message = "Authentication required" });
            }

            try
            {
                var summary = await _notificationService.GetNotificationSummaryAsync(userId.Value);
                return Ok(new { success = true, summary = summary });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Gets unread notification count for the current user
        /// </summary>
        /// <returns>Number of unread notifications</returns>
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { success = false, message = "Authentication required" });
            }

            try
            {
                var count = await _notificationService.GetUnreadNotificationCountAsync(userId.Value);
                return Ok(new { success = true, unreadCount = count });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Marks a specific notification as read
        /// </summary>
        /// <param name="notificationId">ID of the notification to mark as read</param>
        /// <returns>Success status</returns>
        [HttpPut("mark-read/{notificationId}")]
        public async Task<IActionResult> MarkAsRead(int notificationId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { success = false, message = "Authentication required" });
            }

            try
            {
                var result = await _notificationService.MarkNotificationAsReadAsync(notificationId, userId.Value);
                if (result)
                {
                    return Ok(new { success = true, message = "Notification marked as read" });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Failed to mark notification as read or notification not found" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Marks all notifications as read for the current user
        /// </summary>
        /// <returns>Success status</returns>
        [HttpPut("mark-all-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { success = false, message = "Authentication required" });
            }

            try
            {
                var result = await _notificationService.MarkAllNotificationsAsReadAsync(userId.Value);
                if (result)
                {
                    return Ok(new { success = true, message = "All notifications marked as read" });
                }
                else
                {
                    return BadRequest(new { success = false, message = "No notifications to mark as read" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Creates a custom notification (admin use)
        /// </summary>
        /// <param name="request">Notification creation request</param>
        /// <returns>ID of created notification</returns>
        [HttpPost("admin/create")]
        public async Task<IActionResult> CreateNotification([FromBody] CreateNotificationRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { success = false, message = "Authentication required" });
            }

            try
            {
                // Set the FromUserID to the current user (admin)
                request.FromUserID = userId.Value;
                
                var notificationId = await _notificationService.CreateCustomNotificationAsync(request);
                if (notificationId > 0)
                {
                    return Ok(new { success = true, notificationId = notificationId, message = "Notification created successfully" });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Failed to create notification" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Creates an admin message notification for a specific user
        /// </summary>
        /// <param name="request">Admin message request</param>
        /// <returns>Success status</returns>
        [HttpPost("admin/message")]
        public async Task<IActionResult> SendAdminMessage([FromBody] AdminMessageRequest request)
        {
            var adminId = GetCurrentUserId();
            if (adminId == null)
            {
                return Unauthorized(new { success = false, message = "Authentication required" });
            }

            try
            {
                var notificationId = await _notificationService.CreateAdminMessageNotificationAsync(
                    request.UserID,
                    request.Title,
                    request.Message,
                    adminId.Value,
                    request.ActionUrl
                );

                if (notificationId > 0)
                {
                    return Ok(new { success = true, notificationId = notificationId, message = "Admin message sent successfully" });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Failed to send admin message" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Creates a system update notification for all users or specific users
        /// </summary>
        /// <param name="request">System update request</param>
        /// <returns>Success status</returns>
        [HttpPost("admin/system-update")]
        public async Task<IActionResult> SendSystemUpdate([FromBody] SystemUpdateRequest request)
        {
            var adminId = GetCurrentUserId();
            if (adminId == null)
            {
                return Unauthorized(new { success = false, message = "Authentication required" });
            }

            try
            {
                int notificationsCreated = 0;

                if (request.UserIDs != null && request.UserIDs.Any())
                {
                    // Send to specific users
                    notificationsCreated = await _notificationService.CreateBulkNotificationsAsync(
                        request.UserIDs,
                        NotificationTypes.SystemUpdate,
                        request.Title,
                        request.Message,
                        adminId.Value,
                        request.ActionUrl
                    );
                }
                else
                {
                    // For sending to all users, you'd need to implement a method to get all user IDs
                    return BadRequest(new { success = false, message = "User IDs are required for system updates" });
                }

                return Ok(new { 
                    success = true, 
                    notificationsCreated = notificationsCreated, 
                    message = $"System update sent to {notificationsCreated} users" 
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        private int? GetCurrentUserId()
        {
            return NewsSite.BL.User.GetCurrentUserId(Request, User);
        }
    }

    // Request models for the notification endpoints
    public class AdminMessageRequest
    {
        public int UserID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? ActionUrl { get; set; }
    }

    public class SystemUpdateRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? ActionUrl { get; set; }
        public List<int>? UserIDs { get; set; }
    }
}
