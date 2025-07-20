using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsSite.BL;
using NewsSite.Models;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace NewsSite.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly DBservices _dbService;

        public PostsController()
        {
            _dbService = new DBservices();
        }

        [HttpGet]
        public IActionResult GetPosts([FromQuery] int page = 1, [FromQuery] int limit = 10, [FromQuery] string? filter = null)
        {
            try
            {
                // Get current user ID from session/token (for now using mock ID)
                int? currentUserId = GetCurrentUserId(); // You'll need to implement this based on your auth

                var articles = _dbService.GetAllNewsArticles(page, limit, filter, currentUserId);
                
                // Calculate total pages (you might want to modify the stored procedure to return this)
                var totalPages = Math.Max(1, (int)Math.Ceiling(articles.Count / (double)limit));

                var response = articles.Select(a => new
                {
                    articleID = a.ArticleID,
                    title = a.Title,
                    content = a.Content,
                    imageURL = a.ImageURL,
                    publishDate = a.PublishDate,
                    category = a.Category,
                    sourceURL = a.SourceURL,
                    sourceName = a.SourceName,
                    user = new { username = a.Username },
                    likes = a.LikesCount,
                    views = a.ViewsCount,
                    isLiked = a.IsLiked,
                    isSaved = a.IsSaved
                }).ToList();

                return Ok(new { posts = response, totalPages });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error loading posts", error = ex.Message });
            }
        }

        [HttpPost("Like/{id}")]
        // [Authorize] // Temporarily removed
        public IActionResult LikePost(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var result = _dbService.ToggleArticleLike(id, userId.Value);
                return Ok(new { message = $"Post {result} successfully", action = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error liking post", error = ex.Message });
            }
        }

        [HttpPost("Report/{id}")]
        // [Authorize] // Temporarily removed
        public IActionResult ReportPost(int id, [FromBody] ReportRequest? request = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var success = _dbService.ReportArticle(id, userId.Value, request?.Reason);
                if (success)
                {
                    return Ok(new { message = "Post reported successfully" });
                }
                return StatusCode(500, new { message = "Error reporting post" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error reporting post", error = ex.Message });
            }
        }

        [HttpPost("Save/{id}")]
        // [Authorize] // Temporarily removed
        public IActionResult SavePost(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var result = _dbService.ToggleSaveArticle(id, userId.Value);
                return Ok(new { message = $"Post {result} successfully", action = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error saving post", error = ex.Message });
            }
        }

        [HttpPost("View/{id}")]
        public IActionResult RecordView(int id)
        {
            try
            {
                var userId = GetCurrentUserId(); // Can be null for anonymous users
                var success = _dbService.RecordArticleView(id, userId);
                return Ok(new { message = "View recorded" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error recording view", error = ex.Message });
            }
        }

        [HttpPost("Create")]
        // [Authorize] // Temporarily removed
        public IActionResult CreatePost([FromBody] CreatePostRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var article = new NewsArticle
                {
                    Title = request.Title,
                    Content = request.Content,
                    ImageURL = request.ImageURL,
                    Category = request.Category,
                    SourceURL = request.SourceURL,
                    SourceName = request.SourceName,
                    UserID = userId.Value,
                    PublishDate = DateTime.Now
                };

                var articleId = _dbService.CreateNewsArticle(article);
                return Ok(new { message = "Post created successfully", postId = articleId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating post", error = ex.Message });
            }
        }

        [HttpGet("User/{userId}")]
        public IActionResult GetUserPosts(int userId, [FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            try
            {
                var articles = _dbService.GetArticlesByUser(userId, page, limit);
                
                var response = articles.Select(a => new
                {
                    articleID = a.ArticleID,
                    title = a.Title,
                    content = a.Content,
                    imageURL = a.ImageURL,
                    publishDate = a.PublishDate,
                    category = a.Category,
                    sourceURL = a.SourceURL,
                    sourceName = a.SourceName,
                    likes = a.LikesCount,
                    views = a.ViewsCount
                }).ToList();

                return Ok(new { posts = response });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error loading user posts", error = ex.Message });
            }
        }

        // Proper JWT authentication method
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

                // Try to get from Authorization header
                var authHeader = Request.Headers["Authorization"].FirstOrDefault();
                if (authHeader != null && authHeader.StartsWith("Bearer "))
                {
                    var token = authHeader.Substring("Bearer ".Length).Trim();
                    var handler = new JwtSecurityTokenHandler();
                    
                    if (handler.CanReadToken(token))
                    {
                        var jsonToken = handler.ReadJwtToken(token);
                        var idClaim = jsonToken.Claims.FirstOrDefault(x => x.Type == "id");
                        
                        if (idClaim != null && int.TryParse(idClaim.Value, out int tokenUserId))
                        {
                            return tokenUserId;
                        }
                    }
                }

                // Fallback: check for JWT token in cookies
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(int id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == null)
                {
                    return Unauthorized(new { success = false, message = "Authentication required" });
                }

                // Get the post to check ownership
                var post = await _dbService.GetNewsArticleById(id);
                if (post == null)
                {
                    return NotFound(new { success = false, message = "Post not found" });
                }

                // Check if current user owns the post or is admin
                if (post.UserID != currentUserId && !IsCurrentUserAdmin())
                {
                    return StatusCode(403, new { success = false, message = "You don't have permission to delete this post" });
                }

                // Delete the post
                bool success = await _dbService.DeleteNewsArticle(id);
                if (success)
                {
                    return Ok(new { success = true, message = "Post deleted successfully" });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Failed to delete post" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Internal server error: " + ex.Message });
            }
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

                // Check Authorization header
                var authHeader = Request.Headers["Authorization"].FirstOrDefault();
                if (authHeader != null && authHeader.StartsWith("Bearer "))
                {
                    var token = authHeader.Substring("Bearer ".Length).Trim();
                    var handler = new JwtSecurityTokenHandler();
                    
                    if (handler.CanReadToken(token))
                    {
                        var jsonToken = handler.ReadJwtToken(token);
                        var adminTokenClaim = jsonToken.Claims.FirstOrDefault(x => x.Type == "isAdmin");
                        
                        if (adminTokenClaim != null && bool.TryParse(adminTokenClaim.Value, out bool tokenIsAdmin))
                        {
                            return tokenIsAdmin;
                        }
                    }
                }

                // Check cookies
                var jwtCookie = Request.Cookies["jwtToken"];
                if (!string.IsNullOrEmpty(jwtCookie))
                {
                    var handler = new JwtSecurityTokenHandler();
                    if (handler.CanReadToken(jwtCookie))
                    {
                        var jsonToken = handler.ReadJwtToken(jwtCookie);
                        var adminCookieClaim = jsonToken.Claims.FirstOrDefault(x => x.Type == "isAdmin");
                        
                        if (adminCookieClaim != null && bool.TryParse(adminCookieClaim.Value, out bool cookieIsAdmin))
                        {
                            return cookieIsAdmin;
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
    }
}