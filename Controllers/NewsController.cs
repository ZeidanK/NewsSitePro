using Microsoft.AspNetCore.Mvc;
using NewsSite.BL;
using NewsSite.Services;
using NewsSitePro.Models;
using NewsSitePro.Services;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.IO;

namespace NewsSite.Controllers
{
    [Route("api/[controller]")]
    public class NewsController : Controller
    {
        private readonly INewsApiService _newsApiService;
        private readonly DBservices _dbServices;
        private readonly ILogger<NewsController> _logger;

        public NewsController(INewsApiService newsApiService, DBservices dbServices, ILogger<NewsController> logger)
        {
            _newsApiService = newsApiService;
            _dbServices = dbServices;
            _logger = logger;
        }

        // GET: api/News/headlines
        [HttpGet("headlines")]
        public async Task<ActionResult<List<NewsApiArticle>>> GetTopHeadlines(
            [FromQuery] string category = "general", 
            [FromQuery] string country = "us", 
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var articles = await _newsApiService.FetchTopHeadlinesAsync(category, country, pageSize);
                return Ok(articles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching top headlines");
                return StatusCode(500, "Error fetching news");
            }
        }

        // GET: api/News/search
        [HttpGet("search")]
        public async Task<ActionResult<List<NewsApiArticle>>> SearchNews([FromQuery] string query, [FromQuery] int pageSize = 20)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return BadRequest("Query parameter is required");
                }

