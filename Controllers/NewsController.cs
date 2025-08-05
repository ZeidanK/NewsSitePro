/**
 * NewsController.cs
 * Purpose: Handles news article operations, API integrations, and news-related functionality
 * Responsibilities: News article CRUD, external API integration, news categorization, article management
 * Architecture: Uses NewsService and UserService from BL layer for business logic and data operations
 */

using Microsoft.AspNetCore.Mvc;
using NewsSite.BL;
using NewsSite.BL.Services;
using NewsSite.Services;
using NewsSitePro.Models;
using NewsSitePro.Services;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using System.IO;

namespace NewsSite.Controllers
{
    [Route("api/[controller]")]
    public class NewsController : Controller
    {
        private readonly INewsApiService _newsApiService;
        private readonly INewsService _newsService;
        private readonly IUserService _userService;
        private readonly ILogger<NewsController> _logger;
        private readonly SystemSettingsOptions _systemSettings;

        public NewsController(
            INewsApiService newsApiService, 
            INewsService newsService, 
            IUserService userService, 
            ILogger<NewsController> logger,
            IOptions<SystemSettingsOptions> systemSettings)
        {
            _newsApiService = newsApiService;
            _newsService = newsService;
            _userService = userService;
            _logger = logger;
            _systemSettings = systemSettings.Value;
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
        public async Task<ActionResult<List<NewsArticle>>> GetNewsFromDatabase(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? category = null,
            [FromQuery] int? currentUserId = null)
        {
            try
            {
                var articles = await _newsService.GetAllNewsArticlesAsync(pageNumber, pageSize, category, currentUserId);
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
        /// <summary>
        /// Renders posts as HTML using ViewComponents with enhanced context awareness
        /// Routes to appropriate feed based on feed parameter
        /// Includes user relationship data (follow status) for personalized rendering
        /// </summary>
        [HttpGet("posts/rendered")]
        public async Task<IActionResult> GetPostsRendered(
            [FromQuery] int page = 1, 
            [FromQuery] int limit = 10,
            [FromQuery] string? feed = null,
            [FromQuery] string? category = null)
        {
            try
            {
                // Route to specific feed endpoints based on feed parameter
                switch (feed?.ToLower())
                {
                    case "following":
                        return await GetFollowingPostsRendered(page, limit, category);
                    
                    case "saved":
                        return await GetSavedPostsRendered(page, limit, category, null);
                    
                    case "trending":
                        return await GetTrendingPostsRendered(page, limit, category);
                    
                    case "all":
                    case null:
                    default:
                        // Continue with default all posts logic
                        break;
                }

                // Default: Get all posts
                // Get current user ID using the centralized method
                int? currentUserId = NewsSite.BL.User.GetCurrentUserId(Request, User);
                
                // Get current user object for context creation
                User? currentUser = null;
                if (currentUserId.HasValue && currentUserId.Value > 0)
                {
                    currentUser = await GetCurrentUserAsync(currentUserId.Value);
                }

                var articles = await _newsService.GetAllNewsArticlesAsync(page, limit, category, currentUserId);
                
                // Load follow status for all post authors if user is logged in
                Dictionary<int, bool> followStatusMap = new Dictionary<int, bool>();
                if (currentUserId.HasValue)
                {
                    followStatusMap = await LoadFollowStatusMapAsync(currentUserId.Value, articles);
                }
                
                // Create enhanced view model with context-aware rendering
                var viewModel = CreatePostsListViewModel(articles, currentUser, feed ?? "all", followStatusMap);
                
                return View("_PostsList", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rendering posts");
                return StatusCode(500, "Error rendering posts");
            }
        }

        // GET: api/News/posts/saved - Returns server-rendered HTML for saved articles using ViewComponents
        /// <summary>
        /// Renders saved articles as HTML using ViewComponents with enhanced context awareness
        /// Only shows articles that the current user has saved
        /// </summary>
        [HttpGet("posts/saved")]
        public async Task<IActionResult> GetSavedPostsRendered(
            [FromQuery] int page = 1, 
            [FromQuery] int limit = 10,
            [FromQuery] string? category = null,
            [FromQuery] string? search = null)
        {
            try
            {
                // Get current user ID using the centralized method
                int? currentUserId = NewsSite.BL.User.GetCurrentUserId(Request, User);
                
                if (!currentUserId.HasValue)
                {
                    return Unauthorized("User must be logged in to view saved articles");
                }
                
                // Get current user object for context creation
                User? currentUser = await GetCurrentUserAsync(currentUserId.Value);

                // Use the existing GetSavedArticlesByUserAsync method instead
                var savedArticles = await _newsService.GetSavedArticlesByUserAsync(currentUserId.Value, page, limit);
                
                // Apply filters manually if needed
                if (!string.IsNullOrEmpty(category) && category != "all")
                {
                    savedArticles = savedArticles.Where(a => 
                        string.Equals(a.Category, category, StringComparison.OrdinalIgnoreCase)
                    ).ToList();
                }
                
                if (!string.IsNullOrEmpty(search))
                {
                    savedArticles = savedArticles.Where(a => 
                        (a.Title?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (a.Content?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
                    ).ToList();
                }
                
                // Load follow status for all post authors
                var followStatusMap = await LoadFollowStatusMapAsync(currentUserId.Value, savedArticles);
                
                // Create enhanced view model with context-aware rendering
                var viewModel = CreatePostsListViewModel(savedArticles, currentUser, "saved", followStatusMap);
                
                return View("_PostsList", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rendering saved posts: {Error}", ex.Message);
                return StatusCode(500, "Error rendering saved posts");
            }
        }

        // GET: api/News/posts/following - Returns server-rendered HTML for posts from followed users
        /// <summary>
        /// Renders posts from followed users as HTML using ViewComponents
        /// Only shows articles from users that the current user follows
        /// </summary>
        [HttpGet("posts/following")]
        public async Task<IActionResult> GetFollowingPostsRendered(
            [FromQuery] int page = 1, 
            [FromQuery] int limit = 10,
            [FromQuery] string? category = null)
        {
            try
            {
                // Get current user ID using the centralized method
                int? currentUserId = NewsSite.BL.User.GetCurrentUserId(Request, User);
                
                if (!currentUserId.HasValue)
                {
                    return Unauthorized("User must be logged in to view following feed");
                }
                
                // Get current user object for context creation
                User? currentUser = await GetCurrentUserAsync(currentUserId.Value);

                // Get posts from followed users using the Business Logic layer
                var followingPosts = await _newsService.GetFollowingPostsAsync(currentUserId.Value, page, limit, category);
                
                // All these users are followed by definition, so set all follow status to true
                var followStatusMap = CreateFollowStatusMapForFollowing(followingPosts);
                
                // Create enhanced view model with context-aware rendering
                var viewModel = CreatePostsListViewModel(followingPosts, currentUser, "following", followStatusMap);
                
                return View("_PostsList", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rendering following posts");
                return StatusCode(500, "Error rendering following posts");
            }
        }

        // GET: api/News/posts/trending - Returns server-rendered HTML for trending posts
        /// <summary>
        /// Renders trending posts as HTML using ViewComponents
        /// Shows articles with high engagement (likes, views, comments)
        /// </summary>
        [HttpGet("posts/trending")]
        public async Task<IActionResult> GetTrendingPostsRendered(
            [FromQuery] int page = 1, 
            [FromQuery] int limit = 10,
            [FromQuery] string? category = null)
        {
            try
            {
                // Get current user ID using the centralized method
                int? currentUserId = NewsSite.BL.User.GetCurrentUserId(Request, User);
                
                // Get current user object for context creation
                User? currentUser = await GetCurrentUserAsync(currentUserId ?? 0);

                // Get trending posts using the Business Logic layer
                int articlesToFetch = page * limit; // Fetch enough for pagination
                var trendingPosts = await _newsService.GetTrendingArticlesAsync(articlesToFetch, category, currentUserId);
                
                // Apply pagination (since we might get more than needed)
                var paginatedPosts = trendingPosts
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToList();
                
                // Load follow status for all post authors if user is logged in
                Dictionary<int, bool> followStatusMap = new Dictionary<int, bool>();
                if (currentUserId.HasValue)
                {
                    followStatusMap = await LoadFollowStatusMapAsync(currentUserId.Value, paginatedPosts);
                }
                
                // Create enhanced view model with context-aware rendering
                var viewModel = CreatePostsListViewModel(paginatedPosts, currentUser, "trending", followStatusMap);
                
                return View("_PostsList", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rendering trending posts");
                return StatusCode(500, "Error rendering trending posts");
            }
        }

        // REUSABLE HELPER METHODS

        /// <summary>
        /// Get current user object with error handling
        /// </summary>
        private async Task<User?> GetCurrentUserAsync(int userId)
        {
            try
            {
                return await _userService.GetUserByIdAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get current user {UserId}", userId);
                return null;
            }
        }

        /// <summary>
        /// Load follow status map for all unique user IDs in articles
        /// </summary>
        private async Task<Dictionary<int, bool>> LoadFollowStatusMapAsync(int currentUserId, List<NewsArticle> articles)
        {
            var followStatusMap = new Dictionary<int, bool>();
            
            if (!articles.Any()) return followStatusMap;

            var uniqueUserIds = articles.Select(p => p.UserID).Distinct().Where(uid => uid != currentUserId);
            
            foreach (var userId in uniqueUserIds)
            {
                try
                {
                    var isFollowing = await _userService.IsUserFollowingAsync(currentUserId, userId);
                    ViewData["IsFollowing_" + userId] = isFollowing;
                    followStatusMap[userId] = isFollowing;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to get follow status for user {UserId}", userId);
                    ViewData["IsFollowing_" + userId] = false;
                    followStatusMap[userId] = false;
                }
            }
            
            return followStatusMap;
        }

        /// <summary>
        /// Create follow status map for following feed (all users are followed)
        /// </summary>
        private Dictionary<int, bool> CreateFollowStatusMapForFollowing(List<NewsArticle> articles)
        {
            var followStatusMap = new Dictionary<int, bool>();
            var uniqueUserIds = articles.Select(p => p.UserID).Distinct();
            
            foreach (var userId in uniqueUserIds)
            {
                ViewData["IsFollowing_" + userId] = true;
                followStatusMap[userId] = true;
            }
            
            return followStatusMap;
        }

        /// <summary>
        /// Create PostsListViewModel with consistent structure
        /// </summary>
        private PostsListViewModel CreatePostsListViewModel(List<NewsArticle> articles, User? currentUser, string feedType, Dictionary<int, bool> followStatusMap)
        {
            return new PostsListViewModel
            {
                Posts = articles,
                CurrentUser = currentUser,
                FeedType = feedType,
                FollowStatusMap = followStatusMap
            };
        }

        // GET: api/News/posts/enhanced - Returns server-rendered HTML with enhanced context
        // Uses real relationship data for more accurate context-aware rendering
        [HttpGet("posts/enhanced")]
        public async Task<IActionResult> GetPostsEnhancedRendered(
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
                        currentUser = await _userService.GetUserByIdAsync(currentUserId.Value);
                    }
                    catch
                    {
                        // Continue without user context if user retrieval fails
                    }
                }

                var articles = await _newsService.GetAllNewsArticlesAsync(page, limit, category, currentUserId);
                
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
        public async Task<IActionResult> GetPostsByContext(
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
                        currentUser = await _userService.GetUserByIdAsync(currentUserId.Value);
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
                        var allArticlesForProfile = await _newsService.GetAllNewsArticlesAsync(page, limit, null, currentUserId);
                        articles = allArticlesForProfile.Where(p => p.UserID == userId.Value).ToList();
                        break;
                    case "saved":
                        // Get saved posts for current user
                        if (!currentUserId.HasValue)
                            return Unauthorized("User must be logged in to view saved posts");
                        // TODO: Implement GetSavedPosts in DBservices
                        var allArticlesForSaved = await _newsService.GetAllNewsArticlesAsync(page, limit, null, currentUserId);
                        articles = allArticlesForSaved.Where(p => p.IsSaved).ToList();
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
                        articles = await _newsService.GetAllNewsArticlesAsync(page, limit, null, currentUserId);
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
        public async Task<IActionResult> PublishArticle([FromBody] PublishArticleRequest request)
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
                    UserID = GetSystemUserId(), // Use system/admin user ID
                    LikesCount = 0,
                    ViewsCount = 0
                };

                // Save to database using existing method
                int articleId = await _newsService.CreateNewsArticleAsync(newsArticle);

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
        /// Admin endpoint to publish multiple articles at once (bulk publish)
        /// </summary>
        [HttpPost("publish-articles")]
        public async Task<IActionResult> PublishArticles([FromBody] PublishArticlesRequest request)
        {
            try
            {
                // Verify admin permissions
                if (!IsCurrentUserAdmin())
                {
                    return Forbid("Admin access required");
                }

                if (request.Articles == null || !request.Articles.Any())
                {
                    return BadRequest(new { success = false, message = "No articles provided" });
                }

                int publishedCount = 0;
                int systemUserId = GetSystemUserId();
                var errors = new List<string>();

                foreach (var article in request.Articles)
                {
                    try
                    {
                        var newsArticle = new NewsArticle
                        {
                            Title = article.Title,
                            Content = article.Content,
                            ImageURL = article.ImageUrl,
                            SourceURL = article.SourceUrl,
                            SourceName = article.SourceName,
                            Category = article.Category,
                            PublishDate = DateTime.Now,
                            UserID = systemUserId,
                            LikesCount = 0,
                            ViewsCount = 0
                        };

                        int articleId = await _newsService.CreateNewsArticleAsync(newsArticle);
                        if (articleId > 0)
                        {
                            publishedCount++;
                        }
                        else
                        {
                            errors.Add($"Failed to save article: {article.Title}");
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Error publishing '{article.Title}': {ex.Message}");
                        _logger.LogError(ex, "Error publishing individual article: {Title}", article.Title);
                    }
                }

                _logger.LogInformation($"Bulk publish completed: {publishedCount}/{request.Articles.Count()} articles published");

                return Ok(new { 
                    success = true, 
                    published = publishedCount,
                    total = request.Articles.Count(),
                    errors = errors,
                    message = $"Successfully published {publishedCount} out of {request.Articles.Count()} articles"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk publish operation");
                return StatusCode(500, new { 
                    success = false, 
                    message = "An error occurred during bulk publishing: " + ex.Message 
                });
            }
        }

        /// <summary>
        /// Public endpoint to publish articles - requires user authentication but not admin
        /// </summary>
        [HttpPost("publish")]
        public async Task<IActionResult> PublishArticleForUser([FromBody] PublishArticlesRequest request)
        {
            try
            {
                // Check if user is logged in
                var currentUserId = NewsSite.BL.User.GetCurrentUserId(Request, User);
                if (!currentUserId.HasValue || currentUserId.Value <= 0)
                {
                    return Unauthorized(new { 
                        success = false, 
                        message = "You must be logged in to publish articles",
                        requiresLogin = true 
                    });
                }

                if (request.Articles == null || !request.Articles.Any())
                {
                    return BadRequest(new { success = false, message = "No articles provided" });
                }

                int publishedCount = 0;
                var errors = new List<string>();

                foreach (var article in request.Articles)
                {
                    try
                    {
                        var newsArticle = new NewsArticle
                        {
                            Title = article.Title,
                            Content = article.Content,
                            ImageURL = article.ImageUrl,
                            SourceURL = article.SourceUrl,
                            SourceName = article.SourceName,
                            Category = article.Category,
                            PublishDate = DateTime.Now,
                            UserID = currentUserId.Value, // Use the logged-in user's ID
                            LikesCount = 0,
                            ViewsCount = 0
                        };

                        int articleId = await _newsService.CreateNewsArticleAsync(newsArticle);
                        if (articleId > 0)
                        {
                            publishedCount++;
                        }
                        else
                        {
                            errors.Add($"Failed to save article: {article.Title}");
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Error publishing '{article.Title}': {ex.Message}");
                        _logger.LogError(ex, "Error publishing individual article: {Title}", article.Title);
                    }
                }

                _logger.LogInformation($"User publish completed: {publishedCount}/{request.Articles.Count()} articles published by user {currentUserId}");

                return Ok(new { 
                    success = true, 
                    published = publishedCount,
                    total = request.Articles.Count(),
                    errors = errors,
                    message = $"Successfully published {publishedCount} out of {request.Articles.Count()} articles"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in user publish operation");
                return StatusCode(500, new { 
                    success = false, 
                    message = "An error occurred during publishing: " + ex.Message 
                });
            }
        }

        /// <summary>
        /// Get published articles for admin review
        /// Returns articles with admin-specific metadata
        /// </summary>
        [HttpGet("admin/published")]
        public async Task<IActionResult> GetPublishedArticlesForAdmin([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                // Verify admin permissions
                if (!IsCurrentUserAdmin())
                {
                    return Forbid("Admin access required");
                }

                // Get published articles with pagination
                var articles = await _newsService.GetAllNewsArticlesAsync(page, pageSize);
                
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
        /// Uses configuration-based system user ID instead of hardcoded values
        /// Falls back to configured admin user ID if current user is not available
        /// </summary>
        private int GetSystemUserId()
        {
            try
            {
                // Validate configuration first
                if (!_systemSettings.IsValid())
                {
                    _logger.LogWarning("Invalid system settings configuration, using fallback values");
                    return 1; // Ultimate fallback
                }

                // Try to get current logged-in user ID
                var currentUserId = BL.User.GetCurrentUserId(Request, User);
                
                if (currentUserId.HasValue && currentUserId.Value > 0)
                {
                    // If admin user is logged in, use their ID for audit trail
                    _logger.LogDebug("Using current admin user ID {UserId} for system operation", currentUserId.Value);
                    return currentUserId.Value;
                }
                
                // If no admin is logged in, use configured system user ID
                _logger.LogDebug("Using configured system user ID {UserId} for system operation", _systemSettings.DefaultSystemUserId);
                return _systemSettings.DefaultSystemUserId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error determining system user ID, falling back to admin fallback user");
                return _systemSettings?.AdminFallbackUserId ?? 1;
            }
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

        //==========================================================================================
        // SIMPLE API ENDPOINTS FOR FRONTEND COMPATIBILITY
        //==========================================================================================

        /// <summary>
        /// GET: /api/saved - Simple endpoint for saved articles (frontend compatibility)
        /// Returns JSON array of saved articles for the authenticated user
        /// </summary>
        [HttpGet("/api/saved")]
        public async Task<ActionResult<List<NewsArticle>>> GetSavedArticlesApi()
        {
            try
            {
                var userId = NewsSite.BL.User.GetCurrentUserId(Request, User);
                if (userId == null)
                {
                    return Unauthorized("User must be logged in to view saved articles");
                }

                var savedArticles = await _newsService.GetSavedArticlesByUserAsync(userId.Value, 1, 20);
                return Ok(savedArticles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching saved articles API");
                return StatusCode(500, "Error fetching saved articles");
            }
        }

        /// <summary>
        /// GET: /api/shared - Simple endpoint for posts from followed users (frontend compatibility)
        /// Returns JSON array of posts from users the current user follows
        /// </summary>
        [HttpGet("/api/shared")]
        public async Task<ActionResult<List<NewsArticle>>> GetSharedArticlesApi()
        {
            try
            {
                var userId = NewsSite.BL.User.GetCurrentUserId(Request, User);
                if (userId == null)
                {
                    return Unauthorized("User must be logged in to view following feed");
                }

                var followingPosts = await _newsService.GetFollowingPostsAsync(userId.Value, 1, 20);
                return Ok(followingPosts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching following posts API");
                return StatusCode(500, "Error fetching following posts");
            }
        }

        /// <summary>
        /// GET: /api/posts - Simple endpoint for all posts (frontend compatibility)
        /// Returns JSON array of all posts with proper user context
        /// </summary>
        [HttpGet("/api/posts")]
        public async Task<ActionResult<List<NewsArticle>>> GetAllPostsApi([FromQuery] int page = 1, [FromQuery] int limit = 20)
        {
            try
            {
                var userId = NewsSite.BL.User.GetCurrentUserId(Request, User);
                var articles = await _newsService.GetAllNewsArticlesAsync(page, limit, null, userId);
                return Ok(articles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all posts API");
                return StatusCode(500, "Error fetching posts");
            }
        }

        /// <summary>
        /// GET: /api/tags - Simple endpoint for available categories/tags (frontend compatibility)
        /// Returns JSON array of available news categories
        /// </summary>
        [HttpGet("/api/tags")]
        public ActionResult<List<string>> GetTagsApi()
        {
            var categories = new List<string> 
            { 
                "general", "business", "entertainment", "health", 
                "science", "sports", "technology" 
            };
            return Ok(categories);
        }

        /// <summary>
        /// Public endpoint to browse news from external APIs
        /// Used by public News page for users to browse articles
        /// </summary>
        [HttpPost("browse-external")]
        public async Task<ActionResult> BrowseExternalNews([FromBody] PublicFetchNewsRequest request)
        {
            try
            {
                _logger.LogInformation("BrowseExternalNews called with request: {@Request}", request);

                List<NewsApiArticle> articles = new List<NewsApiArticle>();

                try
                {
                    // Fetch articles based on request type and filters
                    switch (request.Type.ToLower())
                    {
                        case "top-headlines":
                            _logger.LogInformation("Fetching top headlines for category: {Category}, country: {Country}", 
                                request.Category, request.Country);
                            
                            if (!string.IsNullOrEmpty(request.Sources))
                            {
                                // For now, use everything endpoint with source in query
                                // You can enhance this later with a dedicated sources method
                                string sourceQuery = $"source:{request.Sources.Split(',')[0]}";
                                articles = await _newsApiService.FetchEverythingAsync(sourceQuery, request.PageSize);
                            }
                            else
                            {
                                articles = await _newsApiService.FetchTopHeadlinesAsync(
                                    request.Category ?? "general", request.Country ?? "us", request.PageSize);
                            }
                            break;

                        case "everything":
                            _logger.LogInformation("Fetching everything for category: {Category}", request.Category);
                            string query = !string.IsNullOrEmpty(request.Category) && request.Category != "general" 
                                ? request.Category 
                                : "news";
                            articles = await _newsApiService.FetchEverythingAsync(query, request.PageSize);
                            break;

                        case "breaking":
                            _logger.LogInformation("Fetching breaking news");
                            articles = await _newsApiService.FetchEverythingAsync("breaking news", request.PageSize);
                            break;

                        default:
                            _logger.LogInformation("Using default: top headlines");
                            articles = await _newsApiService.FetchTopHeadlinesAsync(
                                request.Category ?? "general", request.Country ?? "us", request.PageSize);
                            break;
                    }
                }
                catch (Exception apiEx)
                {
                    _logger.LogWarning(apiEx, "Failed to fetch from News API, returning mock data for testing");
                    
                    // Return mock data for testing when API is not configured
                    articles = CreateMockNewsArticles(request.Category ?? "general");
                }

                _logger.LogInformation("Successfully fetched {Count} articles for public browsing", articles.Count);

                return Ok(new { 
                    success = true, 
                    articles = articles, 
                    message = $"Successfully found {articles.Count} articles" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error browsing external news");
                return StatusCode(500, new { 
                    success = false, 
                    message = "Failed to fetch news: " + ex.Message
                });
            }
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
    /// Request model for public news browsing
    /// Used by public News page with additional source filtering
    /// </summary>
    public class PublicFetchNewsRequest
    {
        public string Type { get; set; } = "top-headlines"; // top-headlines, everything, breaking
        public string? Category { get; set; } = "general";
        public string? Country { get; set; } = "us";
        public string? Sources { get; set; } = null; // e.g., "bbc-news,cnn,reuters"
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

    /// <summary>
    /// Request model for publishing multiple articles at once
    /// </summary>
    public class PublishArticlesRequest
    {
        public IEnumerable<PublishArticleRequest> Articles { get; set; } = new List<PublishArticleRequest>();
    }
}
