using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsSitePro.Models;
using Microsoft.AspNetCore.Identity;
using NewsSite.BL;
using NewsSite.Models;

namespace NewsSite.Pages
{
    // [Authorize] // Temporarily removed to fix 401 error - we'll handle auth in the page
    public class PostModel : PageModel
    {
        private readonly DBservices _dbService;

        public HeaderViewModel HeaderData { get; set; } = new HeaderViewModel();
        public NewsArticle? PostData { get; set; }
        public List<Comment> Comments { get; set; } = new List<Comment>();
        public bool IsIndividualPost { get; set; } = false;

        public PostModel()
        {
            _dbService = new DBservices();
        }

        public async Task<IActionResult> OnGet(int? id = null)
        {
            try
            {
                var isAuthenticated = User?.Identity?.IsAuthenticated ?? false;
                int? currentUserId = null;
                
                // Try to get user ID from JWT token first
                var jwtToken = Request.Cookies["jwtToken"] ?? Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "");
                if (!string.IsNullOrEmpty(jwtToken))
                {
                    try
                    {
                        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                        var jsonToken = handler.ReadJwtToken(jwtToken);
                        var userIdClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "userId" || c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
                        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                        {
                            currentUserId = userId;
                            isAuthenticated = true;
                        }
                    }
                    catch (Exception)
                    {
                        // Token might be invalid, continue without authentication
                    }
                }

                // Fallback to claims if available
                if (!currentUserId.HasValue && isAuthenticated)
                {
                    var userIdClaim = User?.Claims?.FirstOrDefault(c => c.Type == "userId")?.Value;
                    if (int.TryParse(userIdClaim, out int userId))
                    {
                        currentUserId = userId;
                    }
                }
                
                HeaderData = new HeaderViewModel
                {
                    UserName = isAuthenticated ? User?.Identity?.Name ?? "Guest" : "Guest",
                    NotificationCount = isAuthenticated ? 3 : 0,
                    CurrentPage = id.HasValue ? "Post" : "NewsFeed",
                    user = isAuthenticated ? new User 
                    { 
                        Id = currentUserId ?? 0,
                        Name = User?.Identity?.Name ?? "Guest",
                        Email = User?.Claims?.FirstOrDefault(c => c.Type == "email")?.Value ?? "",
                        IsAdmin = User?.IsInRole("Admin") == true || User?.Claims?.Any(c => c.Type == "isAdmin" && c.Value == "True") == true
                    } : null
                };
                
                ViewData["HeaderData"] = HeaderData;

                // If ID is provided, load individual post
                if (id.HasValue && id.Value > 0)
                {
                    try
                    {
                        IsIndividualPost = true;
                        PostData = await _dbService.GetNewsArticleById(id.Value);
                        
                        if (PostData == null)
                        {
                            ViewData["ErrorMessage"] = "Post not found.";
                            return NotFound();
                        }

                        // Get comments for this post
                        Comments = await _dbService.GetCommentsByPostId(id.Value) ?? new List<Comment>();

                        // Increment view count
                        _dbService.RecordArticleView(id.Value, currentUserId);
                    }
                    catch (Exception ex)
                    {
                        // Log error in production
                        ViewData["ErrorMessage"] = "Error loading post: " + ex.Message;
                        IsIndividualPost = false;
                        return Page();
                    }
                }
                else
                {
                    IsIndividualPost = false;
                }

                return Page();
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "An unexpected error occurred: " + ex.Message;
                return Page();
            }
        }

        // API method to add a comment
        public async Task<IActionResult> OnPostAddComment([FromBody] AddCommentRequest request)
        {
            try
            {
                var isAuthenticated = User?.Identity?.IsAuthenticated ?? false;
                if (!isAuthenticated)
                {
                    return new JsonResult(new { success = false, message = "Please log in to comment" });
                }

                var userId = int.Parse(User?.Claims?.FirstOrDefault(c => c.Type == "userId")?.Value ?? "0");
                if (userId == 0)
                {
                    return new JsonResult(new { success = false, message = "Invalid user" });
                }

                var comment = new Comment
                {
                    PostID = request.PostId,
                    UserID = userId,
                    Content = request.Content,
                    CreatedAt = DateTime.Now
                };

                var success = await _dbService.CreateComment(comment);
                
                if (success)
                {
                    return new JsonResult(new { success = true, message = "Comment added successfully" });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Failed to add comment" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
            }
        }
    }

    public class AddCommentRequest
    {
        public int PostId { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}