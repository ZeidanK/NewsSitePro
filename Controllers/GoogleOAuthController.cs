/**
 * GoogleOAuthController.cs
 * Purpose: Handles Google OAuth authentication flow and session management
 * Responsibilities: OAuth callback handling, user authentication via Google, session management
 * Architecture: Uses GoogleOAuthService from BL layer for OAuth operations and token management
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsSite.BL;
using NewsSite.BL.Services;
using NewsSite.BL.Extensions;

namespace NewsSite.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GoogleOAuthController : ControllerBase
    {
        private readonly IGoogleOAuthService _googleOAuthService;
        private readonly IConfiguration _config;

        public GoogleOAuthController(IGoogleOAuthService googleOAuthService, IConfiguration config)
        {
            _googleOAuthService = googleOAuthService;
            _config = config;
        }

        /// <summary>
        /// Get Google OAuth URL for client-side redirection
        /// </summary>
        [HttpGet("auth-url")]
        public IActionResult GetGoogleAuthUrl()
        {
            try
            {
                var isDevelopment = HttpContext.Request.Host.Host == "localhost" || 
                                    HttpContext.Request.Host.Host.StartsWith("127.0.0.1");
                
                var authUrl = _googleOAuthService.GenerateGoogleOAuthUrl(isDevelopment);
                
                return Ok(new { authUrl, isDevelopment });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to generate OAuth URL", error = ex.Message });
            }
        }

        /// <summary>
        /// Handle Google OAuth callback
        /// </summary>
        [HttpPost("callback")]
        public async Task<IActionResult> HandleGoogleCallback([FromBody] GoogleOAuthRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.AuthorizationCode))
                {
                    return BadRequest(new { message = "Authorization code is required" });
                }

                // Get client info for session tracking
                request.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                request.UserAgent = HttpContext.Request.Headers["User-Agent"].ToString();
                request.DeviceInfo = GetDeviceInfo();

                var result = await _googleOAuthService.HandleGoogleOAuthAsync(request);

                if (!result.Success)
                {
                    return BadRequest(new { message = result.Message });
                }

                // Set JWT token in cookie
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = false, // Allow JavaScript access
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTime.UtcNow.AddDays(30)
                };

                Response.Cookies.Append("jwtToken", result.Token!, cookieOptions);

                return Ok(new
                {
                    success = true,
                    token = result.Token,
                    user = new
                    {
                        id = result.User!.Id,
                        name = result.User.Name,
                        email = result.User.Email,
                        isAdmin = result.User.IsAdmin,
                        isGoogleUser = result.User.IsGoogleUser,
                        profilePicture = result.User.ProfilePicture
                    },
                    isNewUser = result.IsNewUser,
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "OAuth callback failed", error = ex.Message });
            }
        }

        /// <summary>
        /// Validate current session
        /// </summary>
        [HttpPost("validate-session")]
        public async Task<IActionResult> ValidateSession([FromBody] SessionValidationRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.SessionToken))
                {
                    return BadRequest(new { message = "Session token is required" });
                }

                var result = await _googleOAuthService.ValidateSessionAsync(request.SessionToken);

                if (!result.IsValid)
                {
                    return Unauthorized(new { message = result.Message });
                }

                return Ok(new
                {
                    isValid = true,
                    user = new
                    {
                        id = result.User!.Id,
                        name = result.User.Name,
                        email = result.User.Email,
                        isAdmin = result.User.IsAdmin,
                        isGoogleUser = result.User.IsGoogleUser,
                        profilePicture = result.User.ProfilePicture
                    },
                    session = new
                    {
                        sessionId = result.Session!.SessionID,
                        loginTime = result.Session.LoginTime,
                        expiryTime = result.Session.ExpiryTime
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Session validation failed", error = ex.Message });
            }
        }

        /// <summary>
        /// Logout user and invalidate session
        /// </summary>
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.SessionToken))
                {
                    return BadRequest(new { message = "Session token is required" });
                }

                var success = await _googleOAuthService.LogoutAsync(request.SessionToken, request.LogoutReason);

                if (success)
                {
                    // Clear JWT cookie
                    Response.Cookies.Delete("jwtToken");
                    
                    return Ok(new { message = "Logout successful" });
                }

                return BadRequest(new { message = "Logout failed" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Logout failed", error = ex.Message });
            }
        }

        /// <summary>
        /// Get user's login history
        /// </summary>
        [HttpGet("login-history")]
        [Authorize]
        public async Task<IActionResult> GetLoginHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var currentUserId = User.GetUserId();
                if (currentUserId == null)
                {
                    return Unauthorized(new { message = "Authentication required" });
                }

                var history = await _googleOAuthService.GetUserLoginHistoryAsync(currentUserId.Value, page, pageSize);

                return Ok(new
                {
                    sessions = history.Sessions.Select(s => new
                    {
                        sessionId = s.SessionID,
                        deviceInfo = s.DeviceInfo,
                        ipAddress = s.IpAddress,
                        loginTime = s.LoginTime,
                        lastActivityTime = s.LastActivityTime,
                        logoutTime = s.LogoutTime,
                        logoutReason = s.LogoutReason,
                        isActive = s.IsActive,
                        sessionDurationMinutes = s.SessionDurationMinutes
                    }),
                    totalSessions = history.TotalSessions,
                    pageNumber = history.PageNumber,
                    pageSize = history.PageSize,
                    totalPages = history.TotalPages
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to retrieve login history", error = ex.Message });
            }
        }

        /// <summary>
        /// Admin endpoint to get session statistics
        /// </summary>
        [HttpGet("session-stats")]
        [Authorize]
        public async Task<IActionResult> GetSessionStats()
        {
            try
            {
                var currentUserId = User.GetUserId();
                if (currentUserId == null)
                {
                    return Unauthorized(new { message = "Authentication required" });
                }

                // Check if user is admin (you can implement this check)
                // For now, allowing all authenticated users
                
                var stats = await _googleOAuthService.GetSessionStatsAsync();

                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to retrieve session stats", error = ex.Message });
            }
        }

        /// <summary>
        /// Admin endpoint to cleanup expired sessions
        /// </summary>
        [HttpPost("cleanup-sessions")]
        [Authorize]
        public async Task<IActionResult> CleanupSessions()
        {
            try
            {
                var currentUserId = User.GetUserId();
                if (currentUserId == null)
                {
                    return Unauthorized(new { message = "Authentication required" });
                }

                // Check if user is admin (implement admin check)
                
                await _googleOAuthService.CleanupExpiredSessionsAsync();

                return Ok(new { message = "Session cleanup completed" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Session cleanup failed", error = ex.Message });
            }
        }

        private string GetDeviceInfo()
        {
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
            
            if (userAgent.Contains("Mobile"))
                return "Mobile";
            else if (userAgent.Contains("Tablet"))
                return "Tablet";
            else
                return "Desktop";
        }
    }

    // Request models for this controller
    public class SessionValidationRequest
    {
        public string SessionToken { get; set; } = string.Empty;
    }
}
