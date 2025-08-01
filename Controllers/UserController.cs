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
        // [Authorize] // Temporarily removed for testing
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
            try
            {
                var jwtToken = Request.Cookies["jwtToken"];
                if (!string.IsNullOrEmpty(jwtToken))
                {
                    try
                    {
                        var currentUser = new User().ExtractUserFromJWT(jwtToken);
                        return currentUser?.Id;
                    }
                    catch
                    {
                        return null;
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        [HttpGet("Stats")]
        public async Task<IActionResult> GetUserStats()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == null)
                {
                    return Unauthorized(new { message = "Authentication required" });
                }

                var stats = await _userService.GetUserStatsAsync(currentUserId.Value);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving user stats", error = ex.Message });
            }
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
                // Check authentication
                var currentUserId = GetCurrentUserId();
                if (currentUserId == null)
                {
                    return Unauthorized(new { success = false, message = "Authentication required" });
                }

                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { success = false, message = "No file selected" });
                }

                // Validate file type
                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
                if (!allowedTypes.Contains(file.ContentType.ToLower()))
                {
                    return BadRequest(new { success = false, message = "Only JPEG, PNG, GIF, and WebP images are allowed" });
                }

                // Validate file size (max 5MB)
                if (file.Length > 5 * 1024 * 1024)
                {
                    return BadRequest(new { success = false, message = "File size must be less than 5MB" });
                }

                // Create uploads directory if it doesn't exist
                var uploadsPath = Path.Combine("wwwroot", "uploads", "profile-pictures");
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                // Generate unique filename with user ID prefix for organization
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var fileName = $"user_{currentUserId}_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString("N")[..8]}{fileExtension}";
                var filePath = Path.Combine(uploadsPath, fileName);

                // Get the current user's existing profile picture to delete it later
                var currentUser = await _userService.GetUserByIdAsync(currentUserId.Value);
                string? oldProfilePicPath = null;
                if (currentUser != null && !string.IsNullOrEmpty(currentUser.ProfilePicture))
                {
                    // Extract filename from URL path to delete old file
                    var oldFileName = Path.GetFileName(currentUser.ProfilePicture);
                    if (!string.IsNullOrEmpty(oldFileName))
                    {
                        oldProfilePicPath = Path.Combine("wwwroot", "uploads", "profile-pictures", oldFileName);
                    }
                }

                // Save new file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Update user's profile picture in database
                var imageUrl = $"/uploads/profile-pictures/{fileName}";
                var updateSuccess = await _userService.UpdateUserProfilePicAsync(currentUserId.Value, imageUrl);

                if (updateSuccess)
                {
                    // Delete old profile picture file if it exists and update was successful
                    if (!string.IsNullOrEmpty(oldProfilePicPath) && System.IO.File.Exists(oldProfilePicPath))
                    {
                        try
                        {
                            System.IO.File.Delete(oldProfilePicPath);
                        }
                        catch
                        {
                            // Log the error but don't fail the request
                            // In production, you should log this properly
                        }
                    }

                    return Ok(new { 
                        success = true, 
                        message = "Profile picture uploaded successfully", 
                        imageUrl = imageUrl 
                    });
                }
                else
                {
                    // Delete the uploaded file if database update failed
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                    return StatusCode(500, new { success = false, message = "Failed to update profile picture in database" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Upload failed", error = ex.Message });
            }
        }

        [HttpPost("Follow/{id}")]
        public async Task<IActionResult> FollowUser(int id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (!currentUserId.HasValue)
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                if (currentUserId.Value == id)
                {
                    return BadRequest(new { message = "Cannot follow yourself" });
                }

                // Use actual database implementation
                var dbService = new DBservices();
                var result = await dbService.ToggleUserFollow(currentUserId.Value, id);

                return Ok(new { 
                    action = result.Action,
                    message = $"User {result.Action} successfully",
                    isFollowing = result.IsFollowing
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating follow status", error = ex.Message });
            }
        }

        [HttpGet("Follow/Status/{id}")]
        public async Task<IActionResult> GetFollowStatus(int id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (!currentUserId.HasValue)
                {
                    return Ok(new { isFollowing = false });
                }

                var dbService = new DBservices();
                var isFollowing = await dbService.IsUserFollowing(currentUserId.Value, id);

                return Ok(new { isFollowing = isFollowing });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error checking follow status", error = ex.Message });
            }
        }

        [HttpPost("Block/{id}")]
        public IActionResult BlockUser(int id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (!currentUserId.HasValue)
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                if (currentUserId.Value == id)
                {
                    return BadRequest(new { message = "Cannot block yourself" });
                }

                // For now, return a mock response since we don't have the block system implemented yet
                // In a real implementation, you would add the user to a blocked users table
                var isBlocked = false; // Placeholder - would check database
                var action = isBlocked ? "unblocked" : "blocked";

                return Ok(new { 
                    action = action,
                    message = $"User {action} successfully",
                    isBlocked = !isBlocked
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating block status", error = ex.Message });
            }
        }
    }

    // Request models
}
