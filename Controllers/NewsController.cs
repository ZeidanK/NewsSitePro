using Microsoft.AspNetCore.Mvc;
using NewsSite.BL;
using NewsSite.Services;

namespace NewsSite.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsController : ControllerBase
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
    }
}
