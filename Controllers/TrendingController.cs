using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NewsSite.BL;

namespace NewsSite.Controllers
{
    /// <summary>
    /// Controller responsible for trending topics functionality
    /// Handles basic trending topics operations with engagement-based calculations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TrendingController : ControllerBase
    {
        private readonly DBservices _dbServices;

        public TrendingController(DBservices dbServices)
        {
            _dbServices = dbServices;
        }

        /// <summary>
        /// Get current trending topics
        /// </summary>
        /// <param name="count">Number of trending topics to return (default: 10)</param>
        /// <param name="category">Optional category filter</param>
        /// <returns>List of trending topics</returns>
        [HttpGet]
        public async Task<ActionResult<TrendingTopicsResponse>> GetTrendingTopics(
            [FromQuery] int count = 10, 
            [FromQuery] string? category = null)
        {
            try
            {
                var topics = await _dbServices.GetTrendingTopicsAsync(count, category);
                
                var response = new TrendingTopicsResponse
                {
                    Topics = topics,
                    TotalCount = topics.Count,
                    Category = category,
                    MinScore = 0.0,
                    TimeWindowHours = 24,
                    GeneratedAt = DateTime.UtcNow
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to get trending topics", details = ex.Message });
            }
        }

        /// <summary>
        /// Get trending topics grouped by categories
        /// </summary>
        /// <param name="topicsPerCategory">Number of topics per category (default: 3)</param>
        /// <returns>List of trending topics grouped by category</returns>
        [HttpGet("by-category")]
        public async Task<ActionResult<List<TrendingTopic>>> GetTrendingTopicsByCategory(
            [FromQuery] int topicsPerCategory = 3)
        {
            try
            {
                var topics = await _dbServices.GetTrendingTopicsByCategoryAsync(topicsPerCategory);
                return Ok(topics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to get trending topics by category", details = ex.Message });
            }
        }

        /// <summary>
        /// Get articles related to a specific trending topic
        /// </summary>
        /// <param name="topic">The trending topic</param>
        /// <param name="category">Category of the topic</param>
        /// <param name="count">Number of articles to return (default: 10)</param>
        /// <returns>List of related articles</returns>
        [HttpGet("related-articles")]
        [Authorize]
        public async Task<ActionResult<List<NewsArticle>>> GetRelatedArticles(
            [FromQuery] string topic,
            [FromQuery] string category,
            [FromQuery] int count = 10)
        {
            try
            {
                if (string.IsNullOrEmpty(topic) || string.IsNullOrEmpty(category))
                {
                    return BadRequest(new { error = "Topic and category are required" });
                }

                // Get current user ID from claims
                var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "UserID");
                int? currentUserId = userIdClaim != null ? int.Parse(userIdClaim.Value) : null;

                var articles = await _dbServices.GetTrendingTopicRelatedArticlesAsync(topic, category, count, currentUserId);
                return Ok(articles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to get related articles", details = ex.Message });
            }
        }

        /// <summary>
        /// Manually refresh trending topics calculation (Admin only)
        /// </summary>
        /// <param name="timeWindowHours">Time window in hours for calculation (default: 24)</param>
        /// <param name="maxTopics">Maximum number of topics to calculate (default: 20)</param>
        /// <returns>Success status and message</returns>
        [HttpPost("refresh")]
        [Authorize]
        public async Task<ActionResult> RefreshTrendingTopics(
            [FromQuery] int timeWindowHours = 24,
            [FromQuery] int maxTopics = 20)
        {
            try
            {
                // Check if user is admin (you may need to adjust this based on your auth system)
                var isAdminClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "IsAdmin");
                if (isAdminClaim == null || !bool.Parse(isAdminClaim.Value))
                {
                    return Forbid("Admin access required");
                }

                var (success, message) = await _dbServices.CalculateTrendingTopicsAsync(timeWindowHours, maxTopics);
                
                if (success)
                {
                    // Also cleanup old topics
                    await _dbServices.CleanupOldTrendingTopicsAsync(48);
                    return Ok(new { success = true, message });
                }
                else
                {
                    return StatusCode(500, new { success = false, message });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to refresh trending topics", details = ex.Message });
            }
        }

        /// <summary>
        /// Get trending topics for the right sidebar (simplified version)
        /// </summary>
        /// <returns>Top 5 trending topics for sidebar display</returns>
        [HttpGet("sidebar")]
        public async Task<ActionResult<List<object>>> GetSidebarTrendingTopics()
        {
            try
            {
                var topics = await _dbServices.GetTrendingTopicsAsync(5);
                
                // Format for sidebar display
                var sidebarTopics = topics.Select(t => new
                {
                    topic = t.Topic,
                    category = t.Category,
                    count = FormatCount(t.TotalInteractions),
                    score = Math.Round(t.TrendScore, 1)
                }).ToList();

                return Ok(sidebarTopics);
            }
            catch (Exception ex)
            {
                // Return fallback data for sidebar
                var fallbackTopics = new[]
                {
                    new { topic = "AI Technology", category = "Technology", count = "25.4K posts", score = 85.5 },
                    new { topic = "World Cup 2025", category = "Sports", count = "18.2K posts", score = 78.2 },
                    new { topic = "Election Updates", category = "Politics", count = "32.1K posts", score = 72.8 },
                    new { topic = "Climate Action", category = "Environment", count = "12.8K posts", score = 65.3 },
                    new { topic = "Health Trends", category = "Health", count = "9.5K posts", score = 58.7 }
                };

                return Ok(fallbackTopics);
            }
        }

        /// <summary>
        /// Clean up old trending topics (Admin only)
        /// </summary>
        /// <param name="maxAgeHours">Maximum age in hours (default: 24)</param>
        /// <returns>Cleanup results</returns>
        [HttpDelete("cleanup")]
        [Authorize]
        public async Task<ActionResult> CleanupOldTopics([FromQuery] int maxAgeHours = 24)
        {
            try
            {
                // Check if user is admin
                var isAdminClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "IsAdmin");
                if (isAdminClaim == null || !bool.Parse(isAdminClaim.Value))
                {
                    return Forbid("Admin access required");
                }

                var (deletedCount, message) = await _dbServices.CleanupOldTrendingTopicsAsync(maxAgeHours);
                return Ok(new { deletedCount, message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to cleanup old topics", details = ex.Message });
            }
        }

        /// <summary>
        /// Helper method to format interaction counts for display
        /// </summary>
        /// <param name="count">Raw interaction count</param>
        /// <returns>Formatted count string</returns>
        private static string FormatCount(int count)
        {
            if (count >= 1000000)
                return $"{count / 1000000.0:F1}M posts";
            else if (count >= 1000)
                return $"{count / 1000.0:F1}K posts";
            else
                return $"{count} posts";
        }
    }
}
