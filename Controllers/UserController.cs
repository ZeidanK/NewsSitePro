using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsSite.BL;
using NewsSite.BL.Services;
using NewsSite.Models;

namespace NewsSite.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly INewsService _newsService;

        public UserController(IUserService userService, INewsService newsService)
        {
            _userService = userService;
            _newsService = newsService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                // Don't return sensitive information
                var userInfo = new
                {
                    id = user.Id,
                    name = user.Name,
                    email = user.Email,
                    bio = user.Bio,
                    joinDate = user.JoinDate,
                    isAdmin = user.IsAdmin
                };

                return Ok(userInfo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving user", error = ex.Message });
            }
        }

        [HttpGet("{id}/stats")]
        public async Task<IActionResult> GetUserStats(int id)
        {
            try
            {
                var stats = await _userService.GetUserStatsAsync(id);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving user stats", error = ex.Message });
            }
        }

        [HttpGet("{id}/posts")]
        public async Task<IActionResult> GetUserPosts(int id, [FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            try
            {
                var articles = await _newsService.GetArticlesByUserAsync(id, page, limit);
                return Ok(new { posts = articles });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error loading user posts", error = ex.Message });
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchUsers([FromQuery] string query, [FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return BadRequest(new { message = "Search query is required" });
                }

                var users = await _userService.SearchUsersAsync(query, page, limit);
                
                // Return only public information
                var publicUsers = users.Select(u => new
                {
                    id = u.Id,
                    name = u.Name,
                    bio = u.Bio,
                    joinDate = u.JoinDate
                }).ToList();

                return Ok(new { users = publicUsers });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error searching users", error = ex.Message });
            }
        }

        [HttpPut("UpdateProfile")]
        [Authorize] // Re-enable authorization
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == null)
                {
                    return Unauthorized(new { message = "Authentication required" });
                }

                var success = await _userService.UpdateUserProfileAsync(currentUserId.Value, request.Username, request.Bio);
                if (success)
                {
                    return Ok(new { message = "Profile updated successfully" });
                }
                else
                {
                    return BadRequest(new { message = "Failed to update profile" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating profile", error = ex.Message });
            }
        }

        [HttpPut("ChangePassword")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == null)
                {
                    return Unauthorized(new { message = "Authentication required" });
                }

                // Validate current password first
                var user = await _userService.GetUserByIdAsync(currentUserId.Value);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                if (request.CurrentPassword != user.PasswordHash)
                {
                    return BadRequest(new { message = "Current password is incorrect" });
                }

                // For now, use simple string assignment (in real app, hash the password)
                var newPasswordHash = request.NewPassword;
                var success = await _userService.ChangePasswordAsync(currentUserId.Value, request.CurrentPassword, request.NewPassword);

                if (success)
                {
                    return Ok(new { message = "Password changed successfully" });
                }
                else
                {
                    return BadRequest(new { message = "Failed to change password" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error changing password", error = ex.Message });
            }
        }

        private int? GetCurrentUserId()
        {
            // Implement JWT token parsing logic here
            // For now, return a mock ID
            return 1; // This should be replaced with actual JWT parsing
        }

        [HttpGet("Preferences")]
        // [Authorize] // Temporarily removed
        public IActionResult GetUserPreferences()
        {
            try
            {
                // TODO: Implement actual preferences retrieval from database
                // For now, return mock data
                var preferences = new
                {
                    categories = new[] { "technology", "sports" },
                    emailNotifications = true,
                    pushNotifications = false,
                    weeklyDigest = true
                };

                return Ok(preferences);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving preferences", error = ex.Message });
            }
        }

        [HttpPost("UploadProfilePic")]
        public async Task<IActionResult> UploadProfilePic(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { success = false, message = "No file selected" });
                }

                // Validate file type
                var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif" };
                if (!allowedTypes.Contains(file.ContentType))
                {
                    return BadRequest(new { success = false, message = "Only JPEG, PNG, and GIF images are allowed" });
                }

                // Validate file size (max 5MB)
                if (file.Length > 5 * 1024 * 1024)
                {
                    return BadRequest(new { success = false, message = "File size must be less than 5MB" });
                }

                // Create uploads directory if it doesn't exist
                var uploadsPath = Path.Combine("wwwroot", "uploads", "profiles");
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                // Generate unique filename
                var fileExtension = Path.GetExtension(file.FileName);
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsPath, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // TODO: Update user's profile picture in database
                // For now, just return the URL
                var imageUrl = $"/uploads/profiles/{fileName}";
                
                return Ok(new { 
                    success = true, 
                    message = "Profile picture uploaded successfully", 
                    imageUrl = imageUrl 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Upload failed", error = ex.Message });
            }
        }
    }

    // Request models
}
