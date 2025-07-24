using Microsoft.AspNetCore.Mvc;
using NewsSite.BL;

namespace NewsSite.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ViewController : ControllerBase
    {
        private readonly DBservices _dbService;

        public ViewController()
        {
            _dbService = new DBservices();
        }

        // GET api/View/User/{userId}
        [HttpGet("User/{userId}")]
        public IActionResult GetUserProfile(int userId)
        {
            try
            {
                // Get user basic info
                var user = _dbService.GetUser(null, userId, null);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                // Get user statistics
                var userStats = _dbService.GetUserStats(userId);

                // Get user's recent posts
                var recentPosts = _dbService.GetArticlesByUser(userId, 1, 10);

                // Combine into UserProfile object
                var userProfile = new UserProfile
                {
                    UserID = user.Id,
                    Username = user.Name,
                    Email = user.Email,
                    Bio = user.Bio ?? "",
                    JoinDate = user.JoinDate,
                    IsAdmin = user.IsAdmin,
                    Activity = userStats,
                    RecentPosts = recentPosts
                };

                return Ok(userProfile);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error loading user profile", error = ex.Message });
            }
        }

        // GET api/View/User/{userId}/Posts
        [HttpGet("User/{userId}/Posts")]
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
                    sourceURL = a.SourceURL,
                    sourceName = a.SourceName,
                    publishDate = a.PublishDate,
                    category = a.Category,
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
    }
}
