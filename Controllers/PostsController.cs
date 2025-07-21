using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsSite.BL;
using NewsSite.Models;
using NewsSite.Services;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace NewsSite.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly DBservices _dbService;
        private readonly INewsApiService _newsApiService;
        private readonly ILogger<PostsController> _logger;

        public PostsController(DBservices dbService, INewsApiService newsApiService, ILogger<PostsController> logger)
        {
            _dbService = dbService;
            _newsApiService = newsApiService;
            _logger = logger;
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

        // News API Integration Endpoints

        /// <summary>
        /// Manually trigger sync of News API articles to database
        /// </summary>
        [HttpPost("sync-news")]
        public async Task<IActionResult> SyncNewsFromApi()
        {
            try
            {
                var articlesAdded = await _newsApiService.SyncNewsArticlesToDatabase();
                return Ok(new { 
                    success = true, 
                    articlesAdded = articlesAdded,
                    message = $"Successfully synced {articlesAdded} new articles from News API" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing news from API");
                return StatusCode(500, new { 
                    success = false, 
                    message = "Failed to sync news from API" 
                });
            }
        }

        /// <summary>
        /// Get fresh headlines from News API (not stored in database)
        /// </summary>
        [HttpGet("live-headlines")]
        public async Task<IActionResult> GetLiveHeadlines(
            [FromQuery] string? category = null,
            [FromQuery] string? country = null,
            [FromQuery] int? pageSize = null)
        {
            try
            {
                // Apply defaults if parameters are missing
                category = string.IsNullOrEmpty(category) ? "general" : category;
                country = string.IsNullOrEmpty(country) ? "us" : country;
                pageSize = pageSize ?? 20;
                
                var articles = await _newsApiService.FetchTopHeadlinesAsync(category, country, pageSize.Value);
                
                // Transform to match your existing frontend format
                var response = articles.Select(a => new
                {
                    articleID = 0, // Live articles don't have DB IDs yet
                    title = a.Title,
                    content = a.Description ?? a.Content ?? "",
                    imageURL = a.UrlToImage,
                    publishDate = a.PublishedAt,
                    category = category,
                    sourceURL = a.Url,
                    sourceName = a.Source.Name,
                    user = new { username = "NewsBot" },
                    likes = 0,
                    views = 0,
                    isLiked = false,
                    isSaved = false,
                    isLive = true // Flag to indicate this is from News API
                }).ToList();

                return Ok(new { 
                    posts = response,
                    totalPages = 1,
                    currentPage = 1,
                    isLive = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching live headlines");
                return StatusCode(500, new { 
                    success = false, 
                    message = "Failed to fetch live headlines" 
                });
            }
        }

        /// <summary>
        /// Search News API for specific topics
        /// </summary>
        [HttpGet("search-live")]
        public async Task<IActionResult> SearchLiveNews(
            [FromQuery] string query,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return BadRequest(new { success = false, message = "Query parameter is required" });
                }

                var articles = await _newsApiService.FetchEverythingAsync(query, pageSize);
                
                // Transform to match your existing frontend format
                var response = articles.Select(a => new
                {
                    articleID = 0,
                    title = a.Title,
                    content = a.Description ?? a.Content ?? "",
                    imageURL = a.UrlToImage,
                    publishDate = a.PublishedAt,
                    category = "search",
                    sourceURL = a.Url,
                    sourceName = a.Source.Name,
                    user = new { username = "NewsBot" },
                    likes = 0,
                    views = 0,
                    isLiked = false,
                    isSaved = false,
                    isLive = true
                }).ToList();

                return Ok(new { 
                    posts = response,
                    totalPages = 1,
                    currentPage = 1,
                    query = query,
                    isLive = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching live news for query: {query}");
                return StatusCode(500, new { 
                    success = false, 
                    message = "Failed to search live news" 
                });
            }
        }

        /// <summary>
        /// Add a News API article to the database as a new post
        /// </summary>
        [HttpPost("save-live-article")]
        public IActionResult SaveLiveArticle([FromBody] NewsApiArticle article)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == null)
                {
                    return Unauthorized(new { success = false, message = "Authentication required" });
                }

                // Create NewsArticle from NewsApiArticle
                var newsArticle = new NewsArticle
                {
                    Title = article.Title,
                    Content = article.Content ?? article.Description ?? "",
                    SourceURL = article.Url,
                    SourceName = article.Source.Name,
                    ImageURL = article.UrlToImage,
                    PublishDate = article.PublishedAt,
                    Category = "general", // You might want to detect category
                    UserID = currentUserId.Value,
                    Username = User?.Identity?.Name ?? "User"
                };

                var articleId = _dbService.CreateNewsArticle(newsArticle);
                if (articleId > 0)
                {
                    return Ok(new { 
                        success = true, 
                        articleId = articleId,
                        message = "Article saved successfully" 
                    });
                }
                else
                {
                    return BadRequest(new { 
                        success = false, 
                        message = "Failed to save article" 
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving live article");
                return StatusCode(500, new { 
                    success = false, 
                    message = "Failed to save article" 
                });
            }
        }
    }
}