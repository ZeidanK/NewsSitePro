using NewsSite.BL;

namespace NewsSitePro.Models
{
    public class PostCardViewModel
    {
        public NewsArticle Post { get; set; } = new NewsArticle();
        public PostDisplayConfig Config { get; set; } = new PostDisplayConfig();
        public User? CurrentUser { get; set; }
        public string? BaseUrl { get; set; }
    }
}
