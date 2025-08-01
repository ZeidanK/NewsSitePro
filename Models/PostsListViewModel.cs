using NewsSite.BL;

namespace NewsSitePro.Models
{
    /// <summary>
    /// ViewModel for the PostsList partial view that provides context-aware post rendering.
    /// This model contains the posts to display along with the current user context
    /// needed for proper permission and relationship-based rendering.
    /// </summary>
    public class PostsListViewModel
    {
        public List<NewsArticle> Posts { get; set; } = new List<NewsArticle>();
        public User? CurrentUser { get; set; }
        public string FeedType { get; set; } = "all";
        public PostViewMode ViewMode { get; set; } = PostViewMode.Feed;
    }
}
