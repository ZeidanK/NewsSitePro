/**
 * ViewController.cs
 * Purpose: Handles view-related operations, view component rendering, and UI data preparation
 * Responsibilities: View component management, UI data formatting, view state management, client-side data preparation
 * Architecture: Uses PostService from BL layer for view data preparation and rendering support
 */

using Microsoft.AspNetCore.Mvc;
using NewsSite.BL;
using NewsSite.BL.Services;

namespace NewsSite.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ViewController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly INewsService _newsService;

        public ViewController(IUserService userService, INewsService newsService)
        {
            _userService = userService;
            _newsService = newsService;
        }

        // GET api/View/User/{userId}
        [HttpGet("User/{userId}")]
        public async Task<IActionResult> GetUserProfile(int userId)
        {
            try
            {
                // Get user basic info
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                // Get user statistics
                var userStats = await _userService.GetUserStatsAsync(userId);

                // Get user's recent posts
                var recentPosts = await _newsService.GetArticlesByUserAsync(userId, 1, 10);

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
        public async Task<IActionResult> GetUserPosts(int userId, [FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            try
            {
                var articles = await _newsService.GetArticlesByUserAsync(userId, page, limit);
                
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
