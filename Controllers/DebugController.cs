using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Rendering;
using NewsSite.BL;
using NewsSitePro.Models;
using System.Text;
using System.IdentityModel.Tokens.Jwt;

namespace NewsSitePro.Controllers
{
    [Route("api/[controller]")]
    public class DebugController : Controller
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

        [HttpPost("TestPost/Context")]
        public async Task<IActionResult> GetPostContext([FromBody] TestPostContextRequest request)
        {
            try
            {
                // Get the post data
                var post = await _dbService.GetNewsArticleById(request.PostId);
                if (post == null)
                {
                    return NotFound("Post not found");
                }

                // Get current user - same logic as Index page
                User? currentUser = null;
                int? currentUserId = null;
                
                var jwt = Request.Cookies["jwtToken"];
                if (!string.IsNullOrEmpty(jwt))
                {
                    try
                    {
                        var user = new User().ExtractUserFromJWT(jwt);
                        currentUser = _dbService.GetUserById(user.Id);
                        currentUserId = user.Id;
                    }
                    catch
                    {
                        // Invalid JWT, continue as guest
                    }
                }
                
                // Fallback to basic authentication check
                if (currentUser == null && HttpContext.User.Identity?.IsAuthenticated == true)
                {
                    currentUser = new User 
                    { 
                        Id = 1, 
                        Name = HttpContext.User.Identity.Name ?? "Test User", 
                        IsAdmin = false 
                    };
                }

                // Create the appropriate context based on request
                PostDisplayContext context = request.ContextType.ToLower() switch
                {
                    "feed" => PostContextFactory.CreateFeedContext(currentUser, post, "all"),
                    "individual" => PostContextFactory.CreateIndividualContext(currentUser, post),
                    "profile" => PostContextFactory.CreateProfileContext(currentUser, post, post.UserID),
                    "admin" => PostContextFactory.CreateAdminContext(currentUser, post),
                    "compact" => PostContextFactory.CreateCompactContext(currentUser, post),
                    "search" => PostContextFactoryExtensions.CreateSearchContext(currentUser, post),
                    "trending" => PostContextFactoryExtensions.CreateTrendingContext(currentUser, post),
                    "saved" => PostContextFactoryExtensions.CreateSavedContext(currentUser, post),
                    "mobile" => PostContextFactoryExtensions.CreateMobileContext(currentUser, post),
                    _ => PostContextFactory.CreateFeedContext(currentUser, post, "all")
                };

                // Apply feature overrides
                if (request.ShowComments.HasValue)
                    context.ShowComments = request.ShowComments.Value;
                
                if (request.ShowFullContent.HasValue)
                    context.ShowFullContent = request.ShowFullContent.Value;
                
                if (request.ShowFollowButton.HasValue)
                    context.ShowFollowButton = request.ShowFollowButton.Value;

                // Create the ViewModel
                var viewModel = new PostCardViewModel
                {
                    Post = post,
                    Context = context,
                    CurrentUser = currentUser
                };

                // Return the ViewComponent directly - this will render the actual component
                return ViewComponent("PostCard", viewModel);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }

    public class TestPostContextRequest
    {
        public int PostId { get; set; }
        public string ContextType { get; set; } = "feed";
        public bool? ShowComments { get; set; }
        public bool? ShowFullContent { get; set; }
        public bool? ShowFollowButton { get; set; }
    }
}
