using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsSite.BL;
using NewsSitePro.Models;

namespace NewsSite.Pages
{
    /// <summary>
    /// Demo page showing different post rendering contexts and layouts.
    /// This page demonstrates the Enhanced ViewComponent System's flexibility
    /// for displaying posts in various contexts with different permissions and layouts.
    /// </summary>
    public class PostRenderingDemoModel : PageModel
    {
        public HeaderViewModel HeaderData { get; set; } = new HeaderViewModel();
        public NewsArticle SamplePost { get; set; } = new NewsArticle();
        private readonly DBservices _dbService;

        public PostRenderingDemoModel()
        {
            _dbService = new DBservices();
        }

        public void OnGet()
        {
            var jwt = Request.Cookies["jwtToken"];
            User? currentUser = null;

            if (!string.IsNullOrEmpty(jwt))
            {
                try
                {
                    var user = new User().ExtractUserFromJWT(jwt);
                    currentUser = _dbService.GetUserById(user.Id);
                }
                catch
                {
                    // Handle JWT extraction failure
                }
            }

            HeaderData = new HeaderViewModel
            {
                UserName = currentUser?.Name ?? "Guest",
                NotificationCount = currentUser != null ? 3 : 0,
                CurrentPage = "Demo",
                user = currentUser
            };

            // Create a sample post for demonstration
            SamplePost = new NewsArticle
            {
                ArticleID = 1,
                Title = "Sample News Article for Context Demo",
                Content = "This is a sample news article that demonstrates how the Enhanced ViewComponent System works with different contexts and layouts. The system automatically adjusts the display based on the viewing context, user relationships, and permissions.",
                Category = "Technology",
                Username = "DemoAuthor",
                UserID = 999,
                PublishDate = DateTime.Now.AddHours(-2),
                ImageURL = "https://via.placeholder.com/400x200",
                SourceURL = "https://example.com",
                SourceName = "Demo Source",
                LikesCount = 42,
                ViewsCount = 1337,
                IsLiked = false,
                IsSaved = false
            };
        }
    }
}
