using Microsoft.AspNetCore.Mvc;
using NewsSite.BL;
using NewsSite.DAL;
using NewsSite.Models;

namespace NewsSite.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IConfiguration _config;

        public UserController(IConfiguration config)
        {
            _config = config;
        }

        // Save an article
        [HttpPost("save")]
        public IActionResult SaveArticle([FromBody] SaveArticleRequest request)
        {
            var repo = new SavedArticleRepository();
            bool success = repo.SaveArticle(request.UserId, request.ArticleId);
            if (!success)
                return BadRequest("Failed to save article.");

            return Ok("Article saved successfully.");
        }

        // Share an article
        [HttpPost("share")]
        public IActionResult ShareArticle([FromBody] ShareArticleRequest request)
        {
            var repo = new SharedArticleRepository();
            bool success = repo.ShareArticle(request.Article, request.UserId);
            if (!success)
                return BadRequest("Failed to share article.");

            return Ok("Article shared successfully.");
        }

        // Report an article
        [HttpPost("report")]
        public IActionResult ReportArticle([FromBody] ReportRequest request)
        {
            var repo = new ReportRepository();
            bool success = repo.ReportArticle(request.UserId, request.ArticleId, request.Reason);
            if (!success)
                return BadRequest("Failed to report article.");

            return Ok("Report submitted successfully.");
        }

        // Block a user
        [HttpPost("block")]
        public IActionResult BlockUser([FromBody] BlockUserRequest request)
        {
            var repo = new UserRepository(_config);
            bool success = repo.BlockUser(request.BlockerId, request.BlockedId);
            if (!success)
                return BadRequest("Failed to block user.");

            return Ok("User blocked successfully.");
        }

        // Update interest tags
        [HttpPut("interests")]
        public IActionResult UpdateInterestTags([FromBody] InterestTagsRequest request)
        {
            var repo = new InterestTagRepository();
            bool success = repo.UpdateUserTags(request.UserId, request.Tags);
            if (!success)
                return BadRequest("Failed to update tags.");

            return Ok("Interest tags updated.");
        }
    }
}
