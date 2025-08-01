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
        private readonly IUserRelationshipService _relationshipService;

        public NewsController(INewsApiService newsApiService, DBservices dbServices, ILogger<NewsController> logger, IUserRelationshipService relationshipService)
        {
            _newsApiService = newsApiService;
            _dbServices = dbServices;
            _logger = logger;
            _relationshipService = relationshipService;
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
                
                // Create enhanced view model with context-aware rendering
                var viewModel = new PostsListViewModel
                {
                    Posts = articles,
                    CurrentUser = currentUser,
                    FeedType = feed ?? "all"
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
    }
}
