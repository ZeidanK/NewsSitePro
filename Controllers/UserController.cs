using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsSite.BL;
using NewsSite.Models;

namespace NewsSite.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        [HttpPut("UpdateProfile")]
        // [Authorize] // Temporarily removed
        public IActionResult UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            try
            {
                // TODO: Implement actual profile update logic with database
                // For now, return success
                return Ok(new { message = "Profile updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating profile", error = ex.Message });
            }
        }

        [HttpPut("UpdatePreferences")]
        // [Authorize] // Temporarily removed
        public IActionResult UpdatePreferences([FromBody] UserPreferencesRequest request)
        {
            try
            {
                // TODO: Implement actual preferences update logic with database
                // For now, return success
                return Ok(new { message = "Preferences updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating preferences", error = ex.Message });
            }
        }

        [HttpGet("Stats")]
        // [Authorize] // Temporarily removed
        public IActionResult GetUserStats()
        {
            try
            {
                // TODO: Implement actual stats retrieval from database
                // For now, return mock data
                var stats = new
                {
                    postsCount = 12,
                    likesCount = 45,
                    savedCount = 23,
                    followersCount = 78
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving stats", error = ex.Message });
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
    public class UpdateProfileRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
    }

    public class UserPreferencesRequest
    {
        public string[] Categories { get; set; } = Array.Empty<string>();
        public bool EmailNotifications { get; set; }
        public bool PushNotifications { get; set; }
        public bool WeeklyDigest { get; set; }
    }
}