                var articles = await _newsApiService.FetchEverythingAsync(query, pageSize);
                return Ok(articles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching news for query: {query}");
                return StatusCode(500, "Error searching news");
            }
        }

        // POST: api/News/sync
        [HttpPost("sync")]
        public async Task<ActionResult<int>> SyncNewsToDatabase()
        {
            try
            {
                var articlesAdded = await _newsApiService.SyncNewsArticlesToDatabase();
                return Ok(new { ArticlesAdded = articlesAdded, Message = $"Successfully synced {articlesAdded} articles" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing news to database");
                return StatusCode(500, "Error syncing news");
            }
        }

        // GET: api/News/categories
        [HttpGet("categories")]
        public ActionResult<List<string>> GetAvailableCategories()
        {
            var categories = new List<string> 
            { 
                "general", "business", "entertainment", "health", 
                "science", "sports", "technology" 
            };
            return Ok(categories);
        }

        // GET: api/News/database
        [HttpGet("database")]
        public ActionResult<List<NewsArticle>> GetNewsFromDatabase(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? category = null,
            [FromQuery] int? currentUserId = null)
        {
            try
            {
                var articles = _dbServices.GetAllNewsArticles(pageNumber, pageSize, category, currentUserId);
                return Ok(articles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching news from database");
                return StatusCode(500, "Error fetching news from database");
            }
        }

        // GET: api/News/posts/rendered - Returns server-rendered HTML for ViewComponents
        // Enhanced to support context-driven rendering with user relationships and permissions
        [HttpGet("posts/rendered")]
        public IActionResult GetPostsRendered(
            [FromQuery] int page = 1, 
            [FromQuery] int limit = 10,
            [FromQuery] string? feed = null,
            [FromQuery] string? category = null)
        {
            try
            {
                // Get current user ID using the centralized method
                int? currentUserId = NewsSite.BL.User.GetCurrentUserId(Request, User);
                
                // Get current user object for context creation
                User? currentUser = null;
                if (currentUserId.HasValue)
                {
                    try
                    {
                        currentUser = _dbServices.GetUserById(currentUserId.Value);
                    }
                    catch
                    {
                        // Continue without user context if user retrieval fails
                    }
                }

                var articles = _dbServices.GetAllNewsArticles(page, limit, category, currentUserId);
                
                // Load follow status for all post authors if user is logged in
                Dictionary<int, bool> followStatusMap = new Dictionary<int, bool>();
                if (currentUserId.HasValue && articles.Any())
                {
                    var uniqueUserIds = articles.Select(p => p.UserID).Distinct().Where(uid => uid != currentUserId.Value);
                    foreach (var userId in uniqueUserIds)
                    {
                        try
                        {
                            var isFollowing = _dbServices.IsUserFollowing(currentUserId.Value, userId).Result;
                            ViewData["IsFollowing_" + userId] = isFollowing;
                            followStatusMap[userId] = isFollowing;
                        }
                        catch
                        {
                            // If follow status check fails, default to false
                            ViewData["IsFollowing_" + userId] = false;
                            followStatusMap[userId] = false;
                        }
                    }
                }
                
                // Create enhanced view model with context-aware rendering
                var viewModel = new PostsListViewModel
                {
                    Posts = articles,
                    CurrentUser = currentUser,
                    FeedType = feed ?? "all",
                    FollowStatusMap = followStatusMap
                };
                
                return View("_PostsList", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rendering posts");
                return StatusCode(500, "Error rendering posts");
            }
        }

        // GET: api/News/posts/enhanced - Returns server-rendered HTML with enhanced context
        // Uses real relationship data for more accurate context-aware rendering
        [HttpGet("posts/enhanced")]
        public IActionResult GetPostsEnhancedRendered(
            [FromQuery] int page = 1, 
            [FromQuery] int limit = 10,
            [FromQuery] string? feed = null,
            [FromQuery] string? category = null,
            [FromQuery] string? context = null)
        {
            try
            {
                // Get current user ID using the centralized method
                int? currentUserId = NewsSite.BL.User.GetCurrentUserId(Request, User);
                
                // Get current user object for context creation
                User? currentUser = null;
                if (currentUserId.HasValue)
                {
                    try
                    {
                        currentUser = _dbServices.GetUserById(currentUserId.Value);
                    }
                    catch
                    {
                        // Continue without user context if user retrieval fails
                    }
                }

                var articles = _dbServices.GetAllNewsArticles(page, limit, category, currentUserId);
                
                // Create enhanced view model with context-aware rendering
                var viewModel = new EnhancedPostsListViewModel
                {
                    Posts = articles,
                    CurrentUser = currentUser,
                    FeedType = feed ?? "all",
                    ContextType = context ?? "feed",
                    UseEnhancedContext = true
                };
                
                return View("_EnhancedPostsList", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rendering enhanced posts");
                return StatusCode(500, "Error rendering enhanced posts");
            }
        }

        // GET: api/News/posts/context/{contextType} - Returns posts for specific context
        [HttpGet("posts/context/{contextType}")]
        public IActionResult GetPostsByContext(
            string contextType,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10,
            [FromQuery] int? userId = null)
        {
            try
            {
                // Get current user ID using the centralized method
                int? currentUserId = NewsSite.BL.User.GetCurrentUserId(Request, User);
                
                // Get current user object for context creation
                User? currentUser = null;
                if (currentUserId.HasValue)
                {
                    try
                    {
                        currentUser = _dbServices.GetUserById(currentUserId.Value);
                    }
                    catch
                    {
                        // Continue without user context if user retrieval fails
                    }
                }

                List<NewsArticle> articles;
                
                // Get articles based on context type
                switch (contextType.ToLower())
                {
                    case "profile":
                        // Get posts by specific user
                        if (!userId.HasValue)
                            return BadRequest("userId is required for profile context");
                        // TODO: Implement GetUserPosts in DBservices
                        articles = _dbServices.GetAllNewsArticles(page, limit, null, currentUserId)
                            .Where(p => p.UserID == userId.Value).ToList();
                        break;
                    case "saved":
                        // Get saved posts for current user
                        if (!currentUserId.HasValue)
                            return Unauthorized("User must be logged in to view saved posts");
                        // TODO: Implement GetSavedPosts in DBservices
                        articles = _dbServices.GetAllNewsArticles(page, limit, null, currentUserId)
                            .Where(p => p.IsSaved).ToList();
                        break;
                    case "following":
                        // Get posts from followed users
                        if (!currentUserId.HasValue)
                            return Unauthorized("User must be logged in to view following feed");
                        // TODO: Implement following logic
                        articles = new List<NewsArticle>();
                        break;
                    default:
                        // Default to all posts
                        articles = _dbServices.GetAllNewsArticles(page, limit, null, currentUserId);
                        break;
                }
                
                // Create enhanced view model with context-aware rendering
                var viewModel = new EnhancedPostsListViewModel
                {
                    Posts = articles,
                    CurrentUser = currentUser,
                    FeedType = "all",
                    ContextType = contextType,
                    UseEnhancedContext = true,
                    ProfileUserId = userId
                };
                
                return View("_EnhancedPostsList", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error rendering posts for context: {contextType}");
                return StatusCode(500, $"Error rendering posts for context: {contextType}");
            }
        }

        // ADMIN-ONLY ENDPOINTS FOR NEWS MANAGEMENT
        // These endpoints are specifically designed for the AdminNews page

        /// <summary>
        /// Admin endpoint to fetch news from external APIs
        /// Used by AdminNews page to get fresh articles for review
        /// </summary>
        [HttpPost("fetch-external")]
        public async Task<ActionResult> FetchExternalNews([FromBody] FetchNewsRequest request)
        {
            try
            {
                _logger.LogInformation("FetchExternalNews called with request: {@Request}", request);
                
                // Verify admin permissions
                if (!IsCurrentUserAdmin())
                {
                    _logger.LogWarning("Non-admin user attempted to access fetch-external endpoint");
                    return Forbid("Admin access required");
                }

                _logger.LogInformation("Admin verification passed, fetching articles...");

                List<NewsApiArticle> articles = new List<NewsApiArticle>();

                try
                {
                    // Fetch articles based on request type
                    switch (request.Type.ToLower())
                    {
                        case "latest":
                        case "top-headlines":
                            _logger.LogInformation("Fetching top headlines for category: {Category}", request.Category);
                            articles = await _newsApiService.FetchTopHeadlinesAsync(
                                request.Category, request.Country, request.PageSize);
                            break;

                        case "breaking":
                            _logger.LogInformation("Fetching breaking news");
                            articles = await _newsApiService.FetchEverythingAsync(
                                "breaking news", request.PageSize);
                            break;

                        default:
                            _logger.LogInformation("Using default: top headlines for category: {Category}", request.Category);
                            articles = await _newsApiService.FetchTopHeadlinesAsync(
                                request.Category, request.Country, request.PageSize);
                            break;
                    }
                }
                catch (Exception apiEx)
                {
                    _logger.LogWarning(apiEx, "Failed to fetch from News API, returning mock data for testing");
                    
                    // Return mock data for testing when API is not configured
                    articles = CreateMockNewsArticles(request.Category);
                }

                _logger.LogInformation("Successfully fetched {Count} articles", articles.Count);

                return Ok(new { 
                    success = true, 
                    articles = articles, 
                    message = $"Successfully fetched {articles.Count} articles" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching external news for admin");
                return StatusCode(500, new { 
                    success = false, 
                    message = "Failed to fetch news: " + ex.Message,
                    details = ex.ToString()
                });
            }
        }

        /// <summary>
        /// Admin endpoint to publish approved articles to the main feed
        /// Converts external articles to internal NewsArticle format
        /// </summary>
        [HttpPost("publish-article")]
        public ActionResult PublishArticle([FromBody] PublishArticleRequest request)
        {
            try
            {
                // Verify admin permissions
                if (!IsCurrentUserAdmin())
                {
                    return Forbid("Admin access required");
                }

                // Create NewsArticle from the request
                var newsArticle = new NewsArticle
                {
                    Title = request.Title,
                    Content = request.Content,
                    ImageURL = request.ImageUrl,
                    SourceURL = request.SourceUrl,
                    SourceName = request.SourceName,
                    Category = request.Category,
                    PublishDate = DateTime.Parse(request.PublishDate),
                    UserID = (int)GetSystemUserId(), // Use system/admin user ID
                    LikesCount = 0,
                    ViewsCount = 0
                };

                // Save to database using existing method
                int articleId = _dbServices.CreateNewsArticle(newsArticle);

                if (articleId > 0)
                {
                    _logger.LogInformation($"Admin published article: {request.Title}");
                    return Ok(new { 
                        success = true, 
                        articleId = articleId,
                        message = "Article published successfully" 
                    });
                }
                else
                {
                    return Ok(new { 
                        success = false, 
                        message = "Failed to save article to database" 
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing article");
                return Ok(new { 
                    success = false, 
                    message = "Failed to publish article: " + ex.Message 
                });
            }
        }

        /// <summary>
        /// Get published articles for admin review
        /// Returns articles with admin-specific metadata
        /// </summary>
        [HttpGet("admin/published")]
        public ActionResult GetPublishedArticlesForAdmin([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                // Verify admin permissions
                if (!IsCurrentUserAdmin())
                {
                    return Forbid("Admin access required");
                }

                // Get published articles with pagination
                var articles = _dbServices.GetAllNewsArticles(page, pageSize);
                
                // Add admin-specific metadata
                var adminArticles = articles.Select(article => new
                {
                    article.ArticleID,
                    article.Title,
                    article.Content,
                    article.ImageURL,
                    article.SourceURL,
                    article.SourceName,
                    article.Category,
                    article.PublishDate,
                    article.LikesCount,
                    article.ViewsCount,
                    Status = "published",
                    PublishedBy = "Admin" // Could be enhanced to track specific admin
                }).ToList();

                return Ok(new { 
                    success = true, 
                    articles = adminArticles,
                    totalCount = adminArticles.Count,
                    page = page,
                    pageSize = pageSize
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting published articles for admin");
                return StatusCode(500, new { 
                    success = false, 
                    message = "Failed to retrieve articles" 
                });
            }
        }

        // HELPER METHODS FOR ADMIN FUNCTIONALITY

        /// <summary>
        /// Check if current user has admin privileges
        /// Uses same logic as other admin controllers for consistency
        /// </summary>
        private bool IsCurrentUserAdmin()
        {
            try
            {
                // Check cookies for JWT token first (same as Admin page)
                var jwtCookie = Request.Cookies["jwtToken"];
                if (!string.IsNullOrEmpty(jwtCookie))
                {
                    var user = new User().ExtractUserFromJWT(jwtCookie);
                    return user.IsAdmin;
                }

                // Fallback to JWT claims
                var adminClaim = User.FindFirst("isAdmin");
                if (adminClaim != null && bool.TryParse(adminClaim.Value, out bool isAdmin))
                {
                    return isAdmin;
                }

                // Check Authorization header for JWT token
                var authHeader = Request.Headers["Authorization"].FirstOrDefault();
                if (authHeader != null && authHeader.StartsWith("Bearer "))
                {
                    var token = authHeader.Substring("Bearer ".Length).Trim();
                    var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                    
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

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking admin status");
                return false;
            }
        }

        /// <summary>
        /// Get system user ID for publishing admin-sourced articles
        /// Could be enhanced to use actual admin user ID
        /// </summary>
        private int? GetSystemUserId()
        {
            return BL.User.GetCurrentUserId(Request, User);
            // For now, return a default system user ID
            // This could be enhanced to:
            // 1. Use the actual admin user's ID
            // 2. Create a dedicated "System" or "NewsBot" user
            // 3. Get from configuration
            return 1; // Assuming user ID 1 exists as admin/system user
        }

        /// <summary>
        /// Create mock news articles for testing when API is not available
        /// </summary>
        private List<NewsApiArticle> CreateMockNewsArticles(string category)
        {
            return new List<NewsApiArticle>
            {
                new NewsApiArticle
                {
                    Title = $"Sample {category} News Article 1",
                    Description = "This is a sample news article for testing the admin news management system.",
                    Content = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.",
                    Url = "https://example.com/article1",
                    UrlToImage = "https://via.placeholder.com/400x200",
                    PublishedAt = DateTime.Now.AddHours(-2),
                    Source = new NewsApiSource { Name = "Test News" }
                },
                new NewsApiArticle
                {
                    Title = $"Sample {category} News Article 2",
                    Description = "Another sample news article for testing purposes.",
                    Content = "Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.",
                    Url = "https://example.com/article2",
                    UrlToImage = "https://via.placeholder.com/400x200",
                    PublishedAt = DateTime.Now.AddHours(-4),
                    Source = new NewsApiSource { Name = "Demo Source" }
                },
                new NewsApiArticle
                {
                    Title = $"Sample {category} News Article 3",
                    Description = "A third sample article to demonstrate the admin interface.",
                    Content = "Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur.",
                    Url = "https://example.com/article3",
                    UrlToImage = "https://via.placeholder.com/400x200",
                    PublishedAt = DateTime.Now.AddHours(-6),
                    Source = new NewsApiSource { Name = "Sample News" }
                }
            };
        }
    }

    // REQUEST/RESPONSE MODELS FOR ADMIN ENDPOINTS

    /// <summary>
    /// Request model for fetching external news
    /// Used by AdminNews page to specify what type of news to fetch
    /// </summary>
    public class FetchNewsRequest
    {
        public string Type { get; set; } = "latest"; // latest, top-headlines, breaking
        public string Category { get; set; } = "general";
        public string Country { get; set; } = "us";
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// Request model for publishing articles
    /// Contains all necessary data to create a NewsArticle
    /// </summary>
    public class PublishArticleRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? SourceUrl { get; set; }
        public string? SourceName { get; set; }
        public string Category { get; set; } = "general";
        public string PublishDate { get; set; } = DateTime.Now.ToString();
    }
}
