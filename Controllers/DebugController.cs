using Microsoft.AspNetCore.Mvc;
using NewsSite.BL;

namespace NewsSitePro.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DebugController : ControllerBase
    {
        private readonly DBservices _dbService;

        public DebugController()
        {
            _dbService = new DBservices();
        }

        [HttpGet("posts")]
        public IActionResult GetPosts()
        {
            try
            {
                var posts = _dbService.GetAllNewsArticles(1, 3, null, null);
                
                var debugInfo = posts.Select(post => new
                {
                    ArticleID = post.ArticleID,
                    Title = post.Title,
                    Username = post.Username,
                    UserProfilePicture = post.UserProfilePicture,
                    UserProfilePictureLength = post.UserProfilePicture?.Length ?? 0,
                    IsUserProfilePictureNull = post.UserProfilePicture == null,
                    IsUserProfilePictureEmpty = string.IsNullOrEmpty(post.UserProfilePicture)
                }).ToList();

                return Ok(debugInfo);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}
